using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Helpers;
using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Web.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Inferno.Web.Areas.Tenants.Controllers.Api;

[Authorize]
public class TenantApiController : BaseODataController<Tenant, int>
{
    private readonly IMembershipService membershipService;
    private readonly ITenantDataCleaner tenantDataCleaner;

    public TenantApiController(
        IAuthorizationService authorizationService,
        IRepository<Tenant> repository,
        IMembershipService membershipService,
        ITenantDataCleaner tenantDataCleaner)
        : base(authorizationService, repository)
    {
        this.membershipService = membershipService;
        this.tenantDataCleaner = tenantDataCleaner;
    }

    public override Task<IActionResult> Get(ODataQueryOptions<Tenant> options, CancellationToken cancellationToken) => base.Get(options, cancellationToken);

    public override async Task<IActionResult> Post([FromBody] Tenant entity, CancellationToken cancellationToken)
    {
        var result = await base.Post(entity, cancellationToken);
        int tenantId = entity.Id; // EF should have populated the ID in base.Post()
        await membershipService.EnsureAdminRoleForTenantAsync(tenantId);

        var mediaFolder = new DirectoryInfo(CommonHelper.MapPath("~/Media/Uploads/Tenant_" + tenantId));
        if (!mediaFolder.Exists)
        {
            mediaFolder.Create();
        }

        return result;
    }

    public override async Task<IActionResult> Delete(int key, CancellationToken cancellationToken)
    {
        await tenantDataCleaner.CleanAsync(key);
        return await base.Delete(key, cancellationToken);
    }

    protected override int GetId(Tenant entity) => entity.Id;

    protected override void SetNewId(Tenant entity)
    {
    }

    protected override string ReadPermission => StandardPolicies.FullAccess;

    protected override string WritePermission => StandardPolicies.FullAccess;
}