using System.Text.RegularExpressions;

namespace Inferno.Web.ContentManagement;

public static partial class CmsConstants
{
    public static partial class RegexPatterns
    {
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

    public static class Areas
    {
        public const string Blocks = "Admin/Blocks";
        public const string Localization = "Admin/Localization";
        public const string Media = "Admin/Media";
        public const string Messaging = "Admin/Messaging";
        public const string Menus = "Admin/Menus";
        public const string Newsletters = "Admin/Newsletters";
        public const string Pages = "Admin/Pages";
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
        public const string BlogRead = "Cms.Blog.Read";
        public const string BlogWrite = "Cms.Blog.Write";

        public const string ContentBlocksRead = "Cms.ContentBlocks.Read";
        public const string ContentBlocksWrite = "Cms.ContentBlocks.Write";
        public const string ContentZonesRead = "Cms.ContentZones.Read";
        public const string ContentZonesWrite = "Cms.ContentZones.Write";

        public const string MediaRead = "Cms.Media.Read";
        public const string MediaWrite = "Cms.Media.Write";

        public const string MenusRead = "Cms.Menus.Read";
        public const string MenusWrite = "Cms.Menus.Write";

        public const string NewsletterRead = "Cms.Newsletters.Read";
        public const string NewsletterWrite = "Cms.Newsletters.Write";

        public const string PageHistoryRead = "Cms.PageHistory.Read";
        public const string PageHistoryRestore = "Cms.PageHistory.Restore";
        public const string PageHistoryWrite = "Cms.PageHistory.Write";
        public const string PagesRead = "Cms.Pages.Read";
        public const string PagesWrite = "Cms.Pages.Write";

        public const string SitemapRead = "Cms.Sitemap.Read";
        public const string SitemapWrite = "Cms.Sitemap.Write";

        public const string MessagingRead = "Cms.Messaging.Read";
        public const string MessagingWrite = "Cms.Messaging.Write";
    }

}