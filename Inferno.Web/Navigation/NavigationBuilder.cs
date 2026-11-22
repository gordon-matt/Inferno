namespace Inferno.Web.Navigation;

/// <summary>Collects <see cref="MenuItem"/> instances for one provider.</summary>
public class NavigationBuilder
{
    private IEnumerable<MenuItem> Contained { get; set; }

    /// <summary>
    /// Adds an item with <paramref name="caption"/> and <paramref name="position"/>, then lets <paramref name="itemBuilder"/> fill it.
    /// A non-empty <paramref name="cssClass"/> replaces any class the callback set.
    /// </summary>
    /// <param name="caption">The item text.</param>
    /// <param name="position">The sort position. <see langword="null"/> is allowed.</param>
    /// <param name="itemBuilder">Fills the new item. It is not optional.</param>
    /// <param name="cssClass">A class applied after the callback. <see langword="null"/> or empty leaves the callback's class.</param>
    /// <returns>This builder.</returns>
    public NavigationBuilder Add(
        string caption,
        string position,
        Action<NavigationItemBuilder> itemBuilder,
        string cssClass = null)
    {
        var childBuilder = new NavigationItemBuilder();

        childBuilder.Caption(caption);
        childBuilder.Position(position);
        itemBuilder(childBuilder);
        Contained = (Contained ?? Enumerable.Empty<MenuItem>()).Concat(childBuilder.Build());

        if (!string.IsNullOrEmpty(cssClass))
        {
            childBuilder.CssClass(cssClass);
        }

        return this;
    }

    /// <summary>Appends <paramref name="menuItem"/> as it is.</summary>
    /// <param name="menuItem">The item to add.</param>
    /// <returns>This builder.</returns>
    public NavigationBuilder Add(MenuItem menuItem)
    {
        Contained = (Contained ?? Enumerable.Empty<MenuItem>()).Concat(new[] { menuItem });
        return this;
    }

    /// <summary>
    /// Adds an item and passes <paramref name="parent"/> into the callback.
    /// A non-empty <paramref name="cssClass"/> replaces any class the callback set.
    /// </summary>
    /// <typeparam name="TParent">The value handed to the callback.</typeparam>
    /// <param name="caption">The item text.</param>
    /// <param name="position">The sort position. <see langword="null"/> is allowed.</param>
    /// <param name="itemBuilder">Fills the new item. It receives this item's builder and <paramref name="parent"/>.</param>
    /// <param name="parent">The value passed to <paramref name="itemBuilder"/>.</param>
    /// <param name="cssClass">A class applied after the callback. <see langword="null"/> or empty leaves the callback's class.</param>
    /// <returns>This builder.</returns>
    public NavigationBuilder Add<TParent>(
        string caption,
        string position,
        Action<NavigationItemBuilder, TParent> itemBuilder,
        TParent parent,
        string cssClass = null)
    {
        var childBuilder = new NavigationItemBuilder();
        childBuilder.Caption(caption);
        childBuilder.Position(position);
        itemBuilder(childBuilder, parent);
        Contained = (Contained ?? Enumerable.Empty<MenuItem>()).Concat(childBuilder.Build());

        if (!string.IsNullOrEmpty(cssClass))
        {
            childBuilder.CssClass(cssClass);
        }

        return this;
    }

    /// <summary>Adds an item with no position.</summary>
    /// <param name="caption">The item text.</param>
    /// <param name="itemBuilder">Fills the new item.</param>
    /// <param name="cssClass">A class applied after the callback.</param>
    /// <returns>This builder.</returns>
    public NavigationBuilder Add(
        string caption,
        Action<NavigationItemBuilder> itemBuilder,
        string cssClass = null) => Add(caption, null, itemBuilder, cssClass);

    /// <summary>Adds an item with no caption and no position.</summary>
    /// <param name="itemBuilder">Fills the new item.</param>
    /// <param name="cssClass">A class applied after the callback.</param>
    /// <returns>This builder.</returns>
    public NavigationBuilder Add(Action<NavigationItemBuilder> itemBuilder, string cssClass = null) => Add(null, null, itemBuilder, cssClass);

    /// <summary>Adds an item that the callback does not change.</summary>
    /// <param name="caption">The item text.</param>
    /// <param name="cssClass">The item class.</param>
    /// <returns>This builder.</returns>
    public NavigationBuilder Add(string caption, string cssClass = null) => Add(caption, null, x => { }, cssClass);

    /// <summary>The items added so far.</summary>
    /// <returns>The items. Empty when none were added.</returns>
    public virtual IEnumerable<MenuItem> Build() => (Contained ?? Enumerable.Empty<MenuItem>()).ToList();
}