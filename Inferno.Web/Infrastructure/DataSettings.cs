using Extenso;
using Inferno.Helpers;

namespace Inferno.Web.Infrastructure;

/// <summary>The installation file: database, first administrator, and demo-data flags.</summary>
public class DataSettings
{
    /// <summary>The database provider name.</summary>
    public string ProviderName { get; set; }

    /// <summary>The database connection string. Empty means the site is not installed.</summary>
    public string ConnectionString { get; set; }

    /// <summary>The email used to create the first administrator when membership is empty.</summary>
    public string AdminEmail { get; set; }

    /// <summary>The password used to create the first administrator. Cleared after that user is created.</summary>
    public string AdminPassword { get; set; }

    /// <summary>Whether startup should insert sample content.</summary>
    public bool CreateSampleData { get; set; }

    /// <summary>The language installed for a new site.</summary>
    public string DefaultLanguage { get; set; }

    /// <summary>Set by the demo data once it has created its tenants, until the theme tables exist and each tenant has its theme.</summary>
    public bool DemoThemesPending { get; set; }

    /// <summary>Whether a provider and a connection string are both set.</summary>
    /// <returns><see langword="true"/> when both are non-empty.</returns>
    public bool IsValid() => !string.IsNullOrEmpty(ProviderName) && !string.IsNullOrEmpty(ConnectionString);
}

/// <summary>Reads and writes <see cref="DataSettings"/> at <c>~/App_Data/InfernoSettings.xml</c>.</summary>
public static class DataSettingsManager
{
    private const string virtualPath = "~/App_Data/InfernoSettings.xml";

    /// <summary>Loads the settings file, or a new empty instance when the file is missing or blank.</summary>
    /// <returns>The stored settings, or a new <see cref="DataSettings"/>.</returns>
    public static DataSettings LoadSettings()
    {
        string filePath = CommonHelper.MapPath(virtualPath);

        return File.Exists(filePath) ? File.ReadAllText(filePath)?.XmlDeserialize<DataSettings>() ?? new DataSettings() : new DataSettings();
    }

    /// <summary>Writes <paramref name="settings"/> to <c>~/App_Data/InfernoSettings.xml</c>, creating the file when it is missing.</summary>
    /// <param name="settings">The settings to store.</param>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static void SaveSettings(DataSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        //use webHelper.MapPath instead of HostingEnvironment.MapPath which is not available in unit tests
        string filePath = CommonHelper.MapPath(virtualPath);
        if (!File.Exists(filePath))
        {
            using (File.Create(filePath))
            {
                //we use 'using' to close the file after it's created
            }
        }

        File.WriteAllText(filePath, settings.XmlSerialize());
    }
}

/// <summary>Remembers whether a connection string has been saved, so startup does not read the file on every check.</summary>
public static class DataSettingsHelper
{
    private static bool? isDatabaseInstalled;

    /// <summary>Whether <see cref="DataSettings.ConnectionString"/> was non-empty the last time it was read. The first read loads the file.</summary>
    public static bool IsDatabaseInstalled
    {
        get
        {
            if (!isDatabaseInstalled.HasValue)
            {
                var settings = DataSettingsManager.LoadSettings();
                isDatabaseInstalled = settings != null && !string.IsNullOrEmpty(settings.ConnectionString);
            }
            return isDatabaseInstalled.Value;
        }
    }

    /// <summary>Forgets the cached installation flag so the next read loads the file again.</summary>
    public static void ResetCache() => isDatabaseInstalled = null;
}