using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Inferno.Security;
using Inferno.Tenants.Entities;
using Inferno.Web.Infrastructure;
using Inferno.Web.Security;
using Inferno.Web.Tenants;
using InfernoCMS.Areas.Identity;
using InfernoCMS.Identity;
using InfernoCMS.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InfernoCMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

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
                .AddInfernoJwtBearer(Configuration);


            services.AddInfernoAuthorization(Configuration);

            #endregion Account / Identity

            services.AddSingleton(Configuration);

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

            services.AddRouting((routeOptions) =>
            {
                routeOptions.AppendTrailingSlash = true;
                routeOptions.LowercaseUrls = true;
            });

            services.AddMultitenancy<Tenant, InfernoTenantResolver>();

            services.AddInfernoLocalization();

            services.AddControllersWithViews();
            services.AddRazorPages().AddNewtonsoftJson();

            services.AddServerSideBlazor();

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddResponseCompression();

            ////  TODO: Will this work for Blazor?? For themes..
            //services.Configure<RazorViewEngineOptions>(options =>
            //{
            //    options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
            //});

            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddBlazorise(options =>
            {
                options.Immediate = true; // optional
            })
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();

            services.AddResponsiveFileManager(options =>
            {
                options.MaxSizeUpload = 32;
            });

            services.AddHttpContextAccessor();

            services.AddHttpClient();

            services.ConfigureInferno(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
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

            var requestLocalizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(requestLocalizationOptions.Value);

            app.UseSession();

            app.UseStaticFiles();
            app.UseDefaultFiles(); // For PeachPie

            // PeachPie / Responsive File Manager
            app.UseResponsiveFileManager();

            app.UseRouting();

            app.UseAuthentication();
            //app.UseIdentityServer();
            app.UseAuthorization();

            app.UseMultitenancy<Tenant>();

            app.UseInferno();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapAreaControllerRoute("admin_route", "Admin", "Admin/{controller}/{action}/{id?}");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}