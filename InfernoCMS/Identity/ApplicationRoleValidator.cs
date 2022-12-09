﻿using Inferno.Identity;
using Microsoft.AspNetCore.Identity;

namespace InfernoCMS.Identity
{
    public class ApplicationRoleValidator : InfernoRoleValidator<ApplicationRole>
    {
        public ApplicationRoleValidator(IdentityErrorDescriber errors = null)
            : base(errors)
        {
        }
    }
}