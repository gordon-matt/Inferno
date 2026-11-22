using Inferno.Identity;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity;

/// <summary>The Identity role store for <see cref="ApplicationRole"/>.</summary>
public class ApplicationRoleStore : InfernoRoleStore<ApplicationRole, ApplicationDbContext>
{
    /// <summary>Creates a store for <see cref="ApplicationRole"/> on <paramref name="context"/>.</summary>
    /// <param name="context">The application database context.</param>
    /// <param name="describer">Describes Identity errors. <see langword="null"/> lets the base store choose the default describer.</param>
    public ApplicationRoleStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
    {
    }
}