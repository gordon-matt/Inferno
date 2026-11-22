namespace Inferno.Web.Navigation;

/// <summary>The breadcrumb trail held by <see cref="IWorkContext"/>.</summary>
public class BreadcrumbCollection : List<Breadcrumb>
{
    /// <summary>Appends a step.</summary>
    /// <param name="text">The step text.</param>
    /// <param name="url">The step address. <see langword="null"/> leaves it unlinked.</param>
    /// <param name="iconCssClass">The icon class. <see langword="null"/> leaves it unset.</param>
    public void Add(string text, string url = null, string iconCssClass = null) => Add(new Breadcrumb { Text = text, Url = url, Icon = iconCssClass });

    /// <summary>Inserts a step at <paramref name="index"/>.</summary>
    /// <param name="index">The position. The same rules as <see cref="List{T}.Insert(int, T)"/> apply.</param>
    /// <param name="text">The step text.</param>
    /// <param name="url">The step address. <see langword="null"/> leaves it unlinked.</param>
    /// <param name="iconCssClass">The icon class. <see langword="null"/> leaves it unset.</param>
    public void Insert(int index, string text, string url = null, string iconCssClass = null) => Insert(index, new Breadcrumb { Text = text, Url = url, Icon = iconCssClass });
}