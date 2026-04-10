// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Services.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.DeviceRegistry.Services;

public class DeviceRegistryService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<DeviceRegistryService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IDeviceRegistryService
{
    private readonly ILogger<DeviceRegistryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ResourceQueryResults<DeviceRegistryNamespaceInfo>> ListNamespacesAsync(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.DeviceRegistry/namespaces",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToNamespaceInfoModel,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error listing Device Registry namespaces. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                subscription,
                resourceGroup);
            throw;
        }
    }

    private static DeviceRegistryNamespaceInfo ConvertToNamespaceInfoModel(JsonElement item)
    {
        var data = DeviceRegistryNamespaceData.FromJson(item);
        if (data == null)
        {
            throw new InvalidOperationException("Failed to deserialize Device Registry Namespace data.");
        }
        return new DeviceRegistryNamespaceInfo(
            Name: data.ResourceName ?? string.Empty,
            Id: data.ResourceId,
            Location: data.Location,
            ProvisioningState: data.Properties?.ProvisioningState,
            Uuid: data.Properties?.Uuid,
            ResourceGroup: data.ResourceGroup,
            Type: data.ResourceType);
    }
}
