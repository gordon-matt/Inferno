using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Caching
{
    /// <summary>
    /// Represents a MemoryCache
    /// </summary>
    public class MemoryCacheManager : ICacheManager, IDisposable
    {
        private readonly IMemoryCache cache;
        private readonly HashSet<string> keys;
        private bool isDisposed;

        public MemoryCacheManager(IServiceProvider serviceProvider)
        {
            keys = new HashSet<string>();
            cache = serviceProvider.GetService<IMemoryCache>();
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public virtual T Get<T>(string key)
        {
            return cache.Get<T>(key);
        }

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

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(cacheTimeInMinutes));

            cache.Set(key, data, cacheEntryOptions);

            if (!IsSet(key))
            {
                keys.Add(key);
            }
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

            bool isSet = (cacheEntry != null);

            if (!isSet)
            {
                if (keys.Contains(key))
                {
                    keys.Remove(key);
                }
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

            if (IsSet(key))
            {
                keys.Remove(key);
            }
        }

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        public virtual void RemoveByPattern(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<String>();

            foreach (string key in keys)
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
            foreach (string key in keys)
            {
                Remove(key);
            }
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
}