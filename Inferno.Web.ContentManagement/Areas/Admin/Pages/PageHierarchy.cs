using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// Reads the page tree. Pages point at their parent with <see cref="Page.ParentId"/>. A page whose parent
/// no longer exists is shown at the top level rather than disappearing.
/// </summary>
internal static class PageHierarchy
{
    /// <summary>Lists the pages directly under <paramref name="parentId"/>, in display order. A <see langword="null"/> parent is the top level, which also includes pages whose parent is missing.</summary>
    /// <param name="pages">The pages to search.</param>
    /// <param name="parentId">The parent, or <see langword="null"/> for the top level.</param>
    /// <returns>The children, ordered by <see cref="Page.Order"/> and then name.</returns>
    public static List<Page> ChildrenOf(IReadOnlyCollection<Page> pages, Guid? parentId)
    {
        var ids = pages.Select(page => page.Id).ToHashSet();

        return pages
            .Where(page => parentId is null
                ? page.ParentId is null || !ids.Contains(page.ParentId.Value)
                : page.ParentId == parentId)
            .OrderBy(page => page.Order)
            .ThenBy(page => page.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>Reports whether <paramref name="parentId"/> may become the parent of <paramref name="pageId"/>. It must exist, and must not be the page itself or one of its sub-pages.</summary>
    /// <param name="pages">The pages to search.</param>
    /// <param name="pageId">The page being moved. <see cref="Guid.Empty"/> is used when the page does not exist yet.</param>
    /// <param name="parentId">The proposed parent. <see langword="null"/> is always allowed.</param>
    /// <returns><see langword="true"/> when the parent is allowed.</returns>
    public static bool CanBeParent(IReadOnlyCollection<Page> pages, Guid pageId, Guid? parentId)
    {
        if (parentId is null)
        {
            return true;
        }

        var byId = pages.ToDictionary(page => page.Id);
        if (!byId.ContainsKey(parentId.Value))
        {
            return false;
        }

        var seen = new HashSet<Guid>();
        Guid? cursor = parentId;
        while (cursor is { } id)
        {
            if (id == pageId || !seen.Add(id))
            {
                return false;
            }

            cursor = byId.TryGetValue(id, out var node) ? node.ParentId : null;
        }

        return true;
    }

    /// <summary>Lists every page, parents before their sub-pages, with how deeply each is nested.</summary>
    /// <param name="pages">The pages to flatten.</param>
    /// <returns>Each page and its depth. A page that would be visited twice is skipped.</returns>
    public static List<(Page Page, int Depth)> Flatten(IReadOnlyCollection<Page> pages)
    {
        var result = new List<(Page, int)>();
        var visited = new HashSet<Guid>();

        void Visit(Guid? parentId, int depth)
        {
            foreach (var page in ChildrenOf(pages, parentId))
            {
                if (!visited.Add(page.Id))
                {
                    continue;
                }

                result.Add((page, depth));
                Visit(page.Id, depth + 1);
            }
        }

        Visit(null, 0);
        return result;
    }
}
