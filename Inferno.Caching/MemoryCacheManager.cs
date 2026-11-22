using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Caching;

/// <summary>
/// <see cref="IMemoryCache"/> wrapper that keeps its own key list, because the memory cache cannot be enumerated.
/// A replaced entry stays on that list. Removal, expiry, and <see cref="Clear"/> take it off.
/// <see cref="Dispose"/> disposes the <see cref="IMemoryCache"/> resolved from the provider, which is the shared cache when that registration is a singleton.
/// </summary>
public class MemoryCacheManager : ICacheManager, IDisposable
{
    private readonly IMemoryCache cache;
    private readonly ConcurrentDictionary<string, byte> keys;
    private bool isDisposed;

    /// <summary>Reads <see cref="IMemoryCache"/> from <paramref name="serviceProvider"/>.</summary>
    /// <param name="serviceProvider">The provider that holds the memory cache. The cache must be registered.</param>
    public MemoryCacheManager(IServiceProvider serviceProvider)
    {
        keys = new ConcurrentDictionary<string, byte>();
        cache = serviceProvider.GetService<IMemoryCache>();
    }

    /// <inheritdoc />
    public virtual T Get<T>(string key) => cache.Get<T>(key);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void Remove(string key)
    {
        cache.Remove(key);
        keys.TryRemove(key, out _);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void Clear()
    {
        foreach (string key in keys.Keys)
        {
            cache.Remove(key);
        }

        keys.Clear();
    }

    #region IDisposable Implementation

    /// <summary>Disposes the resolved memory cache when <paramref name="disposing"/> is <see langword="true"/>.</summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
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

    /// <summary>Disposes the resolved memory cache.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Implementation
}
