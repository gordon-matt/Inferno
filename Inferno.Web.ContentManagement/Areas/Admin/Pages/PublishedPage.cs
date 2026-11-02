namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>A published page as the public site renders it.</summary>
public sealed class PublishedPage
{
    public string Title { get; init; }

    public string MetaTitle { get; init; }

    public string MetaKeywords { get; init; }

    public string MetaDescription { get; init; }

    public string BodyContent { get; init; }
}
