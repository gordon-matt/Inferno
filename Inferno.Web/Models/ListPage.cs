namespace Inferno.Web.Models;

/// <summary>One page of an admin list and the size of the whole filtered list, for the pager.</summary>
/// <typeparam name="T">The row type.</typeparam>
/// <param name="Items">The rows on this page.</param>
/// <param name="TotalCount">How many rows match the filter, including rows on other pages.</param>
public sealed record ListPage<T>(IReadOnlyList<T> Items, int TotalCount)
{
    /// <summary>A page with no rows and a total of zero.</summary>
    public static ListPage<T> Empty { get; } = new([], 0);
}

/// <summary>Search text, ordering and paging for an admin list. <see cref="PageNumber"/> starts at 1.</summary>
/// <param name="Search">Text to filter on. <see langword="null"/> means no search.</param>
/// <param name="SortField">The field name to sort by. <see langword="null"/> leaves the order to the caller.</param>
/// <param name="SortDescending"><see langword="true"/> to sort high to low.</param>
/// <param name="PageNumber">The page to return. The first page is 1.</param>
/// <param name="PageSize">How many rows to return.</param>
public sealed record ListQuery(string Search = null, string SortField = null, bool SortDescending = false, int PageNumber = 1, int PageSize = 24)
{
    /// <summary>How many rows to skip to reach <see cref="PageNumber"/>.</summary>
    public int Skip => Math.Max(0, PageNumber - 1) * PageSize;
}
