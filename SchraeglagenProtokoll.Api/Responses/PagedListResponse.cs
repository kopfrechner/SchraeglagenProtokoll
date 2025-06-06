using Marten.Pagination;

namespace SchraeglagenProtokoll.Api.Responses;

public class PagedListResponse<T>()
{
    public IReadOnlyList<T> Items { get; init; }

    public long Count { get; init; }
    public long PageNumber { get; init; }
    public long PageSize { get; init; }
    public long PageCount { get; init; }
    public long TotalItemCount { get; init; }
    public bool HasPreviousPage { get; init; }

    public bool HasNextPage { get; init; }
    public bool IsFirstPage { get; init; }
    public bool IsLastPage { get; init; }
    public long FirstItemOnPage { get; init; }
    public long LastItemOnPage { get; init; }
}

public static class PagedListExtensions
{
    public static PagedListResponse<T> ToResponse<T>(this IPagedList<T> pagedList) =>
        new()
        {
            Items = pagedList.ToList(),
            Count = pagedList.Count,
            PageNumber = pagedList.PageNumber,
            PageSize = pagedList.PageSize,
            PageCount = pagedList.PageCount,
            TotalItemCount = pagedList.TotalItemCount,
            HasPreviousPage = pagedList.HasPreviousPage,
            HasNextPage = pagedList.HasNextPage,
            IsFirstPage = pagedList.IsFirstPage,
            IsLastPage = pagedList.IsLastPage,
            FirstItemOnPage = pagedList.FirstItemOnPage,
            LastItemOnPage = pagedList.LastItemOnPage,
        };
}
