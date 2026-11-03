using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Caching;

/// <summary>
/// Represents a MemoryCache
/// </summary>
public class MemoryCacheManager : ICacheManager, IDisposable
{
    private readonly IMemoryCache cache;
    private readonly ConcurrentDictionary<string, byte> keys;
    private bool isDisposed;

    public MemoryCacheManager(IServiceProvider serviceProvider)
    {
        keys = new ConcurrentDictionary<string, byte>();
        cache = serviceProvider.GetService<IMemoryCache>();
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key.</returns>
    public virtual T Get<T>(string key) => cache.Get<T>(key);

    /// <summary>
    /// Adds the specified key and object to the cache.
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="data">Data</param>
    /// <param name="cacheTimeInMinutes">Cache time</param>
    public virtual void Set(string key, object data, int cacheTimeInMinutes)
    {
        if (data == null)
        {
            return;
        }

        // A replaced entry keeps its key; only genuine removals and expiries stop tracking it.
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(cacheTimeInMinutes))
            .RegisterPostEvictionCallback((evictedKey, _, reason, _) =>
            {
                if (reason != EvictionReason.Replaced && evictedKey is string stringKey)
                {
                    keys.TryRemove(stringKey, out _);
                }
            });

        cache.Set(key, data, cacheEntryOptions);
        keys.TryAdd(key, 0);
    }

    /// <summary>
    /// Gets a value indicating whether the value associated with the specified key is cached
    /// </summary>
    /// <param name="key">key</param>
    /// <returns>Result</returns>
    public virtual bool IsSet(string key)
    {
        // It might be set in "keys", but expired (set to null) in the actual cache!
        //  So we need to ALWAYS check actual cache!
        cache.TryGetValue(key, out object cacheEntry);

        bool isSet = cacheEntry != null;

        if (!isSet)
        {
            keys.TryRemove(key, out _);
        }

        return isSet;
    }

    /// <summary>
    /// Removes the value with the specified key from the cache
    /// </summary>
    /// <param name="key">/key</param>
    public virtual void Remove(string key)
    {
        cache.Remove(key);
        keys.TryRemove(key, out _);
    }

    /// <summary>
    /// Removes items by pattern
    /// </summary>
    /// <param name="pattern">pattern</param>
    public virtual void RemoveByPattern(string pattern)
    {
        var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var keysToRemove = new List<string>();

        foreach (string key in keys.Keys)
        {
            if (regex.IsMatch(key))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (string key in keysToRemove)
        {
            Remove(key);
        }
    }

    /// <summary>
    /// Clear all cache data
    /// </summary>
    public virtual void Clear()
    {
        foreach (string key in keys.Keys)
        {
            cache.Remove(key);
        }

        keys.Clear();
    }

    #region IDisposable Implementation

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                cache?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            isDisposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~MemoryCacheManager()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Implementation
}