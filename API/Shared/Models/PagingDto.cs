namespace API.Shared.Models;

public record PagingDto<T>(int PageSize, int PageIndex, int Records, int Pages, IEnumerable<T> Items)
{
    public static PagingDto<T> Empty(int pageSize) => new PagingDto<T>(pageSize, 1, 0, 0, []);
}
