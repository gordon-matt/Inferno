using Dependo;
using Dependo.Autofac;
using Flatmate;
using Inferno.Identity;
using Inferno.Plugins;
using Inferno.Tenants.Entities;
using Inferno.Tenants;
using Inferno.Web.ContentManagement.Security;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Navigation;
using IdentityNexus;
using Inferno.Web.Areas.Admin;
using LogVue;
using LogVue.Data.Sql;
using Inferno.Web.Infrastructure;
using Inferno.Web.Localization;
using Localize.Components;
using Localize.Components.Pages;
using Inferno.Web.Security;
using Inferno.Web.Tenants;
using InfernoCMS.Areas.Identity;
using InfernoCMS.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Pickstack;
using Pickstack.FileManager.WebFilez;
using Pickstack.RichTextEditor.TinyMce7;

ExcelPackage.License.SetNonCommercialOrganization("Inferno");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new DependoAutofacServiceProviderFactory());

#region Services

var services = builder.Services;
var configuration = builder.Configuration;

var installedSettings = Inferno.Web.Infrastructure.DataSettingsManager.LoadSettings();
string configuredConnection = configuration.GetConnectionString("DefaultConnection");
bool databaseInstalled = !string.IsNullOrWhiteSpace(installedSettings.ConnectionString)
    || !string.IsNullOrWhiteSpace(configuredConnection);
string connectionString = !string.IsNullOrWhiteSpace(installedSettings.ConnectionString)
    ? installedSettings.ConnectionString
    : configuredConnection;

if (databaseInstalled)
{
    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
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
if (!string.IsNullOrWhiteSpace(connectionString))
{
    services.AddLogVueSqlServer(connectionString);
}
services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Inferno CMS";
    shell.BrandHref = "/admin";
    shell.LogoUrl = "img/logo.png";
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
});

// After every AddNetForgeComponents call, which registers the static BuildMenu catalog.
services.Replace(ServiceDescriptor.Scoped<INetForgeMenuCatalog, AdminMenuCatalog>());

services.ScopeUsersToTenant<ApplicationUser, int?>(user => user.TenantId, _ => TenantKey.OfNullable((int?)CurrentTenantId.Resolve()));
services.ScopeRolesToTenant<ApplicationRole, int?>(role => role.TenantId, _ => TenantKey.OfNullable((int?)CurrentTenantId.Resolve()));

services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddInfernoJwtBearer(configuration);

services.AddInfernoAuthorization(configuration);

// Register CMS-specific authorization policies (Blog, Pages, Menus, etc.). These are
// referenced from CMS OData controllers but aren't part of the core Identity module,
// so they're appended here via post-configuration of AuthorizationOptions.
services.Configure<AuthorizationOptions>(options => options.AddInfernoCmsPolicies());

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

services.AddCmsLocalization(databaseInstalled ? connectionString : null);

services
    .AddControllersWithViews()
    .AddInfernoApplicationParts()
    .AddInfernoPlugins(configuration, builder.Environment);

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
    .AddTinyMce7(editor => editor
        .WithHeight(400)
        .WithToolbar("undo redo | bold italic underline | alignleft aligncenter alignright | bullist numlist | link image media | code"));

services.AddHttpContextAccessor();

services.AddHttpClient();

services.ConfigureInferno(configuration);

#endregion Services

var app = builder.Build();

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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Themes")),
    RequestPath = "/Themes"
});
app.UseInfernoPlugins();
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

app.MapIdentityNexus().RequireAuthorization(InfernoWebPolicies.MembershipManage);
app.MapLogVue().RequireAuthorization(InfernoWebPolicies.LogRead);
app.MapLocalize();
app.MapControllers();
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
    .Distinct()
    .ToArray();

app.MapRazorComponents<InfernoCMS.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(routerAssemblies);

#endregion Pipeline

app.Run();