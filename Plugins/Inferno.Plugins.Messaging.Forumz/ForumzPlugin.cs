using Forumz;
using Forumz.Data.Sql;
using Forumz.Services;
using Inferno.Plugins.Messaging.Forumz;
using Inferno.Web.Components;
using Inferno.Web.Configuration;
using Inferno.Web.ContentManagement.Infrastructure;
using Inferno.Web.Infrastructure;
using Inferno.Web.Navigation;
using Inferno.Web.Security.Permissions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pluginz;
using Pluginz.Web;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Draws the forum at /forums. By default it sits under Inferno's menu. A tenant can open it in a
/// new tab with the forum's own top bar. The library owns the pages; this plugin supplies Inferno's
/// tenant, permissions and database. Endpoints cover the feeds, the stylesheet, and the forum document.
/// </summary>
public sealed class ForumzPlugin : PluginBase, IWebPlugin
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services, PluginContext context)
    {
        var installed = DataSettingsManager.LoadSettings();

        services.AddForumz(options =>
        {
            options.Title = "Forums";
            options.HomePath = "/";
            options.LogoutPath = "/Identity/Account/LogOut";
            // PublicSiteLayout is the site's top bar. The forum's own links render under it.
            options.PublicLayout = typeof(PublicSiteLayout);
            // Inferno's other editors keep WebFilez. Forum posts do not open it.
            options.UseFileManager = false;
        });

        if (!string.IsNullOrWhiteSpace(installed.ConnectionString))
        {
            services.AddForumzSqlServer(installed.ConnectionString);
        }

        // Registered after AddForumz so these replace the single-tenant defaults.
        services.AddScoped<IForumTenantScope, InfernoForumTenantScope>();
        services.AddScoped<IForumAccess, InfernoForumAccess>();
        services.AddScoped<IForumPresentation, InfernoForumPresentation>();
        services.AddScoped<ISettings, ForumSettings>();
        services.AddSingleton<IPermissionProvider, ForumzPermissionProvider>();
        services.AddSingleton<INavigationProvider, ForumzNavigationProvider>();
        services.AddSingleton<IAutoMenuProvider, ForumzAutoMenuProvider>();
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints, PluginRuntimeContext context) =>
        endpoints.MapForumz();
}
