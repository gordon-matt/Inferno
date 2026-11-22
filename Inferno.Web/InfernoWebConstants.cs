namespace Inferno.Web;

/// <summary>Area names, cache keys, and work-context state names used across Inferno.Web.</summary>
public class InfernoWebConstants
{
    /// <summary>MVC area names.</summary>
    public static class Areas
    {
        /// <summary>The admin area.</summary>
        public const string Admin = "Admin";

        /// <summary>The configuration area.</summary>
        public const string Configuration = "Admin/Configuration";

        //public const string Indexing = "Admin/Indexing";
        /// <summary>The localization area.</summary>
        public const string Localization = "Admin/Localization";

        /// <summary>The log area.</summary>
        public const string Log = "Admin/Log";

        /// <summary>The membership area.</summary>
        public const string Membership = "Admin/Membership";

        /// <summary>The plugins area.</summary>
        public const string Plugins = "Admin/Plugins";

        /// <summary>The tenants area.</summary>
        public const string Tenants = "Admin/Tenants";
    }

    /// <summary>Keys for the application cache.</summary>
    public static class CacheKeys
    {
        /// <summary>The cached default culture.</summary>
        public const string CurrentCulture = "Inferno.Web.CacheKeys.CurrentCulture";

        /// <summary>Cache key for one settings type. <c>{0}</c> is the tenant id and <c>{1}</c> is the settings type name.</summary>
        public const string SettingsKeyFormat = "Inferno.Web.CacheKeys.Settings.Tenant_{0}_{1}";

        /// <summary>Pattern that matches every settings key for one tenant. <c>{0}</c> is the tenant id.</summary>
        public const string SettingsKeysPatternFormat = "Inferno.Web.CacheKeys.Settings.Tenant_{0}_.*";
    }

    /// <summary>Names passed to <see cref="IWorkContext.GetState{T}"/>.</summary>
    public static class StateProviders
    {
        /// <summary>The current culture code.</summary>
        public const string CurrentCultureCode = "CurrentCultureCode";

        /// <summary>The current user.</summary>
        public const string CurrentUser = "CurrentUser";
    }
}