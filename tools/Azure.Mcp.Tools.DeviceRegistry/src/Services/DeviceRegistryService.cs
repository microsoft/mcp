// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Services.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.DeviceRegistry.Services;

public class DeviceRegistryService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IDeviceRegistryService
{
    public async Task<ResourceQueryResults<DeviceRegistryNamespaceInfo>> ListNamespacesAsync(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        return await ExecuteResourceQueryAsync(
            "Microsoft.DeviceRegistry/namespaces",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToNamespaceInfoModel,
            cancellationToken: cancellationToken);
    }

    private static DeviceRegistryNamespaceInfo ConvertToNamespaceInfoModel(JsonElement item)
    {
        var data = DeviceRegistryNamespaceData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to deserialize Device Registry Namespace data.");
        return new DeviceRegistryNamespaceInfo(
            Name: data.Name ?? string.Empty,
            Id: data.Id,
            Location: data.Location,
            ProvisioningState: data.Properties?.ProvisioningState,
            Uuid: data.Properties?.Uuid,
            ResourceGroup: data.ResourceGroup,
            Type: data.Type);
    }
}
