using API.Domain.Entities;

namespace API.Shared.Models
{
    public class PagingViewModel<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Pages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagingViewModel(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

      
    }
} 