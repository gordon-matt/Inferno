using System.Configuration;
using Autofac.Core;
using Dependo.Autofac;
using Duende.IdentityServer.Models;
using Extenso.AspNetCore.OData;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using InfernoCMS.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using OData.Swagger.Services;

namespace InfernoCMS.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseServiceProviderFactory(new DependableAutofacServiceProviderFactory());

            // Add services to the container.

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<ApplicationUserStore>()
            .AddRoleStore<ApplicationRoleStore>()
            //.AddRoleValidator<ApplicationRoleValidator>()
            .AddDefaultTokenProviders();

            string migrationsAssembly = typeof(ApplicationDbContext).Assembly.GetName().Name;

            builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.EmitStaticAudienceClaim = true;
            })
            //.AddConfigurationStore(storeOptions =>
            //    storeOptions.ConfigureDbContext = b =>
            //        b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), options => options.MigrationsAssembly(migrationsAssembly)))
            //.AddOperationalStore(storeOptions =>
            //    storeOptions.ConfigureDbContext = b =>
            //        b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), options => options.MigrationsAssembly(migrationsAssembly)))
            .AddConfigurationStore<ApplicationDbContext>(options =>
                options.ConfigureDbContext = b => b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")))
            .AddOperationalStore<ApplicationDbContext>(options =>
                options.ConfigureDbContext = b => b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")))
            .AddServerSideSessions()
            .AddAspNetIdentity<ApplicationUser>();

            builder.Services.AddInfernoLocalization();

            builder.Services.AddControllers()
                .AddOData((options, serviceProvider) =>
                {
                    options.Select().Expand().Filter().OrderBy().SetMaxTop(null).Count();

                    var registrars = serviceProvider.GetRequiredService<IEnumerable<IODataRegistrar>>();
                    foreach (var registrar in registrars)
                    {
                        registrar.Register(options);
                    }
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddOdataSwaggerSupport();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Use odata route debug, /$odata
            app.UseODataRouteDebug();

            // If you want to use /$openapi, enable the middleware.
            //app.UseODataOpenApi();

            // Add OData /$query middleware
            app.UseODataQueryRequest();

            // Add the OData Batch middleware to support OData $Batch
            //app.UseODataBatching();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapControllers();

            IdentityServerSeedData.EnsureSeedData(app);

            app.Run();
        }
    }
}