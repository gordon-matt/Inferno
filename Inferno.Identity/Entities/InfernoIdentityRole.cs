using System.Runtime.Serialization;
using Inferno.Tenants.Entities;
using Microsoft.AspNetCore.Identity;

namespace Inferno.Identity.Entities
{
    public abstract class InfernoIdentityRole : IdentityRole, ITenantEntity
    {
        public InfernoIdentityRole()
            : base()
        {
        }

        public InfernoIdentityRole(string roleName)
            : base(roleName)
        {
        }

        public int? TenantId { get; set; }

        #region IEntity Members

        [IgnoreDataMember] // OData v8 does not like this property and will break if we don't use [IgnoreDataMember] here.
        public object[] KeyValues
        {
            get { return new object[] { Id }; }
        }

        #endregion IEntity Members
    }
}