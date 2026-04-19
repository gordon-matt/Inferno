using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Data.Entities
{
    public class ApplicationRole : InfernoIdentityRole
    {
        public virtual ICollection<ApplicationUser> Users { get; set; }

        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; }
    }
}