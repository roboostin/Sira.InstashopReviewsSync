namespace API.Application.Services.ReviewaDataFilter
{
    public class ReviewDateFilterService : IReviewDateFilterService
    {
        private readonly ILogger<ReviewDateFilterService> _logger;

        // Static arrays for Arabic time words
        private static readonly string[] HourWords = { "ساعة", "ساعات", "ساعه", "ساعتين" };
        private static readonly string[] MinuteWords = { "دقيقة", "دقائق", "دقيقه", "دقايق" };
        private static readonly string[] SecondWords = { "ثانية", "ثواني", "ثانيه", "ثوانى" };
        private static readonly string[] DayWords = { "يوم", "أيام", "ايام", "يومين" };

        public ReviewDateFilterService(ILogger<ReviewDateFilterService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsWithinLast24HoursElmenus(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return false;

            try
            {
                // Elmenus uses ISO 8601 format: "2025-11-26T14:19:05.000Z"
                if (DateTime.TryParse(dateString, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime reviewDate))
                {
                    // Ensure UTC if not already
                    if (reviewDate.Kind == DateTimeKind.Unspecified)
                    {
                        reviewDate = DateTime.SpecifyKind(reviewDate, DateTimeKind.Utc);
                    }
                    else if (reviewDate.Kind == DateTimeKind.Local)
                    {
                        reviewDate = reviewDate.ToUniversalTime();
                    }

                    var now = DateTime.UtcNow;
                    var cutoffTime = now.AddDays(-1); // 24 hours ago from now

                    _logger.LogDebug("Checking Elmenus review date: {ReviewDate} against cutoff: {CutoffTime} (Now: {Now})",
                        reviewDate, cutoffTime, now);

                    // Check if the review date is within the last 24 hours
                    bool isWithin24Hours = reviewDate >= cutoffTime;

                    _logger.LogDebug("Elmenus review date: {ReviewDate}, Cutoff: {CutoffTime}, IsWithin24Hours: {IsWithin24Hours}",
                        reviewDate, cutoffTime, isWithin24Hours);

                    return isWithin24Hours;
                }

                // If we can't parse the date, exclude the review
                _logger.LogInformation("Could not parse Elmenus date string: {DateText}", dateString);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error parsing Elmenus date: {DateText}", dateString);
                return false;
            }
        }

        public bool IsWithinLast24HoursMrsool(string relativeTimeText)
        {
            if (string.IsNullOrEmpty(relativeTimeText))
                return false;

            // Check for hours, minutes, seconds, or days (all within 24 hours)
            bool isHours = HourWords.Any(word => relativeTimeText.Contains(word, StringComparison.OrdinalIgnoreCase));
            bool isMinutes = MinuteWords.Any(word => relativeTimeText.Contains(word, StringComparison.OrdinalIgnoreCase));
            bool isSeconds = SecondWords.Any(word => relativeTimeText.Contains(word, StringComparison.OrdinalIgnoreCase));
            bool isDays = DayWords.Any(word => relativeTimeText.Contains(word, StringComparison.OrdinalIgnoreCase));

            return isHours || isMinutes || isSeconds || isDays;
        }

        public bool IsWithinLast24HoursInstashop(string dateText)
        {
            if (string.IsNullOrEmpty(dateText))
                return false;

            try
            {
                // Instashop format: "10/2/2025, 3:10:28 PM"
                var dateFormats = new[]
                {
                    "M/d/yyyy, h:mm:ss tt",
                    "MM/dd/yyyy, h:mm:ss tt",
                    "M/d/yyyy, hh:mm:ss tt",
                    "MM/dd/yyyy, hh:mm:ss tt",
                    "M/d/yyyy, H:mm:ss",
                    "MM/dd/yyyy, H:mm:ss"
                };

                DateTime reviewDate = default;
                bool parsed = false;

                foreach (var format in dateFormats)
                {
                    if (DateTime.TryParseExact(dateText, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        reviewDate = parsedDate;
                        parsed = true;
                        break;
                    }
                }

                if (!parsed)
                {
                    // Try general parsing as fallback
                    if (!DateTime.TryParse(dateText, out DateTime parsedDate))
                    {
                        _logger.LogInformation("Could not parse Instashop date string: {DateText}", dateText);
                        return false;
                    }
                    reviewDate = parsedDate;
                }

                // Convert parsed date to UTC
                // Instashop dates are typically in local timezone (EET - UTC+2), so we assume Local and convert to UTC
                if (reviewDate.Kind == DateTimeKind.Unspecified)
                {
                    // Assume the date is in local timezone (EET) and convert to UTC
                    // If the server is in a different timezone, adjust accordingly
                    reviewDate = DateTime.SpecifyKind(reviewDate, DateTimeKind.Local);
                    reviewDate = reviewDate.ToUniversalTime();
                }
                else if (reviewDate.Kind == DateTimeKind.Local)
                {
                    reviewDate = reviewDate.ToUniversalTime();
                }
                // If already UTC, no conversion needed

                var now = DateTime.UtcNow;
                var cutoffTime = now.AddDays(-1); // 24 hours ago from now

                _logger.LogDebug("Checking Instashop review date: {ReviewDate} (UTC) against cutoff: {CutoffTime} (Now: {Now})",
                    reviewDate, cutoffTime, now);

                // Check if the review date is within the last 24 hours
                bool isWithin24Hours = reviewDate >= cutoffTime;

                _logger.LogDebug("Instashop review date: {ReviewDate}, Cutoff: {CutoffTime}, IsWithin24Hours: {IsWithin24Hours}",
                    reviewDate, cutoffTime, isWithin24Hours);

                return isWithin24Hours;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error parsing Instashop date: {DateText}", dateText);
                return false;
            }
        }
    }
}
