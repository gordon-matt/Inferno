using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// Picks the version a public request should render: published for the current culture,
/// then the last archived version for that culture, then the same two steps for the invariant culture.
/// </summary>
public static class PageVersionSelector
{
    /// <summary>
    /// Picks the version to render from <paramref name="versions"/>.
    /// </summary>
    /// <param name="versions">The versions that share a slug. <see langword="null"/> is treated as none.</param>
    /// <param name="cultureCode">The culture to try first. Compared exactly, so <see langword="null"/> matches versions whose culture is <see langword="null"/>.</param>
    /// <returns>
    /// The newest published version for the culture, then its newest archived version,
    /// then the same two steps for a <see langword="null"/> culture. <see langword="null"/> when nothing matches.
    /// </returns>
    public static PageVersion Select(IEnumerable<PageVersion> versions, string cultureCode)
    {
        var matches = versions?.ToList() ?? [];
        if (matches.Count == 0)
        {
            return null;
        }

        return Pick(matches, cultureCode, VersionStatus.Published)
            ?? Pick(matches, cultureCode, VersionStatus.Archived)
            ?? Pick(matches, null, VersionStatus.Published)
            ?? Pick(matches, null, VersionStatus.Archived);
    }

    private static PageVersion Pick(IEnumerable<PageVersion> versions, string cultureCode, VersionStatus status) =>
        versions
            .Where(x => x.CultureCode == cultureCode && x.Status == status)
            .OrderByDescending(x => x.DateModifiedUtc)
            .FirstOrDefault();
}
