using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Dependo.Autofac;
using Inferno.Plugins;
using Inferno.Tenants.Entities;
using Inferno.Web.ContentManagement.Security;
using Inferno.Web.Infrastructure;
using Inferno.Web.Tenants;
using InfernoCMS.Areas.Identity;
using InfernoCMS.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

ExcelPackage.License.SetNonCommercialOrganization("Inferno");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new DependoAutofacServiceProviderFactory());

#region Services

var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

#region Account / Identity

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/log-off";
    options.AccessDeniedPath = "/account/access-denied";
});

services.AddInfernoIdentity().AddDefaultUI();

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

// Peachpie needs this
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

services.AddCors(options => options.AddPolicy("AllowAll", p => p
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()));

services.AddRouting(routeOptions =>
{
    routeOptions.AppendTrailingSlash = true;
    routeOptions.LowercaseUrls = true;
});

services.AddMultitenancy<Tenant, InfernoTenantResolver>();

services.AddInfernoLocalization();

services
    .AddControllersWithViews()
    .AddInfernoApplicationParts()
    .AddInfernoPlugins(configuration, builder.Environment);

services.AddRazorPages().AddNewtonsoftJson();

services.AddServerSideBlazor();

services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
services.AddResponseCompression();

services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
services.AddDatabaseDeveloperPageExceptionFilter();

services.AddBlazorise(options => options.Immediate = true)
.AddBootstrap5Providers()
.AddFontAwesomeIcons();

services.AddResponsiveFileManager(options => options.MaxSizeUpload = 32);

services.AddHttpContextAccessor();

services.AddHttpClient();

services.ConfigureInferno(configuration);

#endregion Services

var app = builder.Build();

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

var requestLocalizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(requestLocalizationOptions!.Value);

app.UseSession();

app.UseStaticFiles();
app.UseInfernoPlugins();
app.UseDefaultFiles(); // For PeachPie

// PeachPie / Responsive File Manager
app.UseResponsiveFileManager();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultitenancy<Tenant>();

app.UseInferno();

app.MapControllers();
app.MapAreaControllerRoute("admin_route", "Admin", "Admin/{controller}/{action}/{id?}");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

#endregion Pipeline

app.Run();