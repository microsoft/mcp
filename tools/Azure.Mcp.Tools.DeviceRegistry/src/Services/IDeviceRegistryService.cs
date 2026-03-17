// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.DeviceRegistry.Models;

namespace Azure.Mcp.Tools.DeviceRegistry.Services;

public interface IDeviceRegistryService
{
    Task<ResourceQueryResults<DeviceRegistryNamespaceInfo>> ListNamespacesAsync(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
