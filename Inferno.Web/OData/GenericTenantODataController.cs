﻿using Dependo;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Security;
using Inferno.Tenants.Entities;

namespace Inferno.Web.OData
{
    public abstract class GenericTenantODataController<TEntity, TKey> : BaseODataController<TEntity, TKey>
        where TEntity : BaseEntity<TKey>, ITenantEntity
    {
        private readonly IWorkContext workContext;

        #region Constructors

        public GenericTenantODataController(IRepository<TEntity> repository)
            : base(repository)
        {
            workContext = EngineContext.Current.Resolve<IWorkContext>();
        }

        #endregion Constructors

        #region GenericODataController<TEntity, TKey> Members

        protected override async Task<IQueryable<TEntity>> ApplyMandatoryFilterAsync(IQueryable<TEntity> query)
        {
            int tenantId = GetTenantId();
            if (await AuthorizeAsync(StandardPolicies.FullAccess))
            {
                // TODO: Not sure if this is the best solution. Maybe we should only show the items with NULL for Tenant ID?
                return query.Where(x => x.TenantId == null || x.TenantId == tenantId);
            }
            return query.Where(x => x.TenantId == tenantId);
        }

        #endregion GenericODataController<TEntity, TKey> Members

        protected virtual int GetTenantId() => workContext.CurrentTenant.Id;

        protected override async Task<bool> CanViewEntity(TEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (await AuthorizeAsync(StandardPolicies.FullAccess))
            {
                return true; // Only the super admin should have full access
            }

            // If not admin user, but possibly the tenant user...

            if (await AuthorizeAsync(ReadPermission))
            {
                int tenantId = GetTenantId();
                return entity.TenantId == tenantId;
            }

            return false;
        }

        protected override async Task<bool> CanModifyEntity(TEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (await AuthorizeAsync(StandardPolicies.FullAccess))
            {
                return true; // Only the super admin should have full access
            }

            // If not admin user, but possibly the tenant...

            if (await AuthorizeAsync(WritePermission))
            {
                int tenantId = GetTenantId();
                return entity.TenantId == tenantId;
            }

            return false;
        }

        protected override void OnBeforeSave(TEntity entity)
        {
            base.OnBeforeSave(entity);
            entity.TenantId = GetTenantId();
        }
    }
}