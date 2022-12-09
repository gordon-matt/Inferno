using Inferno.Identity;
using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity
{
    public class ApplicationUserStore : ApplicationUserStore<ApplicationUser>
    {
        public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }
    }

    public abstract class ApplicationUserStore<TUser> : InfernoUserStore<TUser, ApplicationRole, ApplicationDbContext>
        where TUser : InfernoIdentityUser, new()
    {
        public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }
    }
}