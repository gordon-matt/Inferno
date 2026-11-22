namespace Inferno.Web.Navigation;

/// <summary>One entry in a menu, including its children.</summary>
public class MenuItem
{
    /// <summary>The text shown for the item. Also the value used when two items are compared.</summary>
    public string Text { get; set; }

    //public string Description { get; set; }

    /// <summary>The URL before <see cref="INavigationManager.GetUrl"/> turns it into <see cref="Href"/>.</summary>
    public string Url { get; set; }

    /// <summary>The href written while the menu is finished. Empty until then.</summary>
    public string Href { get; set; }

    /// <summary>The sort position. See <see cref="INavigationManager.BuildMenu"/> for how positions are ordered.</summary>
    public string Position { get; set; }

    /// <summary>Set when <see cref="Href"/> equals the request origin.</summary>
    public bool Selected { get; set; }

    /// <summary>Child items. <see langword="null"/> until a builder assigns them.</summary>
    public IEnumerable<MenuItem> Items { get; set; }

    /// <summary>Policies required to show the item. Empty when none are required.</summary>
    public IEnumerable<string> Policies { get; set; } = Enumerable.Empty<string>();

    /// <summary>The CSS class.</summary>
    public string CssClass { get; set; }

    /// <summary>The icon name.</summary>
    public string IconName { get; set; }

    /// <summary>Whether the link leaves the site. Joining items sets this when any of them is external.</summary>
    public bool IsExternal { get; set; }

    /// <summary>Returns <see cref="Text"/>.</summary>
    /// <returns>The item text.</returns>
    public override string ToString() => Text;
}