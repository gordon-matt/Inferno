using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Extenso.AspNetCore.OData;
using Inferno.Security;
using Inferno.Tenants.Entities;
using Inferno.Web.Tenants;
using InfernoCMS.Areas.Identity;
using InfernoCMS.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;

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

            //services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/log-off";
                options.AccessDeniedPath = "/account/access-denied";
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<ApplicationUserStore>()
            .AddRoleStore<ApplicationRoleStore>()
            //.AddRoleValidator<ApplicationRoleValidator>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(StandardPolicies.FullAccess, policy => policy.RequireClaim("Permission", "FullAccess"));
            });

            #endregion Account / Identity

            services.AddRouting((routeOptions) =>
            {
                routeOptions.AppendTrailingSlash = true;
                routeOptions.LowercaseUrls = true;
            });

            services.AddMultitenancy<Tenant, InfernoTenantResolver>();

            services.AddInfernoLocalization();

            services.AddControllersWithViews();
            services.AddRazorPages()
                .AddNewtonsoftJson()
                .AddOData((options, serviceProvider) =>
                {
                    options.Select().Expand().Filter().OrderBy().SetMaxTop(null).Count();

                    var registrars = serviceProvider.GetRequiredService<IEnumerable<IODataRegistrar>>();
                    foreach (var registrar in registrars)
                    {
                        registrar.Register(options);
                    }
                });

            services.AddServerSideBlazor();

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

            services.AddHttpContextAccessor();
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
            app.UseStaticFiles();

            // Use odata route debug, /$odata
            app.UseODataRouteDebug();

            // If you want to use /$openapi, enable the middleware.
            //app.UseODataOpenApi();

            // Add OData /$query middleware
            app.UseODataQueryRequest();

            // Add the OData Batch middleware to support OData $Batch
            //app.UseODataBatching();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMultitenancy<Tenant>();

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