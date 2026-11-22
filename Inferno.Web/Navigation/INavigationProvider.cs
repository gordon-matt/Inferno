namespace Inferno.Web.Navigation;

/// <summary>Contributes items to one named menu.</summary>
public interface INavigationProvider
{
    /// <summary>The menu this provider contributes to. Other menus do not call it.</summary>
    string MenuName { get; }

    /// <summary>Adds this provider's items to <paramref name="builder"/>.</summary>
    /// <param name="builder">The builder for this provider's items.</param>
    void GetNavigation(NavigationBuilder builder);
}