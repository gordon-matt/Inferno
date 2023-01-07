using System.Text.RegularExpressions;

namespace Inferno.Web.ContentManagement
{
    public static partial class CmsConstants
    {
        public static partial class RegexPatterns
        {
            public static readonly Regex Email = EmailRegex();

            [GeneratedRegex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,4}$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
            private static partial Regex EmailRegex();
        }

        public static class Areas
        {
            public const string Blocks = "Admin/Blocks";
            public const string Blog = "Admin/Blog";
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
            internal const string BlogPosts = "BlogPosts";
            internal const string BlogPostTags = "BlogPostTags";
            internal const string BlogCategories = "BlogCategories";
            internal const string BlogTags = "BlogTags";
            internal const string ContentBlocks = "ContentBlocks";
            internal const string EntityTypeContentBlocks = "EntityTypeContentBlocks";
            internal const string HistoricPages = "HistoricPages";
            internal const string MenuItems = "MenuItems";
            internal const string Menus = "Menus";
            internal const string MessageTemplates = "MessageTemplates";
            internal const string Pages = "Pages";
            internal const string PageVersions = "PageVersions";
            internal const string PageTypes = "PageTypes";
            internal const string SitemapConfig = "SitemapConfig";
            internal const string QueuedEmails = "QueuedEmails";
            internal const string QueuedSMS = "QueuedSMS";
            internal const string Zones = "Zones";
        }

        public static class Policies
        {
            public const string BlogRead = "Blog: Read";
            public const string BlogWrite = "Blog: Write";

            public const string ContentBlocksRead = "Content Blocks: Read";
            public const string ContentBlocksWrite = "Content Blocks: Write";
            public const string ContentZonesRead = "Content Zones: Read";
            public const string ContentZonesWrite = "Content Zones: Write";

            public const string MediaRead = "Media: Read";
            public const string MediaWrite = "Media: Write";

            public const string MenusRead = "Menus: Read";
            public const string MenusWrite = "Menus: Write";

            public const string PageHistoryRead = "Page History: Read";
            public const string PageHistoryRestore = "Page History: Restore";
            public const string PageHistoryWrite = "Page History: Write";
            public const string PagesRead = "Pages: Read";
            public const string PagesWrite = "Pages: Write";
            public const string PageTypesRead = "Page Types: Read";
            public const string PageTypesWrite = "Page Types: Write";

            public const string SitemapRead = "Sitemap: Read";
            public const string SitemapWrite = "Sitemap: Write";
        }

        public static class ODataRoutes
        {
            public const string Prefix = "inferno/cms";

            public static class EntitySetNames
            {
                public const string BlogCategory = "BlogCategoryApi";
                public const string BlogPost = "BlogPostApi";
                public const string BlogPostTag = "BlogPostTagApi";
                public const string BlogTag = "BlogTagApi";
                public const string ContentBlock = "ContentBlockApi";
                public const string EntityTypeContentBlock = "EntityTypeContentBlockApi";
                public const string Menu = "MenuApi";
                public const string MenuItem = "MenuItemApi";
                public const string Page = "PageApi";
                public const string PageTree = "PageTreeApi";
                public const string PageType = "PageTypeApi";
                public const string PageVersion = "PageVersionApi";
                public const string Subscriber = "SubscriberApi";
                public const string XmlSitemap = "XmlSitemapApi";
                public const string Zone = "ZoneApi";
            }
        }
    }
}