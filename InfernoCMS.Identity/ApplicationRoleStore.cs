using Inferno.Identity;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
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