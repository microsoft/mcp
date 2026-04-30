// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Caching;

/// <summary>
/// An implementation of <see cref="ICacheService"/> that never caches.
/// </summary>
public sealed class NoopCacheService : ICacheService
{
    public ValueTask ClearAsync(CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public ValueTask ClearGroupAsync(string group, CancellationToken cancellationToken) => ValueTask.CompletedTask;

    public ValueTask DeleteAsync(string group, string key, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<T?>(default);

    public ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group, CancellationToken cancellationToken)
        => ValueTask.FromResult<IEnumerable<string>>(Array.Empty<string>());

    public ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
