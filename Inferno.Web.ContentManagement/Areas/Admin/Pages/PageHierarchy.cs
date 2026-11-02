using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// Reads the page tree. Pages point at their parent with <see cref="Page.ParentId"/>. A page whose parent
/// no longer exists is shown at the top level rather than disappearing.
/// </summary>
internal static class PageHierarchy
{
    /// <summary>The pages directly under <paramref name="parentId"/> (the top level when null), in display order.</summary>
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

    /// <summary>Whether <paramref name="parentId"/> may become the parent of <paramref name="pageId"/>: it must exist, and must not be the page itself or one of its sub-pages.</summary>
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

    /// <summary>Every page, parents before their sub-pages, with how deeply each is nested.</summary>
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
