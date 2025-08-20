// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;

namespace Azure.Mcp.Tools.Redis.Services;

public interface IRedisService
{
    /// <summary>
    /// Lists Azure Cache for Redis caches (Basic, Standard, and Premium tier caches) in the specified subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="authMethod">Authentication method to use</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of Redis Cache details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<Cache>> ListCachesAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);


    /// <summary>
    /// Lists Azure Managed Redis and Azure Redis Enterprise clusters (`Balanced`, `MemoryOptimized`, `FlashOptimized`, `ComputeOptimized`, `Enterprise`, `EnterpriseFlash` tier clusters) in the specified subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="authMethod">Authentication method to use</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of Redis Cluster details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<Cluster>> ListClustersAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Lists the databases in the specified Redis cluster.
    /// </summary>
    /// <param name="clusterName">Name of the Redis cluster</param>
    /// <param name="resourceGroupName">Name of the resource group containing the Redis cluster</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="authMethod">Authentication method to use</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of database details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<Database>> ListDatabasesAsync(
        string clusterName,
        string resourceGroupName,
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Lists the access policy assignments in the specified Redis cache.
    /// </summary>
    /// <param name="cacheName">Name of the Redis cache</param>
    /// <param name="resourceGroupName">Name of the resource group containing the Redis cache</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="authMethod">Authentication method to use</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of access policy assignments</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<AccessPolicyAssignment>> ListAccessPolicyAssignmentsAsync(
        string cacheName,
        string resourceGroupName,
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

}
