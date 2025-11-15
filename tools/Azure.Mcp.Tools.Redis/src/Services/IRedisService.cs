// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Models;

namespace Azure.Mcp.Tools.Redis.Services;

public interface IRedisService
{
    /// <summary>
    /// Lists Azure Managed Redis, Azure Redis Enterprise, and Azure Cache for Redis resources in the specified subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>List of Redis resource details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<Resource>> ListResourcesAsync(
    string subscription,
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null,
    CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new Redis resource in the specified subscription and resource group.
    /// Returns when the resource deployment is initiated.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group this resource will be created in</param>
    /// <param name="name">The name of the Redis resource to create</param>
    /// <param name="location">The location/region for the Redis resource</param>
    /// <param name="sku">The requested SKU to create (default "Balanced_B0")</param>
    /// <param name="accessKeyAuthenticationEnabled">Whether to use access keys for authentication (default false)</param>
    /// <param name="modules">The modules to enable (e.g. "RedisJSON", "RedisBloom")</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Details of the Redis resource being created.</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<Resource> CreateResourceAsync(
        string subscription,
        string resourceGroup,
        string name,
        string location,
        string? sku,
        bool? accessKeyAuthenticationEnabled,
        string[]? modules = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
