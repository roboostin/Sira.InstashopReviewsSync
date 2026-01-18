using API.Application.Features.Common.Locations.Queries;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record CheckCompanyTalabatLocationScrapeStatusCommand(
        long CompanyID
    ) : IRequest<RequestResult<bool>>;

    public class CheckCompanyTalabatLocationScrapeStatusCommandHandler : IRequestHandler<CheckCompanyTalabatLocationScrapeStatusCommand, RequestResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CheckCompanyTalabatLocationScrapeStatusCommandHandler> _logger;

        public CheckCompanyTalabatLocationScrapeStatusCommandHandler(
            IMediator mediator,
            ILogger<CheckCompanyTalabatLocationScrapeStatusCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(CheckCompanyTalabatLocationScrapeStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var threeHoursAgo = DateTime.UtcNow.AddHours(-3);

                var locationsResult = await _mediator.Send(new GetActiveTalabatLocationsQuery(), cancellationToken)
                    .ConfigureAwait(false);

                if (!locationsResult.IsSuccess || locationsResult.Data == null || locationsResult.Data.Count == 0)
                {
                    return RequestResult<bool>.Success(true);
                }

                var companyLocations = locationsResult.Data
                    .Where(l => l.CompanyID == request.CompanyID)
                    .ToList();

                if (companyLocations.Count == 0)
                {
                    return RequestResult<bool>.Success(true);
                }

                var staleLocationsCount = 0;

                foreach (var location in companyLocations)
                {
                    // Check if LastSuccessfulScrapeTime is null or more than 3 hours ago
                    if (!location.LastSuccessfulScrapeTime.HasValue || 
                        location.LastSuccessfulScrapeTime.Value < threeHoursAgo)
                    {
                        staleLocationsCount++;
                        _logger.LogError(
                            "No reviews returned from scraper for Talabat location. CompanyID: {CompanyID}, LocationID: {LocationID}, LocationName: {LocationName}, LastSuccessfulScrapeTime: {LastSuccessfulScrapeTime}",
                            request.CompanyID,
                            location.LocationID,
                            location.LocationName,
                            location.LastSuccessfulScrapeTime?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "Never");
                    }
                }

                return RequestResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking Talabat location scrape status for company {CompanyID}", request.CompanyID);
                return RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None);
            }
        }
    }
}

