using System.Text.RegularExpressions;

namespace Inferno.Web.ContentManagement;

/// <summary>Shared CMS values: area paths, permission names, and the newsletter email pattern.</summary>
public static partial class CmsConstants
{
    /// <summary>Patterns used when the CMS checks visitor input.</summary>
    public static partial class RegexPatterns
    {
        /// <summary>An email address with a two- to four-letter domain suffix. Matching ignores case.</summary>
        public static readonly Regex Email = EmailRegex();

        [GeneratedRegex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex EmailRegex();
    }

    /// <summary>Zone names the public layout renders. A zone is created the first time that layout is shown.</summary>
    public static class LayoutZones
    {
        /// <summary>End of the public layout, for script blocks such as analytics.</summary>
        public const string Scripts = "Scripts";
    }

    /// <summary>Path prefixes of the CMS admin areas.</summary>
    public static class Areas
    {
        /// <summary>The content-blocks admin area.</summary>
        public const string Blocks = "Admin/Blocks";

        /// <summary>The localization admin area.</summary>
        public const string Localization = "Admin/Localization";

        /// <summary>The media admin area.</summary>
        public const string Media = "Admin/Media";

        /// <summary>The messaging admin area.</summary>
        public const string Messaging = "Admin/Messaging";

        /// <summary>The menus admin area.</summary>
        public const string Menus = "Admin/Menus";

        /// <summary>The newsletters admin area.</summary>
        public const string Newsletters = "Admin/Newsletters";

        /// <summary>The pages admin area.</summary>
        public const string Pages = "Admin/Pages";

        /// <summary>The sitemap admin area.</summary>
        public const string Sitemap = "Admin/Sitemap";
    }

    internal static class Tables
    {
        internal const string ContentBlocks = "ContentBlocks";
        internal const string HistoricPages = "HistoricPages";
        internal const string MenuItems = "MenuItems";
        internal const string Menus = "Menus";
        internal const string MessageTemplates = "MessageTemplates";
        internal const string Pages = "Pages";
        internal const string PageVersions = "PageVersions";
        internal const string SitemapConfig = "SitemapConfig";
        internal const string QueuedEmails = "QueuedEmails";
        internal const string QueuedSMS = "QueuedSMS";
        internal const string Zones = "Zones";
    }

    /// <summary>
    /// The CMS permissions. Each value is both a policy name and a stored claim value, so renaming one
    /// orphans existing grants. <see cref="Security.CmsPermissionProvider"/> describes them.
    /// </summary>
    public static class Policies
    {
        /// <summary>View the blog.</summary>
        public const string BlogRead = "Cms.Blog.Read";

        /// <summary>Write blog posts. Implies <see cref="BlogRead"/>.</summary>
        public const string BlogWrite = "Cms.Blog.Write";

        /// <summary>View content blocks.</summary>
        public const string ContentBlocksRead = "Cms.ContentBlocks.Read";

        /// <summary>Edit content blocks. Implies <see cref="ContentBlocksRead"/>.</summary>
        public const string ContentBlocksWrite = "Cms.ContentBlocks.Write";

        /// <summary>View content zones.</summary>
        public const string ContentZonesRead = "Cms.ContentZones.Read";

        /// <summary>Edit content zones. Implies <see cref="ContentZonesRead"/>.</summary>
        public const string ContentZonesWrite = "Cms.ContentZones.Write";

        /// <summary>View media.</summary>
        public const string MediaRead = "Cms.Media.Read";

        /// <summary>Upload and delete media. Implies <see cref="MediaRead"/>.</summary>
        public const string MediaWrite = "Cms.Media.Write";

        /// <summary>View menus.</summary>
        public const string MenusRead = "Cms.Menus.Read";

        /// <summary>Edit menus. Implies <see cref="MenusRead"/>.</summary>
        public const string MenusWrite = "Cms.Menus.Write";

        /// <summary>View newsletter subscribers.</summary>
        public const string NewsletterRead = "Cms.Newsletters.Read";

        /// <summary>Manage newsletter subscribers. Implies <see cref="NewsletterRead"/>.</summary>
        public const string NewsletterWrite = "Cms.Newsletters.Write";

        /// <summary>View page history.</summary>
        public const string PageHistoryRead = "Cms.PageHistory.Read";

        /// <summary>Restore page versions. Implies <see cref="PageHistoryRead"/>.</summary>
        public const string PageHistoryRestore = "Cms.PageHistory.Restore";

        /// <summary>Delete page history. Implies <see cref="PageHistoryRead"/>.</summary>
        public const string PageHistoryWrite = "Cms.PageHistory.Write";

        /// <summary>View pages.</summary>
        public const string PagesRead = "Cms.Pages.Read";

        /// <summary>Edit pages. Implies <see cref="PagesRead"/>.</summary>
        public const string PagesWrite = "Cms.Pages.Write";

        /// <summary>View the XML sitemap.</summary>
        public const string SitemapRead = "Cms.Sitemap.Read";

        /// <summary>Edit the XML sitemap. Implies <see cref="SitemapRead"/>.</summary>
        public const string SitemapWrite = "Cms.Sitemap.Write";

        /// <summary>View message templates and queued email.</summary>
        public const string MessagingRead = "Cms.Messaging.Read";

        /// <summary>Edit message templates and queued email. Implies <see cref="MessagingRead"/>.</summary>
        public const string MessagingWrite = "Cms.Messaging.Write";
    }

}