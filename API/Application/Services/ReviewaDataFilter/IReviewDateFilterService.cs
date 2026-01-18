namespace API.Application.Services.ReviewaDataFilter
{
    public interface IReviewDateFilterService
    {
        bool IsWithinLast24HoursElmenus(string? dateString);

        bool IsWithinLast24HoursTalabat(string dateText);

        bool IsWithinLast24HoursMrsool(string relativeTimeText);
    }
}
