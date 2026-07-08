// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IIoTHubService
{
    public async Task<ResourceQueryResults<IoTHubDescription>> GetIoTHub(
        string? name,
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        string? additionalFilter = null;
        if (!string.IsNullOrEmpty(name))
        {
            var escapedName = name.Replace("'", "''", StringComparison.Ordinal);
            additionalFilter = $"name =~ '{escapedName}'";
        }

        var result = await ExecuteResourceQueryAsync(
            "Microsoft.Devices/IotHubs",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToIoTHubDescription,
            additionalFilter: additionalFilter,
            cancellationToken: cancellationToken,
            tenant: tenant);

        return result;
    }

    private static IoTHubDescription ConvertToIoTHubDescription(JsonElement element)
    {
        var id = element.TryGetProperty("id", out var idProperty)
            ? idProperty.GetString() ?? string.Empty
            : string.Empty;
        var hubName = element.TryGetProperty("name", out var nameProperty)
            ? nameProperty.GetString() ?? string.Empty
            : string.Empty;
        var location = element.TryGetProperty("location", out var locationProperty)
            ? locationProperty.GetString() ?? string.Empty
            : string.Empty;
        var resourceGroup = element.TryGetProperty("resourceGroup", out var resourceGroupProperty)
            ? resourceGroupProperty.GetString() ?? string.Empty
            : string.Empty;
        var subscriptionId = element.TryGetProperty("subscriptionId", out var subscriptionIdProperty)
            ? subscriptionIdProperty.GetString() ?? string.Empty
            : string.Empty;

        string skuName = string.Empty;
        long capacity = 0;
        if (element.TryGetProperty("sku", out var sku))
        {
            skuName = sku.TryGetProperty("name", out var skuNameProperty)
                ? skuNameProperty.GetString() ?? string.Empty
                : string.Empty;
            capacity = sku.TryGetProperty("capacity", out var capacityProperty)
                ? capacityProperty.GetInt64()
                : 0;
        }

        string state = "Unknown";
        string hostName = string.Empty;
        if (element.TryGetProperty("properties", out var properties))
        {
            state = properties.TryGetProperty("state", out var stateProperty)
                ? stateProperty.GetString() ?? "Unknown"
                : "Unknown";
            hostName = properties.TryGetProperty("hostName", out var hostNameProperty)
                ? hostNameProperty.GetString() ?? string.Empty
                : string.Empty;
        }

        return new IoTHubDescription(
            id,
            hubName,
            location,
            resourceGroup,
            subscriptionId,
            skuName,
            capacity,
            state,
            hostName
        );
    }
}
