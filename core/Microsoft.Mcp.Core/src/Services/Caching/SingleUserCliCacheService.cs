// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Azure.Mcp.Core.Services.Caching;

/// <summary>
/// An implementation of <see cref="ICacheService"/> for single-user CLI scenarios using in-memory caching.
/// </summary>
/// <param name="memoryCache">A memory cache.</param>
/// <remarks>
/// <para>
/// Do not instantiate directly. Use <see cref="CachingServiceCollectionExtensions.AddSingleUserCliCacheService"/>.
/// </para>
/// <para>
/// For multi-user web API scenarios, use <see cref="HttpServiceCacheService"/>.
/// </para>
/// </remarks>
public class SingleUserCliCacheService(IMemoryCache memoryCache) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;

    // Use ConcurrentDictionary with lock object per group to ensure thread-safe HashSet access.
    private static readonly ConcurrentDictionary<string, HashSet<string>> s_groupKeys = new();
    private static readonly ConcurrentDictionary<string, object> s_groupLocks = new();

    public ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = GetGroupKey(group, key);
        return _memoryCache.TryGetValue(cacheKey, out T? value) ? new ValueTask<T?>(value) : default;
    }

    public ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (data == null)
            return default;

        string cacheKey = GetGroupKey(group, key);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        _memoryCache.Set(cacheKey, data, options);

        // Track the key in the group with thread-safe access to the HashSet.
        var groupLock = s_groupLocks.GetOrAdd(group, static _ => new object());
        lock (groupLock)
        {
            var keys = s_groupKeys.GetOrAdd(group, static _ => new HashSet<string>());
            keys.Add(key);
        }

        return default;
    }

    public ValueTask DeleteAsync(string group, string key, CancellationToken cancellationToken)
    {
        string cacheKey = GetGroupKey(group, key);
        _memoryCache.Remove(cacheKey);

        // Remove from group tracking with thread-safe access.
        if (s_groupLocks.TryGetValue(group, out var groupLock))
        {
            lock (groupLock)
            {
                if (s_groupKeys.TryGetValue(group, out var keys))
                {
                    keys.Remove(key);
                }
            }
        }

        return default;
    }

    public ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group, CancellationToken cancellationToken)
    {
        if (s_groupKeys.TryGetValue(group, out var keys))
        {
            // Return a snapshot to avoid concurrent modification during enumeration.
            var groupLock = s_groupLocks.GetOrAdd(group, static _ => new object());
            lock (groupLock)
            {
                return new ValueTask<IEnumerable<string>>(keys.ToArray().AsEnumerable());
            }
        }

        return new ValueTask<IEnumerable<string>>([]);
    }

    public ValueTask ClearAsync(CancellationToken cancellationToken)
    {
        // Clear all items from the memory cache
        if (_memoryCache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
        }

        // Clear all group tracking
        s_groupKeys.Clear();
        s_groupLocks.Clear();

        return default;
    }

    public ValueTask ClearGroupAsync(string group, CancellationToken cancellationToken)
    {
        // If this group doesn't exist, nothing to do
        if (!s_groupKeys.TryGetValue(group, out var keys))
        {
            return default;
        }

        // Snapshot the keys under lock, then remove from cache outside the lock.
        string[] keysSnapshot;
        if (s_groupLocks.TryGetValue(group, out var groupLock))
        {
            lock (groupLock)
            {
                keysSnapshot = [.. keys];
            }
        }
        else
        {
            keysSnapshot = [.. keys];
        }

        // Remove each key in the group from the cache
        foreach (var key in keys)
        {
            string cacheKey = GetGroupKey(group, key);
            _memoryCache.Remove(cacheKey);
        }

        // Remove the group from tracking
        s_groupKeys.TryRemove(group, out _);
        s_groupLocks.TryRemove(group, out _);

        return default;
    }

    private static string GetGroupKey(string group, string key) => $"{group}:{key}";
}
