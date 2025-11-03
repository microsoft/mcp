namespace Azure.Mcp.Core.Services.Caching;

/// <summary>
/// An implementation of <see cref="ICacheService"/> for multi-user web API scenarios.
/// </summary>
/// <param name="memoryCache">A memory cache.</param>
/// <remarks>
/// <para>
/// Do not instantiate directly. Use <see cref="CachingServiceCollectionExtensions.AddHttpServiceCacheService"/>.
/// </para>
/// <para>
/// For single-user CLI scenarios, use <see cref="SingleUserCliCacheService"/>.
/// </para>
/// </remarks>
// TODO implement this with IHttpContextAccessor + IMemoryCache. This is a no-op stub for now
// until we decide how to handle this. For example, do we only cache within a request rather
// than across requests by the same user? What are the implication of advanced Entra features
// like Conditional Access on caching?
public class HttpServiceCacheService : ICacheService
{
    public ValueTask ClearAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearGroupAsync(string group)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(string group, string key)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<T?> GetAsync<T>(string group, string key, TimeSpan? expiration = null)
    {
        return ValueTask.FromResult<T?>(default);
    }

    public ValueTask<IEnumerable<string>> GetGroupKeysAsync(string group)
    {
        return ValueTask.FromResult<IEnumerable<string>>(Array.Empty<string>());
    }

    public ValueTask SetAsync<T>(string group, string key, T data, TimeSpan? expiration = null)
    {
        return ValueTask.CompletedTask;
    }
}
