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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached value or default if not found.</returns>
    ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache using a group and key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="group">The group name.</param>
    /// <param name="key">The cache key within the group.</param>
    /// <param name="data">The data to cache.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a value from the cache using a group and key.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <param name="key">The cache key within the group.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask DeleteAsync(string group, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all keys in a specific group.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of keys in the specified group.</returns>
    ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask ClearAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Clears all items from a specific group in the cache.
    /// </summary>
    /// <param name="group">The group name to clear.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask ClearGroupAsync(string group, CancellationToken cancellationToken);
}
