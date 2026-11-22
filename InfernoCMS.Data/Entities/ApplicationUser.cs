using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Data.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class
/// <summary>The application account, including its role links and claims.</summary>
public class ApplicationUser : InfernoIdentityUser
{
    /// <summary>Role membership rows for this user.</summary>
    public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

    /// <summary>Claims stored directly on this user.</summary>
    public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
}