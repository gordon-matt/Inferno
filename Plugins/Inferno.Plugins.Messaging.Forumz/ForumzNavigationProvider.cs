using Dependo;
using Forumz;
using Inferno.Web;
using Inferno.Web.ContentManagement;
using Inferno.Web.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Adds Forums under Messaging. A tenant that wants the forum's own top bar opens it in a new tab.
/// </summary>
public sealed class ForumzNavigationProvider : INavigationProvider
{
    public ForumzNavigationProvider()
    {
        Localizer = DependoResolver.Instance.Resolve<IStringLocalizer>();
    }

    private IStringLocalizer Localizer { get; }

    public string MenuName => InfernoWebConstants.Areas.Admin;

    private static bool OwnTopBar()
    {
        var http = DependoResolver.Instance.Resolve<IHttpContextAccessor>();
        return http.HttpContext?.RequestServices.GetService<IForumPresentation>()?.UseOwnTopBar == true;
    }

    public void GetNavigation(NavigationBuilder builder)
    {
        builder.Add(Localizer[InfernoCmsLocalizableStrings.Messaging.Title].Value, "2", menu =>
        {
            menu.Add("Forums", "6", item =>
            {
                item.Url("/forums").Icon("chat");
                if (OwnTopBar())
                {
                    item.IsExternal();
                }
            });
        });
    }
}
