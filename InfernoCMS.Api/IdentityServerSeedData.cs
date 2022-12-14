using System.Security.Claims;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using InfernoCMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Api
{
    internal class IdentityServerSeedData
    {
        internal static void EnsureSeedData(WebApplication app)
        {
            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
                EnsureSeedData(context);
            }
        }

        internal static void EnsureSeedData(ApplicationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in IdentityServerConfig.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in IdentityServerConfig.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in IdentityServerConfig.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in IdentityServerConfig.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.IdentityProviders.Any())
            {
                context.IdentityProviders.Add(new OidcProvider
                {
                    Scheme = "demoidsrv",
                    DisplayName = "IdentityServer",
                    Authority = "https://demo.duendesoftware.com",
                    ClientId = "login",
                }.ToEntity());
                context.SaveChanges();
            }
        }
    }
}