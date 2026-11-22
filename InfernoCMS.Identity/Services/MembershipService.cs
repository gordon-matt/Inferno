using Extenso.Data.Entity;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity.Services;

/// <inheritdoc cref="IdentityMembershipService" />
public class MembershipService : IdentityMembershipService
{
    /// <summary>Passes the Identity services through to <see cref="IdentityMembershipService"/>.</summary>
    /// <param name="contextFactory">Opens a context for the role-link and user queries.</param>
    /// <param name="userManager">Creates, updates, and deletes users.</param>
    /// <param name="roleManager">Creates, updates, and deletes roles.</param>
    /// <param name="userProfileRepository">Reads and writes profile entries.</param>
    public MembershipService(
        IDbContextFactory contextFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IRepository<UserProfileEntry> userProfileRepository)
        : base(contextFactory, userManager, roleManager, userProfileRepository)
    {
    }
}