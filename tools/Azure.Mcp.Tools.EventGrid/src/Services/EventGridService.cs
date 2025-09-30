// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.EventGrid.Commands;
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

    public async Task<EventPublishResult> PublishEventAsync(
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
                throw new InvalidOperationException("Publishing failed with the following error message: " + errorMessage);
            }

            if (topic.Data.Endpoint == null)
            {
                var errorMessage = $"Event Grid topic '{topicName}' does not have a valid endpoint.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException("Publishing failed with the following error message: " + errorMessage);
            }

            // Get credential using standardized method from base class for Azure AD authentication
            var credential = await GetCredential(tenant);

            // Parse and validate event data directly to EventGridEventSchema
            var eventGridEventSchemas = ParseAndValidateEventData(eventData, eventSchema ?? "EventGridEvent");

            var publisherClient = new EventGridPublisherClient(topic.Data.Endpoint, credential);

            // Serialize each event individually to JSON using source-generated context
            var eventsData = eventGridEventSchemas.Select(eventSchema =>
            {
                var jsonString = JsonSerializer.Serialize(eventSchema, EventGridJsonContext.Default.EventGridEventSchema);
                return BinaryData.FromString(jsonString);
            }).ToArray();

            var eventCount = eventsData.Length;
            _logger.LogInformation("Publishing {EventCount} events to topic '{TopicName}' with operation ID: {OperationId}",
                eventCount, topicName, operationId);

            try
            {
                await publisherClient.SendEventsAsync(eventsData);
            }
            catch (Exception publishEx)
            {
                _logger.LogError(publishEx, "Failed to publish events to topic '{TopicName}' with operation ID: {OperationId}",
                    topicName, operationId);
                throw;
            }

            _logger.LogInformation("Successfully published {EventCount} events to topic '{TopicName}'",
                eventCount, topicName);

            return new EventPublishResult(
                Status: "Success",
                Message: $"Successfully published {eventCount} event(s) to topic '{topicName}'.",
                PublishedEventCount: eventCount,
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

    private static IEnumerable<Models.EventGridEventSchema> ParseAndValidateEventData(string eventData, string eventSchema)
    {
        try
        {
            // Parse the JSON data
            var jsonDocument = JsonDocument.Parse(eventData);

            IEnumerable<Models.EventGridEventSchema> events;

            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Handle array of events - use lazy evaluation
                events = jsonDocument.RootElement.EnumerateArray()
                    .Select(eventElement => CreateEventGridEventFromJsonElement(eventElement, eventSchema));
            }
            else
            {
                // Handle single event - return single item enumerable
                events = new[] { CreateEventGridEventFromJsonElement(jsonDocument.RootElement, eventSchema) };
            }

            var eventsList = events.ToList();
            if (eventsList.Count == 0)
            {
                throw new ArgumentException("No valid events found in the provided event data.");
            }

            return eventsList;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in event data: {ex.Message}", ex);
        }
    }

    private static Models.EventGridEventSchema CreateEventGridEventFromJsonElement(JsonElement eventElement, string eventSchema)
    {
        var eventJson = eventElement.GetRawText();

        if (eventSchema.Equals("CloudEvents", StringComparison.OrdinalIgnoreCase))
        {
            var cloudEvent = JsonSerializer.Deserialize(eventJson, EventGridJsonContext.Default.CloudEvent);
            if (cloudEvent == null)
                throw new ArgumentException("Failed to deserialize CloudEvent");

            // Validate datacontenttype for CloudEvents
            var dataContentType = cloudEvent.DataContentType ?? MediaTypeNames.Application.Json;
            if (!string.Equals(dataContentType, MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(dataContentType) || !dataContentType.Contains('/'))
                {
                    throw new ArgumentException($"Invalid datacontenttype '{dataContentType}' in CloudEvent with id '{cloudEvent.Id}'. Must be a valid MIME type (e.g., 'application/xml', 'text/plain').");
                }
            }

            return new Models.EventGridEventSchema
            {
                Id = cloudEvent.Id ?? Guid.NewGuid().ToString(),
                Subject = cloudEvent.Source ?? cloudEvent.Subject ?? "/default/subject",
                EventType = cloudEvent.Type ?? "CustomEvent",
                DataVersion = cloudEvent.SpecVersion ?? "1.0",
                Data = cloudEvent.Data.HasValue ? JsonNode.Parse(cloudEvent.Data.Value.GetRawText()) : null,
                EventTime = cloudEvent.Time ?? DateTimeOffset.UtcNow
            };
        }
        else if (eventSchema.Equals("EventGrid", StringComparison.OrdinalIgnoreCase))
        {
            var eventGridEvent = JsonSerializer.Deserialize(eventJson, EventGridJsonContext.Default.EventGridEventInput);
            if (eventGridEvent == null)
                throw new ArgumentException("Failed to deserialize EventGrid event");

            return new Models.EventGridEventSchema
            {
                Id = eventGridEvent.Id ?? Guid.NewGuid().ToString(),
                Subject = eventGridEvent.Subject ?? "/default/subject",
                EventType = eventGridEvent.EventType ?? "CustomEvent",
                DataVersion = eventGridEvent.DataVersion ?? "1.0",
                Data = eventGridEvent.Data.HasValue ? JsonNode.Parse(eventGridEvent.Data.Value.GetRawText()) : null,
                EventTime = eventGridEvent.EventTime ?? DateTimeOffset.UtcNow
            };
        }
        else // Custom schema - try both CloudEvents and EventGrid field names
        {
            var flexibleEvent = JsonSerializer.Deserialize(eventJson, EventGridJsonContext.Default.CustomEvent);
            if (flexibleEvent == null)
                throw new ArgumentException("Failed to deserialize custom event");

            return new Models.EventGridEventSchema
            {
                Id = flexibleEvent.Id ?? Guid.NewGuid().ToString(),
                Subject = flexibleEvent.Subject ?? flexibleEvent.Source ?? "/default/subject",
                EventType = flexibleEvent.EventType ?? flexibleEvent.Type ?? "CustomEvent",
                DataVersion = flexibleEvent.DataVersion ?? flexibleEvent.SpecVersion ?? "1.0",
                Data = flexibleEvent.Data.HasValue ? JsonNode.Parse(flexibleEvent.Data.Value.GetRawText()) : null,
                EventTime = flexibleEvent.EventTime ?? flexibleEvent.Time ?? DateTimeOffset.UtcNow
            };
        }
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
