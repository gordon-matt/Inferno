using Dependo;
using Extenso.Data.Entity;
using Inferno.Data.Services;
using Inferno.Security.Membership.Permissions;
using Inferno.Tenants.Entities;

namespace Inferno.Web.OData
{
    public abstract class GenericTenantODataController<TEntity, TKey> : GenericODataController<TEntity, TKey>
        where TEntity : class, ITenantEntity
    {
        private readonly IWorkContext workContext;

        #region Constructors

        public GenericTenantODataController(IGenericDataService<TEntity> service)
            : base(service)
        {
            workContext = EngineContext.Current.Resolve<IWorkContext>();
        }

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
            if (await CheckPermissionAsync(StandardPermissions.FullAccess))
            {
                // TODO: Not sure if this is the best solution. Maybe we should only show the items with NULL for Tenant ID?
                return query.Where(x => x.TenantId == null || x.TenantId == tenantId);
            }
            return query.Where(x => x.TenantId == tenantId);
        }

        #endregion GenericODataController<TEntity, TKey> Members

        protected virtual int GetTenantId()
        {
            return workContext.CurrentTenant.Id;
        }

        protected override async Task<bool> CanViewEntityAsync(TEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (await CheckPermissionAsync(StandardPermissions.FullAccess))
            {
                return true; // Only the super admin should have full access
            }

            // If not admin user, but possibly the tenant user...

            if (await CheckPermissionAsync(ReadPermission))
            {
                int tenantId = GetTenantId();
                return entity.TenantId == tenantId;
            }

            return false;
        }

        protected override async Task<bool> CanModifyEntityAsync(TEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (await CheckPermissionAsync(StandardPermissions.FullAccess))
            {
                return true; // Only the super admin should have full access
            }

            // If not admin user, but possibly the tenant...

            if (await CheckPermissionAsync(WritePermission))
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