// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.ResourceManager.DeviceRegistry;

namespace Azure.Mcp.Tools.DeviceRegistry.Services;

public interface IDeviceRegistryService
{
    Task<List<DeviceRegistryNamespaceResource>> GetNamespacesAsync(
        string subscriptionId,
        string? resourceGroupName = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DeviceRegistryNamespaceResource> CreateNamespaceAsync(
        string subscriptionId,
        string resourceGroupName,
        string namespaceName,
        string location,
        Dictionary<string, string>? tags = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DeviceRegistryNamespaceResource> GetNamespaceAsync(
        string subscriptionId,
        string resourceGroupName,
        string namespaceName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
