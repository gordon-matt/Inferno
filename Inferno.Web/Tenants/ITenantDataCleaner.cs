namespace Inferno.Web.Tenants;

public interface ITenantDataCleaner
{
    Task CleanAsync(int tenantId);
}
