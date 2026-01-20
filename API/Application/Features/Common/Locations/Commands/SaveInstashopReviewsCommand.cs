using API.Domain.Entities;
using API.Domain.Entities.Client;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace API.Application.Features.Common.Locations.Commands
{
    public record SaveInstashopReviewsCommand(
        long LocationId,
        List<DTOs.InstashopReview> Reviews,
        DateTime TimeStamp,
        string LocationName,
        int MaxReviews,
        long companyID,
        SourceType Source = SourceType.Instashop) : IRequest<RequestResult<bool>>;

    public class SaveInstashopReviewsCommandHandler : IRequestHandler<SaveInstashopReviewsCommand, RequestResult<bool>>
    {
        private const int NEGATIVE_SENTIMENT_THRESHOLD = 3;
        private const string SENTIMENT_NEGATIVE = "Negative";
        private const string SENTIMENT_POSITIVE = "Positive";

        // Static date formats array for performance - Instashop format: "10/2/2025, 3:10:28 PM"
        private static readonly string[] DateFormats = {
            "M/d/yyyy, h:mm:ss tt",      // "10/2/2025, 3:10:28 PM"
            "MM/dd/yyyy, h:mm:ss tt",    // "10/02/2025, 3:10:28 PM"
            "M/d/yyyy, hh:mm:ss tt",     // "10/2/2025, 03:10:28 PM"
            "MM/dd/yyyy, hh:mm:ss tt",   // "10/02/2025, 03:10:28 PM"
            "M/d/yyyy, H:mm:ss",         // "10/2/2025, 15:10:28"
            "MM/dd/yyyy, H:mm:ss",       // "10/02/2025, 15:10:28"
            "yyyy-MM-dd HH:mm:ss",       // "2025-10-02 15:10:28"
            "yyyy-MM-ddTHH:mm:ss",       // "2025-10-02T15:10:28"
        };

        private readonly IRepository<Review> _repository;
        private readonly IRepository<Location> _locationRepository;
        private readonly ILogger<SaveInstashopReviewsCommandHandler> _logger;

        public SaveInstashopReviewsCommandHandler(
            IRepository<Review> repository,
            IRepository<Location> locationRepository,
            ILogger<SaveInstashopReviewsCommandHandler> logger)
        {
            _repository = repository;
            _locationRepository = locationRepository;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(SaveInstashopReviewsCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogInformation("SaveInstashopReviewsCommand request is null");
                return RequestResult<bool>.Failure(ErrorCode.SaveReviewsFailed, "Request is null");
            }

            if (request.Reviews == null || request.Reviews.Count <= 0)
            {
                _logger.LogInformation("No reviews to save for location {LocationId}", request.LocationId);
                return RequestResult<bool>.Failure(ErrorCode.SaveReviewsFailed, "No reviews provided");
            }

            var reviewsToAdd = new List<Review>();

            foreach (var item in request.Reviews)
            {
                try
                {
                    // Validate required fields
                    if (item == null)
                    {
                        _logger.LogInformation("Null review item encountered, skipping");
                        continue;
                    }

                    // Validate ProductAccuracy and DeliverySpeed are within valid range (1-5) before calculation
                    if (item.ProductAccuracy < 1 || item.ProductAccuracy > 5 || 
                        item.DeliverySpeed < 1 || item.DeliverySpeed > 5)
                    {
                        _logger.LogInformation("Invalid rating values (ProductAccuracy: {ProductAccuracy}, DeliverySpeed: {DeliverySpeed}) for review, skipping", 
                            item.ProductAccuracy, item.DeliverySpeed);
                        continue;
                    }

                    // Calculate rating from ProductAccuracy and DeliverySpeed (average)
                    var rating = (int)Math.Round((item.ProductAccuracy + item.DeliverySpeed) / 2.0);
                    rating = Math.Max(1, Math.Min(5, rating)); // Ensure rating is within 1-5 range

                    var sentiment = DetermineSentiment(item.Comment, rating);
                    var publishedAt = ParseInstashopDate(item?.CreatedAt);

                    var review = new Review
                    {
                        LocationID = request.LocationId,
                        LocationName = request.LocationName ?? string.Empty,
                        Feedback = item.Comment ?? string.Empty,
                        Rate = rating,
                        ReviewerName = string.Empty, 
                        PublishedAt = publishedAt,
                        ReviewDate = DateOnly.FromDateTime(publishedAt),
                        ScrapedAt = DateTime.UtcNow,
                        CompanyID = request.companyID,
                        Sentiment = sentiment,
                        Source = request.Source,
                        CreatedBy = 0
                    };

                    reviewsToAdd.Add(review);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Failed to process Instashop review with date '{Date}' for location {LocationName}. Skipping this review.",
                        item?.CreatedAt ?? "null", request.LocationName);
                    continue;
                }
            }

            if (reviewsToAdd.Count > 0)
            {
                _repository.AddRange(reviewsToAdd);
                _logger.LogInformation("Successfully prepared {Count} Instashop reviews for location {LocationId}",
                    reviewsToAdd.Count, request.LocationId);
                
                // Update InstashopAverageRating on Location entity
                await UpdateLocationAverageRating(request.LocationId);
                
                return RequestResult<bool>.Success(true);
            }

            return RequestResult<bool>.Failure(ErrorCode.SaveReviewsFailed, "No valid reviews to save");
        }

        private static string DetermineSentiment(string comment, int rate)
        {
            // Based on the guide: if rating ≤ 3: Negative, if > 3: Positive
            if (rate <= NEGATIVE_SENTIMENT_THRESHOLD)
            {
                return SENTIMENT_NEGATIVE;
            }
            return SENTIMENT_POSITIVE;
        }

        private DateTime ParseInstashopDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                _logger.LogInformation("Empty date string provided, using current date");
                return DateTime.UtcNow;
            }

            try
            {
                DateTime parsedDate = default;
                bool parsed = false;

                // Try the expected format first: "10/2/2025, 3:10:28 PM"
                foreach (var format in DateFormats)
                {
                    if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        parsedDate = result;
                        parsed = true;
                        break;
                    }
                }

                if (!parsed)
                {
                    // Try general parsing as last resort
                    if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2))
                    {
                        parsedDate = result2;
                        parsed = true;
                    }
                }

                if (!parsed)
                {
                    _logger.LogInformation("Could not parse date string '{DateString}', using current date", dateString);
                    return DateTime.UtcNow;
                }

                // Convert parsed date to UTC
                // Instashop dates are typically in local timezone (EET - UTC+2), so we assume Local and convert to UTC
                if (parsedDate.Kind == DateTimeKind.Unspecified)
                {
                    // Assume the date is in local timezone (EET) and convert to UTC
                    parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);
                    parsedDate = parsedDate.ToUniversalTime();
                }
                else if (parsedDate.Kind == DateTimeKind.Local)
                {
                    parsedDate = parsedDate.ToUniversalTime();
                }
                // If already UTC, no conversion needed

                return parsedDate;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error parsing date string '{DateString}', using current date", dateString);
                return DateTime.UtcNow;
            }
        }

        private async Task UpdateLocationAverageRating(long locationId)
        {
            try
            {
                // Calculate average rating from all Instashop reviews for this location
                var averageRating = await _repository
                    .Get(r => r.LocationID == locationId && r.Source == SourceType.Instashop)
                    .Select(r => (double?)r.Rate)
                    .DefaultIfEmpty()
                    .AverageAsync();

                if (averageRating.HasValue)
                {
                    _locationRepository.SaveIncluded(
                        new Location
                        {
                            ID = locationId,
                            InstashopAverageRating = averageRating.Value
                        },
                        nameof(Location.InstashopAverageRating)
                    );

                    await _locationRepository.SaveChangesAsync();

                    _logger.LogInformation("Updated InstashopAverageRating for LocationID {LocationID} to {Rating}",
                        locationId, averageRating.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating InstashopAverageRating for LocationID {LocationID}", locationId);
                // Don't fail the whole operation if average rating update fails
            }
        }
    }
}
