using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Enums;
using API.Shared.Helpers;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.LaunchTalabatReviewScraper.Commands
{
    public record LaunchTalabatReviewScraperOrchestrator(
        long LocationId,
        List<int> TalabatLocationIds,
        DateTime TimeStamp,
        string LocationName,
        long CompanyID,
        int LimitPerRequest = 5,
        int MaxReviews = 10
    ) : IRequest<RequestResult<bool>>;

    public class LaunchTalabatReviewScraperOrchestratorHandler : IRequestHandler<LaunchTalabatReviewScraperOrchestrator, RequestResult<bool>>
    {
        private const int MAX_LIMIT_PER_REQUEST = 10;
        private const int MAX_REVIEWS = 10;
        private const string SCRAPER_BASE_URL = "https://scraping-render-9ijs.onrender.com";
        private const string SCRAPER_ENDPOINT = "/talabat";
        private readonly IMediator _mediator;
        private readonly UserState _userState;
        private readonly ILogger<LaunchTalabatReviewScraperOrchestratorHandler> _logger;

        public LaunchTalabatReviewScraperOrchestratorHandler(
            IMediator mediator,
            UserState userState,
            ILogger<LaunchTalabatReviewScraperOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _userState = userState;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(LaunchTalabatReviewScraperOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.LimitPerRequest <= 0 || request.LimitPerRequest > MAX_LIMIT_PER_REQUEST)
                {
                    _logger.LogInformation("Invalid LimitPerRequest: {LimitPerRequest}. Must be between 1 and {Max}",
                        request.LimitPerRequest, MAX_LIMIT_PER_REQUEST);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "LimitPerRequest must be between 1 and 10000");
                }

                if (request.MaxReviews <= 0 || request.MaxReviews > MAX_REVIEWS)
                {
                    _logger.LogInformation("Invalid MaxReviews: {MaxReviews}. Must be between 1 and {Max}",
                        request.MaxReviews, MAX_REVIEWS);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "MaxReviews must be between 1 and 50000");
                }

                // Validate TalabatLocationIds
                if (request.TalabatLocationIds == null || request.TalabatLocationIds.Count == 0)
                {
                    _logger.LogInformation("TalabatLocationIds is null or empty for LocationId: {LocationId}", request.LocationId);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "TalabatLocationIds must contain at least one location ID");
                }

                var firstLocationId = request.TalabatLocationIds.FirstOrDefault();
                if (firstLocationId == 0)
                {
                    _logger.LogInformation("Invalid TalabatLocationId (0) for LocationId: {LocationId}", request.LocationId);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "Invalid TalabatLocationId");
                }

                // Prepare query parameters
                var queryParameters = new List<QueryParam>
                {
                    new("locationId", firstLocationId.ToString()),
                    new("timeStamp", request.TimeStamp.ToString("yyyy-MM-dd:HH:mm")),
                    new("limitPerRequest", request.LimitPerRequest.ToString()),
                    new("maxReviews", request.MaxReviews.ToString())
                };

                // Call the scraper API
                var (_, response) = await RestSharpHelper.GetAsync<TalabatReviewResponse>(
                    SCRAPER_BASE_URL,
                    SCRAPER_ENDPOINT,
                    null,
                    queryParameters
                );

                if (response != null && response.Reviews != null && response.Reviews.Reviews != null && response.Reviews.Reviews.Count > 0)
                {
                    var saveResult = await _mediator.Send(new SaveTalabatReviewsCommand(
                        request.LocationId,
                        response.Reviews.Reviews,
                        request.TimeStamp,
                        request.LocationName,
                        request.MaxReviews,
                        request.CompanyID
                    ), cancellationToken);

                    if (!saveResult.IsSuccess)
                    {
                        return RequestResult<bool>.Failure(saveResult.ErrorCode, saveResult.Message);
                    }

                    return RequestResult<bool>.Success(true, "Talabat review scraper launched successfully");
                }

                return RequestResult<bool>.Failure();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error launching Talabat review scraper for LocationId: {LocationId}", request.LocationId);
                return RequestResult<bool>.Failure(ErrorCode.NotFound, $"An error occurred while launching the scraper: {ex.Message}");
            }

        }
    }
}
