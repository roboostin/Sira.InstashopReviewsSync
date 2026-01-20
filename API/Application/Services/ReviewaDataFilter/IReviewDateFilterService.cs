namespace API.Application.Services.ReviewaDataFilter
{
    public interface IReviewDateFilterService
    {
        bool IsWithinLast24HoursElmenus(string? dateString);

        bool IsWithinLast24HoursMrsool(string relativeTimeText);

        bool IsWithinLast24HoursInstashop(string dateText);
    }
}
