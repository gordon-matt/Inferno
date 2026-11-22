namespace Inferno.Web.Navigation;

/// <summary>One step in <see cref="IWorkContext.Breadcrumbs"/>.</summary>
public class Breadcrumb
{
    /// <summary>The text shown for this step.</summary>
    public string Text { get; set; }

    /// <summary>The address of this step. <see langword="null"/> when the step is not a link.</summary>
    public string Url { get; set; }

    /// <summary>The icon class for this step.</summary>
    public string Icon { get; set; }
}