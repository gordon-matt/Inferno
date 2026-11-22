namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>A published page as the public site renders it.</summary>
public sealed class PublishedPage
{
    /// <summary>The page title.</summary>
    public string Title { get; init; }

    /// <summary>The title used in the document head.</summary>
    public string MetaTitle { get; init; }

    /// <summary>The meta keywords.</summary>
    public string MetaKeywords { get; init; }

    /// <summary>The meta description.</summary>
    public string MetaDescription { get; init; }

    /// <summary>The HTML shown to visitors.</summary>
    public string BodyContent { get; init; }
}
