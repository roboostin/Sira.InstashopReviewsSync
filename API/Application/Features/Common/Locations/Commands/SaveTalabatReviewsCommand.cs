using API.Domain.Entities.Client;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using System.Globalization;

namespace API.Application.Features.Common.Locations.Commands
{
    public record SaveTalabatReviewsCommand(
        long LocationId,
        List<DTOs.Review> Reviews,
        DateTime TimeStamp,
        string LocationName,
        int MaxReviews,
        long companyID) : IRequest<RequestResult<bool>>;

    public class SaveTalabatReviewsCommandHandler : IRequestHandler<SaveTalabatReviewsCommand, RequestResult<bool>>
    {
        private const int NEGATIVE_SENTIMENT_THRESHOLD = 3;
        private const string SENTIMENT_NEGATIVE = "Negative";
        private const string SENTIMENT_POSITIVE = "Positive";

        // Static date formats array for performance
        private static readonly string[] DateFormats = {
            "dd MMMM yyyy",      // "28 October 2025"
            "d MMMM yyyy",       // "8 October 2025"
            "dd MMM yyyy",       // "28 Oct 2025"
            "d MMM yyyy",        // "8 Oct 2025"
            "yyyy-MM-dd",        // "2025-10-28"
            "MM/dd/yyyy",        // "10/28/2025"
            "dd/MM/yyyy"         // "28/10/2025"
        };

        private readonly IRepository<Review> _repository;
        private readonly ILogger<SaveTalabatReviewsCommandHandler> _logger;

        public SaveTalabatReviewsCommandHandler(
            IRepository<Review> repository,
            ILogger<SaveTalabatReviewsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(SaveTalabatReviewsCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogInformation("SaveTalabatReviewsCommand request is null");
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

                    // Validate rate is within valid range (1-5 typically)
                    if (item.Rate < 1 || item.Rate > 5)
                    {
                        _logger.LogInformation("Invalid rate {Rate} for review, skipping", item.Rate);
                        continue;
                    }

                    var sentiment = DetermineSentiment(item.Feedback, item.Rate);
                    var publishedAt = ParseTalabatDate(item?.Date);

                    var review = new Review
                    {
                        LocationID = request.LocationId,
                        LocationName = request.LocationName ?? string.Empty,
                        Feedback = item.Feedback ?? string.Empty,
                        Rate = item.Rate,
                        ReviewerName = item.FirstName ?? string.Empty,
                        PublishedAt = publishedAt,
                        ReviewDate = DateOnly.FromDateTime(publishedAt),
                        ScrapedAt = DateTime.UtcNow,
                        CompanyID = request.companyID,
                        Sentiment = sentiment,
                        CreatedBy = 0
                    };

                    reviewsToAdd.Add(review);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Failed to process Talabat review with date '{Date}' for location {LocationName}. Skipping this review.",
                        item?.Date ?? "null", request.LocationName);
                    continue;
                }
            }

            if (reviewsToAdd.Count > 0)
            {
                _repository.AddRange(reviewsToAdd);
                _logger.LogInformation("Successfully prepared {Count} Talabat reviews for location {LocationId}",
                    reviewsToAdd.Count, request.LocationId);
                return RequestResult<bool>.Success(true);
            }

            return RequestResult<bool>.Failure(ErrorCode.SaveReviewsFailed, "No valid reviews to save");
        }

        private static string DetermineSentiment(string feedback, int rate)
        {
            if (string.IsNullOrEmpty(feedback))
            {
                return rate <= NEGATIVE_SENTIMENT_THRESHOLD ? SENTIMENT_NEGATIVE : SENTIMENT_POSITIVE;
            }
            return string.Empty;
        }

        private DateTime ParseTalabatDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                _logger.LogInformation("Empty date string provided, using current date");
                return DateTime.UtcNow;
            }

            try
            {
                // Try the expected format first: "28 October 2025"
                if (DateTime.TryParseExact(dateString, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }

                // Try alternative formats
                foreach (var format in DateFormats)
                {
                    if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    {
                        return result;
                    }
                }

                // Try general parsing as last resort
                if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }

                _logger.LogInformation("Could not parse date string '{DateString}', using current date", dateString);
                return DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error parsing date string '{DateString}', using current date", dateString);
                return DateTime.UtcNow;
            }
        }
    }
}
