using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Identity;

/// <summary>Rejects a blank role name and a name already used by another role in the same tenant.</summary>
/// <typeparam name="TRole">The role type.</typeparam>
public abstract class InfernoRoleValidator<TRole> : RoleValidator<TRole>
    where TRole : InfernoIdentityRole
{
    /// <summary>Creates a validator that uses <paramref name="errors"/> to describe failures.</summary>
    /// <param name="errors">The error describer. <see langword="null"/> creates a new <see cref="IdentityErrorDescriber"/>.</param>
    public InfernoRoleValidator(IdentityErrorDescriber errors = null)
        : base(errors)
    {
        Describer = errors ?? new IdentityErrorDescriber();
    }

    private IdentityErrorDescriber Describer { get; set; }

    /// <summary>
    /// Accepts the role when its name is not blank and no other role in the same tenant has that name as its normalized name.
    /// </summary>
    /// <param name="manager">The role manager whose roles are searched.</param>
    /// <param name="role">The role to validate. Its tenant selects the scope of the duplicate check.</param>
    /// <returns>A failed result when the name is blank or already used in that tenant. Otherwise success.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="manager"/> or <paramref name="role"/> is <see langword="null"/>.</exception>
    public override async Task<IdentityResult> ValidateAsync(RoleManager<TRole> manager, TRole role)
    {
        if (manager == null)
        {
            throw new ArgumentNullException(nameof(manager));
        }
        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }
        var errors = new List<IdentityError>();
        await ValidateRoleName(manager, role, errors);
        return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    }

    private async Task ValidateRoleName(
        RoleManager<TRole> manager,
        TRole role,
        ICollection<IdentityError> errors)
    {
        string roleName = await manager.GetRoleNameAsync(role);
        if (string.IsNullOrWhiteSpace(roleName))
        {
            errors.Add(Describer.InvalidRoleName(roleName));
        }
        else
        {
            var owner = await manager.Roles.FirstOrDefaultAsync(x => x.TenantId == role.TenantId && x.NormalizedName == roleName);

            if (owner != null &&
                !string.Equals(await manager.GetRoleIdAsync(owner), await manager.GetRoleIdAsync(role)))
            {
                errors.Add(Describer.DuplicateRoleName(roleName));
            }
        }
    }
}