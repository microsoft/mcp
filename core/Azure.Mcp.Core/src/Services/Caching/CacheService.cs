// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Azure.Mcp.Core.Services.Caching;

public class CacheService(IMemoryCache memoryCache) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private static readonly ConcurrentDictionary<string, HashSet<string>> s_groupKeys = new();

    public ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null)
    {
        string cacheKey = GetGroupKey(group, key);
        return _memoryCache.TryGetValue(cacheKey, out T? value) ? new ValueTask<T?>(value) : default;
    }

    public ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null)
    {
        if (data == null)
            return default;

        string cacheKey = GetGroupKey(group, key);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        _memoryCache.Set(cacheKey, data, options);

        // Track the key in the group
        s_groupKeys.AddOrUpdate(
            group,
            new HashSet<string> { key },
            (_, keys) =>
            {
                keys.Add(key);
                return keys;
            });

        return default;
    }

    public ValueTask DeleteAsync(string group, string key)
    {
        string cacheKey = GetGroupKey(group, key);
        _memoryCache.Remove(cacheKey);

        // Remove from group tracking
        if (s_groupKeys.TryGetValue(group, out var keys))
        {
            keys.Remove(key);
        }

        return default;
    }

    public ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group)
    {
        if (s_groupKeys.TryGetValue(group, out var keys))
        {
            return new ValueTask<IEnumerable<string>>(keys.AsEnumerable());
        }

        return new ValueTask<IEnumerable<string>>([]);
    }

    public ValueTask ClearAsync()
    {
        // Clear all items from the memory cache
        if (_memoryCache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
        }

        // Clear all group tracking
        s_groupKeys.Clear();

        return default;
    }

    public ValueTask ClearGroupAsync(string group)
    {
        // If this group doesn't exist, nothing to do
        if (!s_groupKeys.TryGetValue(group, out var keys))
        {
            return default;
        }

        // Remove each key in the group from the cache
        foreach (var key in keys)
        {
            string cacheKey = GetGroupKey(group, key);
            _memoryCache.Remove(cacheKey);
        }

        // Remove the group from tracking
        s_groupKeys.TryRemove(group, out _);

        return default;
    }

    private static string GetGroupKey(string group, string key) => $"{group}:{key}";
}
