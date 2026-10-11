using System.Collections.Generic;

namespace PMS.Application.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; init; } = new List<T>();
        public long TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
