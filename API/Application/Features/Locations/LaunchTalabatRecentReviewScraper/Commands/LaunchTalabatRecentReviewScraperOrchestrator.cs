using API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.DTOs;
using API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Queries;
using API.Shared.Helpers;
using API.Shared.Models;
using Hangfire;
using MediatR;

namespace API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Commands
{
    public record LaunchTalabatRecentReviewScraperOrchestrator(int LimitPerRequest = 10, int MaxReviews = 100) : IRequest<RequestResult<bool>>;

    public class LaunchTalabatRecentReviewScraperOrchestratorHandler : IRequestHandler<LaunchTalabatRecentReviewScraperOrchestrator, RequestResult<bool>>
    {
        private readonly IMediator _mediator;
        private const double JOB_DISTRIBUTION_MINUTES = 30.0;
        private static readonly DateTime LOGGING_CUTOFF_DATE = new DateTime(2025, 11, 15);
        private readonly ILogger<LaunchTalabatRecentReviewScraperOrchestratorHandler> _logger;

        public LaunchTalabatRecentReviewScraperOrchestratorHandler(IMediator mediator, ILogger<LaunchTalabatRecentReviewScraperOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(LaunchTalabatRecentReviewScraperOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                var talabatLocationsResult = await _mediator.Send(new GetTalabatLocationsQuery(), cancellationToken).ConfigureAwait(false);

                if (!talabatLocationsResult.IsSuccess)
                {
                    _logger.LogInformation("No Talabat locations found");
                    return RequestResult<bool>.Failure(talabatLocationsResult.ErrorCode);
                }

                int totalJobsEnqueued = 0;
                double delayIntervalMinutes = 0;

                // Calculate total jobs to determine delay interval
                int totalJobs = talabatLocationsResult.Data.Sum(location => location.TalabatLocationIDs?.Count ?? 0);

                if (totalJobs > 1)
                {
                    // Distribute jobs over configured minutes to avoid overwhelming the scraper API
                    delayIntervalMinutes = JOB_DISTRIBUTION_MINUTES / totalJobs;
                }

                var companiesIDs = talabatLocationsResult.Data.Select(loc => loc.CompanyID).Distinct().ToList();
               
                foreach (var location in talabatLocationsResult.Data)
                {
                    if (location == null)
                    {
                        _logger.LogInformation("Null location encountered, skipping");
                        continue;
                    }


                    if (location.TalabatLocationIDs == null || location.TalabatLocationIDs.Count == 0)
                    {
                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogInformation("Location {LocationName} has no Talabat IDs", location.LocationName);

                        continue;
                    }
                    var talabatReviewCountResult = await _mediator.Send(new GetTalabatLocationReviewCountQuery(location.LocationId), cancellationToken).ConfigureAwait(false);
                    var talabatReviewCount = talabatReviewCountResult.IsSuccess ? talabatReviewCountResult.Data : 0;

                    foreach (var talabatLocationId in location.TalabatLocationIDs)
                    {
                        if (totalJobsEnqueued == 0)
                        {
                            // First job - enqueue immediately
                            // Note: Hangfire background jobs run in a different context, but we pass cancellationToken for consistency
                            BackgroundJob.Enqueue<IMediator>(mediator =>
                                mediator.Send(
                                    new ProcessTalabatLocationRecentReviewsCommand(
                                        location,
                                        talabatLocationId,
                                        talabatReviewCount,
                                        request.LimitPerRequest,
                                        request.MaxReviews),
                                    cancellationToken));

                            if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                                _logger.LogInformation("Enqueued immediate processing for Talabat location {LocationName}, TalabatID: {TalabatLocationID} ({JobIndex}/{TotalJobs})",
                                location.LocationName, talabatLocationId, totalJobsEnqueued + 1, totalJobs);
                        }
                        else
                        {
                            // Schedule with delay to distribute load
                            // Note: Hangfire background jobs run in a different context, but we pass cancellationToken for consistency
                            TimeSpan delay = TimeSpan.FromMinutes(totalJobsEnqueued * delayIntervalMinutes);
                            BackgroundJob.Schedule<IMediator>(
                                mediator => mediator.Send(
                                    new ProcessTalabatLocationRecentReviewsCommand(
                                        location,
                                        talabatLocationId,
                                        talabatReviewCount,
                                        request.LimitPerRequest,
                                        request.MaxReviews),
                                    cancellationToken),
                                delay);

                            if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                                _logger.LogInformation("Scheduled processing for Talabat location {LocationName}, TalabatID: {TalabatLocationID} in {DelayMinutes:F2} minutes ({JobIndex}/{TotalJobs})",
                                location.LocationName, talabatLocationId, delay.TotalMinutes, totalJobsEnqueued + 1, totalJobs);
                        }

                        totalJobsEnqueued++;
                    }
                }

                if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                    _logger.LogInformation("Successfully enqueued {JobCount} Talabat location processing jobs", totalJobsEnqueued);

                return totalJobsEnqueued > 0 ? RequestResult<bool>.Success()
                    : RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching Talabat recent review scraper orchestrator");
                return RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None);
            }
        }

    }
}
