using Dependo;
using Inferno.Web.Configuration;

namespace Inferno.Web
{
    public class InfernoWebConstants
    {
        private static string defaultAdminLayoutPath;
        private static string defaultFrontendLayoutPath;

        public static string DefaultAdminLayoutPath
        {
            get
            {
                if (string.IsNullOrEmpty(defaultAdminLayoutPath))
                {
                    var siteSettings = EngineContext.Current.Resolve<SiteSettings>();

                    defaultAdminLayoutPath = string.IsNullOrEmpty(siteSettings.AdminLayoutPath)
                        ? "~/Areas/Admin/Views/Shared/_Layout.cshtml"
                        : siteSettings.AdminLayoutPath;
                }
                return defaultAdminLayoutPath;
            }
        }

        public static string DefaultFrontendLayoutPath
        {
            get
            {
                if (string.IsNullOrEmpty(defaultFrontendLayoutPath))
                {
                    var siteSettings = EngineContext.Current.Resolve<SiteSettings>();

                    defaultFrontendLayoutPath = string.IsNullOrEmpty(siteSettings.DefaultFrontendLayoutPath)
                        ? "~/Views/Shared/_Layout.cshtml"
                        : siteSettings.DefaultFrontendLayoutPath;
                }
                return defaultFrontendLayoutPath;
            }
        }

        public static class Areas
        {
            public const string Admin = "Admin";
            public const string Configuration = "Configuration";

            //public const string Indexing = "Indexing";
            public const string Localization = "Localization";

            public const string Log = "Log";
            public const string Membership = "Membership";
            public const string Plugins = "Plugins";
            public const string ScheduledTasks = "ScheduledTasks";
            public const string Tenants = "Tenants";
        }

        public static class CacheKeys
        {
            public const string CurrentCulture = "Inferno.Web.CacheKeys.CurrentCulture";

            /// <summary>
            /// {0} for TenantId, {1} for settings "Type"
            /// </summary>
            public const string SettingsKeyFormat = "Inferno.Web.CacheKeys.Settings.Tenant_{0}_{1}";

            /// <summary>
            /// {0}: Tenant ID
            /// </summary>
            public const string SettingsKeysPatternFormat = "Inferno.Web.CacheKeys.Settings.Tenant_{0}_.*";
        }

        public static class StateProviders
        {
            public const string CurrentCultureCode = "CurrentCultureCode";
            public const string CurrentTheme = "CurrentTheme";
            public const string CurrentUser = "CurrentUser";
        }

        /// <summary>
        /// Resets static variables to NULL
        /// </summary>
        public static void ResetCache()
        {
            defaultAdminLayoutPath = null;
            defaultFrontendLayoutPath = null;
        }
    }
}