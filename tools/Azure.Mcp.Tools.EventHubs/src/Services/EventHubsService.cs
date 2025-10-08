// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.EventHubs.Commands;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.ResourceManager.EventHubs;
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

    public async Task<List<EventHubInfo>> ListEventHubsAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);

            if (namespaceResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'");
            }

            var eventHubList = new List<EventHubInfo>();

            await foreach (var eventHub in namespaceResource.Value.GetEventHubs())
            {
                eventHubList.Add(ConvertToEventHubInfo(eventHub.Data, resourceGroup));
            }

            return eventHubList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing event hubs in namespace '{NamespaceName}' for subscription '{Subscription}'",
                namespaceName, subscription);
            throw;
        }
    }

    public async Task<EventHubInfo?> GetEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);

            if (namespaceResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'");
            }

            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName);

            if (eventHubResource?.Value == null)
            {
                return null;
            }

            return ConvertToEventHubInfo(eventHubResource.Value.Data, resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving event hub '{EventHubName}' in namespace '{NamespaceName}' for subscription '{Subscription}'",
                eventHubName, namespaceName, subscription);
            throw;
        }
    }

    private static EventHubInfo ConvertToEventHubInfo(EventHubData eventHub, string resourceGroup)
    {
        return new EventHubInfo(
            Name: eventHub.Name,
            Id: eventHub.Id.ToString(),
            ResourceGroup: resourceGroup,
            Location: null, // Event hubs inherit location from namespace
            PartitionCount: eventHub.PartitionCount.HasValue ? (int)eventHub.PartitionCount.Value : null,
            MessageRetentionInDays: eventHub.RetentionDescription?.RetentionTimeInHours.HasValue == true
                ? (int)(eventHub.RetentionDescription.RetentionTimeInHours.Value / 24)
                : null,
            Status: eventHub.Status?.ToString(),
            CreatedOn: eventHub.CreatedOn,
            UpdatedOn: eventHub.UpdatedOn,
            PartitionIds: eventHub.PartitionIds?.ToList());
    }

    public async Task<EventHubInfo> CreateOrUpdateEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        int? partitionCount = null,
        long? messageRetentionInHours = null,
        string? status = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);

            if (namespaceResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'");
            }

            var eventHubData = new EventHubData();

            if (partitionCount.HasValue)
            {
                eventHubData.PartitionCount = partitionCount.Value;
            }

            if (messageRetentionInHours.HasValue && eventHubData.RetentionDescription != null)
            {
                eventHubData.RetentionDescription.RetentionTimeInHours = messageRetentionInHours.Value;
            }

            if (!string.IsNullOrEmpty(status) && eventHubData.Status.HasValue)
            {
                // Status is typically read-only, so we'll log a warning if attempted
                _logger.LogWarning("Status cannot be directly set on EventHub creation/update. Current status: {Status}", eventHubData.Status);
            }

            var operation = await namespaceResource.Value.GetEventHubs()
                .CreateOrUpdateAsync(WaitUntil.Completed, eventHubName, eventHubData);

            if (operation?.Value == null)
            {
                throw new InvalidOperationException($"Failed to create or update event hub '{eventHubName}'");
            }

            return ConvertToEventHubInfo(operation.Value.Data, resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating event hub '{EventHubName}' in namespace '{NamespaceName}' for subscription '{Subscription}'",
                eventHubName, namespaceName, subscription);
            throw;
        }
    }

    public async Task<bool> DeleteEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);

            if (namespaceResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'");
            }

            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetIfExistsAsync(eventHubName);

            if (eventHubResource?.Value == null)
            {
                _logger.LogInformation("Event hub '{EventHubName}' not found in namespace '{NamespaceName}', nothing to delete", eventHubName, namespaceName);
                return false;
            }

            await eventHubResource.Value.DeleteAsync(WaitUntil.Completed);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting event hub '{EventHubName}' in namespace '{NamespaceName}' for subscription '{Subscription}'",
                eventHubName, namespaceName, subscription);
            throw;
        }
    }

    public async Task<ConsumerGroup> UpdateConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? userMetadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(consumerGroupName, eventHubName, namespaceName, resourceGroup, subscription);

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);
            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName);

            var consumerGroupData = new EventHubsConsumerGroupData();
            if (!string.IsNullOrEmpty(userMetadata))
            {
                consumerGroupData.UserMetadata = userMetadata;
            }

            var operation = await eventHubResource.Value.GetEventHubsConsumerGroups().CreateOrUpdateAsync(
                WaitUntil.Completed,
                consumerGroupName,
                consumerGroupData);

            var consumerGroupResource = operation.Value;
            if (string.IsNullOrEmpty(consumerGroupResource.Id))
            {
                throw new InvalidOperationException("Consumer group resource ID is missing");
            }

            var resourceId = new ResourceIdentifier(consumerGroupResource.Id!);

            return new ConsumerGroup(
                Name: consumerGroupResource.Data.Name,
                Id: consumerGroupResource.Id!,
                ResourceGroup: resourceId.ResourceGroupName ?? resourceGroup,
                Namespace: namespaceName,
                EventHub: eventHubName,
                Location: consumerGroupResource.Data.Location?.ToString(),
                UserMetadata: consumerGroupResource.Data.UserMetadata,
                CreationTime: consumerGroupResource.Data.CreatedOn,
                UpdatedTime: consumerGroupResource.Data.UpdatedOn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating/updating consumer group '{ConsumerGroupName}' in Event Hub '{EventHubName}' of namespace '{NamespaceName}'",
                consumerGroupName, eventHubName, namespaceName);
            throw;
        }
    }

    public async Task<bool> DeleteConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(consumerGroupName, eventHubName, namespaceName, resourceGroup, subscription);

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);
            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName);

            var consumerGroupResource = await eventHubResource.Value.GetEventHubsConsumerGroups().GetAsync(consumerGroupName);

            await consumerGroupResource.Value.DeleteAsync(WaitUntil.Completed);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting consumer group '{ConsumerGroupName}' from Event Hub '{EventHubName}' of namespace '{NamespaceName}'",
                consumerGroupName, eventHubName, namespaceName);
            throw;
        }
    }

    public async Task<List<ConsumerGroup>> ListConsumerGroupsAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(eventHubName, namespaceName, resourceGroup, subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);

            if (namespaceResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'");
            }

            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName);

            if (eventHubResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hub '{eventHubName}' not found in namespace '{namespaceName}'");
            }

            var consumerGroups = new List<ConsumerGroup>();

            await foreach (var consumerGroup in eventHubResource.Value.GetEventHubsConsumerGroups())
            {
                consumerGroups.Add(ConvertToConsumerGroup(consumerGroup.Data, resourceGroup, namespaceName, eventHubName));
            }

            return consumerGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing consumer groups in Event Hub '{EventHubName}' of namespace '{NamespaceName}' for subscription '{Subscription}'",
                eventHubName, namespaceName, subscription);
            throw;
        }
    }

    public async Task<ConsumerGroup?> GetConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(consumerGroupName, eventHubName, namespaceName, resourceGroup, subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName);

            if (namespaceResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'");
            }

            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName);

            if (eventHubResource?.Value == null)
            {
                throw new KeyNotFoundException($"Event Hub '{eventHubName}' not found in namespace '{namespaceName}'");
            }

            var consumerGroupResource = await eventHubResource.Value.GetEventHubsConsumerGroups().GetIfExistsAsync(consumerGroupName);

            if (consumerGroupResource?.Value == null)
            {
                return null;
            }

            return ConvertToConsumerGroup(consumerGroupResource.Value.Data, resourceGroup, namespaceName, eventHubName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving consumer group '{ConsumerGroupName}' in Event Hub '{EventHubName}' of namespace '{NamespaceName}' for subscription '{Subscription}'",
                consumerGroupName, eventHubName, namespaceName, subscription);
            throw;
        }
    }

    private static ConsumerGroup ConvertToConsumerGroup(EventHubsConsumerGroupData consumerGroupData, string resourceGroup, string namespaceName, string eventHubName)
    {
        return new ConsumerGroup(
            Name: consumerGroupData.Name,
            Id: consumerGroupData.Id?.ToString() ?? string.Empty,
            ResourceGroup: resourceGroup,
            Namespace: namespaceName,
            EventHub: eventHubName,
            Location: consumerGroupData.Location?.ToString(),
            UserMetadata: consumerGroupData.UserMetadata,
            CreationTime: consumerGroupData.CreatedOn,
            UpdatedTime: consumerGroupData.UpdatedOn);
    }

}
