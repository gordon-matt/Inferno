using Inferno.Identity;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity;

/// <summary>Validates <see cref="ApplicationRole"/> names with the tenant-scoped rules from <see cref="InfernoRoleValidator{TRole}"/>.</summary>
public class ApplicationRoleValidator : InfernoRoleValidator<ApplicationRole>
{
    /// <summary>Creates a validator that uses <paramref name="errors"/> to describe failures.</summary>
    /// <param name="errors">The error describer. <see langword="null"/> lets the base validator create one.</param>
    public ApplicationRoleValidator(IdentityErrorDescriber errors = null)
        : base(errors)
    {
    }
}