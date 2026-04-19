// Originally derived from MantleCMS/nopCommerce/Umbraco. See:
// http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx

namespace Inferno.Plugins;

/// <summary>
/// Discovers and shadow-copies plugin assemblies on application startup, then
/// hands them to MVC's <see cref="ApplicationPartManager"/> so their
/// controllers, Razor views and tag helpers are visible to the host.
///
/// All state is static because plugin discovery happens exactly once per
/// process lifetime, before the DI container is built.
/// </summary>
public class PluginManager
{
    #region Constants

    private const string RESERVE_SHADOW_COPY_FOLDER_NAME = "reserve_bin_";
    private const string RESERVE_SHADOW_COPY_FOLDER_NAME_PATTERN = "reserve_bin_*";
    public const string CurrentVersion = "1.00";

    #endregion Constants

    #region Fields

    private static readonly Lock Locker = new();
    private static DirectoryInfo shadowCopyFolder;
    private static readonly List<string> BaseAppLibraries;
    private static DirectoryInfo reserveShadowCopyFolder;

    private static Dictionary<string, bool> installedPlugins = null;

    #endregion Fields

    #region Ctor

    static PluginManager()
    {
        // Snapshot every dll currently next to the host so that PerformFileDeploy
        // can avoid loading a plugin assembly whose name collides with a host one.
        BaseAppLibraries = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
            .GetFiles("*.dll", SearchOption.TopDirectoryOnly)
            .Select(fi => fi.Name)
            .ToList();

        if (!AppDomain.CurrentDomain.BaseDirectory.Equals(Environment.CurrentDirectory, StringComparison.InvariantCultureIgnoreCase))
        {
            BaseAppLibraries.AddRange(new DirectoryInfo(Environment.CurrentDirectory)
                .GetFiles("*.dll", SearchOption.TopDirectoryOnly).Select(fi => fi.Name));
        }

        var refsPathName = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, RefsPathName));
        if (refsPathName.Exists)
        {
            BaseAppLibraries.AddRange(refsPathName.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Select(fi => fi.Name));
        }
    }

    #endregion Ctor

    #region Methods

    /// <summary>
    /// Discover every <c>plugin.json</c> beneath <c>~/Plugins</c>, validate
    /// that the descriptor is compatible with the current <see cref="CurrentVersion"/>,
    /// shadow-copy each plugin's main assembly, load it and register every
    /// resulting <see cref="ApplicationPart"/> with MVC.
    /// </summary>
    public static void Initialize(ApplicationPartManager applicationPartManager, InfernoPluginOptions options)
    {
        ArgumentNullException.ThrowIfNull(applicationPartManager);

        lock (Locker)
        {
            var pluginFolder = new DirectoryInfo(CommonHelper.MapPath(PluginsPath));
            shadowCopyFolder = new DirectoryInfo(CommonHelper.MapPath(ShadowCopyPath));
            reserveShadowCopyFolder = new DirectoryInfo(Path.Combine(
                CommonHelper.MapPath(ShadowCopyPath),
                $"{RESERVE_SHADOW_COPY_FOLDER_NAME}{DateTime.Now.ToFileTimeUtc()}"));

            var referencedPlugins = new List<PluginDescriptor>();
            var incompatiblePlugins = new List<string>();

            try
            {
                var installedPluginSystemNames = GetInstalledPluginNames(CommonHelper.MapPath(InstalledPluginsFilePath));

                Debug.WriteLine("Creating shadow copy folder and querying for DLLs");
                Directory.CreateDirectory(pluginFolder.FullName);
                Directory.CreateDirectory(shadowCopyFolder.FullName);

                var binFiles = shadowCopyFolder.GetFiles("*", SearchOption.AllDirectories);
                if (options.ClearPluginShadowDirectoryOnStartup)
                {
                    foreach (var f in binFiles)
                    {
                        if (f.Name.Equals("placeholder.txt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        Debug.WriteLine("Deleting " + f.Name);
                        try
                        {
                            string fileName = Path.GetFileName(f.FullName);
                            if (fileName.Equals("index.htm", StringComparison.InvariantCultureIgnoreCase))
                            {
                                continue;
                            }

                            File.Delete(f.FullName);
                        }
                        catch (Exception exc)
                        {
                            Debug.WriteLine("Error deleting file " + f.Name + ". Exception: " + exc);
                        }
                    }

                    foreach (var directory in shadowCopyFolder.GetDirectories(RESERVE_SHADOW_COPY_FOLDER_NAME_PATTERN, SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            CommonHelper.DeleteDirectory(directory.FullName);
                        }
                        catch
                        {
                            // best-effort cleanup
                        }
                    }
                }

                foreach (var dfd in GetDescriptionFilesAndDescriptors(pluginFolder))
                {
                    var descriptionFile = dfd.Key;
                    var pluginDescriptor = dfd.Value;

                    if (!pluginDescriptor.SupportedVersions.Contains(CurrentVersion, StringComparer.InvariantCultureIgnoreCase))
                    {
                        incompatiblePlugins.Add(pluginDescriptor.SystemName);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(pluginDescriptor.SystemName))
                    {
                        throw new Exception($"A plugin '{descriptionFile.FullName}' has no system name. Try assigning the plugin a unique name and recompiling.");
                    }

                    if (referencedPlugins.Contains(pluginDescriptor))
                    {
                        throw new Exception($"A plugin with '{pluginDescriptor.SystemName}' system name is already defined");
                    }

                    pluginDescriptor.Installed = installedPluginSystemNames
                        .FirstOrDefault(x => x.Equals(pluginDescriptor.SystemName, StringComparison.InvariantCultureIgnoreCase)) != null;

                    try
                    {
                        if (descriptionFile.Directory == null)
                        {
                            throw new Exception($"Directory cannot be resolved for '{descriptionFile.Name}' description file");
                        }

                        var pluginFiles = descriptionFile.Directory.GetFiles("*.dll", SearchOption.AllDirectories)
                            .Where(x => !binFiles.Select(q => q.FullName).Contains(x.FullName))
                            .Where(x => IsPackagePluginFolder(x.Directory))
                            .ToList();

                        var mainPluginFile = pluginFiles
                            .FirstOrDefault(x => x.Name.Equals(pluginDescriptor.AssemblyFileName, StringComparison.InvariantCultureIgnoreCase));

                        if (mainPluginFile == null)
                        {
                            incompatiblePlugins.Add(pluginDescriptor.SystemName);
                            continue;
                        }

                        pluginDescriptor.OriginalAssemblyFile = mainPluginFile;

                        var pluginAssembly = PerformFileDeploy(mainPluginFile, options);
                        pluginDescriptor.ReferencedAssembly = pluginAssembly;

                        var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
                        foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
                        {
                            applicationPartManager.ApplicationParts.Add(part);
                        }

                        var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(pluginAssembly, throwOnError: true);
                        foreach (var assembly in relatedAssemblies)
                        {
                            partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                            foreach (var part in partFactory.GetApplicationParts(assembly))
                            {
                                applicationPartManager.ApplicationParts.Add(part);
                            }
                        }

                        // A plugin assembly is allowed to contain at most one IPlugin implementation;
                        // we wire the first concrete one we find as the descriptor's PluginType.
                        foreach (var t in pluginAssembly.GetTypes())
                        {
                            if (typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract)
                            {
                                pluginDescriptor.PluginType = t;
                                break;
                            }
                        }

                        referencedPlugins.Add(pluginDescriptor);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        string msg = $"Plugin '{pluginDescriptor.FriendlyName}'. ";
                        foreach (var e in ex.LoaderExceptions)
                        {
                            msg += e.Message + Environment.NewLine;
                        }

                        throw new Exception(msg, ex);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"Plugin '{pluginDescriptor.FriendlyName}'. {ex.Message}";
                        throw new Exception(msg, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Empty;
                for (var e = ex; e != null; e = e.InnerException)
                {
                    msg += e.Message + Environment.NewLine;
                }

                throw new Exception(msg, ex);
            }

            ReferencedPlugins = referencedPlugins;
            IncompatiblePlugins = incompatiblePlugins;
        }
    }

    /// <summary>
    /// Records that the given plugin is installed by appending its system name
    /// to <c>~/App_Data/installedPlugins.json</c>.
    /// </summary>
    public static void MarkPluginAsInstalled(string systemName)
    {
        if (string.IsNullOrEmpty(systemName))
        {
            throw new ArgumentNullException(nameof(systemName));
        }

        string filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
        EnsureFile(filePath);

        var installedPluginSystemNames = GetInstalledPluginNames(filePath);

        bool alreadyMarkedAsInstalled = installedPluginSystemNames
            .Any(pluginName => pluginName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));

        if (!alreadyMarkedAsInstalled)
        {
            installedPluginSystemNames.Add(systemName);
        }

        SaveInstalledPluginNames(installedPluginSystemNames, filePath);
    }

    public static void MarkPluginAsUninstalled(string systemName)
    {
        if (string.IsNullOrEmpty(systemName))
        {
            throw new ArgumentNullException(nameof(systemName));
        }

        string filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
        EnsureFile(filePath);

        var installedPluginSystemNames = GetInstalledPluginNames(filePath);

        bool alreadyMarkedAsInstalled = installedPluginSystemNames
            .Any(pluginName => pluginName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        if (alreadyMarkedAsInstalled)
        {
            installedPluginSystemNames.Remove(systemName);
        }

        SaveInstalledPluginNames(installedPluginSystemNames, filePath);
    }

    public static void MarkAllPluginsAsUninstalled()
    {
        string filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static PluginDescriptor FindPlugin(Type typeInAssembly)
    {
        ArgumentNullException.ThrowIfNull(typeInAssembly);
        return ReferencedPlugins?.FirstOrDefault(plugin => plugin.ReferencedAssembly != null
            && plugin.ReferencedAssembly.FullName.Equals(typeInAssembly.Assembly.FullName, StringComparison.InvariantCultureIgnoreCase));
    }

    public static PluginDescriptor GetPluginDescriptorFromFile(string filePath) =>
        GetPluginDescriptorFromText(File.ReadAllText(filePath));

    public static PluginDescriptor GetPluginDescriptorFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new PluginDescriptor();
        }

        var descriptor = JsonConvert.DeserializeObject<PluginDescriptor>(text);

        if (descriptor.SupportedVersions.Count == 0)
        {
            descriptor.SupportedVersions.Add(CurrentVersion);
        }

        return descriptor;
    }

    public static void SavePluginDescriptor(PluginDescriptor pluginDescriptor)
    {
        if (pluginDescriptor == null)
        {
            throw new ArgumentException(null, nameof(pluginDescriptor));
        }

        if (pluginDescriptor.OriginalAssemblyFile == null)
        {
            throw new Exception($"Cannot load original assembly path for {pluginDescriptor.SystemName} plugin.");
        }

        string filePath = Path.Combine(pluginDescriptor.OriginalAssemblyFile.Directory.FullName, PluginDescriptionFileName);
        if (!File.Exists(filePath))
        {
            throw new Exception($"Description file for {pluginDescriptor.SystemName} plugin does not exist. {filePath}");
        }

        string text = JsonConvert.SerializeObject(pluginDescriptor, Formatting.Indented);
        File.WriteAllText(filePath, text);
    }

    /// <summary>
    /// Removes the plugin's directory from disk. Refuses to delete a plugin
    /// that is still installed.
    /// </summary>
    public static bool DeletePlugin(PluginDescriptor pluginDescriptor)
    {
        if (pluginDescriptor == null)
        {
            return false;
        }

        if (pluginDescriptor.Installed)
        {
            return false;
        }

        if (pluginDescriptor.OriginalAssemblyFile.Directory.Exists)
        {
            CommonHelper.DeleteDirectory(pluginDescriptor.OriginalAssemblyFile.DirectoryName);
        }

        return true;
    }

    public static bool IsPluginInstalled(string systemName)
    {
        installedPlugins ??= [];

        if (!installedPlugins.ContainsKey(systemName))
        {
            bool isInstalled = ReferencedPlugins.Any(x => x.SystemName == systemName && x.Installed);
            installedPlugins.Add(systemName, isInstalled);
        }

        return installedPlugins[systemName];
    }

    #endregion Methods

    #region Utilities

    private static void EnsureFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (File.Create(filePath)) { }
        }
    }

    private static IEnumerable<KeyValuePair<FileInfo, PluginDescriptor>> GetDescriptionFilesAndDescriptors(DirectoryInfo pluginFolder)
    {
        ArgumentNullException.ThrowIfNull(pluginFolder);

        var result = new List<KeyValuePair<FileInfo, PluginDescriptor>>();

        foreach (var descriptionFile in pluginFolder.GetFiles(PluginDescriptionFileName, SearchOption.AllDirectories))
        {
            if (!IsPackagePluginFolder(descriptionFile.Directory))
            {
                continue;
            }

            var pluginDescriptor = GetPluginDescriptorFromFile(descriptionFile.FullName);
            result.Add(new KeyValuePair<FileInfo, PluginDescriptor>(descriptionFile, pluginDescriptor));
        }

        result.Sort((firstPair, nextPair) => firstPair.Value.DisplayOrder.CompareTo(nextPair.Value.DisplayOrder));
        return result;
    }

    private static IList<string> GetInstalledPluginNames(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        string text = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        return JsonConvert.DeserializeObject<IList<string>>(text);
    }

    private static void SaveInstalledPluginNames(IList<string> pluginSystemNames, string filePath)
    {
        string text = JsonConvert.SerializeObject(pluginSystemNames, Formatting.Indented);
        File.WriteAllText(filePath, text);
    }

    private static bool IsAlreadyLoaded(FileInfo fileInfo)
    {
        if (BaseAppLibraries.Any(sli => sli.Equals(fileInfo.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            return true;
        }

        try
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            if (string.IsNullOrEmpty(fileNameWithoutExt))
            {
                throw new Exception($"Cannot get file extension for {fileInfo.Name}");
            }

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = a.FullName.Split(',').FirstOrDefault();
                if (fileNameWithoutExt.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (Exception exc)
        {
            Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + exc);
        }
        return false;
    }

    private static Assembly PerformFileDeploy(FileInfo plug, InfernoPluginOptions options, string shadowCopyPath = "")
    {
        if (plug.Directory?.Parent == null)
        {
            throw new InvalidOperationException($"The plugin directory for the {plug.Name} file exists in a folder outside of the allowed Inferno folder hierarchy");
        }

        if (!options.UsePluginsShadowCopy)
        {
            return RegisterPluginDefinition(options, plug);
        }

        if (string.IsNullOrEmpty(shadowCopyPath))
        {
            shadowCopyPath = shadowCopyFolder.FullName;
        }

        var shadowCopyPlugFolder = Directory.CreateDirectory(shadowCopyPath);
        var shadowCopiedPlug = ShadowCopyFile(plug, shadowCopyPlugFolder);

        Assembly shadowCopiedAssembly = null;
        try
        {
            shadowCopiedAssembly = RegisterPluginDefinition(options, shadowCopiedPlug);
        }
        catch (FileLoadException)
        {
            if (!options.CopyLockedPluginAssembilesToSubdirectoriesOnStartup ||
                !shadowCopyPath.Equals(shadowCopyFolder.FullName))
            {
                throw;
            }
        }

        return shadowCopiedAssembly ?? PerformFileDeploy(plug, options, reserveShadowCopyFolder.FullName);
    }

    private static Assembly RegisterPluginDefinition(InfernoPluginOptions options, FileInfo plug)
    {
        Assembly pluginAssembly;
        try
        {
            pluginAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(plug.FullName);
        }
        catch (FileLoadException)
        {
            if (options.UseUnsafeLoadAssembly)
            {
                pluginAssembly = Assembly.UnsafeLoadFrom(plug.FullName);
            }
            else
            {
                throw;
            }
        }

        return pluginAssembly;
    }

    private static FileInfo ShadowCopyFile(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
    {
        bool shouldCopy = true;
        var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

        if (shadowCopiedPlug.Exists)
        {
            // Compare creation timestamps - works on every supported file system,
            // unlike LastWriteTimeUtc on FAT32. Identical timestamps mean the
            // existing shadow copy is already up to date.
            bool areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks >= plug.CreationTimeUtc.Ticks;
            if (areFilesIdentical)
            {
                Debug.WriteLine("Not copying; files appear identical: '{0}'", shadowCopiedPlug.Name);
                shouldCopy = false;
            }
            else
            {
                Debug.WriteLine("New plugin found; Deleting the old file: '{0}'", shadowCopiedPlug.Name);
                File.Delete(shadowCopiedPlug.FullName);
            }
        }

        if (!shouldCopy)
        {
            return shadowCopiedPlug;
        }

        try
        {
            File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
        }
        catch (IOException)
        {
            Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
            // Sometimes devenv keeps a handle on the dll. Renaming releases the
            // lock without affecting the assembly's identity, after which we can
            // re-copy the new bits.
            try
            {
                string oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                File.Move(shadowCopiedPlug.FullName, oldFile);
            }
            catch (IOException exc)
            {
                throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
            }

            File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
        }

        return shadowCopiedPlug;
    }

    private static bool IsPackagePluginFolder(DirectoryInfo folder) =>
        folder?.Parent != null && folder.Parent.Name.Equals(PluginsPathName, StringComparison.InvariantCultureIgnoreCase);

    #endregion Utilities

    #region Properties

    public static string InstalledPluginsFilePath => "~/App_Data/installedPlugins.json";

    public static string PluginsPath => "~/Plugins";

    public static string PluginsPathName => "Plugins";

    public static string ShadowCopyPath => "~/Plugins/bin";

    public static string RefsPathName => "refs";

    public static string PluginDescriptionFileName => "plugin.json";

    /// <summary>
    /// Every plugin descriptor that was successfully discovered and loaded.
    /// </summary>
    public static IEnumerable<PluginDescriptor> ReferencedPlugins { get; set; } = [];

    /// <summary>
    /// Plugin system names whose <c>SupportedVersions</c> didn't include
    /// <see cref="CurrentVersion"/>.
    /// </summary>
    public static IEnumerable<string> IncompatiblePlugins { get; set; } = [];

    #endregion Properties
}
