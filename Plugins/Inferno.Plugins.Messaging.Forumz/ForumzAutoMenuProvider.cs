using System.Security.Principal;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Infrastructure;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Adds Forums to the public menu. The address is a full page load because the forum is a separate Blazor document.
/// </summary>
public sealed class ForumzAutoMenuProvider : IAutoMenuProvider
{
    /// <inheritdoc />
    public string RootUrlSlug => string.Empty;

    /// <inheritdoc />
    public IEnumerable<MenuItem> GetMainMenuItems(IPrincipal user) =>
    [
        new MenuItem
        {
            Id = new Guid("8f4c2a1b-6d3e-4f90-9c1a-2b7e5d0a4c18"),
            Text = "Forums",
            Url = "/forums",
            Enabled = true,
            Position = 1,
            IsExternalUrl = true,
        },
    ];

    /// <inheritdoc />
    public IEnumerable<MenuItem> GetSubMenuItems(string currentUrlSlug, IPrincipal user) => [];
}
