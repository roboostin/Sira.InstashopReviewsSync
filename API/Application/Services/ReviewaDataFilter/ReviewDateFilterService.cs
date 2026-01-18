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

        public bool IsWithinLast24HoursTalabat(string dateText)
        {
            if (string.IsNullOrEmpty(dateText))
                return false;

            try
            {
                // Based on the example "28 October 2025", try to parse the date
                if (DateTime.TryParse(dateText, out DateTime reviewDate))
                {
                    var now = DateTime.UtcNow;
                    var cutoffTime = now.AddDays(-1); // 24 hours ago from now

                    _logger.LogDebug("Checking Talabat review date: {ReviewDate} against cutoff: {CutoffTime} (Now: {Now})",
                        reviewDate, cutoffTime, now);

                    // If the parsed date has no time component (00:00:00), we assume it could be any time during that day
                    // So we check if the review date is within the last 24 hours OR if it's from today/yesterday
                    if (reviewDate.TimeOfDay == TimeSpan.Zero)
                    {
                        // Date only (no time) - check if it's today or yesterday
                        var yesterday = now.Date.AddDays(-1);
                        var today = now.Date;
                        bool isRecentDate = reviewDate.Date >= yesterday && reviewDate.Date <= today;

                        _logger.LogDebug("Date-only Talabat review: {ReviewDate}, Yesterday: {Yesterday}, Today: {Today}, IsRecent: {IsRecent}",
                            reviewDate.Date, yesterday, today, isRecentDate);

                        return isRecentDate;
                    }
                    else
                    {
                        // Date with time - use precise 24-hour check
                        bool isWithin24Hours = reviewDate >= cutoffTime;

                        _logger.LogDebug("Date-time Talabat review: {ReviewDate}, Cutoff: {CutoffTime}, IsWithin24Hours: {IsWithin24Hours}",
                            reviewDate, cutoffTime, isWithin24Hours);

                        return isWithin24Hours;
                    }
                }

                // Fallback: Check for Arabic time indicators within 24 hours
                bool isHours = HourWords.Any(word => dateText.Contains(word, StringComparison.OrdinalIgnoreCase));
                bool isMinutes = MinuteWords.Any(word => dateText.Contains(word, StringComparison.OrdinalIgnoreCase));
                bool isSeconds = SecondWords.Any(word => dateText.Contains(word, StringComparison.OrdinalIgnoreCase));

                bool isArabicTimeIndicator = isHours || isMinutes || isSeconds;

                _logger.LogDebug("Arabic time indicator check for Talabat '{DateText}': Hours={IsHours}, Minutes={IsMinutes}, Seconds={IsSeconds}, Result={Result}",
                    dateText, isHours, isMinutes, isSeconds, isArabicTimeIndicator);

                return isArabicTimeIndicator;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error parsing Talabat date: {DateText}", dateText);
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
    }
}
