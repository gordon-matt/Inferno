using BlogKrafter;
using BlogKrafter.Data.Sql;
using BlogKrafter.Tenancy;
using Dependo;
using Dependo.Autofac;
using Hangfire;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Navigation;
using IdentityNexus;
using Inferno.Identity;
using Inferno.Security;
using Inferno.Tenants;
using Inferno.Tenants.Entities;
using Inferno.Web.Areas.Admin;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Components;
using Inferno.Web.ContentManagement.Security;
using Inferno.Web.Infrastructure;
using Inferno.Web.Localization;
using Inferno.Web.Plugins;
using Inferno.Web.Security;
using Inferno.Tasks.Infrastructure;
using Inferno.Tasks.Scheduling;
using Inferno.Web.Scheduling;
using Inferno.Web.Tenants;
using InfernoCMS.Areas.Identity;
using InfernoCMS.Identity.Infrastructure;
using InfernoCMS.Infrastructure;
using InfernoCMS.Identity.Permissions;
using InfernoCMS.Shared;
using Kameleon;
using Kameleon.Components;
using Kameleon.Data.Sql;
using Kameleon.Tenancy;
using Localize.Components;
using Localize.Components.Pages;
using LogVue;
using LogVue.Data.Sql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using PageKrafter;
using PageKrafter.Components;
using Pickstack;
using Pickstack.FileManager.WebFilez;
using Pickstack.RichTextEditor.TinyMce8;
using Pluginz;
using Pluginz.Components;
using Pluginz.Components.Pages;
using Pluginz.Web;

ExcelPackage.License.SetNonCommercialOrganization("Inferno");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new DependoAutofacServiceProviderFactory());

#region Services

var services = builder.Services;
var configuration = builder.Configuration;

// Installed means the installation page has saved a connection. Until then there is no database.
var installedSettings = Inferno.Web.Infrastructure.DataSettingsManager.LoadSettings();
bool databaseInstalled = !string.IsNullOrWhiteSpace(installedSettings.ConnectionString);
string connectionString = databaseInstalled ? installedSettings.ConnectionString : null;

if (databaseInstalled || EF.IsDesignTime)
{
    // The migration tools need a SQL Server model even before installation has saved a connection.
    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
        connectionString ?? "Server=.;Database=Inferno;Trusted_Connection=True;TrustServerCertificate=True"));
}
else
{
    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("InfernoCMS"));
}

#region Account / Identity

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

services.AddInfernoIdentity().AddDefaultUI();
services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Identity, null));
services.AddNetForgeComponents(ui => ui.ThemeStorageKey = "inferno.theme");
services.AddLogVue(options => options.Title = "Inferno CMS");
// The connection string comes from installation. Until then LogVue starts without a store.
if (!string.IsNullOrWhiteSpace(connectionString))
{
    services.AddLogVueSqlServer(connectionString);
}
services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Inferno CMS";
    shell.BrandHref = "/admin";
    // Logo_Dark is the dark artwork for the light theme. Logo_Light is the pale artwork for the dark theme.
    shell.LogoUrl = "img/Logo_Dark.png";
    shell.LogoDarkUrl = "img/Logo_Light.png";
    shell.NavigationPlacement = ForgeNavigationPlacement.Topbar;
    shell.UseSharedMenu = true;
    shell.SharedMenuName = AdminMenuCatalog.MenuName;
    shell.ShowVersion = false;
    shell.UserMenuItems.Add(new ForgeNavItem { Title = "View site", Href = "/", Icon = "home" });
});

services.AddIdentityNexus<ApplicationUser, ApplicationRole>(options =>
{
    options.BasePath = "/admin/membership";
    options.HomePath = "/admin";
    options.LogoutPath = "/Identity/Account/LogOut";
    options.AddPermissionSections();
});

// After every AddNetForgeComponents call, which registers the static BuildMenu catalog.
services.Replace(ServiceDescriptor.Scoped<INetForgeMenuCatalog, AdminMenuCatalog>());

services.ScopeUsersToTenant<ApplicationUser, int?>(user => user.TenantId, _ => TenantKey.OfNullable((int?)CurrentTenantId.Resolve()));
services.ScopeRolesToTenant<ApplicationRole, int?>(role => role.TenantId, _ => TenantKey.OfNullable((int?)CurrentTenantId.Resolve()));

// Every permission a module declares is a policy. Only policies named by other libraries are added by hand.
services.AddInfernoAuthorization();
services.Configure<AuthorizationOptions>(options =>
{
    options.AddInfernoWebPolicies();
    options.AddInfernoCmsPolicies();
});

#endregion Account / Identity

services.AddSingleton<IConfiguration>(configuration);

services
    .AddMemoryCache()
    .AddDistributedMemoryCache();

services.AddCors(options => options.AddPolicy("AllowAll", p => p
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()));

services.AddRouting(routeOptions =>
{
    routeOptions.AppendTrailingSlash = true;
    routeOptions.LowercaseUrls = true;
});

services.AddFlatmate<Tenant, InfernoTenantResolver>();
services.AddFlatmateCircuit<Tenant>();

#region Themes

// After the NetForge, Identity and authorization setup above: Kameleon only fills in what the host has not.
services.AddScoped<IThemeTenantProvider, InfernoCMS.Infrastructure.InfernoThemeTenantProvider>();
services.AddScoped<IThemeTenantDirectory, InfernoCMS.Infrastructure.InfernoThemeTenantDirectory>();
services.AddKameleon(theme =>
{
    configuration.GetSection(KameleonOptions.SectionName).Bind(theme);

    // MainLayout is the site's own chrome: static themes restyle it, and it is used when no theme resolves.
    theme.HostLayout = typeof(MainLayout);
    theme.AdminLayout = typeof(Inferno.Web.Areas.Admin.Home.Shared.AdminLayout);

    // What every theme shows in a region when the page supplies nothing for it.
    theme.AddRegionContent<SiteBrand>(ThemeRegions.Header);
    theme.AddRegionContent<PublicNavigation>(ThemeRegions.PrimaryNavigation);
    theme.AddRegionContent<AutoBreadcrumbs>(ThemeRegions.Breadcrumbs);
    theme.AddRegionContent<PublicFooter>(ThemeRegions.Footer);
});

// Until installation has saved a connection, theme settings are kept in memory.
if (!string.IsNullOrWhiteSpace(connectionString))
{
    services.AddKameleonSqlServer(connectionString);

    // Registered after Kameleon so it starts after Kameleon has created its tables.
    services.AddHostedService<InfernoCMS.Infrastructure.DemoThemeSeeder>();
}

#endregion Themes

services.AddCmsLocalization(databaseInstalled ? connectionString : null);

// Plugins load here, before the container is built, because only now can they still add services.
builder.AddPluginz<InfernoPluginScope>(options =>
{
    options.RequireAuthentication = true;
    // Plugins load into the whole application, not one tenant, so only the host administrator may see or manage them.
    options.ReadPolicy = StandardPolicies.FullAccess;
    options.ManagePolicy = StandardPolicies.FullAccess;
});
services.AddPluginzWeb();
services.AddPluginzNuGet(nuget => nuget.TagFilter = "inferno-cms");
services.AddPluginzComponents();

services
    .AddControllersWithViews()
    .AddInfernoApplicationParts()
    .AddPluginzParts();

services.AddRazorPages().AddNewtonsoftJson();

services.AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
services.AddResponseCompression();

services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
services.AddDatabaseDeveloperPageExceptionFilter();

services.AddPickstack(configuration)
    .AddWebFilez(files => files
        .UseHostBlazor()
        .Server(server => server.MaxUploadBytes = 32 * 1024 * 1024))
    .AddTinyMce8(editor => editor
        .WithHeight(400)
        .WithToolbar("undo redo | bold italic underline | alignleft aligncenter alignright | bullist numlist | link image media | code"));

services.AddHttpContextAccessor();

services.AddHttpClient();

services.ConfigureInferno(configuration, inferno => inferno.PublicLayout = typeof(KameleonLayout));

if (!string.IsNullOrWhiteSpace(connectionString))
{
    services.AddInfernoHangfire(connectionString);
}
services.ConfigureNetForgeToastSounds(sounds => sounds.EnableSounds = true);

services.AddPageKrafter();
services.AddPageKrafterBlock<PageContentZoneBlock>();
services.AddPageKrafterFilePicker<PickstackFilePicker>();

// The blog is its own database. Until installation has saved a connection there is nothing to migrate.
if (!string.IsNullOrWhiteSpace(connectionString))
{
    services.AddScoped<IBlogTenantProvider, InfernoCMS.Infrastructure.InfernoBlogTenantProvider>();
    services.AddScoped<BlogKrafter.Infrastructure.IBlogFileRootProvider, InfernoCMS.Infrastructure.InfernoBlogFileRootProvider>();
    services.AddBlogKrafter(blog =>
    {
        blog.Title = "Blog";
        blog.PublicLayout = typeof(KameleonLayout);
        blog.AdminLayout = typeof(Inferno.Web.Areas.Admin.Home.Shared.AdminLayout);
    });
    services.AddBlogKrafterSqlServer(connectionString);
}

#endregion Services

var app = builder.Build();

if (databaseInstalled && !EF.IsDesignTime)
{
    // Bring the database up to date on every start, so a new release never meets an old schema.
    // Installation applies the same migrations to a brand new database.
    using var migrationScope = app.Services.CreateScope();
    var database = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;
    var pendingMigrations = database.GetPendingMigrations().ToList();
    if (pendingMigrations.Count > 0)
    {
        app.Logger.LogInformation("Applying {Count} database migration(s): {Migrations}", pendingMigrations.Count, string.Join(", ", pendingMigrations));
        database.Migrate();
    }

    using var blog = migrationScope.ServiceProvider.GetRequiredService<BlogKrafter.Data.IBlogKrafterDbContextFactory>().GetContext();
    blog.Database.Migrate();
}

if (!databaseInstalled)
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/installation")
            || path.StartsWithSegments("/lib")
            || path.StartsWithSegments("/css")
            || path.StartsWithSegments("/img")
            || path.StartsWithSegments("/favicon.png")
            || path.StartsWithSegments("/_framework")
            || path.StartsWithSegments("/_blazor")
            || path.StartsWithSegments("/_content"))
        {
            await next();
            return;
        }

        context.Response.Redirect("/installation");
    });
}

#region Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseLocalize();

app.UseStaticFiles();
app.UsePluginz();
app.UseDefaultFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseFlatmate<Tenant>();

app.Use(async (context, next) =>
{
    var tenant = context.GetTenant<Tenant>();
    if (tenant is { SslEnabled: true } && !context.Request.IsHttps)
    {
        string target = !string.IsNullOrWhiteSpace(tenant.SecureUrl)
            ? tenant.GetPublicBaseUrl(null) + context.Request.PathBase + context.Request.Path + context.Request.QueryString
            : "https://" + context.Request.Host + context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect(target);
        return;
    }

    await next();
});

app.UseInferno();

if (databaseInstalled)
{
    app.UseSecurityHeaders();
    app.UseKnownTenant();
    app.UseMediaGuard();

    // The blog is optional per tenant. A tenant that has switched it off has no /blog pages or feeds.
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/blog")
            && !context.RequestServices.GetRequiredService<Inferno.Web.Configuration.SiteSettings>().EnableBlog)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next();
    });

    app.MapBlogKrafter();
}

if (databaseInstalled)
{
    app.Use(HangfireDashboardAuthorizationFilter.RedirectUnlessAllowed);
    app.MapHangfireDashboard(ScheduledJobs.DashboardPath, new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorizationFilter()],
        AppPath = "/admin",
        DashboardTitle = "Scheduled tasks",
    });
    app.MapGet("/admin/scheduled-tasks", () => Results.Redirect(ScheduledJobs.DashboardPath));
    app.MapGet("/admin/scheduled-tasks/index", () => Results.Redirect(ScheduledJobs.DashboardPath));
}

app.MapIdentityNexus().RequireAuthorization(InfernoWebPolicies.MembershipManage);
// The log belongs to the whole application and its entries are not tagged by tenant, so only the host administrator may read it.
app.MapLogVue().RequireAuthorization(StandardPolicies.FullAccess);
app.MapLocalize();
app.MapKameleon();
app.MapGet("/admin/configuration/themes/index", () => Results.Redirect("/admin/themes"));
app.MapControllers();
app.MapPluginz();
app.MapGet("/admin/plugins", () => Results.Redirect("/pluginz"));
app.MapGet("/admin/plugins/index", () => Results.Redirect("/pluginz"));
app.MapAreaControllerRoute("admin_route", "Admin", "Admin/{controller}/{action}/{id?}");
app.MapRazorPages();
app.UsePickstack();
// Endpoint routing only serves Blazor pages from assemblies it is told about.
// The interactive router discovers the same marker assemblies while navigating.
var routerAssemblies = DependoResolver.Instance
    .ResolveAll<IRouterAssemblyMarker>()
    .Select(marker => marker.GetType().Assembly)
    .Where(assembly => assembly != typeof(InfernoCMS.App).Assembly)
    .Append(typeof(Languages).Assembly)
    .Append(typeof(Plugins).Assembly)
    .Append(KameleonComponents.Assembly)
    .Concat(databaseInstalled ? [typeof(BlogKrafterComponents).Assembly] : [])
    .Concat(app.Services.GetRequiredService<IPluginWebAssemblies>().Assemblies)
    .Distinct()
    .ToArray();

app.MapRazorComponents<InfernoCMS.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(routerAssemblies);

#endregion Pipeline

app.Run();