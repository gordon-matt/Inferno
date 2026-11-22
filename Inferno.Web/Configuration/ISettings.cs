namespace Inferno.Web.Configuration;

/// <summary>A settings document stored as JSON and edited in the admin.</summary>
public interface ISettings
{
    /// <summary>The label stored with the setting and shown in the admin.</summary>
    string Name { get; }

    /// <summary>
    /// True if these settings are global (for all sites) and only the admin user can modify.
    /// False if each tenant can have their own customized settings.
    /// </summary>
    bool IsTenantRestricted { get; }

    /// <summary>The component that edits this settings type.</summary>
    Type EditorType { get; }
}