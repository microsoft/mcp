// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.ResourceManager.EventHubs;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Services;

public class EventHubsService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<EventHubsService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IEventHubsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<EventHubsService> _logger = logger;

    public async Task<List<EventHubsNamespaceInfo>> GetNamespacesAsync(
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.EventHub/namespaces",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToEventHubsNamespaceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Event Hubs namespaces. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                resourceGroup, subscription);
            throw;
        }
    }

    private static EventHubsNamespaceInfo ConvertToEventHubsNamespaceInfo(JsonElement item)
    {
        // Parse the Resource Graph JSON element to extract Event Hubs namespace information
        var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "Unknown" : "Unknown";
        var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? "Unknown" : "Unknown";

        // Extract resource group from resource ID
        var resourceGroup = ExtractResourceGroupFromId(id);

        return new EventHubsNamespaceInfo(
            Name: name,
            Id: id,
            ResourceGroup: resourceGroup);
    }

    public async Task<EventHubsNamespaceDetails?> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

            // Get by resource group and name
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            var response = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);
            var namespaceResource = response.Value;

            return ConvertToEventHubsNamespaceDetails(namespaceResource.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Event Hubs namespace. NamespaceName: {NamespaceName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                namespaceName, resourceGroup, subscription);
            throw;
        }
    }

    private static EventHubsNamespaceDetails ConvertToEventHubsNamespaceDetails(EventHubsNamespaceData data)
    {
        return new EventHubsNamespaceDetails(
            Name: data.Name,
            Id: data.Id.ToString(),
            ResourceGroup: ExtractResourceGroupFromId(data.Id.ToString()),
            Location: data.Location.ToString(),
            Sku: data.Sku != null ? new EventHubsNamespaceSku(
                Name: data.Sku.Name.ToString(),
                Tier: data.Sku.Tier.ToString(),
                Capacity: data.Sku.Capacity) : null,
            Status: data.Status?.ToString(),
            ProvisioningState: data.ProvisioningState?.ToString(),
            CreationTime: data.CreatedOn,
            UpdatedTime: data.UpdatedOn,
            ServiceBusEndpoint: data.ServiceBusEndpoint,
            MetricId: data.MetricId,
            IsAutoInflateEnabled: data.IsAutoInflateEnabled,
            MaximumThroughputUnits: data.MaximumThroughputUnits,
            KafkaEnabled: data.KafkaEnabled,
            ZoneRedundant: data.ZoneRedundant,
            Tags: data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    private static string ExtractResourceGroupFromId(string resourceId)
    {
        // Resource ID format: /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/...
        var parts = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var resourceGroupIndex = Array.IndexOf(parts, "resourceGroups");

        return resourceGroupIndex >= 0 && resourceGroupIndex + 1 < parts.Length
            ? parts[resourceGroupIndex + 1]
            : "Unknown";
    }
}
