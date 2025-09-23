// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Messaging.EventGrid;
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

    public async Task<EventPublishResult> PublishEventsAsync(
        string subscription,
        string? resourceGroup,
        string topicName,
        string eventData,
        string? eventSchema = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var operationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting event publication. OperationId: {OperationId}, Topic: {TopicName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
            operationId, topicName, resourceGroup, subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

            // Find the topic to get its endpoint and access key
            var topic = await FindTopic(subscriptionResource, resourceGroup, topicName);
            if (topic == null)
            {
                var errorMessage = $"Event Grid topic '{topicName}' not found in resource group '{resourceGroup}'. Make sure the topic exists and you have access to it.";
                _logger.LogError(errorMessage);
                return new EventPublishResult(
                    Status: "Failed",
                    Message: errorMessage,
                    PublishedEventCount: 0,
                    OperationId: operationId,
                    PublishedAt: DateTime.UtcNow);
            }

            if (topic.Data.Endpoint == null)
            {
                var errorMessage = $"Event Grid topic '{topicName}' does not have a valid endpoint.";
                _logger.LogError(errorMessage);
                return new EventPublishResult(
                    Status: "Failed",
                    Message: errorMessage,
                    PublishedEventCount: 0,
                    OperationId: operationId,
                    PublishedAt: DateTime.UtcNow);
            }

            // Get the access key for the topic
            var keys = await topic.GetSharedAccessKeysAsync();
            var accessKey = keys.Value.Key1;

            if (string.IsNullOrEmpty(accessKey))
            {
                var errorMessage = $"Unable to retrieve access key for Event Grid topic '{topicName}'.";
                _logger.LogError(errorMessage);
                return new EventPublishResult(
                    Status: "Failed",
                    Message: errorMessage,
                    PublishedEventCount: 0,
                    OperationId: operationId,
                    PublishedAt: DateTime.UtcNow);
            }

            // Parse and validate event data
            var events = ParseAndValidateEventData(eventData, eventSchema ?? "EventGridEvent");

            // Create EventGridPublisherClient and publish events
            var credential = new AzureKeyCredential(accessKey);
            var client = new EventGridPublisherClient(topic.Data.Endpoint, credential);

            // Convert to EventGridEvent objects for publishing
            var eventGridEvents = events.Select(eventData => new EventGridEvent(
                subject: eventData.Subject,
                eventType: eventData.EventType,
                dataVersion: eventData.DataVersion,
                data: BinaryData.FromString(eventData.Data ?? "{}")
            )
            {
                Id = eventData.Id,
                EventTime = eventData.EventTime
            }).ToList();

            _logger.LogInformation("Publishing {EventCount} events to topic '{TopicName}' with operation ID: {OperationId}",
                eventGridEvents.Count, topicName, operationId);

            try
            {
                // Publish the events
                await client.SendEventsAsync(eventGridEvents);
            }
            catch (Exception publishEx)
            {
                _logger.LogError(publishEx, "Failed to publish events to topic '{TopicName}' with operation ID: {OperationId}",
                    topicName, operationId);
                throw;
            }

            _logger.LogInformation("Successfully published {EventCount} events to topic '{TopicName}'",
                eventGridEvents.Count, topicName);

            return new EventPublishResult(
                Status: "Success",
                Message: $"Successfully published {events.Count} event(s) to topic '{topicName}'.",
                PublishedEventCount: events.Count,
                OperationId: operationId,
                PublishedAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish events to topic '{TopicName}' in resource group '{ResourceGroup}'",
                topicName, resourceGroup);

            return new EventPublishResult(
                Status: "Failed",
                Message: $"Failed to publish events: {ex.Message}",
                PublishedEventCount: 0,
                OperationId: operationId,
                PublishedAt: DateTime.UtcNow);
        }
    }

    private static List<EventData> ParseAndValidateEventData(string eventData, string eventSchema)
    {
        try
        {
            var events = new List<EventData>();

            // Parse the JSON data
            var jsonDocument = JsonDocument.Parse(eventData);

            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Handle array of events
                foreach (var eventElement in jsonDocument.RootElement.EnumerateArray())
                {
                    events.Add(CreateEventDataFromJsonElement(eventElement, eventSchema));
                }
            }
            else
            {
                // Handle single event
                events.Add(CreateEventDataFromJsonElement(jsonDocument.RootElement, eventSchema));
            }

            if (events.Count == 0)
            {
                throw new ArgumentException("No valid events found in the provided event data.");
            }

            return events;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in event data: {ex.Message}", ex);
        }
    }

    private static EventData CreateEventDataFromJsonElement(JsonElement eventElement, string eventSchema)
    {
        // Extract required properties
        var eventType = eventElement.TryGetProperty("eventType", out var eventTypeProp) ? eventTypeProp.GetString() :
                       eventElement.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : "CustomEvent";

        var subject = eventElement.TryGetProperty("subject", out var subjectProp) ? subjectProp.GetString() : "/default/subject";

        var dataVersion = eventElement.TryGetProperty("dataVersion", out var dataVersionProp) ? dataVersionProp.GetString() : "1.0";

        // Extract data payload as raw JSON string for AOT compatibility
        string? data = null;
        if (eventElement.TryGetProperty("data", out var dataProp))
        {
            data = dataProp.GetRawText();
        }

        var id = eventElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : Guid.NewGuid().ToString();
        var eventTime = eventElement.TryGetProperty("eventTime", out var timeProp) && timeProp.TryGetDateTimeOffset(out var eventTimeValue)
                       ? eventTimeValue : DateTimeOffset.UtcNow;

        // Create a simple event data structure
        return new EventData(
            Id: id ?? Guid.NewGuid().ToString(),
            Subject: subject ?? "/default/subject",
            EventType: eventType ?? "CustomEvent",
            DataVersion: dataVersion ?? "1.0",
            Data: data,
            EventTime: eventTime,
            Schema: eventSchema);
    }

    // Simple record to hold event data for validation
    private record EventData(
        string Id,
        string Subject,
        string EventType,
        string DataVersion,
        string? Data,
        DateTimeOffset EventTime,
        string Schema);

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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing resource group '{ResourceGroup}'", resourceGroup);
                throw;
            }
        }
        else
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching topics across subscription");
                throw;
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
