using Inferno.Identity;
using Inferno.Identity.Entities;
using InfernoCMS.Data;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Identity
{
    public class ApplicationUserStore : ApplicationUserStore<ApplicationUser>
    {
        public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }

        //public override async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ThrowIfDisposed();
        //    return await Users
        //        .Include(x => x.Claims)
        //        .FirstOrDefaultAsync(u =>
        //            u.Id == userId
        //            && (u.TenantId == TenantId || (u.TenantId == null)),
        //        cancellationToken);
        //}

        //public override async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ThrowIfDisposed();
        //    return await Users
        //        .Include(x => x.Claims)
        //        .FirstOrDefaultAsync(u =>
        //            u.NormalizedEmail == normalizedEmail
        //            && (u.TenantId == TenantId || (u.TenantId == null)),
        //        cancellationToken);
        //}

        //public override async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ThrowIfDisposed();
        //    return await Users
        //        .Include(x => x.Claims)
        //        .FirstOrDefaultAsync(u =>
        //            u.NormalizedUserName == normalizedUserName
        //            && (u.TenantId == TenantId || (u.TenantId == null)),
        //        cancellationToken);
        //}
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