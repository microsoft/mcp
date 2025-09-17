// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Options;
using Azure.ResourceManager;
using Azure.ResourceManager.EventGrid;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.EventGrid.Services;

public class EventGridService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IEventGridService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<List<EventGridTopicInfo>> GetTopicsAsync(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);
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
        RetryPolicyOptions? retryPolicy = null)
    {
        var subscriptions = new List<EventGridSubscriptionInfo>();

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

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
                // Check if location filter applies
                if (string.IsNullOrEmpty(location) || string.Equals(topic.Data.Location.ToString(), location, StringComparison.OrdinalIgnoreCase))
                {
                    // Get event subscriptions for the specific topic using the correct ARM SDK pattern
                    await foreach (var subscription in topic.GetTopicEventSubscriptions().GetAllAsync())
                    {
                        subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
                    }
                }
                return; // Found custom topic, no need to check system topics
            }

            // If not found in custom topics, check system topics
            var systemTopic = await FindSystemTopic(subscriptionResource, resourceGroup, topicName);
            if (systemTopic != null)
            {
                // Check if location filter applies
                if (string.IsNullOrEmpty(location) || string.Equals(systemTopic.Data.Location.ToString(), location, StringComparison.OrdinalIgnoreCase))
                {
                    await foreach (var subscription in systemTopic.GetSystemTopicEventSubscriptions().GetAllAsync())
                    {
                        subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
                    }
                }
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
                        // Check if location filter applies
                        if (string.IsNullOrEmpty(location) || string.Equals(topic.Data.Location.ToString(), location, StringComparison.OrdinalIgnoreCase))
                        {
                            // Get event subscriptions for each topic using the correct ARM SDK pattern
                            await foreach (var subscription in topic.GetTopicEventSubscriptions().GetAllAsync())
                            {
                                subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
                            }
                        }
                    }
                    catch
                    {
                        // Continue with other topics if one fails
                        continue;
                    }
                }

                // Also check system topics in the resource group
                await foreach (var systemTopic in resourceGroupResource.Value.GetSystemTopics().GetAllAsync())
                {
                    try
                    {
                        // Check if location filter applies
                        if (string.IsNullOrEmpty(location) || string.Equals(systemTopic.Data.Location.ToString(), location, StringComparison.OrdinalIgnoreCase))
                        {
                            await foreach (var subscription in systemTopic.GetSystemTopicEventSubscriptions().GetAllAsync())
                            {
                                subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
                            }
                        }
                    }
                    catch
                    {
                        // Continue with other system topics if one fails
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
                        // Check if location filter applies
                        if (string.IsNullOrEmpty(location) || string.Equals(topic.Data.Location.ToString(), location, StringComparison.OrdinalIgnoreCase))
                        {
                            // Get event subscriptions for each topic using the correct ARM SDK pattern
                            await foreach (var subscription in topic.GetTopicEventSubscriptions().GetAllAsync())
                            {
                                subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
                            }
                        }
                    }
                    catch
                    {
                        // Continue with other topics if one fails
                        continue;
                    }
                }

                // Also check system topics across all resource groups
                await foreach (var systemTopic in subscriptionResource.GetSystemTopicsAsync())
                {
                    try
                    {
                        // Check if location filter applies
                        if (string.IsNullOrEmpty(location) || string.Equals(systemTopic.Data.Location.ToString(), location, StringComparison.OrdinalIgnoreCase))
                        {
                            await foreach (var subscription in systemTopic.GetSystemTopicEventSubscriptions().GetAllAsync())
                            {
                                subscriptions.Add(CreateSubscriptionInfo(subscription.Data));
                            }
                        }
                    }
                    catch
                    {
                        // Continue with other system topics if one fails
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

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "EventGrid destination types are well-known SDK types")]
    private static EventGridSubscriptionInfo CreateSubscriptionInfo(EventGridSubscriptionData subscriptionData)
    {
        string? endpointType = null;
        string? endpointUrl = null;

        // Extract endpoint information based on type
        if (subscriptionData.Destination != null)
        {
            endpointType = subscriptionData.Destination.GetType().Name;

            // Try to extract endpoint URL from different destination types
            var destinationType = subscriptionData.Destination.GetType();
            var endpointProperty = destinationType.GetProperty("EndpointUri") ??
                                  destinationType.GetProperty("EndpointUrl") ??
                                  destinationType.GetProperty("Endpoint");

            if (endpointProperty != null)
            {
                var endpointValue = endpointProperty.GetValue(subscriptionData.Destination);
                endpointUrl = endpointValue?.ToString();
            }
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
}
