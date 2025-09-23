// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventGrid.Commands;
using Azure.Mcp.Tools.EventGrid.Models;
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

            // Get credential using standardized method from base class for Azure AD authentication
            var credential = await GetCredential(tenant);

            // Parse and validate event data directly to EventGridEventSchema
            var eventGridEventSchemas = ParseAndValidateEventData(eventData, eventSchema ?? "EventGridEvent");

            // Use EventGridPublisherClient with BinaryData for AOT-compatible publishing
            var publisherClient = new EventGridPublisherClient(topic.Data.Endpoint, credential);

            // Serialize each event individually to JSON using source-generated context and convert to BinaryData
            var eventsBinaryData = eventGridEventSchemas.Select(eventSchema =>
            {
                var jsonString = JsonSerializer.Serialize(eventSchema, EventGridJsonContext.Default.EventGridEventSchema);
                return BinaryData.FromString(jsonString);
            }).ToArray();

            // Get event count for logging (this will materialize the enumerable once)
            var eventCount = eventsBinaryData.Length;
            _logger.LogInformation("Publishing {EventCount} events to topic '{TopicName}' with operation ID: {OperationId}",
                eventCount, topicName, operationId);

            try
            {
                // Send events using EventGridPublisherClient with BinaryData (AOT-compatible)
                await publisherClient.SendEventsAsync(eventsBinaryData);
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

    private static IEnumerable<EventGridEventSchema> ParseAndValidateEventData(string eventData, string eventSchema)
    {
        try
        {
            // Parse the JSON data
            var jsonDocument = JsonDocument.Parse(eventData);

            IEnumerable<EventGridEventSchema> events;

            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Handle array of events - use lazy evaluation
                events = jsonDocument.RootElement.EnumerateArray()
                    .Select(eventElement => CreateEventGridEventSchemaFromJsonElement(eventElement, eventSchema));
            }
            else
            {
                // Handle single event - return single item enumerable
                events = new[] { CreateEventGridEventSchemaFromJsonElement(jsonDocument.RootElement, eventSchema) };
            }

            // Force evaluation to validate all events before returning
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

    private static EventGridEventSchema CreateEventGridEventSchemaFromJsonElement(JsonElement eventElement, string eventSchema)
    {
        string? eventType, subject, dataVersion;
        DateTimeOffset eventTime;

        // Extract event ID early for logging purposes
        var id = eventElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : Guid.NewGuid().ToString();

        if (eventSchema.Equals("CloudEvents", StringComparison.OrdinalIgnoreCase))
        {
            // CloudEvents spec handling (v1.0)
            eventType = eventElement.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : "CustomEvent";

            // CloudEvents uses "source" field, but we can fall back to "subject" for compatibility
            subject = eventElement.TryGetProperty("source", out var sourceProp) ? sourceProp.GetString() :
                     eventElement.TryGetProperty("subject", out var subjectProp) ? subjectProp.GetString() : "/default/subject";

            // CloudEvents uses "specversion" for schema version
            dataVersion = eventElement.TryGetProperty("specversion", out var specProp) ? specProp.GetString() : "1.0";

            // CloudEvents uses "time" field
            eventTime = eventElement.TryGetProperty("time", out var timeProp) && timeProp.TryGetDateTimeOffset(out var timeValue)
                       ? timeValue : DateTimeOffset.UtcNow;

            // Handle datacontenttype - CloudEvents v1.0 spec field for content type of data payload
            var dataContentType = eventElement.TryGetProperty("datacontenttype", out var dataContentTypeProp)
                ? dataContentTypeProp.GetString()
                : "application/json"; // Default per CloudEvents spec

            // Log and validate datacontenttype for debugging and monitoring purposes

            if (!string.Equals(dataContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                // Log when non-JSON content types are used - this helps with debugging
                // Note: EventGrid will accept the event regardless of datacontenttype,
                // but subscribers should handle non-JSON content types appropriately
                // Common non-JSON types: application/xml, text/plain, application/octet-stream

                // For now, we'll just validate that it's a recognized MIME type format
                if (string.IsNullOrWhiteSpace(dataContentType) || !dataContentType.Contains('/'))
                {
                    throw new ArgumentException($"Invalid datacontenttype '{dataContentType}' in CloudEvent with id '{id}'. Must be a valid MIME type (e.g., 'application/xml', 'text/plain').");
                }
            }
        }
        else if (eventSchema.Equals("EventGrid", StringComparison.OrdinalIgnoreCase))
        {
            // EventGrid spec handling
            eventType = eventElement.TryGetProperty("eventType", out var eventTypeProp) ? eventTypeProp.GetString() : "CustomEvent";
            subject = eventElement.TryGetProperty("subject", out var subjectProp) ? subjectProp.GetString() : "/default/subject";
            dataVersion = eventElement.TryGetProperty("dataVersion", out var dataVersionProp) ? dataVersionProp.GetString() : "1.0";
            eventTime = eventElement.TryGetProperty("eventTime", out var timeProp) && timeProp.TryGetDateTimeOffset(out var eventTimeValue)
                       ? eventTimeValue : DateTimeOffset.UtcNow;
        }
        else // Custom schema
        {
            // For custom schema, try both CloudEvents and EventGrid field names for flexibility
            eventType = eventElement.TryGetProperty("eventType", out var eventTypeProp) ? eventTypeProp.GetString() :
                       eventElement.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : "CustomEvent";

            subject = eventElement.TryGetProperty("subject", out var subjectProp) ? subjectProp.GetString() :
                     eventElement.TryGetProperty("source", out var sourceProp) ? sourceProp.GetString() : "/default/subject";

            dataVersion = eventElement.TryGetProperty("dataVersion", out var dataVersionProp) ? dataVersionProp.GetString() :
                         eventElement.TryGetProperty("specversion", out var specProp) ? specProp.GetString() : "1.0";

            eventTime = eventElement.TryGetProperty("eventTime", out var eventTimeProp) && eventTimeProp.TryGetDateTimeOffset(out var eventTimeValue) ? eventTimeValue :
                       eventElement.TryGetProperty("time", out var timeProp) && timeProp.TryGetDateTimeOffset(out var timeValue) ? timeValue : DateTimeOffset.UtcNow;
        }

        // Extract data payload and parse as JsonNode for AOT compatibility
        JsonNode? data = null;
        if (eventElement.TryGetProperty("data", out var dataProp))
        {
            data = JsonNode.Parse(dataProp.GetRawText());
        }

        // For CloudEvents schema, we've already captured datacontenttype above for validation/logging
        // The EventGrid schema doesn't have a direct equivalent, so we don't persist it in the final event

        // Create EventGridEventSchema directly
        return new EventGridEventSchema
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Subject = subject ?? "/default/subject",
            EventType = eventType ?? "CustomEvent",
            DataVersion = dataVersion ?? "1.0",
            Data = data,
            EventTime = eventTime
        };
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