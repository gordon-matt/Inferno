using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Data.Entities;

/// <summary>An application role, with its user links and claims.</summary>
public class ApplicationRole : InfernoIdentityRole
{
    /// <summary>Users associated with this role.</summary>
    public virtual ICollection<ApplicationUser> Users { get; set; }

    /// <summary>Claims stored on this role, including permission grants.</summary>
    public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; }
}