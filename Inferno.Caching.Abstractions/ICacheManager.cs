namespace Inferno.Caching;

/// <summary>
/// Application cache. A missing key returns the default value, and a <see langword="null"/> value is not stored.
/// </summary>
public interface ICacheManager
{
    /// <summary>Returns the value stored for <paramref name="key"/>.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The stored value, or the default for <typeparamref name="T"/> when the key is missing or holds <see langword="null"/>.</returns>
    T Get<T>(string key);

    /// <summary>
    /// Stores <paramref name="data"/> under <paramref name="key"/> with a sliding expiration.
    /// A <see langword="null"/> value is ignored and does not replace an existing entry.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The value to store. <see langword="null"/> is not stored.</param>
    /// <param name="cacheTimeInMinutes">Sliding lifetime, in minutes.</param>
    void Set(string key, object data, int cacheTimeInMinutes);

    /// <summary>Reports whether a non-null value is stored for <paramref name="key"/>.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns><see langword="true"/> when the key holds a non-null value.</returns>
    bool IsSet(string key);

    /// <summary>Removes the value stored for <paramref name="key"/>.</summary>
    /// <param name="key">The cache key.</param>
    void Remove(string key);

    /// <summary>Removes every tracked key that matches <paramref name="pattern"/>.</summary>
    /// <param name="pattern">A regular expression, matched against tracked keys without regard to case.</param>
    /// <exception cref="ArgumentException"><paramref name="pattern"/> is not a valid regular expression.</exception>
    void RemoveByPattern(string pattern);

    /// <summary>Removes every key this cache is tracking. Entries it did not store are left in place.</summary>
    void Clear();
}
