#nullable disable

using Inferno.Helpers;
using Inferno.Localization.Entities;
using Inferno.Logging.Entities;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
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

namespace InfernoCMS.Services;

public class TenantDataCleaner : ITenantDataCleaner
{
    private readonly ApplicationDbContext dbContext;

    public TenantDataCleaner(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task CleanAsync(int tenantId)
    {
        var postIds = await dbContext.Set<BlogPost>()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync();
        if (postIds.Count > 0)
        {
            await dbContext.Set<BlogPostTag>().Where(x => postIds.Contains(x.PostId)).ExecuteDeleteAsync();
        }

        await dbContext.Set<BlogPost>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<BlogCategory>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<BlogTag>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();

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

        var localizableIds = blockIds.Concat(entityBlockIds).Select(x => x.ToString()).ToList();
        if (localizableIds.Count > 0)
        {
            await dbContext.Set<LocalizableProperty>().Where(x => localizableIds.Contains(x.EntityId)).ExecuteDeleteAsync();
        }

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
        await dbContext.Set<LocalizableString>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
        await dbContext.Set<Language>().Where(x => x.TenantId == tenantId).ExecuteDeleteAsync();
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
}
