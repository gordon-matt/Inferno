using Inferno.Web.ContentManagement.Areas.Admin.Dashboard;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Xunit;

namespace Inferno.Tests;

public class DashboardSnapshotBuilderTests
{
    private static readonly DateTime Now = new(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_PublishedEnabledPage_CountsPublishedAndIgnoresEmptyDrafts()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(pageId, "Home", enabled: true)],
            versions: [Version(pageId, VersionStatus.Published, Now.AddDays(-2), "Home", """{"metaDescription":"Welcome"}""")]));

        Assert.Equal(1, snapshot.PublishedPages);
        Assert.Equal(0, snapshot.DraftPages);
        Assert.Equal(0, snapshot.AttentionCount);
        Assert.Equal("Home", snapshot.Recent[0].Title);
    }

    [Fact]
    public void Build_DisabledPublishedPage_IsAttentionAndNotAPublishedCount()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(pageId, "Old", enabled: false)],
            versions: [Version(pageId, VersionStatus.Published, Now.AddDays(-1), "Old", """{"metaDescription":"Kept"}""")]));

        Assert.Equal(0, snapshot.PublishedPages);
        Assert.Contains(snapshot.Attention, item => item.Reason == "Switched off");
    }

    [Fact]
    public void Build_PublishedPageWithoutDescription_NeedsAttention()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(pageId, "About", enabled: true)],
            versions: [Version(pageId, VersionStatus.Published, Now.AddDays(-1), "About", "{}")]));

        Assert.Equal(1, snapshot.PublishedPages);
        Assert.Contains(snapshot.Attention, item => item.Reason == "Missing meta description" && item.Href.Contains("/admin/pages/design/"));
    }

    [Fact]
    public void Build_NewerDraft_IsUnpublishedChanges()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(pageId, "About", enabled: true)],
            versions:
            [
                Version(pageId, VersionStatus.Published, Now.AddDays(-5), "About", """{"metaDescription":"Live"}"""),
                Version(pageId, VersionStatus.Draft, Now.AddDays(-1), "About draft", """{"metaDescription":"Live"}"""),
            ]));

        Assert.Equal(1, snapshot.PublishedPages);
        Assert.Equal(0, snapshot.DraftPages);
        Assert.Contains(snapshot.Attention, item => item.Reason == "Has unpublished changes");
    }

    [Fact]
    public void Build_StaleDraftOnly_CountsDraftAndAttention()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(pageId, "Draft page", enabled: true)],
            versions: [Version(pageId, VersionStatus.Draft, Now.AddDays(-40), "Draft page", "{}")]));

        Assert.Equal(1, snapshot.DraftPages);
        Assert.Equal(0, snapshot.PublishedPages);
        Assert.Contains(snapshot.Attention, item => item.Reason.StartsWith("Draft not updated", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_ArchivedOnly_CountsArchivedAndSkipsPagesWithNoVersions()
    {
        var archivedId = Guid.NewGuid();
        var emptyId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(archivedId, "Gone", enabled: true), Page(emptyId, "Empty", enabled: true)],
            versions: [Version(archivedId, VersionStatus.Archived, Now.AddDays(-3), "Gone", "{}")]));

        Assert.Equal(1, snapshot.ArchivedPages);
        Assert.Equal(0, snapshot.PublishedPages);
        Assert.Equal(0, snapshot.DraftPages);
        Assert.DoesNotContain(snapshot.Recent, item => item.Title == "Empty");
    }

    [Fact]
    public void Build_ScheduledAndPublishedPosts_SplitOnPublishDate()
    {
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            posts:
            [
                Post(1, "Live", isDraft: false, publishedAt: null, updatedAt: Now.AddDays(-1)),
                Post(2, "Later", isDraft: false, publishedAt: Now.AddDays(3), updatedAt: Now),
                Post(3, "Notes", isDraft: true, publishedAt: null, updatedAt: Now.AddDays(-2)),
            ]));

        Assert.Equal(1, snapshot.PublishedPosts);
        Assert.Equal(1, snapshot.ScheduledPosts);
        Assert.Equal(1, snapshot.DraftPosts);
        Assert.Equal("Later", snapshot.Scheduled[0].Title);
        Assert.Contains("/admin/blog/posts/2", snapshot.Scheduled[0].Href);
    }

    [Fact]
    public void Build_PublishedPostMissingSeoAndAlt_NeedsAttention()
    {
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            posts: [Post(4, "Thin", isDraft: false, publishedAt: Now.AddDays(-1), updatedAt: Now, metaTitle: "", metaDescription: "", coverUrl: "/cover.jpg", coverAlt: "")]));

        Assert.Contains(snapshot.Attention, item => item.Reason == "Missing meta description");
        Assert.Contains(snapshot.Attention, item => item.Reason == "Missing SEO title");
        Assert.Contains(snapshot.Attention, item => item.Reason == "Cover image has no alt text");
    }

    [Fact]
    public void Build_DraftPost_DoesNotFlagMissingSeo()
    {
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            posts: [Post(5, "Scratch", isDraft: true, publishedAt: null, updatedAt: Now, metaTitle: "", metaDescription: "")]));

        Assert.Equal(1, snapshot.DraftPosts);
        Assert.Empty(snapshot.Attention);
    }

    [Fact]
    public void Build_Activity_BucketsEditsIntoTheMondayWeek()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(Input(
            pages: [Page(pageId, "Home", enabled: true)],
            versions: [Version(pageId, VersionStatus.Published, new DateTime(2026, 11, 10, 8, 0, 0, DateTimeKind.Utc), "Home", """{"metaDescription":"Welcome"}""")],
            posts: [Post(1, "Live", isDraft: false, publishedAt: null, updatedAt: new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc))]));

        Assert.Equal(8, snapshot.Activity.Count);
        Assert.Equal(new DateTime(2026, 11, 9), snapshot.Activity[^1].WeekStart);
        Assert.Equal(1, snapshot.Activity[^1].Pages);
        Assert.Equal(0, snapshot.Activity[^1].Posts);
        Assert.Equal(new DateTime(2026, 11, 2), snapshot.Activity[^2].WeekStart);
        Assert.Equal(1, snapshot.Activity[^2].Posts);
    }

    [Fact]
    public void Build_WithoutPermission_HidesThoseCounts()
    {
        var pageId = Guid.NewGuid();
        var snapshot = DashboardSnapshotBuilder.Build(new DashboardSnapshotInput
        {
            CanReadPages = false,
            CanReadPosts = false,
            CanReadUsers = false,
            BlogEnabled = true,
            UtcNow = Now,
            Pages = [Page(pageId, "Home", enabled: true)],
            Versions = [Version(pageId, VersionStatus.Published, Now, "Home", "{}")],
            Posts = [Post(1, "Live", isDraft: false, publishedAt: null, updatedAt: Now)],
            Users = [new DashboardUserRow("Ada", "ada@example.com", true)],
        });

        Assert.Equal(0, snapshot.PublishedPages);
        Assert.Equal(0, snapshot.PublishedPosts);
        Assert.Equal(0, snapshot.Users);
        Assert.Empty(snapshot.Attention);
    }

    private static DashboardSnapshotInput Input(
        IReadOnlyList<DashboardPageRow> pages = null,
        IReadOnlyList<DashboardVersionRow> versions = null,
        IReadOnlyList<DashboardPostRow> posts = null) => new()
        {
            CanReadPages = true,
            CanReadPosts = true,
            CanReadUsers = true,
            BlogEnabled = true,
            UtcNow = Now,
            Pages = pages ?? [],
            Versions = versions ?? [],
            Posts = posts ?? [],
            Users = [],
            Roles = [],
        };

    private static DashboardPageRow Page(Guid id, string name, bool enabled) => new(id, name, enabled);

    private static DashboardVersionRow Version(Guid pageId, VersionStatus status, DateTime modified, string title, string fields) =>
        new(Guid.NewGuid(), pageId, status, title, fields, modified);

    private static DashboardPostRow Post(
        int id,
        string title,
        bool isDraft,
        DateTime? publishedAt,
        DateTime updatedAt,
        string metaTitle = "Title",
        string metaDescription = "Description",
        string coverUrl = "",
        string coverAlt = "") =>
        new(id, title, isDraft, publishedAt, updatedAt, "Ada", metaTitle, metaDescription, coverUrl, coverAlt);
}
