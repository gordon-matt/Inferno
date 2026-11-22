using Inferno.Identity;
using Inferno.Identity.Entities;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity;

/// <summary>The Identity user store for <see cref="ApplicationUser"/>.</summary>
public class ApplicationUserStore : ApplicationUserStore<ApplicationUser>
{
    /// <summary>Creates a store for <see cref="ApplicationUser"/> on <paramref name="context"/>.</summary>
    /// <param name="context">The application database context.</param>
    /// <param name="describer">Describes Identity errors. <see langword="null"/> lets the base store choose the default describer.</param>
    public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
    {
    }
}

/// <summary>An Identity user store for an <see cref="InfernoIdentityUser"/> saved in <see cref="ApplicationDbContext"/>.</summary>
/// <typeparam name="TUser">The user type.</typeparam>
public abstract class ApplicationUserStore<TUser> : InfernoUserStore<TUser, ApplicationRole, ApplicationDbContext>
    where TUser : InfernoIdentityUser, new()
{
    /// <summary>Creates a store for <typeparamref name="TUser"/> on <paramref name="context"/>.</summary>
    /// <param name="context">The application database context.</param>
    /// <param name="describer">Describes Identity errors. <see langword="null"/> lets the base store choose the default describer.</param>
    public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
    {
    }
}