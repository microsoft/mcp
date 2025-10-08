// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.EventHubs.Commands;
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
        ValidateRequiredParameters(subscription);

        try
        {
            var namespaceDetails = await ExecuteSingleResourceQueryAsync(
                            "Microsoft.EventHub/namespaces",
                            resourceGroup,
                            subscription,
                            retryPolicy,
                            ConvertToNamespace,
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

    public async Task<Namespace> UpdateNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? location = null,
        string? skuName = null,
        string? skuTier = null,
        int? skuCapacity = null,
        bool? isAutoInflateEnabled = null,
        int? maximumThroughputUnits = null,
        bool? kafkaEnabled = null,
        bool? zoneRedundant = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(namespaceName, resourceGroup, subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Data.SubscriptionId;

            var armClient = await CreateArmClientAsync(tenant, retryPolicy);
            var namespaceId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.EventHub/namespaces/{namespaceName}");

            // Get existing namespace to retrieve current values
            var existingNamespace = await GetGenericResourceAsync(armClient, namespaceId);

            // Determine location - use provided or existing
            var actualLocation = !string.IsNullOrEmpty(location) 
                ? new AzureLocation(location) 
                : existingNamespace.Data.Location;

            // Build update content
            var updateContent = new Models.EventHubsNamespaceData
            {
                ResourceId = namespaceId.ToString(),
                ResourceType = "Microsoft.EventHub/namespaces",
                ResourceName = namespaceName,
                Location = actualLocation.ToString(),
                Sku = new Azure.Mcp.Core.Services.Azure.Models.ResourceSku
                {
                    Name = skuName ?? existingNamespace.Data.Sku?.Name ?? "Standard",
                    Tier = skuTier ?? existingNamespace.Data.Sku?.Tier ?? "Standard",
                    Capacity = skuCapacity ?? existingNamespace.Data.Sku?.Capacity
                },
                Tags = tags ?? (existingNamespace.Data.Tags != null ? new Dictionary<string, string>(existingNamespace.Data.Tags) : null),
                Properties = new EventHubsNamespaceProperties
                {
                    IsAutoInflateEnabled = isAutoInflateEnabled,
                    MaximumThroughputUnits = maximumThroughputUnits,
                    KafkaEnabled = kafkaEnabled,
                    ZoneRedundant = zoneRedundant
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                namespaceId,
                actualLocation,
                updateContent,
                EventHubsJsonContext.Default.EventHubsNamespaceData);

            if (!result.HasData)
            {
                throw new InvalidOperationException($"Failed to update Event Hubs namespace '{namespaceName}'");
            }

            // Retrieve the updated namespace using Resource Graph to get the complete state
            var updatedNamespace = await GetNamespaceAsync(namespaceName, resourceGroup, subscription, tenant, retryPolicy);
            return updatedNamespace;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating Event Hubs namespace '{NamespaceName}' in resource group '{ResourceGroup}'",
                namespaceName, resourceGroup);
            throw;
        }
    }

    public async Task<bool> DeleteNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(namespaceName, resourceGroup, subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Data.SubscriptionId;

            var armClient = await CreateArmClientAsync(tenant, retryPolicy);
            var namespaceId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.EventHub/namespaces/{namespaceName}");

            // Get the namespace resource
            var namespaceResource = await GetGenericResourceAsync(armClient, namespaceId);

            // Delete the namespace
            await namespaceResource.DeleteAsync(WaitUntil.Completed);

            _logger.LogInformation(
                "Successfully deleted Event Hubs namespace '{NamespaceName}' from resource group '{ResourceGroup}'",
                namespaceName, resourceGroup);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting Event Hubs namespace '{NamespaceName}' from resource group '{ResourceGroup}'",
                namespaceName, resourceGroup);
            throw;
        }
    }

}
