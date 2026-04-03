// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Mcp.Core.Services.Caching.Pagination;

namespace Microsoft.Mcp.Core.Services.Caching;

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

    /// <summary>
    /// Adds the pagination service infrastructure with <see cref="HostIdentityCallerIdentityResolver"/>
    /// as the default caller identity resolver. Appropriate for stdio mode and HTTP modes
    /// that use hosting environment identity.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This registers <see cref="ICursorRegistry"/>, <see cref="ICallerIdentityResolver"/>,
    /// and <see cref="IPaginationService"/> as singletons.
    /// </para>
    /// <para>
    /// For HTTP On-Behalf-Of scenarios, call <see cref="AddHttpOboPaginationService"/>
    /// after this method to override the caller identity resolver.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddPaginationService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICursorRegistry, CursorRegistry>();
        services.TryAddSingleton<ICallerIdentityResolver, HostIdentityCallerIdentityResolver>();
        services.TryAddSingleton<IPaginationService, PaginationService>();
        return services;
    }

    /// <summary>
    /// Overrides the caller identity resolver to use <see cref="HttpOboCallerIdentityResolver"/>
    /// for HTTP On-Behalf-Of mode. Must be called after <see cref="AddPaginationService"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddHttpOboPaginationService(this IServiceCollection services)
    {
        services.AddSingleton<ICallerIdentityResolver, HttpOboCallerIdentityResolver>();
        return services;
    }
}
