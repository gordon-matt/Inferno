using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;

/// <summary>One sitemap row shown in the admin, matched to a current page version.</summary>
public class SitemapConfigModel
{
    /// <summary>The configuration row identifier.</summary>
    public int Id { get; set; }

    /// <summary>The page version's slug.</summary>
    public string Location { get; set; }

    /// <summary>How often the URL is expected to change.</summary>
    public ChangeFrequency ChangeFrequency { get; set; }

    /// <summary>The sitemap priority, from 0 to 1.</summary>
    public float Priority { get; set; }
}