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
    private readonly ITenantService _tenantService = tenantService;
    private readonly ILogger<EventHubsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<Namespace>> GetNamespacesAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var namespaces = await ExecuteResourceQueryAsync(
                "Microsoft.EventHub/namespaces",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToNamespace);
        return namespaces ?? [];
    }

    private static Namespace ConvertToNamespace(JsonElement item)
    {
        Models.EventHubsNamespaceData? eventHubsNamespace = Models.EventHubsNamespaceData.FromJson(item);
        if (eventHubsNamespace == null)
        {
            throw new InvalidOperationException("Failed to parse EventHubs namespace data");
        }


        if (string.IsNullOrEmpty(eventHubsNamespace.ResourceId))
        {
            throw new InvalidOperationException("Resource ID is missing");
        }

        var id = new ResourceIdentifier(eventHubsNamespace.ResourceId)!;

        if (string.IsNullOrEmpty(id.ResourceGroupName))
        {
            throw new InvalidOperationException("Resource ID is missing resource group");
        }

        if (string.IsNullOrEmpty(eventHubsNamespace.ResourceName))
        {
            throw new InvalidOperationException("Resource Name is missing");
        }

        return new Namespace(
            Name: eventHubsNamespace.ResourceName,
            Id: eventHubsNamespace.ResourceId,
            ResourceGroup: id.ResourceGroupName,
            Location: eventHubsNamespace.Location,
            Sku: new EventHubsNamespaceSku(
                Name: eventHubsNamespace.Sku.Name,
                Tier: eventHubsNamespace.Sku.Tier,
                Capacity: eventHubsNamespace.Sku.Capacity),
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
            Tags: eventHubsNamespace.Tags != null ? new Dictionary<string, string>(eventHubsNamespace.Tags) : null);
    }

    public async Task<Namespace> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var namespaceDetails = await ExecuteSingleResourceQueryAsync(
                            "Microsoft.EventHub/namespaces",
                            resourceGroup: resourceGroup,
                            subscription: subscription,
                            retryPolicy: retryPolicy,
                            converter: ConvertToNamespace,
                            additionalFilter: $"name =~ '{EscapeKqlString(namespaceName)}'");

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

}
