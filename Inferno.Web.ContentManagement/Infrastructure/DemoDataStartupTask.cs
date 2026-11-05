using System.Globalization;
using System.Net;
using BlogKrafter;
using BlogKrafter.Data.Entities;
using Dependo;
using Extenso;
using Extenso.Data.Entity;
using Inferno.Helpers;
using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.Infrastructure;
using Inferno.Web.Security.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Templates;

namespace Inferno.Web.ContentManagement.Infrastructure;

/// <summary>
/// Runs once, after installation, when the installer ticked "Seed demo data". It fills in the first tenant
/// and adds the others from <see cref="DemoTenants"/>: each is a separate site with its own host name, pages,
/// menus, content blocks, accounts and, for most, a blog. The administrator account is created by an earlier
/// startup task. Theme choices are applied later, once the theme tables exist; see <see cref="DataSettings.DemoThemesPending"/>.
/// </summary>
public class DemoDataStartupTask : IStartupTask
{
    private const string MANAGERS_ROLE = "Managers";

    private static readonly char[] Separators = [' ', '\r', '\n', '\t'];

    public int Order => 50;

    public async Task ExecuteAsync()
    {
        var dataSettings = DependoResolver.Instance.Resolve<DataSettings>();
        if (!dataSettings.CreateSampleData)
        {
            return;
        }

        var logger = DependoResolver.Instance.Resolve<ILoggerFactory>().CreateLogger<DemoDataStartupTask>();
        var tenants = DependoResolver.Instance.Resolve<ITenantService>();
        var membership = DependoResolver.Instance.Resolve<IMembershipService>();
        var workContext = DependoResolver.Instance.Resolve<IWorkContext>();

        var host = string.IsNullOrWhiteSpace(dataSettings.AdminEmail)
            ? null
            : await membership.GetUserByEmailAsync(null, dataSettings.AdminEmail);

        foreach (var demo in DemoTenants.All)
        {
            try
            {
                var tenant = await EnsureTenantAsync(tenants, demo);

                // Identity's role and user lookups follow the current tenant, and nothing is current at startup.
                using var scope = workContext.ActAsTenant(tenant);
                await membership.EnsureAdminRoleForTenantAsync(tenant.Id);
                await SeedTenantAsync(tenant.Id, demo, host?.Id, membership, logger);
            }
            catch (Exception exception)
            {
                // A failed demo tenant should not stop the application starting.
                logger.LogError(exception, "The demo data for {Tenant} could not be created.", demo.Name);
            }
        }

        logger.LogInformation(
            "Demo data created for {Count} tenants. Demo accounts use the password {Password}.",
            DemoTenants.All.Count,
            DemoTenants.UserPassword);

        dataSettings.CreateSampleData = false;
        dataSettings.DemoThemesPending = true;
        DataSettingsManager.SaveSettings(dataSettings);
    }

    /// <summary>The first tenant already exists, so it is only given its host names. The others are created the way the admin screen does.</summary>
    private static async Task<Tenant> EnsureTenantAsync(ITenantService tenants, DemoTenant demo)
    {
        Tenant existing;
        using (var connection = tenants.OpenConnection())
        {
            existing = connection.Query(tenant => tenant.Name == demo.Name).FirstOrDefault();

            if (existing is null && demo.Name == DemoTenants.DefaultTenantName)
            {
                existing = connection.Query().OrderBy(tenant => tenant.Id).FirstOrDefault();
            }
        }

        if (existing is not null)
        {
            existing.Url = demo.Url;
            existing.Hosts = demo.Hosts;
            await tenants.UpdateAsync(existing);
            return existing;
        }

        var created = await tenants.InsertAsync(new Tenant
        {
            Name = demo.Name,
            Url = demo.Url,
            Hosts = demo.Hosts,
        });

        var mediaFolder = new DirectoryInfo(CommonHelper.MapPath($"~/Media/Uploads/Tenant_{created.Id}"));
        if (!mediaFolder.Exists)
        {
            mediaFolder.Create();
        }

        return created;
    }

    private static async Task SeedTenantAsync(int tenantId, DemoTenant demo, string hostAdminId, IMembershipService membership, ILogger logger)
    {
        var pages = DependoResolver.Instance.Resolve<IPageService>();
        using (var connection = pages.OpenConnection())
        {
            if (connection.Query(page => page.TenantId == tenantId).Any())
            {
                logger.LogInformation("Tenant {Tenant} already has pages. Its demo data was skipped.", demo.Name);
                return;
            }
        }

        SaveSiteSettings(tenantId, demo);
        var authors = await SeedUsersAsync(tenantId, demo, membership);

        var versions = DependoResolver.Instance.Resolve<IPageVersionService>();
        var zones = DependoResolver.Instance.Resolve<IZoneService>();
        var blocks = DependoResolver.Instance.Resolve<IContentBlockService>();
        var menus = DependoResolver.Instance.Resolve<IMenuService>();
        var menuItems = DependoResolver.Instance.Resolve<IMenuItemService>();

        await SeedHomeAsync(tenantId, demo, zones, blocks);

        int order = 0;
        foreach (var page in demo.Pages)
        {
            await SeedPageAsync(pages, versions, tenantId, null, order++, page);
        }

        foreach (var menu in demo.Menus)
        {
            await SeedMenuAsync(tenantId, menu, menus, menuItems);
        }

        if (demo.EnableBlog)
        {
            using var blogScope = DependoResolver.Instance.Resolve<IServiceProvider>().CreateScope();
            var author = authors.Count > 0 ? authors[0] : (hostAdminId, "Administrator");
            await SeedBlogAsync(tenantId, demo, author, blogScope.ServiceProvider);
        }
    }

    private static void SaveSiteSettings(int tenantId, DemoTenant demo)
    {
        var settingService = DependoResolver.Instance.Resolve<ISettingService>();
        var settings = settingService.GetSettings<SiteSettings>(tenantId);
        settings.SiteName = demo.SiteName;
        settings.HomePageTitle = demo.HomePageTitle;
        settings.EnableBlog = demo.EnableBlog;
        settingService.SaveSettings(settings, tenantId);
    }

    /// <summary>
    /// The Administrators role belongs to the host, so a tenant's own people get a role of their own: every
    /// permission except the ones that reach across tenants. It is what a real tenant would set up.
    /// </summary>
    /// <remarks>
    /// The name includes the tenant because this runs outside any request, where Identity checks for a clash
    /// against the first tenant's roles whichever tenant the role is for.
    /// </remarks>
    private static async Task<InfernoRole> EnsureManagersRoleAsync(int tenantId, DemoTenant demo, IMembershipService membership)
    {
        string roleName = $"{demo.SiteName} {MANAGERS_ROLE}";
        var role = await membership.GetRoleByNameAsync(tenantId, roleName);
        if (role is null)
        {
            await membership.InsertRoleAsync(new InfernoRole { TenantId = tenantId, Name = roleName });
            role = await membership.GetRoleByNameAsync(tenantId, roleName);
        }

        if (role is not null)
        {
            var permissions = DependoResolver.Instance.Resolve<IPermissionCatalog>().All
                .Select(permission => permission.Name)
                .Where(name => name != StandardPolicies.FullAccess);

            var granted = await DependoResolver.Instance.Resolve<IPermissionStore>().SetRolePermissionsAsync(role.Id, permissions);
            if (!granted.IsSuccess)
            {
                throw new InvalidOperationException($"The permissions for role {role.Name} could not be set: {string.Join(' ', granted.Errors)}");
            }
        }
        else
        {
            throw new InvalidOperationException($"The role {roleName} could not be created.");
        }

        return role;
    }

    /// <summary>The tenant's demo accounts. Returns its managers, who are the authors the blog posts are credited to.</summary>
    private static async Task<List<(string Id, string Name)>> SeedUsersAsync(int tenantId, DemoTenant demo, IMembershipService membership)
    {
        var authors = new List<(string Id, string Name)>();
        var managers = demo.Users.Any(user => user.IsManager) ? await EnsureManagersRoleAsync(tenantId, demo, membership) : null;

        foreach (var account in demo.Users.OrderByDescending(user => user.IsManager))
        {
            var user = await membership.GetUserByEmailAsync(tenantId, account.Email);
            if (user is null)
            {
                await membership.InsertUserAsync(
                    new InfernoUser
                    {
                        TenantId = tenantId,
                        UserName = account.Email,
                        Email = account.Email,
                        EmailConfirmed = true,
                    },
                    DemoTenants.UserPassword);

                user = await membership.GetUserByEmailAsync(tenantId, account.Email);
            }

            if (user is null)
            {
                continue;
            }

            await membership.UpdateProfileAsync(user.Id, new Dictionary<string, string>
            {
                ["GivenNames"] = account.GivenName,
                ["FamilyName"] = account.FamilyName,
            });

            if (account.IsManager)
            {
                if (managers is not null)
                {
                    await membership.AssignUserToRolesAsync(tenantId, user.Id, [managers.Id]);
                }

                authors.Add((user.Id, $"{account.GivenName} {account.FamilyName}"));
            }
        }

        return authors;
    }

    private static async Task SeedHomeAsync(int tenantId, DemoTenant demo, IZoneService zones, IContentBlockService blocks)
    {
        var home = await zones.InsertAsync(new Zone { Id = Guid.NewGuid(), TenantId = tenantId, Name = "HomePageContent" });
        await InsertBlockAsync(blocks, home.Id, "Welcome", new HtmlBlock { BodyContent = demo.HomeHtml }, order: 0);

        if (demo.EnableBlog)
        {
            await InsertBlockAsync(blocks, home.Id, "Latest posts", new LastNPostsBlock { NumberOfEntries = 3 }, order: 1);
        }

        var footer = await zones.InsertAsync(new Zone { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Footer" });
        await InsertBlockAsync(blocks, footer.Id, "Footer", new HtmlBlock { BodyContent = demo.FooterHtml }, order: 0);
    }

    private static async Task InsertBlockAsync(IContentBlockService blocks, Guid zoneId, string title, ContentBlockBase content, int order) =>
        await blocks.InsertAsync(new ContentBlock
        {
            Id = Guid.NewGuid(),
            ZoneId = zoneId,
            Title = title,
            BlockName = content.Name,
            BlockType = content.GetType().FullName,
            BlockValues = content.JsonSerialize(),
            Order = order,
            IsEnabled = true,
        });

    private static async Task SeedPageAsync(IPageService pages, IPageVersionService versions, int tenantId, Guid? parentId, int order, DemoPage demo)
    {
        var now = DateTime.UtcNow;
        var page = await pages.InsertAsync(new Page
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ParentId = parentId,
            Name = demo.Name,
            Order = order,
            IsEnabled = true,
            ShowOnMenus = true,
        });

        // The editor reopens the document, so it holds the same content the visitor sees.
        var document = PageDocument.Create();
        document.Root.Add(Nodes.Section(Nodes.Text(demo.Html)));

        await versions.InsertAsync(new PageVersion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PageId = page.Id,
            Status = VersionStatus.Published,
            Title = demo.Name,
            Slug = demo.Slug,
            Fields = StandardPageContent.Serialize(new StandardPageFields
            {
                MetaDescription = WebUtility.HtmlDecode(StripTags(demo.Html)).Trim() is { Length: > 0 } text
                    ? text[..Math.Min(text.Length, 155)]
                    : null,
                Document = PageKrafterJson.Serialize(document),
                BodyContent = demo.Html,
            }),
            DateCreatedUtc = now,
            DateModifiedUtc = now,
        });

        int childOrder = 0;
        foreach (var child in demo.Children)
        {
            await SeedPageAsync(pages, versions, tenantId, page.Id, childOrder++, child);
        }
    }

    private static async Task SeedMenuAsync(int tenantId, DemoMenu demo, IMenuService menus, IMenuItemService menuItems)
    {
        var menu = await menus.InsertAsync(new Menu { Id = Guid.NewGuid(), TenantId = tenantId, Name = demo.Name });

        var items = new List<MenuItem>();
        Flatten(menu.Id, null, demo.Items, items);
        await menuItems.InsertAsync(items);
    }

    private static void Flatten(Guid menuId, Guid? parentId, IEnumerable<DemoMenuItem> source, List<MenuItem> target)
    {
        int position = 0;
        foreach (var item in source)
        {
            var entity = new MenuItem
            {
                Id = Guid.NewGuid(),
                MenuId = menuId,
                ParentId = parentId,
                Text = item.Text,
                Url = item.Url,
                Position = position++,
                Enabled = true,
            };

            target.Add(entity);
            Flatten(menuId, entity.Id, item.Children, target);
        }
    }

    private static async Task SeedBlogAsync(int tenantId, DemoTenant demo, (string Id, string Name) author, IServiceProvider services)
    {
        var categories = services.GetRequiredService<IRepository<BlogCategory>>();
        var tags = services.GetRequiredService<IRepository<BlogTag>>();
        var posts = services.GetRequiredService<IRepository<BlogPost>>();
        var postTags = services.GetRequiredService<IRepository<BlogPostTag>>();

        string tenantKey = tenantId.ToString(CultureInfo.InvariantCulture);
        var categoryIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in demo.Categories)
        {
            var category = await categories.InsertAsync(new BlogCategory { TenantId = tenantKey, Name = name, Slug = Slugify(name) });
            categoryIds[name] = category.Id;
        }

        var tagIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in demo.Posts.SelectMany(post => post.Tags).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var tag = await tags.InsertAsync(new BlogTag { TenantId = tenantKey, Name = name, Slug = Slugify(name) });
            tagIds[name] = tag.Id;
        }

        var now = DateTime.UtcNow;
        foreach (var demoPost in demo.Posts)
        {
            var published = now.AddDays(-demoPost.DaysAgo);
            var post = await posts.InsertAsync(new BlogPost
            {
                TenantId = tenantKey,
                Title = demoPost.Title,
                Slug = demoPost.Slug,
                Summary = demoPost.Summary,
                Excerpt = demoPost.Summary,
                ContentFormat = ContentFormat.Html,
                Content = demoPost.Html,
                ContentHtml = demoPost.Html,
                Status = PostStatus.Published,
                PublishedAt = published,
                IsFeatured = demoPost.IsFeatured,
                CommentsEnabled = true,
                ReadingTimeMinutes = Math.Max(1, StripTags(demoPost.Html).Split(Separators, StringSplitOptions.RemoveEmptyEntries).Length / 200),
                AuthorUserId = string.IsNullOrEmpty(author.Id) ? "demo" : author.Id,
                AuthorName = author.Name,
                CreatedAt = published,
                UpdatedAt = published,
                CategoryId = categoryIds.TryGetValue(demoPost.Category, out int categoryId) ? categoryId : null,
            });

            await postTags.InsertAsync([.. demoPost.Tags.Select(tag => new BlogPostTag { PostId = post.Id, TagId = tagIds[tag] })]);
        }
    }

    private static string Slugify(string value) =>
        string.Concat(value.ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '-')).Trim('-');

    private static string StripTags(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
}
