using API.Domain.UnitOfWork;
using API.Helpers;
using API.Shared.Helpers;
using API.Shared.Models;
using MediatR;
using API.Domain.Enums;
using API.Application.Services.ReviewaDataFilter;
using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.DTOs;
using API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Queries;

namespace API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Commands
{
    public record ProcessTalabatLocationRecentReviewsCommand(
        TalabatLocationDto Location,
        int TalabatLocationId,
        int TalabatReviewsCount,
        int LimitPerRequest = 5,
        int MaxReviews = 100) : IRequest<RequestResult<bool>>;

    public class ProcessTalabatLocationRecentReviewsCommandHandler : IRequestHandler<ProcessTalabatLocationRecentReviewsCommand, RequestResult<bool>>
    {
        private const string SCRAPER_ENDPOINT = "/talabat";
        private const string DEFAULT_SCRAPER_BASE_URL = "https://scraping-render-9ijs.onrender.com";
        private static readonly DateTime LOGGING_CUTOFF_DATE = new DateTime(2025, 12, 19);
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IScraperHttpClientService _scraperHttpClientService;
        private readonly IReviewDateFilterService _reviewDateFilterService;
        private readonly ILogger<ProcessTalabatLocationRecentReviewsCommandHandler> _logger;

        private string GetScraperBaseUrl()
        {
            var baseUrl = ConfigurationHelper.GetConfigurationValue("Scraper:BaseUrl");
            return string.IsNullOrWhiteSpace(baseUrl) ? DEFAULT_SCRAPER_BASE_URL : baseUrl;
        }

        public ProcessTalabatLocationRecentReviewsCommandHandler(
            IMediator mediator,
            ILogger<ProcessTalabatLocationRecentReviewsCommandHandler> logger,
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

        public async Task<RequestResult<bool>> Handle(ProcessTalabatLocationRecentReviewsCommand request, CancellationToken cancellationToken)
        {
            // Validate request and location
            if (request?.Location == null)
            {
                _logger.LogInformation("ProcessTalabatLocationRecentReviewsCommand request or Location is null");
                return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "Request or Location is null");
            }

            var scrapeAttemptTime = DateTime.UtcNow;

            if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                _logger.LogInformation("Processing Talabat location: {LocationName}, ID: {LocationId}, TalabatID: {TalabatLocationID}",
                request.Location.LocationName, request.Location.LocationId, request.TalabatLocationId);

                // Prepare query parameters - target last 24 hours
                var queryParameters = new List<QueryParam>
                {
                    new("locationId", request.TalabatLocationId.ToString()),
                    new("timeStamp", DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd:HH:mm")),
                    new("limitPerRequest", request.LimitPerRequest.ToString()),
                    new("maxReviews", request.MaxReviews.ToString())
                };

                // Call the scraper API using shared service
                var scraperBaseUrl = GetScraperBaseUrl();
                var response = await _scraperHttpClientService.GetReviewsAsync<TalabatReviewResponse>(
                    scraperBaseUrl,
                    SCRAPER_ENDPOINT,
                    queryParameters,
                    cancellationToken).ConfigureAwait(false);

                if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                    _logger.LogInformation("Scraper API response - Success: {Success}, Count: {Count}, Reviews: {ReviewCount}",
                    response?.Success, response?.Reviews?.Count, response?.Reviews?.Reviews?.Count);

                // Update summary if reviews data is available
                if (response?.Reviews != null)
                {
                    int? talabatTotalResponses = null;
                    if (!string.IsNullOrWhiteSpace(response.Reviews.TotalReviews)
                        && int.TryParse(response.Reviews.TotalReviews, out var totalReviewsParsed))
                    {
                        talabatTotalResponses = totalReviewsParsed;
                    }
                    else if (response.Reviews.Count > 0)
                    {
                        talabatTotalResponses = response.Reviews.Count;
                    }

                    talabatTotalResponses ??= response.Reviews.Count;

                    var upsertResult = await _mediator.Send(new UpSertTalabatSourceReviewSummaryOrchestrator(
                        request.Location.LocationId,
                        request.Location.CompanyID,
                        response.Reviews.Rating,
                        talabatTotalResponses.Value
                    ), cancellationToken).ConfigureAwait(false);

                    if (!upsertResult.IsSuccess)
                    {
                        // Log but continue processing reviews
                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogWarning("Failed to upsert source review summary: {Message}", upsertResult.Message);
                    }
                }
            
                if (response?.Success == true && response.Reviews?.Reviews != null && response.Reviews.Reviews.Count > 0)
                {
                    var firstReview = response.Reviews.Reviews.FirstOrDefault();
                    if (firstReview != null && DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                    {
                        _logger.LogInformation("First review - ID: {ID}, FirstName: '{FirstName}', OrderID: {OrderID}, Feedback: '{Feedback}', Rate: {Rate}, Date: '{Date}'",
                            firstReview.ID, firstReview.FirstName, firstReview.OrderID, firstReview.Feedback, firstReview.Rate, firstReview.Date);
                    }

                    // Filter reviews from the last 24 hours
                    var recentReviews = response.Reviews.Reviews
                        .Where(r => _reviewDateFilterService.IsWithinLast24HoursTalabat(r.Date))
                        .ToList();

                    if (recentReviews.Count == 0)
                    {
                        if (DateTime.UtcNow <= LOGGING_CUTOFF_DATE)
                            _logger.LogInformation("No reviews from the last 24 hours found for Talabat location {LocationName}",
                            request.Location.LocationName);
                        return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "No reviews from the last 24 hours found");
                    }

                    var saveResult = await _mediator.Send(new SaveTalabatReviewsCommand(
                        request.Location.LocationId,
                        recentReviews,
                        DateTime.UtcNow,
                        request.Location.LocationName,
                        request.MaxReviews,
                        request.Location.CompanyID
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
                        _logger.LogInformation("Saved {Count} reviews from the last 24 hours for Talabat location {LocationName}",
                        recentReviews.Count, request.Location.LocationName);

                    return RequestResult<bool>.Success(true);
                }
                else
                {
                    await _mediator.Send(new UpdateLocationScrapeTimesCommand(
                    request.Location.LocationId,
                    LastScrapeAttemptTime: scrapeAttemptTime
                    ), cancellationToken).ConfigureAwait(false);
                    
                _logger.LogWarning("No reviews returned from scraper for Talabat location {LocationName}",
                    request.Location.LocationName);

                    return RequestResult<bool>.Failure(ErrorCode.ProcessLocationReviewsFailed, "No reviews returned from scraper");
                }
        }
    }
}
