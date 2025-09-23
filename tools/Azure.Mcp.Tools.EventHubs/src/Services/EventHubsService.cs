// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.EventHubs.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Services;

public class EventHubsService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<EventHubsService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IEventHubsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<EventHubsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<EventHubsNamespaceInfo>> GetNamespacesAsync(
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.EventHub/namespaces",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToEventHubsNamespaceInfo);
    }

    private static EventHubsNamespaceInfo ConvertToEventHubsNamespaceInfo(JsonElement item)
    {
        Models.EventHubsNamespaceData? eventHubsNamespace = Models.EventHubsNamespaceData.FromJson(item);
        if (eventHubsNamespace == null)
            throw new InvalidOperationException("Failed to parse EventHubs namespace data");

        if (string.IsNullOrEmpty(eventHubsNamespace.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(eventHubsNamespace.ResourceId);

        return new EventHubsNamespaceInfo(
            Name: eventHubsNamespace.ResourceName ?? "Unknown",
            Id: eventHubsNamespace.ResourceId ?? "Unknown",
            ResourceGroup: id.ResourceGroupName ?? "Unknown");
    }

    public async Task<EventHubsNamespaceDetails> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var namespaceDetails = await ExecuteSingleResourceQueryAsync(
                            "Microsoft.EventHub/namespaces",
                            resourceGroup,
                            subscription,
                            retryPolicy,
                            ConvertToEventHubsNamespaceDetails,
                            $"name =~ '{EscapeKqlString(namespaceName)}'");

            if (namespaceDetails == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found for subscription '{subscription}'.");
            }
            return namespaceDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving Event Hubs namespace '{NamespaceName}' for subscription '{Subscription}'",
                namespaceName, subscription);
            throw;
        }
    }

    private static EventHubsNamespaceDetails ConvertToEventHubsNamespaceDetails(JsonElement item)
    {
        Models.EventHubsNamespaceData? eventHubsNamespace = Models.EventHubsNamespaceData.FromJson(item);
        if (eventHubsNamespace == null)
            throw new InvalidOperationException("Failed to parse EventHubs namespace data");

        if (string.IsNullOrEmpty(eventHubsNamespace.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(eventHubsNamespace.ResourceId);

        return new EventHubsNamespaceDetails(
            Name: eventHubsNamespace.ResourceName ?? "Unknown",
            Id: eventHubsNamespace.ResourceId,
            ResourceGroup: id.ResourceGroupName ?? "Unknown",
            Location: eventHubsNamespace.Location ?? "Unknown",
            Sku: eventHubsNamespace.Sku != null ? new EventHubsNamespaceSku(
                Name: eventHubsNamespace.Sku.Name,
                Tier: eventHubsNamespace.Sku.Tier,
                Capacity: eventHubsNamespace.Sku.Capacity) : null,
            Status: eventHubsNamespace.Properties?.Status,
            ProvisioningState: eventHubsNamespace.Properties?.ProvisioningState,
            CreationTime: eventHubsNamespace.Properties?.CreatedOn,
            UpdatedTime: eventHubsNamespace.Properties?.UpdatedOn,
            ServiceBusEndpoint: eventHubsNamespace.Properties?.ServiceBusEndpoint,
            MetricId: eventHubsNamespace.Properties?.MetricId,
            IsAutoInflateEnabled: eventHubsNamespace.Properties?.IsAutoInflateEnabled,
            MaximumThroughputUnits: eventHubsNamespace.Properties?.MaximumThroughputUnits,
            KafkaEnabled: eventHubsNamespace.Properties?.KafkaEnabled,
            ZoneRedundant: eventHubsNamespace.Properties?.ZoneRedundant,
            Tags: eventHubsNamespace.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }
}
