﻿using Inferno.Identity.Entities;

namespace InfernoCMS.Data.Entities
{
    public class ApplicationRole : InfernoIdentityRole
    {
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}