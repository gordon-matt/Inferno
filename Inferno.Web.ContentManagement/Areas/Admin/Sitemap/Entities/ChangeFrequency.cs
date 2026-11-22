using System.Xml.Serialization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;

/// <summary>How often a sitemap URL is expected to change. The XML names are the sitemap protocol values.</summary>
public enum ChangeFrequency : byte
{
    /// <summary>The always value. Written as <c>always</c>.</summary>
    [XmlEnum("always")]
    Always = 0,

    /// <summary>The hourly value. Written as <c>hourly</c>.</summary>
    [XmlEnum("hourly")]
    Hourly = 1,

    /// <summary>The daily value. Written as <c>daily</c>.</summary>
    [XmlEnum("daily")]
    Daily = 2,

    /// <summary>The weekly value. Written as <c>weekly</c>. New rows start here.</summary>
    [XmlEnum("weekly")]
    Weekly = 3,

    /// <summary>The monthly value. Written as <c>monthly</c>.</summary>
    [XmlEnum("monthly")]
    Monthly = 4,

    /// <summary>The yearly value. Written as <c>yearly</c>.</summary>
    [XmlEnum("yearly")]
    Yearly = 5,

    /// <summary>The never value. Written as <c>never</c>.</summary>
    [XmlEnum("never")]
    Never = 6
}