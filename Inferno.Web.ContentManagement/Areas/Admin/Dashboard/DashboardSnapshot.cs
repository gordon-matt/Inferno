using System.Globalization;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard;

/// <summary>The rows and permissions <see cref="DashboardSnapshotBuilder"/> turns into a snapshot.</summary>
public sealed class DashboardSnapshotInput
{
    /// <summary>Whether page rows are included. When <see langword="false"/>, page counts stay at zero.</summary>
    public bool CanReadPages { get; init; }

    /// <summary>Whether post rows may be counted. They are also skipped when <see cref="BlogEnabled"/> is <see langword="false"/>.</summary>
    public bool CanReadPosts { get; init; }

    /// <summary>Whether users, roles, and locked-account attention are included.</summary>
    public bool CanReadUsers { get; init; }

    /// <summary>Whether the tenant has the blog switched on.</summary>
    public bool BlogEnabled { get; init; }

    /// <summary>The UTC time used for stale drafts and scheduled posts.</summary>
    public DateTime UtcNow { get; init; }

    /// <summary>The tenant's pages.</summary>
    public IReadOnlyList<DashboardPageRow> Pages { get; init; } = [];

    /// <summary>The tenant's page versions.</summary>
    public IReadOnlyList<DashboardVersionRow> Versions { get; init; } = [];

    /// <summary>The tenant's posts. Counted only when <see cref="CanReadPosts"/> and <see cref="BlogEnabled"/> are both set.</summary>
    public IReadOnlyList<DashboardPostRow> Posts { get; init; } = [];

    /// <summary>The tenant's accounts.</summary>
    public IReadOnlyList<DashboardUserRow> Users { get; init; } = [];

    /// <summary>The tenant's roles and how many accounts are in each.</summary>
    public IReadOnlyList<DashboardRoleRow> Roles { get; init; } = [];
}

/// <summary>A page row for the dashboard.</summary>
/// <param name="Id">The page identifier.</param>
/// <param name="Name">The admin name, used when a version has no title.</param>
/// <param name="IsEnabled">Whether the page is switched on. A published page that is off is counted as needing attention instead of as published.</param>
public sealed record DashboardPageRow(Guid Id, string Name, bool IsEnabled);

/// <summary>A page version row for the dashboard.</summary>
/// <param name="Id">The version identifier. Recent and attention links open this version.</param>
/// <param name="PageId">The page the version belongs to.</param>
/// <param name="Status">Draft, published, or archived.</param>
/// <param name="Title">The version title.</param>
/// <param name="Fields">The stored page fields, read for the meta description.</param>
/// <param name="DateModifiedUtc">When the version was last saved, in UTC.</param>
public sealed record DashboardVersionRow(
    Guid Id,
    Guid PageId,
    VersionStatus Status,
    string Title,
    string Fields,
    DateTime DateModifiedUtc);

/// <summary>A blog post row for the dashboard.</summary>
/// <param name="Id">The post identifier.</param>
/// <param name="Title">The post title.</param>
/// <param name="IsDraft">Whether the post is still a draft.</param>
/// <param name="PublishedAt">The publish time. A future time counts as scheduled.</param>
/// <param name="UpdatedAt">When the post was last updated, in UTC.</param>
/// <param name="AuthorName">The author shown on the recent list.</param>
/// <param name="MetaTitle">The SEO title. Blank published posts are marked as needing attention.</param>
/// <param name="MetaDescription">The meta description. Blank published posts are marked as needing attention.</param>
/// <param name="CoverImageUrl">The cover image. When set, a blank alt text is marked as needing attention.</param>
/// <param name="CoverImageAlt">The cover image's alt text.</param>
public sealed record DashboardPostRow(
    int Id,
    string Title,
    bool IsDraft,
    DateTime? PublishedAt,
    DateTime UpdatedAt,
    string AuthorName,
    string MetaTitle,
    string MetaDescription,
    string CoverImageUrl,
    string CoverImageAlt);

/// <summary>An account row for the dashboard.</summary>
/// <param name="Name">The display name. Blank falls back to <paramref name="Email"/>.</param>
/// <param name="Email">The email address.</param>
/// <param name="Locked">Whether the account is locked. Locked accounts are marked as needing attention.</param>
public sealed record DashboardUserRow(string Name, string Email, bool Locked);

/// <summary>A role and how many accounts are in it.</summary>
/// <param name="Name">The role name.</param>
/// <param name="Count">How many accounts are in the role.</param>
public sealed record DashboardRoleRow(string Name, int Count);

/// <summary>Something the dashboard asks an editor to look at.</summary>
/// <param name="Title">The page, post, or account name.</param>
/// <param name="Reason">Why it needs attention.</param>
/// <param name="Href">Where the item opens.</param>
public sealed record AttentionItem(string Title, string Reason, string Href);

/// <summary>A recently edited page or post.</summary>
/// <param name="Title">The title.</param>
/// <param name="Kind">Page or Post.</param>
/// <param name="Href">Where the item opens.</param>
/// <param name="WhenUtc">When it was last saved, in UTC.</param>
/// <param name="By">The author for a post. <see langword="null"/> for a page.</param>
public sealed record RecentItem(string Title, string Kind, string Href, DateTime WhenUtc, string By);

/// <summary>A post that will be published later.</summary>
/// <param name="Title">The post title.</param>
/// <param name="Href">Where the post opens.</param>
/// <param name="PublishAtUtc">The publish time, in UTC.</param>
public sealed record ScheduledItem(string Title, string Href, DateTime PublishAtUtc);

/// <summary>Edits in one Monday-start week.</summary>
/// <param name="WeekStart">The Monday, as a date.</param>
/// <param name="Pages">How many pages were last saved in the week.</param>
/// <param name="Posts">How many posts were last updated in the week.</param>
public sealed record WeekActivity(DateTime WeekStart, int Pages, int Posts);

/// <summary>An account on the accounts widget.</summary>
/// <param name="Name">The display name, or the email when the name is blank.</param>
/// <param name="Email">The email address.</param>
/// <param name="Locked">Whether the account is locked.</param>
public sealed record AccountItem(string Name, string Email, bool Locked);

/// <summary>The numbers and lists the dashboard widgets show.</summary>
public sealed class DashboardSnapshot
{
    /// <summary>Why the snapshot could not be loaded. <see langword="null"/> when it was built.</summary>
    public string Error { get; init; }

    /// <summary>Whether page counts were included.</summary>
    public bool CanReadPages { get; init; }

    /// <summary>Whether the caller may read posts. Post counts also require <see cref="BlogEnabled"/>.</summary>
    public bool CanReadPosts { get; init; }

    /// <summary>Whether accounts and roles were included.</summary>
    public bool CanReadUsers { get; init; }

    /// <summary>Whether the tenant has the blog switched on.</summary>
    public bool BlogEnabled { get; init; }

    /// <summary>Enabled pages that have a published version.</summary>
    public int PublishedPages { get; init; }

    /// <summary>Pages that have a draft and no published version.</summary>
    public int DraftPages { get; init; }

    /// <summary>Pages that have versions, all of them archived.</summary>
    public int ArchivedPages { get; init; }

    /// <summary>Posts that are published and not scheduled for later.</summary>
    public int PublishedPosts { get; init; }

    /// <summary>Posts that are still drafts.</summary>
    public int DraftPosts { get; init; }

    /// <summary>Posts whose publish time is still in the future.</summary>
    public int ScheduledPosts { get; init; }

    /// <summary>How many accounts were loaded. Zero when the caller cannot read accounts.</summary>
    public int Users { get; init; }

    /// <summary>How many attention items were found, including ones past <see cref="DashboardSnapshotBuilder.ListLimit"/>.</summary>
    public int AttentionCount { get; init; }

    /// <summary>The last <see cref="DashboardSnapshotBuilder.ActivityWeeks"/> weeks, oldest first, starting on Monday.</summary>
    public IReadOnlyList<WeekActivity> Activity { get; init; } = [];

    /// <summary>The most recently edited pages and posts, newest first, limited to <see cref="DashboardSnapshotBuilder.ListLimit"/>.</summary>
    public IReadOnlyList<RecentItem> Recent { get; init; } = [];

    /// <summary>Scheduled posts, soonest first, limited to <see cref="DashboardSnapshotBuilder.ListLimit"/>.</summary>
    public IReadOnlyList<ScheduledItem> Scheduled { get; init; } = [];

    /// <summary>Attention items, most urgent first, limited to <see cref="DashboardSnapshotBuilder.ListLimit"/>.</summary>
    public IReadOnlyList<AttentionItem> Attention { get; init; } = [];

    /// <summary>Roles ordered by count, then name. Empty when the caller cannot read accounts.</summary>
    public IReadOnlyList<DashboardRoleRow> Roles { get; init; } = [];

    /// <summary>Accounts ordered by name, limited to <see cref="DashboardSnapshotBuilder.ListLimit"/>. Empty when the caller cannot read accounts.</summary>
    public IReadOnlyList<AccountItem> Accounts { get; init; } = [];

    /// <summary>Creates a snapshot that failed to load.</summary>
    /// <param name="error">The message to show.</param>
    /// <returns>A snapshot whose <see cref="Error"/> is set and whose lists are empty.</returns>
    public static DashboardSnapshot Failed(string error) => new() { Error = error };
}

/// <summary>Turns the rows the dashboard loaded into the numbers and lists the widgets show.</summary>
public static class DashboardSnapshotBuilder
{
    /// <summary>A draft older than this many days is marked as needing attention.</summary>
    public const int StaleDraftDays = 30;

    /// <summary>How many Monday-start weeks <see cref="Build"/> includes.</summary>
    public const int ActivityWeeks = 8;

    /// <summary>How many recent, scheduled, attention, and account rows <see cref="Build"/> keeps.</summary>
    public const int ListLimit = 8;

    /// <summary>Counts pages and posts and builds the lists the widgets show.</summary>
    /// <param name="input">The loaded rows and the caller's permissions.</param>
    /// <returns>The snapshot. Page and post rows the caller cannot read are ignored.</returns>
    public static DashboardSnapshot Build(DashboardSnapshotInput input)
    {
        var now = input.UtcNow;
        var attention = new List<(int Order, AttentionItem Item)>();
        var recent = new List<RecentItem>();
        var pageEdits = new List<DateTime>();

        int publishedPages = 0;
        int draftPages = 0;
        int archivedPages = 0;

        if (input.CanReadPages)
        {
            var versionsByPage = input.Versions
                .GroupBy(version => version.PageId)
                .ToDictionary(group => group.Key, group => (IReadOnlyList<DashboardVersionRow>)[.. group]);

            foreach (var page in input.Pages)
            {
                versionsByPage.TryGetValue(page.Id, out var versions);
                ReadPage(page, versions ?? [], now, ref publishedPages, ref draftPages, ref archivedPages, attention, recent, pageEdits);
            }
        }

        int publishedPosts = 0;
        int draftPosts = 0;
        int scheduledPosts = 0;
        var scheduled = new List<ScheduledItem>();
        var postEdits = new List<DateTime>();

        if (input.CanReadPosts && input.BlogEnabled)
        {
            foreach (var post in input.Posts)
            {
                ReadPost(post, now, ref publishedPosts, ref draftPosts, ref scheduledPosts, attention, recent, scheduled, postEdits);
            }
        }

        if (input.CanReadUsers)
        {
            foreach (var user in input.Users.Where(user => user.Locked))
            {
                attention.Add((5, new AttentionItem(Display(user), "Account is locked", DashboardLinks.Membership)));
            }
        }

        var ordered = attention
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Item.Title, StringComparer.OrdinalIgnoreCase)
            .Select(item => item.Item)
            .ToList();

        return new DashboardSnapshot
        {
            CanReadPages = input.CanReadPages,
            CanReadPosts = input.CanReadPosts,
            CanReadUsers = input.CanReadUsers,
            BlogEnabled = input.BlogEnabled,
            PublishedPages = publishedPages,
            DraftPages = draftPages,
            ArchivedPages = archivedPages,
            PublishedPosts = publishedPosts,
            DraftPosts = draftPosts,
            ScheduledPosts = scheduledPosts,
            Users = input.CanReadUsers ? input.Users.Count : 0,
            AttentionCount = ordered.Count,
            Attention = [.. ordered.Take(ListLimit)],
            Recent = [.. recent.OrderByDescending(item => item.WhenUtc).Take(ListLimit)],
            Scheduled = [.. scheduled.OrderBy(item => item.PublishAtUtc).Take(ListLimit)],
            Activity = Weeks(pageEdits, postEdits, now),
            Roles = input.CanReadUsers
                ? [.. input.Roles.OrderByDescending(role => role.Count).ThenBy(role => role.Name, StringComparer.OrdinalIgnoreCase)]
                : [],
            Accounts = input.CanReadUsers
                ? [.. input.Users
                    .OrderBy(user => Display(user), StringComparer.OrdinalIgnoreCase)
                    .Take(ListLimit)
                    .Select(user => new AccountItem(Display(user), user.Email, user.Locked))]
                : [],
        };
    }

    private static void ReadPage(
        DashboardPageRow page,
        IReadOnlyList<DashboardVersionRow> versions,
        DateTime now,
        ref int publishedPages,
        ref int draftPages,
        ref int archivedPages,
        List<(int Order, AttentionItem Item)> attention,
        List<RecentItem> recent,
        List<DateTime> pageEdits)
    {
        var published = Latest(versions, VersionStatus.Published);
        var draft = Latest(versions, VersionStatus.Draft);
        var latest = versions
            .Where(version => version.Status != VersionStatus.Archived)
            .OrderByDescending(version => version.DateModifiedUtc)
            .FirstOrDefault();

        if (latest is not null)
        {
            pageEdits.Add(latest.DateModifiedUtc);
            recent.Add(new RecentItem(
                TitleOf(latest, page.Name),
                "Page",
                DashboardLinks.Page(latest.Id),
                latest.DateModifiedUtc,
                null));
        }

        if (published is not null)
        {
            string href = DashboardLinks.Page(published.Id);
            string title = TitleOf(published, page.Name);
            if (page.IsEnabled)
            {
                publishedPages++;
            }
            else
            {
                attention.Add((4, new AttentionItem(title, "Switched off", href)));
            }

            if (string.IsNullOrWhiteSpace(StandardPageContent.Parse(published.Fields).MetaDescription))
            {
                attention.Add((1, new AttentionItem(title, "Missing meta description", href)));
            }

            if (draft is not null && draft.DateModifiedUtc > published.DateModifiedUtc)
            {
                attention.Add((0, new AttentionItem(title, "Has unpublished changes", DashboardLinks.Page(draft.Id))));
            }

            return;
        }

        if (draft is not null)
        {
            draftPages++;
            if (draft.DateModifiedUtc < now.AddDays(-StaleDraftDays))
            {
                int days = (int)(now - draft.DateModifiedUtc).TotalDays;
                attention.Add((0, new AttentionItem(TitleOf(draft, page.Name), $"Draft not updated for {days} days", DashboardLinks.Page(draft.Id))));
            }

            return;
        }

        if (versions.Count > 0)
        {
            archivedPages++;
        }
    }

    private static void ReadPost(
        DashboardPostRow post,
        DateTime now,
        ref int publishedPosts,
        ref int draftPosts,
        ref int scheduledPosts,
        List<(int Order, AttentionItem Item)> attention,
        List<RecentItem> recent,
        List<ScheduledItem> scheduled,
        List<DateTime> postEdits)
    {
        string href = DashboardLinks.Post(post.Id);
        postEdits.Add(post.UpdatedAt);
        recent.Add(new RecentItem(post.Title, "Post", href, post.UpdatedAt, post.AuthorName));

        if (post.IsDraft)
        {
            draftPosts++;
            if (post.UpdatedAt < now.AddDays(-StaleDraftDays))
            {
                int days = (int)(now - post.UpdatedAt).TotalDays;
                attention.Add((0, new AttentionItem(post.Title, $"Draft not updated for {days} days", href)));
            }

            return;
        }

        if (post.PublishedAt is DateTime publishAt && publishAt > now)
        {
            scheduledPosts++;
            scheduled.Add(new ScheduledItem(post.Title, href, publishAt));
        }
        else
        {
            publishedPosts++;
        }

        if (string.IsNullOrWhiteSpace(post.MetaDescription))
        {
            attention.Add((1, new AttentionItem(post.Title, "Missing meta description", href)));
        }

        if (string.IsNullOrWhiteSpace(post.MetaTitle))
        {
            attention.Add((2, new AttentionItem(post.Title, "Missing SEO title", href)));
        }

        if (!string.IsNullOrWhiteSpace(post.CoverImageUrl) && string.IsNullOrWhiteSpace(post.CoverImageAlt))
        {
            attention.Add((3, new AttentionItem(post.Title, "Cover image has no alt text", href)));
        }
    }

    private static IReadOnlyList<WeekActivity> Weeks(IReadOnlyList<DateTime> pageEdits, IReadOnlyList<DateTime> postEdits, DateTime now)
    {
        var thisWeek = StartOfWeek(now);
        var weeks = new List<WeekActivity>(ActivityWeeks);
        for (int ago = ActivityWeeks - 1; ago >= 0; ago--)
        {
            var start = thisWeek.AddDays(-7 * ago);
            var end = start.AddDays(7);
            weeks.Add(new WeekActivity(
                start,
                pageEdits.Count(edit => edit >= start && edit < end),
                postEdits.Count(edit => edit >= start && edit < end)));
        }

        return weeks;
    }

    private static DateTime StartOfWeek(DateTime utcNow)
    {
        var date = utcNow.Date;
        int delta = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-delta);
    }

    private static DashboardVersionRow Latest(IReadOnlyList<DashboardVersionRow> versions, VersionStatus status) =>
        versions
            .Where(version => version.Status == status)
            .OrderByDescending(version => version.DateModifiedUtc)
            .FirstOrDefault();

    private static string TitleOf(DashboardVersionRow version, string pageName) =>
        string.IsNullOrWhiteSpace(version.Title) ? pageName : version.Title;

    private static string Display(DashboardUserRow user) =>
        string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name;
}

internal static class DashboardLinks
{
    public const string Pages = "/admin/pages/index";

    public const string Posts = "/admin/blog/posts";

    public const string Membership = "/admin/membership";

    public const string Settings = "/admin/configuration/settings/index";

    public const string Blocks = "/admin/blocks/content-blocks";

    public static string Page(Guid versionId) => $"/admin/pages/design/{versionId.ToString("D", CultureInfo.InvariantCulture)}";

    public static string Post(int id) => $"/admin/blog/posts/{id.ToString(CultureInfo.InvariantCulture)}";
}
