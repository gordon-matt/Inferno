﻿using Extenso.Data.Entity;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Services
{
    public class MembershipService : IdentityMembershipService
    {
        public MembershipService(
            IDbContextFactory contextFactory,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IRepository<UserProfileEntry> userProfileRepository)
            : base(contextFactory, userManager, roleManager, userProfileRepository)
        {
        }
    }
}