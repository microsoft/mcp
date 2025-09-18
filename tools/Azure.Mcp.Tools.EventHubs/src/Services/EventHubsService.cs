// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
                "Error getting EventHubs namespaces. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                resourceGroup, subscription);
            throw;
        }
    }

    private static EventHubsNamespaceInfo ConvertToEventHubsNamespaceInfo(JsonElement item)
    {
        // Parse the Resource Graph JSON element to extract EventHubs namespace information
        var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "Unknown" : "Unknown";
        var id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? "Unknown" : "Unknown";

        // Extract resource group from resource ID
        var resourceGroup = ExtractResourceGroupFromId(id);

        return new EventHubsNamespaceInfo(
            Name: name,
            Id: id,
            ResourceGroup: resourceGroup);
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
