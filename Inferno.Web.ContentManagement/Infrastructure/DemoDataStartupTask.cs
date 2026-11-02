using System.Net;
using Dependo;
using Extenso;
using Extenso.Data.Entity;
using Inferno.Security.Membership;
using Inferno.Tenants.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.Infrastructure;
using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Templates;

namespace Inferno.Web.ContentManagement.Infrastructure;

/// <summary>
/// Runs once, after installation, when the installer ticked "Seed demo data". The administrator account
/// is created by an earlier startup task, so this one can attach the sample blog post to that account.
/// </summary>
public class DemoDataStartupTask : IStartupTask
{
    public int Order => 50;

    public async Task ExecuteAsync()
    {
        var dataSettings = DependoResolver.Instance.Resolve<DataSettings>();
        if (!dataSettings.CreateSampleData)
        {
            return;
        }

        var tenants = DependoResolver.Instance.Resolve<ITenantService>();
        var pages = DependoResolver.Instance.Resolve<IPageService>();
        var versions = DependoResolver.Instance.Resolve<IPageVersionService>();
        var zones = DependoResolver.Instance.Resolve<IZoneService>();
        var blocks = DependoResolver.Instance.Resolve<IContentBlockService>();
        var menus = DependoResolver.Instance.Resolve<IMenuService>();
        var menuItems = DependoResolver.Instance.Resolve<IMenuItemService>();
        var categories = DependoResolver.Instance.Resolve<IBlogCategoryService>();
        var posts = DependoResolver.Instance.Resolve<IBlogPostService>();
        var membership = DependoResolver.Instance.Resolve<IMembershipService>();

        var admin = string.IsNullOrWhiteSpace(dataSettings.AdminEmail)
            ? null
            : await membership.GetUserByEmailAsync(null, dataSettings.AdminEmail);

        List<int> tenantIds;
        using (var connection = tenants.OpenConnection())
        {
            tenantIds = connection.Query().Select(tenant => tenant.Id).ToList();
        }

        foreach (int tenantId in tenantIds)
        {
            await SeedTenantAsync(tenantId, admin?.Id, pages, versions, zones, blocks, menus, menuItems, categories, posts);
        }

        dataSettings.CreateSampleData = false;
        DataSettingsManager.SaveSettings(dataSettings);
    }

    private static async Task SeedTenantAsync(
        int tenantId,
        string adminUserId,
        IPageService pages,
        IPageVersionService versions,
        IZoneService zones,
        IContentBlockService blocks,
        IMenuService menus,
        IMenuItemService menuItems,
        IBlogCategoryService categories,
        IBlogPostService posts)
    {
        using (var connection = pages.OpenConnection())
        {
            if (connection.Query(page => page.TenantId == tenantId).Any())
            {
                return;
            }
        }

        await SeedHomeAsync(tenantId, zones, blocks);

        var about = await SeedPageAsync(pages, versions, tenantId, null, 1, "About", "about", "About", "A short introduction to the site.");
        await SeedPageAsync(pages, versions, tenantId, about.Id, 0, "The team", "team", "The team", "The people behind the site.");
        await SeedPageAsync(pages, versions, tenantId, null, 2, "Contact", "contact", "Contact", "How to get in touch.");

        var menu = await menus.InsertAsync(new Menu
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Main",
        });

        await menuItems.InsertAsync(
        [
            Item(menu.Id, "About", "/about", 0),
            Item(menu.Id, "The team", "/team", 1),
            Item(menu.Id, "Contact", "/contact", 2),
        ]);

        var category = await categories.InsertAsync(new BlogCategory
        {
            TenantId = tenantId,
            Name = "News",
            UrlSlug = "news",
        });

        var now = DateTime.UtcNow;
        await posts.InsertAsync(new BlogPost
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = string.IsNullOrEmpty(adminUserId) ? "demo" : adminUserId,
            CategoryId = category.Id,
            DateCreatedUtc = now,
            Headline = "Hello from Inferno",
            Slug = "hello-from-inferno",
            ShortDescription = "A sample post, so the blog has something to show.",
            FullDescription = "<p>This post was created with the demo data. Edit it or delete it whenever you like.</p>",
            UseExternalLink = false,
        });
    }

    private static async Task SeedHomeAsync(int tenantId, IZoneService zones, IContentBlockService blocks)
    {
        var zone = await zones.InsertAsync(new Zone
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "HomePageContent",
        });

        var content = new HtmlBlock
        {
            BodyContent = """
                <h1>Welcome</h1>
                <p>This is the demo home page. Its content is a block in the HomePageContent zone, which you can edit under Content Blocks.</p>
                <p>The About and Contact pages were built with the page editor.</p>
                """,
        };

        await blocks.InsertAsync(new ContentBlock
        {
            Id = Guid.NewGuid(),
            ZoneId = zone.Id,
            Title = "Welcome",
            BlockName = content.Name,
            BlockType = typeof(HtmlBlock).FullName,
            BlockValues = content.JsonSerialize(),
            Order = 0,
            IsEnabled = true,
        });
    }

    private static async Task<Page> SeedPageAsync(
        IPageService pages,
        IPageVersionService versions,
        int tenantId,
        Guid? parentId,
        int order,
        string name,
        string slug,
        string heading,
        string paragraph)
    {
        var now = DateTime.UtcNow;
        var page = await pages.InsertAsync(new Page
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ParentId = parentId,
            Name = name,
            Order = order,
            IsEnabled = true,
            ShowOnMenus = true,
        });

        var document = PageDocument.Create();
        document.Root.Add(Nodes.Section(Nodes.Heading(heading, "h1"), Nodes.Text($"<p>{WebUtility.HtmlEncode(paragraph)}</p>")));

        await versions.InsertAsync(new PageVersion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PageId = page.Id,
            Status = VersionStatus.Published,
            Title = heading,
            Slug = slug,
            Fields = StandardPageContent.Serialize(new StandardPageFields
            {
                Document = PageKrafterJson.Serialize(document),
                BodyContent = $"""
                    <section>
                      <h1>{WebUtility.HtmlEncode(heading)}</h1>
                      <p>{WebUtility.HtmlEncode(paragraph)}</p>
                    </section>
                    """,
            }),
            DateCreatedUtc = now,
            DateModifiedUtc = now,
        });

        return page;
    }

    private static MenuItem Item(Guid menuId, string text, string url, int position) => new()
    {
        Id = Guid.NewGuid(),
        MenuId = menuId,
        Text = text,
        Url = url,
        Position = position,
        Enabled = true,
    };
}
