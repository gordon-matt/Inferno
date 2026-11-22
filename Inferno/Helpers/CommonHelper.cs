namespace Inferno.Helpers;

/// <summary>Path helpers for content stored under the site directory.</summary>
public static class CommonHelper
{
    /// <summary>
    /// Directory used by <see cref="MapPath"/> when no base path is passed.
    /// <see langword="null"/> until the host sets it.
    /// </summary>
    public static string BaseDirectory { get; set; }

    /// <summary>
    /// Combines <paramref name="path"/> with a base directory.
    /// A leading <c>~/</c> or <c>/</c> is removed, and <c>/</c> is turned into <c>\</c>.
    /// </summary>
    /// <param name="path">Path relative to the base. Must not be <see langword="null"/>.</param>
    /// <param name="basePath">
    /// Directory to combine with. <see langword="null"/> or empty uses <see cref="BaseDirectory"/>, or an empty string when that is also unset.
    /// </param>
    /// <returns>The combined path.</returns>
    public static string MapPath(string path, string basePath = null)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            basePath = BaseDirectory ?? string.Empty;
        }

        path = path.Replace("~/", string.Empty).TrimStart('/').Replace('/', '\\');
        return Path.Combine(basePath, path);
    }

    /// <summary>
    /// Depth-first recursive delete, with handling for descendant directories open in Windows Explorer.
    /// A locked directory is deleted a second time. A failure on that second attempt propagates.
    /// </summary>
    /// <param name="path">The directory to delete.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/> or empty.</exception>
    public static void DeleteDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(path);
        }

        //find more info about directory deletion
        //and why we use this approach at https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true

        foreach (string directory in Directory.GetDirectories(path))
        {
            DeleteDirectory(directory);
        }

        try
        {
            Directory.Delete(path, true);
        }
        catch (IOException)
        {
            Directory.Delete(path, true);
        }
        catch (UnauthorizedAccessException)
        {
            Directory.Delete(path, true);
        }
    }
}
