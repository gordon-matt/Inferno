using System.Security.Principal;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;

namespace Inferno.Web.ContentManagement.Infrastructure;

/// <summary>Adds items to the public menu from a source other than stored pages.</summary>
public interface IAutoMenuProvider
{
    /// <summary>
    /// URL prefix that turns on <see cref="GetSubMenuItems"/>. An empty value adds no submenu from the current slug.
    /// </summary>
    string RootUrlSlug { get; }

    /// <summary>Items to add to the main menu.</summary>
    /// <param name="user">The current visitor.</param>
    /// <returns>The items. They are ordered with the page links by position and then text.</returns>
    IEnumerable<MenuItem> GetMainMenuItems(IPrincipal user);

    /// <summary>Items to add when the current slug starts with <see cref="RootUrlSlug"/>.</summary>
    /// <param name="currentUrlSlug">The current page slug, without a leading slash.</param>
    /// <param name="user">The current visitor.</param>
    /// <returns>The submenu items.</returns>
    IEnumerable<MenuItem> GetSubMenuItems(string currentUrlSlug, IPrincipal user);
}