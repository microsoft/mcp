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
    /// <param name="authMethod">Authentication method to use</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of Redis resource details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<Resource>> ListResourcesAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

}
