// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.EventHubs.Models;
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
        ValidateRequiredParameters((nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var namespaces = new List<Namespace>();

            if (!string.IsNullOrEmpty(resourceGroup))
            {
                // Get namespaces from specific resource group
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

                if (resourceGroupResource?.Value == null)
                {
                    throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
                }

                await foreach (var namespaceResource in resourceGroupResource.Value.GetEventHubsNamespaces())
                {
                    namespaces.Add(ConvertToNamespace(namespaceResource.Data, resourceGroup));
                }
            }
            else
            {
                // Get namespaces from all resource groups in subscription
                await foreach (var rg in subscriptionResource.GetResourceGroups())
                {
                    await foreach (var namespaceResource in rg.GetEventHubsNamespaces())
                    {
                        namespaces.Add(ConvertToNamespace(namespaceResource.Data, rg.Data.Name));
                    }
                }
            }

            return namespaces;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving Event Hubs namespaces for subscription '{Subscription}' and resource group '{ResourceGroup}'",
                subscription, resourceGroup);
            throw;
        }
    }

    private static Namespace ConvertToNamespace(EventHubsNamespaceData namespaceData, string resourceGroup)
    {
        return new Namespace(
        Name: namespaceData.Name,
        Id: namespaceData.Id.ToString(),
        ResourceGroup: resourceGroup,
        Location: namespaceData.Location.ToString(),
        Sku: new(
            Name: namespaceData.Sku.Name.ToString(),
            Tier: namespaceData.Sku.Tier.ToString(),
            Capacity: namespaceData.Sku.Capacity
        ),
        Status: namespaceData.Status?.ToString(),
        ProvisioningState: namespaceData.ProvisioningState?.ToString(),
        CreationTime: namespaceData.CreatedOn,
        UpdatedTime: namespaceData.UpdatedOn,
        ServiceBusEndpoint: namespaceData.ServiceBusEndpoint,
        MetricId: namespaceData.MetricId,
        IsAutoInflateEnabled: namespaceData.IsAutoInflateEnabled,
        MaximumThroughputUnits: namespaceData.MaximumThroughputUnits,
        KafkaEnabled: namespaceData.KafkaEnabled,
        ZoneRedundant: namespaceData.ZoneRedundant,
        Tags: namespaceData.Tags != null ? new Dictionary<string, string>(namespaceData.Tags) : null);
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

            return ConvertToNamespace(namespaceResource.Value.Data, resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving Event Hubs namespace '{NamespaceName}' for subscription '{Subscription}'",
                namespaceName, subscription);
            throw;
        }
    }

    public async Task<Namespace> CreateOrUpdateNamespaceAsync(
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
        ValidateRequiredParameters((nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                throw new InvalidOperationException($"Resource group '{resourceGroup}' not found");
            }

            // Use resource group location if no location is provided
            var namespaceLocation = location ?? resourceGroupResource.Value.Data.Location.ToString();

            // Create namespace data with required properties
            var namespaceData = new EventHubsNamespaceData(namespaceLocation);

            // Set SKU if provided
            if (!string.IsNullOrEmpty(skuName))
            {
                namespaceData.Sku = new ResourceManager.EventHubs.Models.EventHubsSku(skuName)
                {
                    Tier = skuTier switch
                    {
                        "Basic" => EventHubsSkuTier.Basic,
                        "Standard" => EventHubsSkuTier.Standard,
                        "Premium" => EventHubsSkuTier.Premium,
                        _ => EventHubsSkuTier.Standard
                    },
                    Capacity = skuCapacity
                };
            }

            // Set optional properties
            if (isAutoInflateEnabled.HasValue)
            {
                namespaceData.IsAutoInflateEnabled = isAutoInflateEnabled.Value;
            }

            if (maximumThroughputUnits.HasValue)
            {
                namespaceData.MaximumThroughputUnits = maximumThroughputUnits.Value;
            }

            if (kafkaEnabled.HasValue)
            {
                namespaceData.KafkaEnabled = kafkaEnabled.Value;
            }

            if (zoneRedundant.HasValue)
            {
                namespaceData.ZoneRedundant = zoneRedundant.Value;
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (var tag in tags)
                {
                    namespaceData.Tags.Add(tag.Key, tag.Value);
                }
            }

            // Create or update the namespace
            var operation = await resourceGroupResource.Value.GetEventHubsNamespaces()
                .CreateOrUpdateAsync(WaitUntil.Completed, namespaceName, namespaceData);

            if (operation?.Value == null)
            {
                throw new InvalidOperationException($"Failed to create or update Event Hubs namespace '{namespaceName}'");
            }

            _logger.LogInformation(
                "Successfully created or updated Event Hubs namespace '{NamespaceName}' in resource group '{ResourceGroup}'",
                namespaceName, resourceGroup);

            return ConvertToNamespace(operation.Value.Data, resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating Event Hubs namespace '{NamespaceName}' in resource group '{ResourceGroup}' for subscription '{Subscription}'",
                namespaceName, resourceGroup, subscription);
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
        ValidateRequiredParameters((nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
            var subscriptionId = subscriptionResource.Data.SubscriptionId;

            var armClient = await CreateArmClientAsync(tenant, retryPolicy);
            var namespaceId = EventHubsNamespaceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, namespaceName);

            // Get the namespace resource
            var namespaceResource = await GetGenericResourceAsync(armClient, namespaceId);

            // Delete the namespace
            await namespaceResource.DeleteAsync(WaitUntil.Completed);

            _logger.LogInformation(
                "Successfully deleted Event Hubs namespace '{NamespaceName}' from resource group '{ResourceGroup}'",
                namespaceName, resourceGroup);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation(
                "Event Hubs namespace '{NamespaceName}' not found in resource group '{ResourceGroup}'",
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

    public async Task<List<EventHub>> GetEventHubsAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(namespaceName), namespaceName));

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

            var eventHubList = new List<EventHub>();

            await foreach (var eventHub in namespaceResource.Value.GetEventHubs())
            {
                eventHubList.Add(ConvertToEventHub(eventHub.Data, resourceGroup));
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

    public async Task<EventHub?> GetEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(namespaceName), namespaceName), (nameof(eventHubName), eventHubName));

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

            return ConvertToEventHub(eventHubResource.Value.Data, resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving event hub '{EventHubName}' in namespace '{NamespaceName}' for subscription '{Subscription}'",
                eventHubName, namespaceName, subscription);
            throw;
        }
    }

    private static EventHub ConvertToEventHub(EventHubData eventHub, string resourceGroup)
    {
        return new EventHub(
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

    public async Task<EventHub> CreateOrUpdateEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        int? partitionCount = null,
        long? messageRetentionInHours = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(namespaceName), namespaceName));

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

            if (messageRetentionInHours.HasValue)
            {
                eventHubData.RetentionDescription = new Azure.ResourceManager.EventHubs.Models.RetentionDescription
                {
                    RetentionTimeInHours = messageRetentionInHours.Value,
                    CleanupPolicy = Azure.ResourceManager.EventHubs.Models.CleanupPolicyRetentionDescription.Delete
                };
            }

            var operation = await namespaceResource.Value.GetEventHubs()
                .CreateOrUpdateAsync(WaitUntil.Completed, eventHubName, eventHubData);

            if (operation?.Value == null)
            {
                throw new InvalidOperationException($"Failed to create or update event hub '{eventHubName}'");
            }

            return ConvertToEventHub(operation.Value.Data, resourceGroup);
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
        ValidateRequiredParameters((nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

            try
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

                if (resourceGroupResource?.Value == null)
                {
                    _logger.LogInformation("Resource group '{ResourceGroup}' not found, event hub '{EventHubName}' cannot exist", resourceGroup, eventHubName);
                    return false;
                }

                var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetIfExistsAsync(namespaceName);

                if (namespaceResource?.Value == null)
                {
                    _logger.LogInformation("Event Hubs namespace '{NamespaceName}' not found in resource group '{ResourceGroup}', event hub '{EventHubName}' cannot exist", namespaceName, resourceGroup, eventHubName);
                    return false;
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
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInformation("Resource not found during event hub delete operation - considering successful. Event hub: '{EventHubName}'", eventHubName);
                return false;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("404"))
            {
                _logger.LogInformation("Resource not found during event hub delete operation - considering successful. Event hub: '{EventHubName}', Error: {ErrorMessage}", eventHubName, ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting event hub '{EventHubName}' in namespace '{NamespaceName}' for subscription '{Subscription}'",
                eventHubName, namespaceName, subscription);
            throw;
        }
    }

    public async Task<ConsumerGroup> CreateOrUpdateConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? userMetadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(consumerGroupName), consumerGroupName), (nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

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
        ValidateRequiredParameters((nameof(consumerGroupName), consumerGroupName), (nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));

            try
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                if (resourceGroupResource?.Value == null)
                {
                    _logger.LogInformation("Resource group '{ResourceGroup}' not found, consumer group '{ConsumerGroupName}' cannot exist", resourceGroup, consumerGroupName);
                    return false;
                }

                var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetIfExistsAsync(namespaceName);

                if (namespaceResource?.Value == null)
                {
                    _logger.LogInformation("Event Hubs namespace '{NamespaceName}' not found in resource group '{ResourceGroup}', consumer group '{ConsumerGroupName}' cannot exist", namespaceName, resourceGroup, consumerGroupName);
                    return false;
                }

                var eventHubResource = await namespaceResource.Value.GetEventHubs().GetIfExistsAsync(eventHubName);

                if (eventHubResource?.Value == null)
                {
                    _logger.LogInformation("Event hub '{EventHubName}' not found in namespace '{NamespaceName}', consumer group '{ConsumerGroupName}' cannot exist", eventHubName, namespaceName, consumerGroupName);
                    return false;
                }

                var consumerGroupResource = await eventHubResource.Value.GetEventHubsConsumerGroups().GetIfExistsAsync(consumerGroupName);

                if (consumerGroupResource?.Value == null)
                {
                    _logger.LogInformation("Consumer group '{ConsumerGroupName}' not found in event hub '{EventHubName}', nothing to delete", consumerGroupName, eventHubName);
                    return false;
                }

                await consumerGroupResource.Value.DeleteAsync(WaitUntil.Completed);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogInformation("Resource not found during consumer group delete operation - considering successful. Consumer group: '{ConsumerGroupName}'", consumerGroupName);
                return false;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("404"))
            {
                _logger.LogInformation("Resource not found during consumer group delete operation - considering successful. Consumer group: '{ConsumerGroupName}', Error: {ErrorMessage}", consumerGroupName, ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting consumer group '{ConsumerGroupName}' from Event Hub '{EventHubName}' of namespace '{NamespaceName}'",
                consumerGroupName, eventHubName, namespaceName);
            throw;
        }
    }

    public async Task<List<ConsumerGroup>> GetConsumerGroupsAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

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
        ValidateRequiredParameters((nameof(consumerGroupName), consumerGroupName), (nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

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
