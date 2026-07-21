// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.ResourceManager.IotHub;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IIoTHubService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    public async Task<List<IoTHubDescription>> GetIoTHub(
        string? name,
        string? resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        string? additionalFilter = null;
        if (!string.IsNullOrEmpty(name))
        {
            additionalFilter = $"name =~ '{name}'";
        }

        return await ExecuteResourceQueryAsync(
            "Microsoft.Devices/IotHubs",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToIoTHubDescription,
            additionalFilter: additionalFilter,
            cancellationToken: cancellationToken);
    }

    public async Task<List<IoTHubKey>> GetIoTHubKeys(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var subResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var rg = await subResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken);
        var hub = await rg.Value.GetIotHubDescriptionAsync(name, cancellationToken);

        var keys = new List<IoTHubKey>();
        await foreach (var key in hub.Value.GetKeysAsync(cancellationToken))
        {
            keys.Add(new IoTHubKey(key.KeyName, key.PrimaryKey, key.SecondaryKey, key.Rights.ToString()));
        }
        return keys;
    }

    private IoTHubDescription ConvertToIoTHubDescription(JsonElement element)
    {
        var properties = element.GetProperty("properties");
        var sku = element.GetProperty("sku");

        return new IoTHubDescription(
            element.GetProperty("id").GetString()!,
            element.GetProperty("name").GetString()!,
            element.GetProperty("location").GetString()!,
            element.GetProperty("resourceGroup").GetString()!,
            element.GetProperty("subscriptionId").GetString()!,
            sku.GetProperty("name").GetString()!,
            sku.TryGetProperty("capacity", out var cap) ? cap.GetInt64() : 0,
            properties.TryGetProperty("state", out var state) ? state.GetString()! : "Unknown",
            properties.TryGetProperty("hostName", out var hostName) ? hostName.GetString()! : ""
        );
    }
}
