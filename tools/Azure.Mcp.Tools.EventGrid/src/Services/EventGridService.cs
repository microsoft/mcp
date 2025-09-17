// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.EventGrid;

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

    public Task<List<EventGridSubscriptionInfo>> GetSubscriptionsAsync(
        string subscription,
        string? resourceGroup = null,
        string? topicName = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        // For now, return a placeholder implementation
        // This will be enhanced once we determine the correct Azure SDK methods
        var subscriptions = new List<EventGridSubscriptionInfo>();
        
        // Add a placeholder subscription for demonstration
        subscriptions.Add(new EventGridSubscriptionInfo(
            Name: "placeholder-subscription",
            Type: "Microsoft.EventGrid/eventSubscriptions",
            EndpointType: "WebHook",
            EndpointUrl: "https://example.com/webhook",
            ProvisioningState: "Succeeded",
            DeadLetterDestination: null,
            Filter: null,
            MaxDeliveryAttempts: 30,
            EventTimeToLiveInMinutes: 1440,
            CreatedDateTime: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedDateTime: DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        ));

        return Task.FromResult(subscriptions);
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
}
