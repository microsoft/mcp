// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Caching;

/// <summary>
/// No-operation cache service that performs no caching operations.
/// </summary>
public sealed class NoCacheService : ICacheService
{
    /// <summary>
    /// Returns default value without caching.
    /// </summary>
    public ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null)
    {
        return default;
    }

    /// <summary>
    /// No-operation set that doesn't cache anything.
    /// </summary>
    public ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null)
    {
        return default;
    }

    /// <summary>
    /// No-operation delete that doesn't delete anything.
    /// </summary>
    public ValueTask DeleteAsync(string group, string key)
    {
        return default;
    }

    /// <summary>
    /// Returns empty collection as no keys are cached.
    /// </summary>
    public ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group)
    {
        return new ValueTask<IEnumerable<string>>(Enumerable.Empty<string>());
    }

    /// <summary>
    /// No-operation clear that doesn't clear anything.
    /// </summary>
    public ValueTask ClearAsync()
    {
        return default;
    }

    /// <summary>
    /// No-operation clear group that doesn't clear anything.
    /// </summary>
    public ValueTask ClearGroupAsync(string group)
    {
        return default;
    }
}
