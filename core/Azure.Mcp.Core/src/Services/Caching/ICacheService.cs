// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Caching;

public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache using a group and key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="group">The group name.</param>
    /// <param name="key">The cache key within the group.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <returns>The cached value or default if not found.</returns>
    ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null);

    /// <summary>
    /// Sets a value in the cache using a group and key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="group">The group name.</param>
    /// <param name="key">The cache key within the group.</param>
    /// <param name="data">The data to cache.</param>
    /// <param name="expiration">Optional expiration time.</param>
    ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null);

    /// <summary>
    /// Deletes a value from the cache using a group and key.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <param name="key">The cache key within the group.</param>
    ValueTask DeleteAsync(string group, string key);

    /// <summary>
    /// Gets all keys in a specific group.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <returns>A collection of keys in the specified group.</returns>
    ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group);

    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask ClearAsync();

    /// <summary>
    /// Clears all items from a specific group in the cache.
    /// </summary>
    /// <param name="group">The group name to clear.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask ClearGroupAsync(string group);
}
