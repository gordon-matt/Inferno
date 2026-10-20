namespace Inferno.Web;

public static class WorkContextExtensions
{
    extension(IWorkContext workContext)
    {
        public string TenantRoot => workContext.CurrentTenant is { } tenant ? $"Tenant_{tenant.Id}" : string.Empty;
    }
}