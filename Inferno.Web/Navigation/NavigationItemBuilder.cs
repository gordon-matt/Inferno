namespace Inferno.Web.Navigation;

/// <summary>Fills one <see cref="MenuItem"/> and any children added through <see cref="NavigationBuilder"/>.</summary>
public class NavigationItemBuilder : NavigationBuilder
{
    private readonly MenuItem item;

    /// <summary>Starts a new item.</summary>
    public NavigationItemBuilder()
    {
        item = new MenuItem();
    }

    /// <summary>Sets the item text.</summary>
    /// <param name="caption">The text.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder Caption(string caption)
    {
        item.Text = caption;
        return this;
    }

    /// <summary>Sets the sort position.</summary>
    /// <param name="position">The position text.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder Position(string position)
    {
        item.Position = position;
        return this;
    }

    /// <summary>Sets the URL stored on the item. The href is filled later by <see cref="INavigationManager.GetUrl"/>.</summary>
    /// <param name="url">The URL.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder Url(string url)
    {
        item.Url = url;
        return this;
    }

    /// <summary>Sets the CSS class.</summary>
    /// <param name="className">The class.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder CssClass(string className)
    {
        item.CssClass = className;
        return this;
    }

    /// <summary>Sets the icon name.</summary>
    /// <param name="iconName">The icon name.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder Icon(string iconName) => IconName(iconName);

    /// <summary>Sets the icon name.</summary>
    /// <param name="iconName">The icon name.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder IconName(string iconName)
    {
        item.IconName = iconName;
        return this;
    }

    /// <summary>Marks the item as an external link.</summary>
    /// <param name="isExternal"><see langword="false"/> clears the flag. The default sets it.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder IsExternal(bool isExternal = true)
    {
        item.IsExternal = isExternal;
        return this;
    }

    /// <summary>Returns this item, with any added children as its <see cref="MenuItem.Items"/>.</summary>
    /// <returns>A sequence of one item.</returns>
    public override IEnumerable<MenuItem> Build()
    {
        item.Items = base.Build();
        return new[] { item };
    }

    /// <summary>Appends <paramref name="policies"/> to the policies required to see the item.</summary>
    /// <param name="policies">Policy names. They are added to any already set.</param>
    /// <returns>This builder.</returns>
    public NavigationItemBuilder Permission(params string[] policies)
    {
        item.Policies = item.Policies.Concat(policies);
        return this;
    }
}