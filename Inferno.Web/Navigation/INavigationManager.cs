namespace Inferno.Web.Navigation;

/// <summary>Builds a named menu from the registered providers and turns a menu URL into an href.</summary>
public interface INavigationManager
{
    /// <summary>
    /// Merges every provider whose <see cref="INavigationProvider.MenuName"/> is <paramref name="menuName"/>.
    /// A provider that throws is logged and skipped. Items that compare equal are joined, then ordered by position and text.
    /// Each href is filled from <see cref="GetUrl"/>. An item is selected when that href equals the request origin.
    /// A request with no HTTP context selects nothing.
    /// </summary>
    /// <param name="menuName">The menu to build.</param>
    /// <returns>The merged items.</returns>
    /// <remarks>
    /// Position is compared segment by segment on <c>.</c> and <c>:</c>. The word <c>before</c> sorts first, <c>after</c> sorts last, and an empty position sorts as zero.
    /// </remarks>
    IEnumerable<MenuItem> BuildMenu(string menuName);

    /// <summary>
    /// Returns <paramref name="menuItemUrl"/> unchanged when it is absolute, starts with <c>/</c>, <c>#</c>, or <c>javascript:</c>.
    /// A <c>~/</c> prefix is removed. Any other relative URL is prefixed with the web root.
    /// </summary>
    /// <param name="menuItemUrl">The URL stored on the menu item.</param>
    /// <returns>The href, or <see langword="null"/> when <paramref name="menuItemUrl"/> is null or empty.</returns>
    string GetUrl(string menuItemUrl);
}