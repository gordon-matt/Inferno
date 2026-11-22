namespace Inferno.Caching;

/// <summary>Reads a cached value, or stores the result of a factory when the key is missing.</summary>
public static class CacheExtensions
{
    /// <summary>Returns the cached value, or stores <paramref name="acquire"/> for 60 minutes when the key is missing.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="cacheManager">The cache to read and write.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="acquire">Factory used only when <paramref name="key"/> is missing. A <see langword="null"/> result is not stored, so the next call runs the factory again.</param>
    /// <returns>The cached value, or the factory result.</returns>
    public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire) => Get(cacheManager, key, 60, acquire);

    /// <summary>Returns the cached value, or stores <paramref name="acquire"/> with a sliding expiration when the key is missing.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="cacheManager">The cache to read and write.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="cacheTimeInMinutes">Sliding lifetime, in minutes, used when the value is stored.</param>
    /// <param name="acquire">Factory used only when <paramref name="key"/> is missing. A <see langword="null"/> result is not stored, so the next call runs the factory again.</param>
    /// <returns>The cached value, or the factory result.</returns>
    public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTimeInMinutes, Func<T> acquire)
    {
        // TODO: Consider removing "IsSet()" and just using GetOrCreate(), because IMemoryCache does not support checking if a key is present
        //  and we are trying to keep track of the keys ourselves (See MemoryCacheManager implementation). That might lead to some problems.

        if (cacheManager.IsSet(key))
        {
            return cacheManager.Get<T>(key);
        }
        else
        {
            var result = acquire();
            //if (result != null)
            cacheManager.Set(key, result, cacheTimeInMinutes);
            return result;
        }
    }
}
