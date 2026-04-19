using System.Security.Cryptography;
using System.Text;
using Extenso.Data.Entity;
using Inferno.Web.Mvc.Themes;

namespace Inferno.Web.Areas.Admin.Configuration.Themes.Models;

public class EdmThemeConfiguration : BaseEntity<Guid>
{
    public string Title { get; set; }

    public bool SupportRtl { get; set; }

    public string PreviewImageUrl { get; set; }

    public string PreviewText { get; set; }

    public bool IsDefaultTheme { get; set; }

    public static implicit operator EdmThemeConfiguration(ThemeConfiguration other) => new()
    {
        // Themes don't have a real database identity, but OData v4 requires one.
        // A stable Guid derived from the theme name lets the same theme keep the same
        // Id across requests, which avoids confusing the Radzen DataGrid.
        Id = NameToGuid(other.ThemeName),
        Title = other.ThemeName,
        SupportRtl = other.SupportRtl,
        PreviewImageUrl = other.PreviewImageUrl,
        PreviewText = other.PreviewText
    };

    private static Guid NameToGuid(string name)
    {
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(name ?? string.Empty));
        return new Guid(hash);
    }
}