using API.Domain.UnitOfWork;
using API.Helpers;
using API.Shared.Helpers;
using API.Shared.Models;
using MediatR;
using API.Domain.Enums;
using API.Application.Services.ReviewaDataFilter;
using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.DTOs;
using API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Queries;

namespace API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Commands
{
    public record ProcessInstashopLocationRecentReviewsCommand(
        InstashopLocationDto Location,
        string InstashopClientId,
        int InstashopReviewsCount,
        int LimitPerRequest = 20,
        int MaxReviews = 50) : IRequest<RequestResult<bool>>;

    public class ProcessInstashopLocationRecentReviewsCommandHandler : IRequestHandler<ProcessInstashopLocationRecentReviewsCommand, RequestResult<bool>>
    {
        private const string SCRAPER_ENDPOINT = "/instashop";
        private const string DEFAULT_SCRAPER_BASE_URL = "https://scraping-render-9ijs.onrender.com";
        private static readonly DateTime LOGGING_CUTOFF_DATE = new DateTime(2025, 12, 19);
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IScraperHttpClientService _scraperHttpClientService;
        private readonly IReviewDateFilterService _reviewDateFilterService;
        private readonly ILogger<ProcessInstashopLocationRecentReviewsCommandHandler> _logger;

        private string GetScraperBaseUrl()
        {
            var baseUrl = ConfigurationHelper.GetConfigurationValue("Scraper:BaseUrl");
            return string.IsNullOrWhiteSpace(baseUrl) ? DEFAULT_SCRAPER_BASE_URL : baseUrl;
        }

        public ProcessInstashopLocationRecentReviewsCommandHandler(
            IMediator mediator,
            ILogger<ProcessInstashopLocationRecentReviewsCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IScraperHttpClientService scraperHttpClientService,
            IReviewDateFilterService reviewDateFilterService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _scraperHttpClientService = scraperHttpClientService ?? throw new ArgumentNullException(nameof(scraperHttpClientService));
            _reviewDateFilterService = reviewDateFilterService ?? throw new ArgumentNullException(nameof(reviewDateFilterService));
        }

        public async Task<RequestResult<bool>> Handle(ProcessInstashopLocationRecentReviewsCommand request, CancellationToken cancellationToken)
        {
            // Validate request and location
            if (request?.Location == null)
            {
                _logger.LogInformation("ProcessInstashopLocationRecentReviewsCommand request or Location is null");
                return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "Request or Location is null");
            }

            var scrapeAttemptTime = DateTime.UtcNow;

            if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                _logger.LogInformation("Processing Instashop location: {LocationName}, ID: {LocationId}, InstashopClientId: {InstashopClientId}",
                request.Location.LocationName, request.Location.LocationId, request.InstashopClientId);

                // Prepare query parameters - target last 24 hours
                var queryParameters = new List<QueryParam>
                {
                    new("clientId", request.InstashopClientId),
                    new("timeStamp", DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd:HH:mm")),
                    new("limitPerRequest", request.LimitPerRequest.ToString()),
                    new("maxReviews", request.MaxReviews.ToString())
                };

                // Call the scraper API using shared service
                var scraperBaseUrl = GetScraperBaseUrl();
                var response = await _scraperHttpClientService.GetReviewsAsync<InstashopReviewResponse>(
                    scraperBaseUrl,
                    SCRAPER_ENDPOINT,
                    queryParameters,
                    cancellationToken).ConfigureAwait(false);

                if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                    _logger.LogInformation("Scraper API response - Success: {Success}, Count: {Count}, Reviews: {ReviewCount}",
                    response?.Success, response?.Count, response?.Reviews?.Count);

                // Update summary if reviews data is available
                if (response != null && response.Success && response.Reviews != null && response.Reviews.Count > 0)
                {
                    // Calculate average rating from reviews
                    double? avgRating = null;
                    if (response.Reviews.Count > 0)
                    {
                        var totalRating = response.Reviews.Sum(r => (r.ProductAccuracy + r.DeliverySpeed) / 2.0);
                        avgRating = totalRating / response.Reviews.Count;
                    }

                    var upsertResult = await _mediator.Send(new UpSertInstashopSourceReviewSummaryOrchestrator(
                        request.Location.LocationId,
                        request.Location.CompanyID,
                        avgRating ?? 0,
                        response.Reviews.Count
                    ), cancellationToken).ConfigureAwait(false);

                    if (!upsertResult.IsSuccess)
                    {
                        // Log but continue processing reviews
                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogWarning("Failed to upsert source review summary: {Message}", upsertResult.Message);
                    }
                }
            
                if (response?.Success == true && response.Reviews != null && response.Reviews.Count > 0)
                {
                    var firstReview = response.Reviews.FirstOrDefault();
                    if (firstReview != null && DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                    {
                        _logger.LogInformation("First review - CreatedAt: '{CreatedAt}', Comment: '{Comment}', ProductAccuracy: {ProductAccuracy}, DeliverySpeed: {DeliverySpeed}",
                            firstReview.CreatedAt, firstReview.Comment, firstReview.ProductAccuracy, firstReview.DeliverySpeed);
                    }

                    // Filter reviews from the last 24 hours
                    var recentReviews = response.Reviews
                        .Where(r => _reviewDateFilterService.IsWithinLast24HoursInstashop(r.CreatedAt))
                        .ToList();

                    if (recentReviews.Count == 0)
                    {
                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogInformation("No reviews from the last 24 hours found for Instashop location {LocationName}",
                            request.Location.LocationName);
                        return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "No reviews from the last 24 hours found");
                    }

                    var saveResult = await _mediator.Send(new SaveInstashopReviewsCommand(
                        request.Location.LocationId,
                        recentReviews,
                        DateTime.UtcNow,
                        request.Location.LocationName,
                        request.MaxReviews,
                        request.Location.CompanyID,
                        SourceType.Instashop
                    ), cancellationToken).ConfigureAwait(false);

                    if (!saveResult.IsSuccess)
                    {
                        return RequestResult<bool>.Failure(saveResult.ErrorCode, saveResult.Message);
                    }

                    
                    await _mediator.Send(new UpdateLocationScrapeTimesCommand(
                        request.Location.LocationId,
                        LastScrapeAttemptTime : scrapeAttemptTime,
                        LastSuccessfulScrapeTime: scrapeAttemptTime
                    ), cancellationToken).ConfigureAwait(false);

                    // Save changes - each background job needs its own save
                    await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

                    if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                        _logger.LogInformation("Saved {Count} reviews from the last 24 hours for Instashop location {LocationName}",
                        recentReviews.Count, request.Location.LocationName);

                    return RequestResult<bool>.Success(true);
                }
                else
                {
                    await _mediator.Send(new UpdateLocationScrapeTimesCommand(
                    request.Location.LocationId,
                    LastScrapeAttemptTime: scrapeAttemptTime
                    ), cancellationToken).ConfigureAwait(false);
                    
                    // Provide detailed error message based on response state
                    if (response == null)
                    {
                        _logger.LogWarning("Scraper API returned null response for Instashop location {LocationName}, LocationID: {LocationId}, InstashopClientId: {InstashopClientId}",
                            request.Location.LocationName, request.Location.LocationId, request.InstashopClientId);
                        return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "Scraper API returned null response");
                    }

                    if (!response.Success)
                    {
                        _logger.LogWarning("Scraper API returned unsuccessful response for Instashop location {LocationName}, LocationID: {LocationId}, InstashopClientId: {InstashopClientId}",
                            request.Location.LocationName, request.Location.LocationId, request.InstashopClientId);
                        return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "Scraper API returned unsuccessful response");
                    }

                    if (response.Reviews == null || response.Reviews.Count == 0)
                    {
                        _logger.LogInformation("No reviews found for Instashop location {LocationName}, LocationID: {LocationId}, InstashopClientId: {InstashopClientId}, Count: {Count}",
                            request.Location.LocationName, request.Location.LocationId, request.InstashopClientId, response.Count);
                        return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, $"No reviews found. API returned count: {response.Count}");
                    }

                    _logger.LogWarning("No reviews returned from scraper for Instashop location {LocationName}, LocationID: {LocationId}, InstashopClientId: {InstashopClientId}",
                        request.Location.LocationName, request.Location.LocationId, request.InstashopClientId);

                    return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "No reviews returned from scraper");
                }
        }
    }
}
