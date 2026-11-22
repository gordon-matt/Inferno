using System.Xml.Serialization;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;

[XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
/// <summary>A sitemap urlset written for one tenant.</summary>
public class SitemapXmlFile
{
    /// <summary>Starts with an empty <see cref="Urls"/> set.</summary>
    public SitemapXmlFile()
    {
        Urls = [];
    }

    /// <summary>The URL entries.</summary>
    [XmlElement("url")]
    public HashSet<UrlElement> Urls { get; set; }
}

/// <summary>One URL in a <see cref="SitemapXmlFile"/>.</summary>
public class UrlElement
{
    /// <summary>Starts with an empty <see cref="Links"/> list.</summary>
    public UrlElement()
    {
        Links = [];
    }

    /// <summary>The absolute page URL.</summary>
    [XmlElement("loc")]
    public string Location { get; set; }

    /// <summary>When the page version was last saved, written as yyyy-MM-dd.</summary>
    [XmlElement("lastmod")]
    public string LastModified { get; set; }

    /// <summary>How often the URL is expected to change.</summary>
    [XmlElement("changefreq")]
    public ChangeFrequency ChangeFrequency { get; set; }

    /// <summary>The sitemap priority.</summary>
    [XmlElement("priority")]
    public float Priority { get; set; }

    /// <summary>Alternate-language links. <see langword="null"/> when the site has a single language.</summary>
    [XmlElement("link", Namespace = "http://www.w3.org/1999/xhtml")]
    public List<LinkElement> Links { get; set; }
}

/// <summary>An alternate-language link on a <see cref="UrlElement"/>.</summary>
public class LinkElement
{
    /// <summary>The link relation. Generated links use alternate.</summary>
    [XmlAttribute("rel")]
    public string Rel { get; set; }

    /// <summary>The language of <see cref="Href"/>.</summary>
    [XmlAttribute("hreflang")]
    public string HrefLang { get; set; }

    /// <summary>The absolute URL for <see cref="HrefLang"/>.</summary>
    [XmlAttribute("href")]
    public string Href { get; set; }
}