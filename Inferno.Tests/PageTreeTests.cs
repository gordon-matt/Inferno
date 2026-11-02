using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Xunit;

namespace Inferno.Tests;

public class PageHierarchyTests
{
    private static readonly Guid Home = Guid.NewGuid();
    private static readonly Guid About = Guid.NewGuid();
    private static readonly Guid Team = Guid.NewGuid();
    private static readonly Guid Contact = Guid.NewGuid();

    private static List<Page> Tree() =>
    [
        new Page { Id = Contact, Name = "Contact", Order = 2 },
        new Page { Id = Home, Name = "Home", Order = 0 },
        new Page { Id = About, Name = "About", Order = 1 },
        new Page { Id = Team, Name = "Team", ParentId = About },
    ];

    [Fact]
    public void ChildrenOf_ListsTheTopLevelThenSubPagesInOrder()
    {
        var pages = Tree();

        Assert.Equal(["Home", "About", "Contact"], PageHierarchy.ChildrenOf(pages, null).Select(page => page.Name));
        Assert.Equal(["Team"], PageHierarchy.ChildrenOf(pages, About).Select(page => page.Name));
    }

    [Fact]
    public void A_page_whose_parent_is_missing_is_shown_at_the_top_level()
    {
        var pages = Tree();
        pages.Add(new Page { Id = Guid.NewGuid(), Name = "Orphan", Order = 9, ParentId = Guid.NewGuid() });

        Assert.Contains("Orphan", PageHierarchy.ChildrenOf(pages, null).Select(page => page.Name));
    }

    [Fact]
    public void A_page_can_move_to_the_top_level_or_under_an_unrelated_page()
    {
        var pages = Tree();

        Assert.True(PageHierarchy.CanBeParent(pages, Team, null));
        Assert.True(PageHierarchy.CanBeParent(pages, Team, Contact));
        Assert.True(PageHierarchy.CanBeParent(pages, Contact, Team));
    }

    [Fact]
    public void A_page_cannot_go_under_itself_or_its_own_sub_pages()
    {
        var pages = Tree();

        Assert.False(PageHierarchy.CanBeParent(pages, About, About));
        Assert.False(PageHierarchy.CanBeParent(pages, About, Team));
    }

    [Fact]
    public void A_parent_that_does_not_exist_is_refused()
    {
        Assert.False(PageHierarchy.CanBeParent(Tree(), Home, Guid.NewGuid()));
    }

    [Fact]
    public void Flatten_puts_parents_before_their_sub_pages_with_their_depth()
    {
        var flat = PageHierarchy.Flatten(Tree());

        Assert.Equal(
            [("Home", 0), ("About", 0), ("Team", 1), ("Contact", 0)],
            flat.Select(item => (item.Page.Name, item.Depth)));
    }
}

public class SavePointTests
{
    private static PageVersion Version(VersionStatus status, string title = "About", string fields = "{\"Document\":\"a\"}") =>
        new() { Status = status, Title = title, Slug = "about", Fields = fields, DateModifiedUtc = DateTime.UtcNow };

    [Theory]
    [InlineData(VersionStatus.Draft)]
    [InlineData(VersionStatus.Published)]
    public void Saving_new_content_keeps_the_earlier_state_for_drafts_and_published_pages(VersionStatus status)
    {
        var current = Version(status);
        var edited = Version(status, fields: "{\"Document\":\"b\"}");

        Assert.True(PageAuthoringService.ShouldKeepSavePoint(current, edited));
    }

    [Fact]
    public void Saving_again_straight_after_a_save_still_keeps_a_save_point()
    {
        // Saves a minute apart used to be treated as one editing session and left no history.
        var current = Version(VersionStatus.Published);
        current.DateModifiedUtc = DateTime.UtcNow.AddMinutes(-1);

        Assert.True(PageAuthoringService.ShouldKeepSavePoint(current, Version(VersionStatus.Published, title: "About us")));
    }

    [Fact]
    public void Publishing_or_saving_nothing_new_keeps_nothing()
    {
        var current = Version(VersionStatus.Draft);

        Assert.False(PageAuthoringService.ShouldKeepSavePoint(current, Version(VersionStatus.Published)));
        Assert.False(PageAuthoringService.ShouldKeepSavePoint(current, Version(VersionStatus.Draft)));
    }

    [Fact]
    public void A_page_that_has_never_had_content_has_nothing_to_keep()
    {
        var current = Version(VersionStatus.Draft, fields: null);

        Assert.False(PageAuthoringService.ShouldKeepSavePoint(current, Version(VersionStatus.Draft, fields: "{\"Document\":\"a\"}")));
    }
}

public class PageAccessTests
{
    [Fact]
    public void Roles_survive_a_round_trip()
    {
        string stored = PageSecurityHelper.CreateAccessRestrictions(["admin-id", "editor-id"]);

        Assert.Equal(["admin-id", "editor-id"], PageSecurityHelper.GetRoleIds(stored));
    }

    [Fact]
    public void A_page_with_no_roles_stores_nothing()
    {
        Assert.Null(PageSecurityHelper.CreateAccessRestrictions([]));
        Assert.Empty(PageSecurityHelper.GetRoleIds(null));
    }

    [Fact]
    public void Unreadable_restrictions_are_treated_as_none_for_the_editor()
    {
        Assert.Empty(PageSecurityHelper.GetRoleIds("not json"));
    }
}
