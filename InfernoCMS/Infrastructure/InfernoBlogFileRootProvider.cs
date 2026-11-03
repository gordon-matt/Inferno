using BlogKrafter.Infrastructure;
using Inferno.Web;

namespace InfernoCMS.Infrastructure;

/// <summary>Opens the blog's file picker on the current tenant's folder, like the Media page and the page editor do.</summary>
public sealed class InfernoBlogFileRootProvider(IWorkContext workContext) : IBlogFileRootProvider
{
    public string GetFileRoot() => workContext.TenantRoot;
}
