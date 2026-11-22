using BlogKrafter.Infrastructure;
using Inferno.Web;
using PageKrafter.Files;

namespace InfernoCMS.Infrastructure;

/// <summary>Opens BlogKrafter and PageKrafter file pickers on the current tenant's folder.</summary>
/// <param name="workContext">Supplies the current tenant's upload folder.</param>
public sealed class InfernoBlogFileRootProvider(IWorkContext workContext) : IBlogFileRootProvider, IFileRootProvider
{
    /// <summary>Returns the current tenant's upload folder.</summary>
    /// <returns>The current tenant's media folder name, or an empty string when there is no current tenant.</returns>
    public string? GetFileRoot() => workContext.TenantRoot;
}
