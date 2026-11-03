namespace Inferno.Web.Models;

/// <summary>One page of an admin list and the size of the whole filtered list, for the pager.</summary>
public sealed record ListPage<T>(IReadOnlyList<T> Items, int TotalCount)
{
    public static ListPage<T> Empty { get; } = new([], 0);
}

/// <summary>Search text, ordering and paging for an admin list. <see cref="PageNumber"/> starts at 1.</summary>
public sealed record ListQuery(string Search = null, string SortField = null, bool SortDescending = false, int PageNumber = 1, int PageSize = 24)
{
    public int Skip => Math.Max(0, PageNumber - 1) * PageSize;
}
