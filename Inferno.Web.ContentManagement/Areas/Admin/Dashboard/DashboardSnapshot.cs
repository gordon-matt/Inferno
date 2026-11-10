using System.Globalization;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard;

public sealed class DashboardSnapshotInput
{
    public bool CanReadPages { get; init; }

    public bool CanReadPosts { get; init; }

    public bool CanReadUsers { get; init; }

    public bool BlogEnabled { get; init; }

    public DateTime UtcNow { get; init; }

    public IReadOnlyList<DashboardPageRow> Pages { get; init; } = [];

    public IReadOnlyList<DashboardVersionRow> Versions { get; init; } = [];

    public IReadOnlyList<DashboardPostRow> Posts { get; init; } = [];

    public IReadOnlyList<DashboardUserRow> Users { get; init; } = [];

    public IReadOnlyList<DashboardRoleRow> Roles { get; init; } = [];
}

public sealed record DashboardPageRow(Guid Id, string Name, bool IsEnabled);

public sealed record DashboardVersionRow(
    Guid Id,
    Guid PageId,
    VersionStatus Status,
    string Title,
    string Fields,
    DateTime DateModifiedUtc);

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

public sealed record DashboardUserRow(string Name, string Email, bool Locked);

public sealed record DashboardRoleRow(string Name, int Count);

public sealed record AttentionItem(string Title, string Reason, string Href);

public sealed record RecentItem(string Title, string Kind, string Href, DateTime WhenUtc, string By);

public sealed record ScheduledItem(string Title, string Href, DateTime PublishAtUtc);

public sealed record WeekActivity(DateTime WeekStart, int Pages, int Posts);

public sealed record AccountItem(string Name, string Email, bool Locked);

public sealed class DashboardSnapshot
{
    public string Error { get; init; }

    public bool CanReadPages { get; init; }

    public bool CanReadPosts { get; init; }

    public bool CanReadUsers { get; init; }

    public bool BlogEnabled { get; init; }

    public int PublishedPages { get; init; }

    public int DraftPages { get; init; }

    public int ArchivedPages { get; init; }

    public int PublishedPosts { get; init; }

    public int DraftPosts { get; init; }

    public int ScheduledPosts { get; init; }

    public int Users { get; init; }

    public int AttentionCount { get; init; }

    public IReadOnlyList<WeekActivity> Activity { get; init; } = [];

    public IReadOnlyList<RecentItem> Recent { get; init; } = [];

    public IReadOnlyList<ScheduledItem> Scheduled { get; init; } = [];

    public IReadOnlyList<AttentionItem> Attention { get; init; } = [];

    public IReadOnlyList<DashboardRoleRow> Roles { get; init; } = [];

    public IReadOnlyList<AccountItem> Accounts { get; init; } = [];

    public static DashboardSnapshot Failed(string error) => new() { Error = error };
}

/// <summary>Turns the rows the dashboard loaded into the numbers and lists the widgets show.</summary>
public static class DashboardSnapshotBuilder
{
    public const int StaleDraftDays = 30;

    public const int ActivityWeeks = 8;

    public const int ListLimit = 8;

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
