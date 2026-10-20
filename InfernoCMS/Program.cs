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
using Pickstack;
using Pickstack.FileManager.WebFilez;
using Pickstack.RichTextEditor.TinyMce7;

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

app.UseStaticFiles();
app.UseInfernoPlugins();
app.UseDefaultFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultitenancy<Tenant>();

app.UseInferno();

app.MapControllers();
app.MapAreaControllerRoute("admin_route", "Admin", "Admin/{controller}/{action}/{id?}");
app.UsePickstack();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

#endregion Pipeline

app.Run();