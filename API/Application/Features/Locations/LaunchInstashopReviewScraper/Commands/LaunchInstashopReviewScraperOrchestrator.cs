using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Enums;
using API.Shared.Helpers;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.LaunchInstashopReviewScraper.Commands
{
    public record LaunchInstashopReviewScraperOrchestrator(
        long LocationId,
        string InstashopClientId,
        DateTime TimeStamp,
        string LocationName,
        long CompanyID,
        int LimitPerRequest = 20,
        int MaxReviews = 50
    ) : IRequest<RequestResult<bool>>;

    public class LaunchInstashopReviewScraperOrchestratorHandler : IRequestHandler<LaunchInstashopReviewScraperOrchestrator, RequestResult<bool>>
    {
        private const int MAX_LIMIT_PER_REQUEST = 10000;
        private const int MAX_REVIEWS = 50000;
        private const string SCRAPER_BASE_URL = "https://scraping-render-9ijs.onrender.com";
        private const string SCRAPER_ENDPOINT = "/instashop";
        private readonly IMediator _mediator;
        private readonly UserState _userState;
        private readonly ILogger<LaunchInstashopReviewScraperOrchestratorHandler> _logger;

        public LaunchInstashopReviewScraperOrchestratorHandler(
            IMediator mediator,
            UserState userState,
            ILogger<LaunchInstashopReviewScraperOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _userState = userState;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(LaunchInstashopReviewScraperOrchestrator request, CancellationToken cancellationToken)
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

                if (string.IsNullOrWhiteSpace(request.InstashopClientId))
                {
                    _logger.LogInformation("InstashopClientId is null or empty for LocationId: {LocationId}", request.LocationId);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "InstashopClientId is required");
                }

                // Prepare query parameters
                var queryParameters = new List<QueryParam>
                {
                    new("clientId", request.InstashopClientId),
                    new("timeStamp", request.TimeStamp.ToString("yyyy-MM-dd:HH:mm")),
                    new("limitPerRequest", request.LimitPerRequest.ToString()),
                    new("maxReviews", request.MaxReviews.ToString())
                };

                // Call the scraper API
                var (_, response) = await RestSharpHelper.GetAsync<InstashopReviewResponse>(
                    SCRAPER_BASE_URL,
                    SCRAPER_ENDPOINT,
                    null,
                    queryParameters
                );

                // Log response details for debugging
                _logger.LogInformation("Scraper API response - Success: {Success}, Count: {Count}, Reviews: {ReviewCount}",
                    response?.Success, response?.Count, response?.Reviews?.Count ?? 0);

                if (response != null && response.Success && response.Reviews != null && response.Reviews.Count > 0)
                {
                    var saveResult = await _mediator.Send(new SaveInstashopReviewsCommand(
                        request.LocationId,
                        response.Reviews,
                        request.TimeStamp,
                        request.LocationName,
                        request.MaxReviews,
                        request.CompanyID,
                        SourceType.Instashop
                    ), cancellationToken);

                    if (!saveResult.IsSuccess)
                    {
                        return RequestResult<bool>.Failure(saveResult.ErrorCode, saveResult.Message);
                    }

                    return RequestResult<bool>.Success(true, "Instashop review scraper launched successfully");
                }

                // Provide detailed error message
                if (response == null)
                {
                    _logger.LogWarning("Scraper API returned null response for LocationId: {LocationId}, InstashopClientId: {InstashopClientId}",
                        request.LocationId, request.InstashopClientId);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "Scraper API returned null response");
                }

                if (!response.Success)
                {
                    _logger.LogWarning("Scraper API returned unsuccessful response for LocationId: {LocationId}, InstashopClientId: {InstashopClientId}",
                        request.LocationId, request.InstashopClientId);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, "Scraper API returned unsuccessful response");
                }

                if (response.Reviews == null || response.Reviews.Count == 0)
                {
                    _logger.LogInformation("No reviews found for LocationId: {LocationId}, InstashopClientId: {InstashopClientId}, Count: {Count}",
                        request.LocationId, request.InstashopClientId, response.Count);
                    return RequestResult<bool>.Failure(ErrorCode.NotFound, $"No reviews found. API returned count: {response.Count}");
                }

                return RequestResult<bool>.Failure(ErrorCode.NotFound, "Unknown error occurred while processing scraper response");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error launching Instashop review scraper for LocationId: {LocationId}", request.LocationId);
                return RequestResult<bool>.Failure(ErrorCode.NotFound, $"An error occurred while launching the scraper: {ex.Message}");
            }

        }
    }
}
