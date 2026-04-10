// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Azure.Mcp.Core.Services.Caching;

/// <summary>
/// Extension methods for configuring cache services.
/// </summary>
public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="SingleUserCliCacheService"/> as an <see cref="ICacheService"/> with lifetime
    /// <see cref="ServiceLifetime.Singleton"/> into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This method registers the single-user CLI cache service which is appropriate for
    /// single-user command-line scenarios where all cached data belongs to a single user.
    /// </para>
    /// <para>
    /// This method will not override any existing <see cref="ICacheService"/> registration.
    /// It can be overridden as needed by specific configurations.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddSingleUserCliCacheService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICacheService, SingleUserCliCacheService>();
        return services;
    }

    /// <summary>
    /// Adds <see cref="HttpServiceCacheService"/> as an <see cref="ICacheService"/> with lifetime
    /// <see cref="ServiceLifetime.Singleton"/> into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This method registers the HTTP service cache service which is appropriate for
    /// multi-user web API scenarios where cached data must be partitioned by user.
    /// </para>
    /// <para>
    /// This method will override any existing <see cref="ICacheService"/> registration.
    /// This is unlike <see cref="AddSingleUserCliCacheService"/>.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddHttpServiceCacheService(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, HttpServiceCacheService>();
        return services;
    }
}
