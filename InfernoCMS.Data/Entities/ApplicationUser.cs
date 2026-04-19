using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Data.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : InfernoIdentityUser
    {
        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
    }
}