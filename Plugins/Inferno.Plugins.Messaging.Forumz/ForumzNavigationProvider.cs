using Dependo;
using Inferno.Web;
using Inferno.Web.ContentManagement;
using Inferno.Web.Navigation;
using Microsoft.Extensions.Localization;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Adds Forums under Messaging. The address is a full page load because the forum is a separate Blazor document.
/// </summary>
public sealed class ForumzNavigationProvider : INavigationProvider
{
    public ForumzNavigationProvider()
    {
        Localizer = DependoResolver.Instance.Resolve<IStringLocalizer>();
    }

    private IStringLocalizer Localizer { get; }

    public string MenuName => InfernoWebConstants.Areas.Admin;

    public void GetNavigation(NavigationBuilder builder)
    {
        builder.Add(Localizer[InfernoCmsLocalizableStrings.Messaging.Title].Value, "2", menu =>
        {
            menu.Add("Forums", "6", item => item
                .Url("/forums")
                .Icon("chat")
                .IsExternal());
        });
    }
}
