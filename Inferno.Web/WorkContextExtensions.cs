namespace Inferno.Web;

/// <summary>Paths derived from the current tenant.</summary>
public static class WorkContextExtensions
{
    extension(IWorkContext workContext)
    {
        /// <summary>The media folder name for the current tenant, or empty when there is no tenant.</summary>
        public string TenantRoot => workContext.CurrentTenant is { } tenant ? $"Tenant_{tenant.Id}" : string.Empty;
    }
}