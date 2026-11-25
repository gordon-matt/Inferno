using Dependo;
using Inferno.Web;
using Inferno.Web.Navigation;
using Kourier.Core;
using Microsoft.Extensions.Localization;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>Adds Email to the shared admin menu. The link loads Kourier's own document.</summary>
public sealed class KourierNavigationProvider : INavigationProvider
{
    /// <summary>Resolves the localizer.</summary>
    public KourierNavigationProvider()
    {
        T = DependoResolver.Instance.Resolve<IStringLocalizer>();
    }

    /// <summary>Localizer used for the menu captions.</summary>
    public IStringLocalizer T { get; set; }

    /// <inheritdoc />
    public string MenuName => InfernoWebConstants.Areas.Admin;

    /// <inheritdoc />
    public void GetNavigation(NavigationBuilder builder) =>
        builder.Add(T["Email"].Value, "2", item => item
            .Url("/admin/email")
            .Icon("mail")
            .Permission(KourierPolicies.View));
}