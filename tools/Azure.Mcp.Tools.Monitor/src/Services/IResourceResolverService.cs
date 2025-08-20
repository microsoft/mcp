// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// Service interface for resolving Azure resource identifiers
/// </summary>
public interface IResourceResolverService
{
    /// <summary>
    /// Resolves a resource identifier from provided parameters
    /// </summary>
    /// <param name="subscription">The subscription ID</param>
    /// <param name="resourceGroup">The resource group name (optional)</param>
    /// <param name="resourceType">The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')</param>
    /// <param name="resourceName">The resource name or full resource ID</param>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <param name="retryPolicy">Optional retry policy parameters</param>
    /// <returns>The full Azure resource ID</returns>
    Task<ResourceIdentifier> ResolveResourceIdAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
