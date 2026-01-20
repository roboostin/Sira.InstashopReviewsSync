using API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Queries;
using API.Shared.Helpers;
using API.Shared.Models;
using Hangfire;
using MediatR;

namespace API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Commands
{
    public record LaunchInstashopRecentReviewScraperOrchestrator(int LimitPerRequest = 20, int MaxReviews = 50) : IRequest<RequestResult<bool>>;

    public class LaunchInstashopRecentReviewScraperOrchestratorHandler : IRequestHandler<LaunchInstashopRecentReviewScraperOrchestrator, RequestResult<bool>>
    {
        private readonly IMediator _mediator;
        private const double JOB_DISTRIBUTION_MINUTES = 30.0;
        private static readonly DateTime LOGGING_CUTOFF_DATE = new DateTime(2025, 11, 15);
        private readonly ILogger<LaunchInstashopRecentReviewScraperOrchestratorHandler> _logger;

        public LaunchInstashopRecentReviewScraperOrchestratorHandler(IMediator mediator, ILogger<LaunchInstashopRecentReviewScraperOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(LaunchInstashopRecentReviewScraperOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                var instashopLocationsResult = await _mediator.Send(new GetInstashopLocationsQuery(), cancellationToken).ConfigureAwait(false);

                if (!instashopLocationsResult.IsSuccess)
                {
                    _logger.LogInformation("No Instashop locations found");
                    return RequestResult<bool>.Failure(instashopLocationsResult.ErrorCode);
                }

                int totalJobsEnqueued = 0;
                double delayIntervalMinutes = 0;

                // Calculate total jobs to determine delay interval
                int totalJobs = instashopLocationsResult.Data.Count;

                if (totalJobs > 1)
                {
                    // Distribute jobs over configured minutes to avoid overwhelming the scraper API
                    delayIntervalMinutes = JOB_DISTRIBUTION_MINUTES / totalJobs;
                }

                var companiesIDs = instashopLocationsResult.Data.Select(loc => loc.CompanyID).Distinct().ToList();
               
                foreach (var location in instashopLocationsResult.Data)
                {
                    if (location == null)
                    {
                        _logger.LogInformation("Null location encountered, skipping");
                        continue;
                    }


                    if (string.IsNullOrEmpty(location.InstashopClientId))
                    {
                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogInformation("Location {LocationName} has no Instashop Client ID", location.LocationName);

                        continue;
                    }
                    var instashopReviewCountResult = await _mediator.Send(new GetInstashopLocationReviewCountQuery(location.LocationId), cancellationToken).ConfigureAwait(false);
                    var instashopReviewCount = instashopReviewCountResult.IsSuccess ? instashopReviewCountResult.Data : 0;

                    if (totalJobsEnqueued == 0)
                    {
                        // First job - enqueue immediately
                        // Note: Hangfire background jobs run in a different context, but we pass cancellationToken for consistency
                        BackgroundJob.Enqueue<IMediator>(mediator =>
                            mediator.Send(
                                new ProcessInstashopLocationRecentReviewsCommand(
                                    location,
                                    location.InstashopClientId,
                                    instashopReviewCount,
                                    request.LimitPerRequest,
                                    request.MaxReviews),
                                cancellationToken));

                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogInformation("Enqueued immediate processing for Instashop location {LocationName}, InstashopClientId: {InstashopClientId} ({JobIndex}/{TotalJobs})",
                            location.LocationName, location.InstashopClientId, totalJobsEnqueued + 1, totalJobs);
                    }
                    else
                    {
                        // Schedule with delay to distribute load
                        // Note: Hangfire background jobs run in a different context, but we pass cancellationToken for consistency
                        TimeSpan delay = TimeSpan.FromMinutes(totalJobsEnqueued * delayIntervalMinutes);
                        BackgroundJob.Schedule<IMediator>(
                            mediator => mediator.Send(
                                new ProcessInstashopLocationRecentReviewsCommand(
                                    location,
                                    location.InstashopClientId,
                                    instashopReviewCount,
                                    request.LimitPerRequest,
                                    request.MaxReviews),
                                cancellationToken),
                            delay);

                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogInformation("Scheduled processing for Instashop location {LocationName}, InstashopClientId: {InstashopClientId} in {DelayMinutes:F2} minutes ({JobIndex}/{TotalJobs})",
                            location.LocationName, location.InstashopClientId, delay.TotalMinutes, totalJobsEnqueued + 1, totalJobs);
                    }

                    totalJobsEnqueued++;
                }

                if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                    _logger.LogInformation("Successfully enqueued {JobCount} Instashop location processing jobs", totalJobsEnqueued);

                return totalJobsEnqueued > 0 ? RequestResult<bool>.Success()
                    : RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching Instashop recent review scraper orchestrator");
                return RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None);
            }
        }

    }
}
