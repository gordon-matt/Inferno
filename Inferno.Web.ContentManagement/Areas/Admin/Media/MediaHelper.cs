using System.Text.RegularExpressions;

namespace Inferno.Web.ContentManagement.Areas.Admin.Media;

/// <summary>Repairs media URLs in stored HTML.</summary>
public static partial class MediaHelper
{
    // TinyMCE does not add a "/" at the start of a URL and as such, the relative URLs will sometimes not work
    /// <summary>Prefixes <c>src="Media</c> image addresses with a slash so they resolve from the site root.</summary>
    /// <param name="html">The HTML to scan. <see langword="null"/> or empty is returned unchanged.</param>
    /// <returns>The HTML, with matching image sources rooted.</returns>
    public static string EnsureCorrectUrls(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        string returnHtml = html;

        var matches = ImgTagRegex().Matches(html);

        foreach (Match match in matches)
        {
            string replacement = match.Value.Replace(@"src=""Media", @"src=""/Media");
            returnHtml = Regex.Replace(returnHtml, Regex.Escape(match.Value), replacement);
        }

        return returnHtml;
    }

    [GeneratedRegex("\\<img.*src=\"Media.*/>")]
    private static partial Regex ImgTagRegex();
}