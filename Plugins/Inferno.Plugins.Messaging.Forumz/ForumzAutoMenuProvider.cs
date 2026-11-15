using System.Security.Principal;
using Dependo;
using Forumz;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Adds Forums to the public menu. A tenant that wants the forum's own top bar gets a new tab.
/// </summary>
public sealed class ForumzAutoMenuProvider : IAutoMenuProvider
{
    /// <inheritdoc />
    public string RootUrlSlug => string.Empty;

    /// <inheritdoc />
    public IEnumerable<MenuItem> GetMainMenuItems(IPrincipal user)
    {
        bool ownTopBar = OwnTopBar();
        return
        [
            new MenuItem
            {
                Id = new Guid("8f4c2a1b-6d3e-4f90-9c1a-2b7e5d0a4c18"),
                Text = "Forums",
                Url = "/forums",
                Enabled = true,
                Position = 1,
                IsExternalUrl = ownTopBar,
                OpenInNewTab = ownTopBar,
            },
        ];
    }

    private static bool OwnTopBar()
    {
        var http = DependoResolver.Instance.Resolve<IHttpContextAccessor>();
        return http.HttpContext?.RequestServices.GetService<IForumPresentation>()?.UseOwnTopBar == true;
    }

    /// <inheritdoc />
    public IEnumerable<MenuItem> GetSubMenuItems(string currentUrlSlug, IPrincipal user) => [];
}
