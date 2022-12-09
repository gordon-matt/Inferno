using Inferno.Identity;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity
{
    public class ApplicationRoleStore : InfernoRoleStore<ApplicationRole, ApplicationDbContext>
    {
        public ApplicationRoleStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }
    }
}