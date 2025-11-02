using System;
using System.Collections.Concurrent;

namespace WinCheck.Core.Helpers;

/// <summary>
/// Simple in-memory cache with expiration
/// </summary>
/// <typeparam name="TKey">Cache key type</typeparam>
/// <typeparam name="TValue">Cached value type</typeparam>
public class CacheHelper<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiration;

    public CacheHelper(TimeSpan defaultExpiration)
    {
        _defaultExpiration = defaultExpiration;
    }

    /// <summary>
    /// Gets a value from cache or computes it
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
    {
        return GetOrAdd(key, valueFactory, _defaultExpiration);
    }

    /// <summary>
    /// Gets a value from cache or computes it with custom expiration
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TValue> valueFactory, TimeSpan expiration)
    {
        // Check if exists and not expired
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpirationTime)
            {
                return entry.Value;
            }
            else
            {
                // Remove expired entry
                _cache.TryRemove(key, out _);
            }
        }

        // Compute new value
        var value = valueFactory();
        var newEntry = new CacheEntry
        {
            Value = value,
            ExpirationTime = DateTime.UtcNow.Add(expiration)
        };

        _cache[key] = newEntry;
        return value;
    }

    /// <summary>
    /// Tries to get a value from cache
    /// </summary>
    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpirationTime)
            {
                value = entry.Value;
                return true;
            }
            else
            {
                // Remove expired entry
                _cache.TryRemove(key, out _);
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Sets a value in cache
    /// </summary>
    public void Set(TKey key, TValue value)
    {
        Set(key, value, _defaultExpiration);
    }

    /// <summary>
    /// Sets a value in cache with custom expiration
    /// </summary>
    public void Set(TKey key, TValue value, TimeSpan expiration)
    {
        var entry = new CacheEntry
        {
            Value = value,
            ExpirationTime = DateTime.UtcNow.Add(expiration)
        };

        _cache[key] = entry;
    }

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    public bool Remove(TKey key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Clears all cached values
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Removes all expired entries
    /// </summary>
    public void RemoveExpired()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = new System.Collections.Generic.List<TKey>();

        foreach (var kvp in _cache)
        {
            if (now >= kvp.Value.ExpirationTime)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Gets the number of cached items (including expired)
    /// </summary>
    public int Count => _cache.Count;

    private class CacheEntry
    {
        public TValue Value { get; set; } = default!;
        public DateTime ExpirationTime { get; set; }
    }
}

/// <summary>
/// Static cache helper with common cache instances
/// </summary>
public static class AppCache
{
    /// <summary>
    /// Cache for system information (5 minutes)
    /// </summary>
    public static CacheHelper<string, object> SystemInfo { get; } = new(TimeSpan.FromMinutes(5));

    /// <summary>
    /// Cache for hardware information (10 minutes)
    /// </summary>
    public static CacheHelper<string, object> HardwareInfo { get; } = new(TimeSpan.FromMinutes(10));

    /// <summary>
    /// Cache for service information (2 minutes)
    /// </summary>
    public static CacheHelper<string, object> ServiceInfo { get; } = new(TimeSpan.FromMinutes(2));

    /// <summary>
    /// Cache for process information (30 seconds)
    /// </summary>
    public static CacheHelper<int, object> ProcessInfo { get; } = new(TimeSpan.FromSeconds(30));

    /// <summary>
    /// Clears all caches
    /// </summary>
    public static void ClearAll()
    {
        SystemInfo.Clear();
        HardwareInfo.Clear();
        ServiceInfo.Clear();
        ProcessInfo.Clear();
    }

    /// <summary>
    /// Removes expired entries from all caches
    /// </summary>
    public static void RemoveExpiredAll()
    {
        SystemInfo.RemoveExpired();
        HardwareInfo.RemoveExpired();
        ServiceInfo.RemoveExpired();
        ProcessInfo.RemoveExpired();
    }
}
