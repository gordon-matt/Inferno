using BlogKrafter.Infrastructure;
using Inferno.Web;
using PageKrafter.Files;

namespace InfernoCMS.Infrastructure;

/// <summary>Opens BlogKrafter and PageKrafter file pickers on the current tenant's folder.</summary>
public sealed class InfernoBlogFileRootProvider(IWorkContext workContext) : IBlogFileRootProvider, IFileRootProvider
{
    public string? GetFileRoot() => workContext.TenantRoot;
}
