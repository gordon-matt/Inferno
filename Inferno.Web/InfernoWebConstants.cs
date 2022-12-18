namespace Inferno.Web
{
    public class InfernoWebConstants
    {
        public static class Areas
        {
            public const string Admin = "Admin";
            public const string Configuration = "Admin/Configuration";

            //public const string Indexing = "Admin/Indexing";
            public const string Localization = "Admin/Localization";

            public const string Log = "Admin/Log";
            public const string Membership = "Admin/Membership";
            public const string Plugins = "Admin/Plugins";
            public const string ScheduledTasks = "Admin/ScheduledTasks";
            public const string Tenants = "Admin/Tenants";
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

        public static class ODataRoutes
        {
            public const string Prefix = "inferno/web";

            public static class EntitySetNames
            {
                public const string Settings = "SettingsApi";
                public const string Tenant = "TenantApi";
            }
        }

        public static class StateProviders
        {
            public const string CurrentCultureCode = "CurrentCultureCode";
            public const string CurrentTheme = "CurrentTheme";
            public const string CurrentUser = "CurrentUser";
        }
    }
}