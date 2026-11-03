#nullable disable

using System.Globalization;
using BlogKrafter.Data;
using BlogKrafter.Data.Entities;
using Inferno.Helpers;
using Inferno.Logging.Entities;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Tenants;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfernoCMS.Services;

public class TenantDataCleaner : ITenantDataCleaner
{
    private readonly ApplicationDbContext dbContext;
    private readonly IServiceProvider services;

    public TenantDataCleaner(ApplicationDbContext dbContext, IServiceProvider services)
    {
        this.dbContext = dbContext;
        this.services = services;
    }

    public async Task CleanAsync(int tenantId)
    {
        await CleanBlogAsync(tenantId);

        var menuIds = await dbContext.Set<Menu>()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync();
        if (menuIds.Count > 0)
        {
            await dbContext.Set<MenuItem>().Where(x => menuIds.Contains(x.MenuId)).ExecuteDeleteAsync();
        }

        await dbContext.Set<Menu>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();

        var zoneIds = await dbContext.Set<Zone>()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync();
        var pageIds = await dbContext.Set<Page>()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync();

        var blockIds = await dbContext.Set<ContentBlock>()
            .Where(x => zoneIds.Contains(x.ZoneId) || (x.PageId != null && pageIds.Contains(x.PageId.Value)))
            .Select(x => x.Id)
            .ToListAsync();
        var entityBlockIds = await dbContext.Set<EntityTypeContentBlock>()
            .Where(x => zoneIds.Contains(x.ZoneId))
            .Select(x => x.Id)
            .ToListAsync();

        if (blockIds.Count > 0)
        {
            await dbContext.Set<ContentBlock>().Where(x => blockIds.Contains(x.Id)).ExecuteDeleteAsync();
        }

        if (entityBlockIds.Count > 0)
        {
            await dbContext.Set<EntityTypeContentBlock>().Where(x => entityBlockIds.Contains(x.Id)).ExecuteDeleteAsync();
        }

        await dbContext.Set<PageVersion>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<Page>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<SitemapConfig>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<Zone>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();

        var templateIds = await dbContext.Set<MessageTemplate>()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync();
        if (templateIds.Count > 0)
        {
            await dbContext.Set<MessageTemplateVersion>().Where(x => templateIds.Contains(x.MessageTemplateId)).ExecuteDeleteAsync();
        }

        await dbContext.Set<MessageTemplate>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<QueuedEmail>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<Setting>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await DeleteLocalizationAsync(tenantId);
        await dbContext.Set<LogEntry>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<UserProfileEntry>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();

        var userIds = await dbContext.Users.Where(x => x.TenantId == tenantId).Select(x => x.Id).ToListAsync();
        if (userIds.Count > 0)
        {
            await dbContext.Set<IdentityUserRole<string>>().Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
            await dbContext.Set<IdentityUserClaim<string>>().Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
            await dbContext.Set<IdentityUserLogin<string>>().Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
            await dbContext.Set<IdentityUserToken<string>>().Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
            await dbContext.Users.Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        }

        var roleIds = await dbContext.Roles.Where(x => x.TenantId == tenantId).Select(x => x.Id).ToListAsync();
        if (roleIds.Count > 0)
        {
            await dbContext.Set<IdentityRoleClaim<string>>().Where(x => roleIds.Contains(x.RoleId)).ExecuteDeleteAsync();
            await dbContext.Roles.Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        }

        string mediaPath = CommonHelper.MapPath("~/Media/Uploads/Tenant_" + tenantId);
        if (Directory.Exists(mediaPath))
        {
            CommonHelper.DeleteDirectory(mediaPath);
        }

        string sitemapPath = CommonHelper.MapPath($"~/sitemap-{tenantId}.xml");
        if (File.Exists(sitemapPath))
        {
            File.Delete(sitemapPath);
        }
    }

    private async Task DeleteLocalizationAsync(int tenantId)
    {
        try
        {
            string scope = tenantId.ToString();
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM localize.LocalizableProperties WHERE ScopeId = {scope}");
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM localize.LocalizableStrings WHERE ScopeId = {scope}");
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM localize.Languages WHERE ScopeId = {scope}");
        }
        catch (Exception)
        {
            // The catalog tables are created by the localization library. A tenant deleted
            // before that database exists has nothing to remove.
        }
    }

    /// <summary>
    /// The blog keeps its own tables. Links and old addresses go before the posts they point at.
    /// </summary>
    private async Task CleanBlogAsync(int tenantId)
    {
        var factory = services.GetService<IBlogKrafterDbContextFactory>();
        if (factory == null)
        {
            return;
        }

        string tenantKey = tenantId.ToString(CultureInfo.InvariantCulture);
        await using var blog = factory.GetContext();
        var postIds = blog.Set<BlogPost>().Where(post => post.TenantId == tenantKey).Select(post => post.Id);
        await blog.Set<BlogPostTag>().Where(link => postIds.Contains(link.PostId)).ExecuteDeleteAsync();
        await blog.Set<BlogSlugRedirect>().Where(redirect => redirect.TenantId == tenantKey).ExecuteDeleteAsync();
        await blog.Set<BlogPost>().Where(post => post.TenantId == tenantKey).ExecuteDeleteAsync();
        await blog.Set<BlogCategory>().Where(category => category.TenantId == tenantKey).ExecuteDeleteAsync();
        await blog.Set<BlogTag>().Where(tag => tag.TenantId == tenantKey).ExecuteDeleteAsync();
    }
}
