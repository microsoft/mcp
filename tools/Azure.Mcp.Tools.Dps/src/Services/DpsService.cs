// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Dps.Models;

namespace Azure.Mcp.Tools.Dps.Services;

/// <summary>
/// Service for Device Provisioning Service operations.
/// </summary>
/// <param name="subscriptionService">Service for Azure subscription operations.</param>
/// <param name="tenantService">Service for Azure tenant operations.</param>
public class DpsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService) : BaseAzureResourceService(subscriptionService, tenantService), IDpsService
{
    /// <inheritdoc/>
    public async Task<List<DpsInstanceInfo>> ListInstancesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.Devices/provisioningServices",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToDpsInstanceInfo,
            cancellationToken: cancellationToken);
    }

    private static string ExtractResourceGroupFromId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return string.Empty;
        }
        var parts = id.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rgIndex = Array.IndexOf(parts, "resourceGroups");
        if (rgIndex >= 0 && rgIndex < parts.Length - 1)
        {
            return parts[rgIndex + 1];
        }
        return string.Empty;
    }

    private static DpsInstanceInfo ConvertToDpsInstanceInfo(JsonElement item)
    {
        var id = item.GetProperty("id").GetString() ?? string.Empty;
        var name = item.GetProperty("name").GetString() ?? string.Empty;
        var location = item.GetProperty("location").GetString() ?? string.Empty;

        // Extract resource group from ID using helper
        var resourceGroup = ExtractResourceGroupFromId(id);

        var instance = new DpsInstanceInfo
        {
            Name = name,
            Id = id,
            ResourceGroup = resourceGroup,
            Location = location
        };

        // Extract properties if available
        if (item.TryGetProperty("properties", out var properties))
        {
            if (properties.TryGetProperty("provisioningState", out var provisioningState))
            {
                instance.ProvisioningState = provisioningState.GetString();
            }

            if (properties.TryGetProperty("serviceOperationsHostName", out var serviceOpsHost))
            {
                instance.ServiceOperationsHostName = serviceOpsHost.GetString();
            }

            if (properties.TryGetProperty("deviceProvisioningHostName", out var deviceProvHost))
            {
                instance.DeviceProvisioningHostName = deviceProvHost.GetString();
            }

            if (properties.TryGetProperty("idScope", out var idScope))
            {
                instance.IdScope = idScope.GetString();
            }

            if (properties.TryGetProperty("allocationPolicy", out var allocationPolicy))
            {
                instance.AllocationPolicy = allocationPolicy.GetString();
            }

            if (properties.TryGetProperty("state", out var state))
            {
                instance.State = state.GetString();
            }
        }

        // Extract SKU if available
        if (item.TryGetProperty("sku", out var sku))
        {
            if (sku.TryGetProperty("name", out var skuName))
            {
                instance.Sku = skuName.GetString();
            }
        }

        return instance;
    }
}
