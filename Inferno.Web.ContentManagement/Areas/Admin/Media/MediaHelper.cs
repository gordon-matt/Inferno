using System.Text.RegularExpressions;

namespace Inferno.Web.ContentManagement.Areas.Admin.Media
{
    public static partial class MediaHelper
    {
        // TinyMCE does not add a "/" at the start of a URL and as such, the relative URLs will sometimes not work
        public static string EnsureCorrectUrls(string html)
        {
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
}