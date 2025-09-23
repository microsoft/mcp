// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.EventGrid;
using Azure.ResourceManager.EventGrid.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.EventGrid.Services;

public class EventGridService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<EventGridService> logger)
    : BaseAzureService(tenantService), IEventGridService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<EventGridService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<EventGridTopicInfo>> GetTopicsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var topics = new List<EventGridTopicInfo>();

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            // Get topics from specific resource group
            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup);

            await foreach (var topic in resourceGroupResource.Value.GetEventGridTopics().GetAllAsync())
            {
                topics.Add(CreateTopicInfo(topic.Data));
            }
        }
        else
        {
            // Get topics from all resource groups in subscription
            await foreach (var topic in subscriptionResource.GetEventGridTopicsAsync())
            {
                topics.Add(CreateTopicInfo(topic.Data));
            }
        }

        return topics;
    }

    public async Task<List<EventGridSubscriptionInfo>> GetSubscriptionsAsync(
        string subscription,
        string? resourceGroup = null,
        string? topicName = null,
        string? location = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var subscriptions = new List<EventGridSubscriptionInfo>();

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

            // If specific topic is requested, get subscriptions for that topic only
            if (!string.IsNullOrEmpty(topicName))
            {
                await GetSubscriptionsForSpecificTopic(subscriptionResource, resourceGroup, topicName, location, subscriptions);
            }
            else
            {
                // Get subscriptions from all topics in the subscription or resource group
                await GetSubscriptionsFromAllTopics(subscriptionResource, resourceGroup, location, subscriptions);
            }
        }
        catch (Exception ex)
        {
            // Log the actual error instead of swallowing it
            throw new InvalidOperationException($"Failed to retrieve EventGrid subscriptions: {ex.Message}", ex);
        }

        return subscriptions;
    }

    private async Task GetSubscriptionsForSpecificTopic(
        SubscriptionResource subscriptionResource,
        string? resourceGroup,
        string topicName,
        string? location,
        List<EventGridSubscriptionInfo> subscriptions)
    {
        try
        {
            // Find the specific custom topic first
            var topic = await FindTopic(subscriptionResource, resourceGroup, topicName);
            if (topic != null)
            {
                await AddSubscriptionsFromTopic(topic.Data.Location.ToString(), location, subscriptions, topic.GetTopicEventSubscriptions().GetAllAsync());
                return; // Found custom topic, no need to check system topics
            }

            // If not found in custom topics, check system topics
            var systemTopic = await FindSystemTopic(subscriptionResource, resourceGroup, topicName);
            if (systemTopic != null)
            {
                await AddSubscriptionsFromSystemTopic(systemTopic.Data.Location.ToString(), location, subscriptions, systemTopic.GetSystemTopicEventSubscriptions().GetAllAsync());
            }
        }
        catch (Exception ex)
        {
            // Log and re-throw to preserve error information
            throw new InvalidOperationException($"Failed to get subscriptions for topic '{topicName}': {ex.Message}", ex);
        }
    }

    private async Task GetSubscriptionsFromAllTopics(
        SubscriptionResource subscriptionResource,
        string? resourceGroup,
        string? location,
        List<EventGridSubscriptionInfo> subscriptions)
    {
        try
        {
            if (!string.IsNullOrEmpty(resourceGroup))
            {
                // Get topics from specific resource group and their subscriptions
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

                // Check custom topics
                await foreach (var topic in resourceGroupResource.Value.GetEventGridTopics().GetAllAsync())
                {
                    try
                    {
                        await AddSubscriptionsFromTopic(topic.Data.Location.ToString(), location, subscriptions, topic.GetTopicEventSubscriptions().GetAllAsync());
                    }
                    catch (Exception ex)
                    {
                        // Continue with other topics if one fails - individual topic access errors
                        // shouldn't block the entire operation since we're aggregating from multiple topics
                        _logger.LogWarning(ex, "Failed to get subscriptions for topic '{TopicName}'. Continuing with other topics.", topic.Data.Name);
                        continue;
                    }
                }                // Also check system topics in the resource group
                await foreach (var systemTopic in resourceGroupResource.Value.GetSystemTopics().GetAllAsync())
                {
                    try
                    {
                        await AddSubscriptionsFromSystemTopic(systemTopic.Data.Location.ToString(), location, subscriptions, systemTopic.GetSystemTopicEventSubscriptions().GetAllAsync());
                    }
                    catch (Exception ex)
                    {
                        // Continue with other system topics if one fails - individual system topic access errors
                        // shouldn't block the entire operation since we're aggregating from multiple topics
                        _logger.LogWarning(ex, "Failed to get subscriptions for system topic '{SystemTopicName}'. Continuing with other topics.", systemTopic.Data.Name);
                        continue;
                    }
                }
            }
            else
            {
                // Get topics from all resource groups and their subscriptions
                await foreach (var topic in subscriptionResource.GetEventGridTopicsAsync())
                {
                    try
                    {
                        await AddSubscriptionsFromTopic(topic.Data.Location.ToString(), location, subscriptions, topic.GetTopicEventSubscriptions().GetAllAsync());
                    }
                    catch (Exception ex)
                    {
                        // Continue with other topics if one fails - individual topic access errors
                        // shouldn't block the entire operation since we're aggregating from multiple topics
                        _logger.LogWarning(ex, "Failed to get subscriptions for topic '{TopicName}'. Continuing with other topics.", topic.Data.Name);
                        continue;
                    }
                }

                // Also check system topics across all resource groups
                await foreach (var systemTopic in subscriptionResource.GetSystemTopicsAsync())
                {
                    try
                    {
                        await AddSubscriptionsFromSystemTopic(systemTopic.Data.Location.ToString(), location, subscriptions, systemTopic.GetSystemTopicEventSubscriptions().GetAllAsync());
                    }
                    catch (Exception ex)
                    {
                        // Continue with other system topics if one fails - individual system topic access errors
                        // shouldn't block the entire operation since we're aggregating from multiple topics
                        _logger.LogWarning(ex, "Failed to get subscriptions for system topic '{SystemTopicName}'. Continuing with other topics.", systemTopic.Data.Name);
                        continue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log and re-throw to preserve error information
            throw new InvalidOperationException($"Failed to get subscriptions from all topics in resource group '{resourceGroup}': {ex.Message}", ex);
        }
    }

    private async Task<EventGridTopicResource?> FindTopic(
        SubscriptionResource subscriptionResource,
        string? resourceGroup,
        string topicName)
    {
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            // Search in specific resource group
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            await foreach (var topic in resourceGroupResource.Value.GetEventGridTopics().GetAllAsync())
            {
                if (topic.Data.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase))
                {
                    return topic;
                }
            }
        }
        else
        {
            // Search in all resource groups
            await foreach (var topic in subscriptionResource.GetEventGridTopicsAsync())
            {
                if (topic.Data.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase))
                {
                    return topic;
                }
            }
        }

        return null;
    }

    private async Task<SystemTopicResource?> FindSystemTopic(
        SubscriptionResource subscriptionResource,
        string? resourceGroup,
        string topicName)
    {
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            // Search in specific resource group
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            await foreach (var systemTopic in resourceGroupResource.Value.GetSystemTopics().GetAllAsync())
            {
                if (systemTopic.Data.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase))
                {
                    return systemTopic;
                }
            }
        }
        else
        {
            // Search in all resource groups
            await foreach (var systemTopic in subscriptionResource.GetSystemTopicsAsync())
            {
                if (systemTopic.Data.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase))
                {
                    return systemTopic;
                }
            }
        }

        return null;
    }

    private static EventGridTopicInfo CreateTopicInfo(EventGridTopicData topicData)
    {
        return new EventGridTopicInfo(
            Name: topicData.Name,
            Location: topicData.Location.ToString(),
            Endpoint: topicData.Endpoint?.ToString(),
            ProvisioningState: topicData.ProvisioningState?.ToString(),
            PublicNetworkAccess: topicData.PublicNetworkAccess?.ToString(),
            InputSchema: topicData.InputSchema?.ToString());
    }

    private static EventGridSubscriptionInfo CreateSubscriptionInfo(EventGridSubscriptionData subscriptionData)
    {
        string? endpointType = null;
        string? endpointUrl = null;

        // Extract endpoint information based on destination type
        if (subscriptionData.Destination != null)
        {
            // Extract both endpoint type and URL using type-safe pattern matching
            (endpointType, endpointUrl) = subscriptionData.Destination switch
            {
                WebHookEventSubscriptionDestination webhook => ("WebHook", webhook.Endpoint?.ToString()),
                AzureFunctionEventSubscriptionDestination azureFunction => ("AzureFunction", azureFunction.ResourceId?.ToString()),
                EventHubEventSubscriptionDestination eventHub => ("EventHub", eventHub.ResourceId?.ToString()),
                HybridConnectionEventSubscriptionDestination hybridConnection => ("HybridConnection", hybridConnection.ResourceId?.ToString()),
                NamespaceTopicEventSubscriptionDestination namespaceTopic => ("NamespaceTopic", namespaceTopic.ResourceId?.ToString()),
                PartnerEventSubscriptionDestination partner => ("Partner", partner.ResourceId),
                ServiceBusQueueEventSubscriptionDestination serviceBusQueue => ("ServiceBusQueue", serviceBusQueue.ResourceId?.ToString()),
                ServiceBusTopicEventSubscriptionDestination serviceBusTopic => ("ServiceBusTopic", serviceBusTopic.ResourceId?.ToString()),
                StorageQueueEventSubscriptionDestination storageQueue => ("StorageQueue", storageQueue.ResourceId?.ToString()),
                MonitorAlertEventSubscriptionDestination => ("MonitorAlert", null), // No endpoint property
                _ => (subscriptionData.Destination.GetType().Name, null) // Unknown or future destination types - fallback to full type name
            };
        }

        // Extract filter information
        string? filterInfo = null;
        if (subscriptionData.Filter != null)
        {
            var filterDetails = new List<string>();

            if (subscriptionData.Filter.SubjectBeginsWith != null)
                filterDetails.Add($"SubjectBeginsWith: {subscriptionData.Filter.SubjectBeginsWith}");

            if (subscriptionData.Filter.SubjectEndsWith != null)
                filterDetails.Add($"SubjectEndsWith: {subscriptionData.Filter.SubjectEndsWith}");

            if (subscriptionData.Filter.IncludedEventTypes?.Any() == true)
                filterDetails.Add($"EventTypes: {string.Join(", ", subscriptionData.Filter.IncludedEventTypes)}");

            if (subscriptionData.Filter.IsSubjectCaseSensitive.HasValue)
                filterDetails.Add($"CaseSensitive: {subscriptionData.Filter.IsSubjectCaseSensitive}");

            filterInfo = filterDetails.Any() ? string.Join("; ", filterDetails) : null;
        }

        return new EventGridSubscriptionInfo(
            Name: subscriptionData.Name,
            Type: subscriptionData.ResourceType.ToString(),
            EndpointType: endpointType,
            EndpointUrl: endpointUrl,
            ProvisioningState: subscriptionData.ProvisioningState?.ToString(),
            DeadLetterDestination: subscriptionData.DeadLetterDestination?.ToString(),
            Filter: filterInfo,
            MaxDeliveryAttempts: subscriptionData.RetryPolicy?.MaxDeliveryAttempts,
            EventTimeToLiveInMinutes: subscriptionData.RetryPolicy?.EventTimeToLiveInMinutes,
            CreatedDateTime: subscriptionData.SystemData?.CreatedOn?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedDateTime: subscriptionData.SystemData?.LastModifiedOn?.ToString("yyyy-MM-ddTHH:mm:ssZ")
        );
    }

    private async Task AddSubscriptionsFromTopic(
        string topicLocation,
        string? locationFilter,
        List<EventGridSubscriptionInfo> subscriptions,
        IAsyncEnumerable<TopicEventSubscriptionResource> subscriptionCollection)
    {
        if (string.IsNullOrEmpty(locationFilter) || string.Equals(topicLocation, locationFilter, StringComparison.OrdinalIgnoreCase))
        {
            await foreach (var subscription in subscriptionCollection)
            {
                subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
            }
        }
    }

    private async Task AddSubscriptionsFromSystemTopic(
        string topicLocation,
        string? locationFilter,
        List<EventGridSubscriptionInfo> subscriptions,
        IAsyncEnumerable<SystemTopicEventSubscriptionResource> subscriptionCollection)
    {
        if (string.IsNullOrEmpty(locationFilter) || string.Equals(topicLocation, locationFilter, StringComparison.OrdinalIgnoreCase))
        {
            await foreach (var subscription in subscriptionCollection)
            {
                subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
            }
        }
    }
}
