// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Mcp.Core.Models;

namespace Microsoft.Mcp.Core.Services.Caching;

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

    private static readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> s_groupKeys = new();

    public ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = CacheKeyBuilder.Build(group, key);
        return _memoryCache.TryGetValue(cacheKey, out T? value) ? new ValueTask<T?>(value) : default;
    }

    public ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (data == null)
            return default;

        string cacheKey = CacheKeyBuilder.Build(group, key);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        _memoryCache.Set(cacheKey, data, options);

        s_groupKeys.AddOrUpdate(
            group,
            [key],
            (_, keys) =>
            {
                keys.Add(key);
                return keys;
            });

        return default;
    }

    public ValueTask DeleteAsync(string group, string key, CancellationToken cancellationToken)
    {
        string cacheKey = CacheKeyBuilder.Build(group, key);
        _memoryCache.Remove(cacheKey);

        // Remove from group tracking
        if (s_groupKeys.TryGetValue(group, out var keys))
        {
            keys.Remove(key);
        }

        return default;
    }

    public ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group, CancellationToken cancellationToken)
    {
        if (s_groupKeys.TryGetValue(group, out var keys))
        {
            // Return a snapshot to avoid concurrent modification during enumeration.
            return new ValueTask<IEnumerable<string>>([.. keys]);
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

        return default;
    }

    public ValueTask ClearGroupAsync(string group, CancellationToken cancellationToken)
    {
        // If this group doesn't exist, nothing to do
        if (!s_groupKeys.TryGetValue(group, out var keys))
        {
            return default;
        }

        string[] keysSnapshot = [.. keys];

        // Remove each key in the group from the cache
        foreach (var key in keysSnapshot)
        {
            string cacheKey = CacheKeyBuilder.Build(group, key);
            _memoryCache.Remove(cacheKey);
        }

        // Remove the group from tracking
        s_groupKeys.TryRemove(group, out _);

        return default;
    }
}
