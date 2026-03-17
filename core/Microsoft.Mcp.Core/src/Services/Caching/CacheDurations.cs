// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Caching;

/// <summary>
/// Provides standardized cache duration constants for consistent caching behavior across all services.
/// </summary>
/// <remarks>
/// Cache durations are categorized by the type of data being cached:
/// <list type="bullet">
///   <item><description><see cref="Tenant"/> – Tenant information rarely changes, so a long cache duration is used.</description></item>
///   <item><description><see cref="Subscription"/> – Subscriptions are relatively stable but may change more frequently than tenants.</description></item>
///   <item><description><see cref="AuthenticatedClient"/> – Authenticated clients (e.g., SDK client instances) are more dynamic and should be refreshed more frequently.</description></item>
///   <item><description><see cref="ServiceData"/> – Service data is critical for user interactions and should have a short cache duration to avoid stale data.</description></item>
/// </list>
/// </remarks>
public static class CacheDurations
{
    /// <summary>
    /// Cache duration for tenant data (12 hours).
    /// Tenant information rarely changes for users.
    /// </summary>
    public static readonly TimeSpan Tenant = TimeSpan.FromHours(12);

    /// <summary>
    /// Cache duration for subscription data (2 hours).
    /// Subscriptions are relatively stable for most users.
    /// </summary>
    public static readonly TimeSpan Subscription = TimeSpan.FromHours(2);

    /// <summary>
    /// Cache duration for authenticated client instances (15 minutes).
    /// Clients such as SDK connections are more dynamic and may change frequently.
    /// </summary>
    public static readonly TimeSpan AuthenticatedClient = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Cache duration for service data (5 minutes).
    /// Service data is critical for user interactions. A short cache duration
    /// helps avoid stale data while reducing service load.
    /// </summary>
    public static readonly TimeSpan ServiceData = TimeSpan.FromMinutes(5);
}
