// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.EventHubs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventHubs.Services;

public sealed class EventHubsService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<EventHubsService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IEventHubsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<EventHubsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Event Hub entity/consumer group creation exhibits read-after-write lag: the PUT can
    // succeed while a near-immediate follow-up GET (performed internally when resolving the
    // ARM operation's Value, or by an explicit GET right after create) still 404s until the
    // new entity propagates. Retry briefly instead of failing the whole operation.
    private static readonly TimeSpan s_eventualConsistencyRetryDelay = TimeSpan.FromSeconds(2);
    private const int EventualConsistencyMaxAttempts = 5;

    private static async Task<T> GetWithEventualConsistencyRetryAsync<T>(
        Func<Task<T?>> getValue,
        Func<Exception> notFoundExceptionFactory,
        CancellationToken cancellationToken) where T : class
    {
        for (var attempt = 1; attempt <= EventualConsistencyMaxAttempts; attempt++)
        {
            try
            {
                var value = await getValue();
                if (value != null)
                {
                    return value;
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404 && attempt < EventualConsistencyMaxAttempts)
            {
                // Fall through to retry below.
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("404") && attempt < EventualConsistencyMaxAttempts)
            {
                // Thrown by NoValueResponse<T>.Value when the service returns no content (e.g. a 404
                // encountered while resolving an ArmOperation's final value). Retry below.
            }

            if (attempt < EventualConsistencyMaxAttempts)
            {
                await Task.Delay(s_eventualConsistencyRetryDelay, cancellationToken);
            }
        }

        throw notFoundExceptionFactory();
    }

    public async Task<List<Namespace>> GetNamespacesAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        // Resource group is optional for subscription-wide listing.
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var namespaces = new List<Namespace>();

        if (!string.IsNullOrWhiteSpace(resourceGroup))
        {
            // Get namespaces from specific resource group
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            if (resourceGroupResource?.Value == null)
            {
                throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
            }

            await foreach (var namespaceResource in resourceGroupResource.Value.GetEventHubsNamespaces().WithCancellation(cancellationToken))
            {
                namespaces.Add(ConvertToNamespace(namespaceResource.Data, resourceGroup));
            }
        }
        else
        {
            // Get all namespaces across the subscription in a single call
            await foreach (var namespaceResource in subscriptionResource.GetEventHubsNamespacesAsync(cancellationToken))
            {
                var rgName = namespaceResource.Data.Id.ResourceGroupName ?? string.Empty;
                namespaces.Add(ConvertToNamespace(namespaceResource.Data, rgName));
            }
        }

        return namespaces;
    }

    private static Namespace ConvertToNamespace(EventHubsNamespaceData namespaceData, string resourceGroup)
    {
        return new(
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
        Tags: namespaceData.Tags?.ToDictionary());
    }

    public async Task<Namespace> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(namespaceName), namespaceName),
            (nameof(resourceGroup), resourceGroup));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        return ConvertToNamespace(namespaceResource.Value.Data, resourceGroup);
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
        RetryPolicyOptions? retryPolicy = null,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
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
                Tier = skuTier?.ToUpperInvariant() switch
                {
                    "BASIC" => EventHubsSkuTier.Basic,
                    "STANDARD" => EventHubsSkuTier.Standard,
                    "PREMIUM" => EventHubsSkuTier.Premium,
                    null or "" => EventHubsSkuTier.Standard,
                    _ => throw new ArgumentException(
                        $"Invalid SKU tier '{skuTier}'. Valid values: Basic, Standard, Premium.",
                        nameof(skuTier))
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
            .CreateOrUpdateAsync(WaitUntil.Started, namespaceName, namespaceData, cancellationToken);

        if (progress != null)
        {
            await WaitForLroCompletionAsync(operation, progress, cancellationToken);
        }
        else
        {
            await WaitForLroCompletionAsync(operation, cancellationToken);
        }

        if (operation?.Value == null)
        {
            throw new InvalidOperationException($"Failed to create or update Event Hubs namespace '{namespaceName}'");
        }

        _logger.LogInformation(
            "Successfully created or updated Event Hubs namespace '{NamespaceName}' in resource group '{ResourceGroup}'",
            namespaceName, resourceGroup);

        return ConvertToNamespace(operation.Value.Data, resourceGroup);
    }

    public async Task<bool> DeleteNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        try
        {
            var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
                ? subscription
                : await _subscriptionService.GetSubscriptionIdByName(subscription, tenant, retryPolicy, cancellationToken);

            var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
            var namespaceId = EventHubsNamespaceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, namespaceName);

            // Get the namespace resource
            var namespaceResource = await GetGenericResourceAsync(armClient, namespaceId, cancellationToken);

            // Delete the namespace
            var deleteOperation = await namespaceResource.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted Event Hubs namespace '{NamespaceName}' from resource group '{ResourceGroup}'",
                namespaceName, resourceGroup);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation(
                "Event Hubs namespace '{NamespaceName}' not found in resource group '{ResourceGroup}'. Nothing was deleted.",
                namespaceName, resourceGroup);
            return false;
        }
    }

    public async Task<List<EventHub>> GetEventHubsAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(namespaceName), namespaceName));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        var eventHubList = new List<EventHub>();

        await foreach (var eventHub in namespaceResource.Value.GetEventHubs().WithCancellation(cancellationToken))
        {
            eventHubList.Add(ConvertToEventHub(eventHub.Data, resourceGroup));
        }

        return eventHubList;
    }

    public async Task<EventHub?> GetEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(namespaceName), namespaceName), (nameof(eventHubName), eventHubName));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName, cancellationToken);

        if (eventHubResource?.Value == null)
        {
            return null;
        }

        return ConvertToEventHub(eventHubResource.Value.Data, resourceGroup);
    }

    private static EventHub ConvertToEventHub(EventHubData eventHub, string resourceGroup)
    {
        return new(
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
        string? status = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(namespaceName), namespaceName));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        // Fetch the existing event hub only when at least one optional property is absent so
        // that a create-or-update PUT preserves fields the caller didn't explicitly set.
        // PartitionCount in particular: Azure rejects a PUT to an existing event hub that
        // omits it with "PartitionCount can only be changed on a Dedicated Event Hub cluster or Premium namespace",
        // even when no partition-count change was requested.
        EventHubData? existingData = null;
        if (!partitionCount.HasValue || !messageRetentionInHours.HasValue || status is null)
        {
            // GetIfExistsAsync returns a NullableResponse whose Value getter throws
            // InvalidOperationException when the entity doesn't exist (a 404 was returned) -
            // the null-conditional operator on the response itself doesn't guard against this,
            // since the response wrapper is never null. Check HasValue first.
            try
            {
                var existingEventHub = await namespaceResource.Value.GetEventHubs().GetIfExistsAsync(eventHubName, cancellationToken);
                if (existingEventHub.HasValue)
                {
                    existingData = existingEventHub.Value?.Data;
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Event hub doesn't exist yet; nothing to merge.
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("404"))
            {
                // Event hub doesn't exist yet; nothing to merge.
            }
        }

        var eventHubData = new EventHubData();

        if (partitionCount.HasValue)
        {
            eventHubData.PartitionCount = partitionCount.Value;
        }
        else if (existingData?.PartitionCount.HasValue == true)
        {
            eventHubData.PartitionCount = existingData.PartitionCount;
        }

        if (messageRetentionInHours.HasValue)
        {
            eventHubData.RetentionDescription = new()
            {
                RetentionTimeInHours = messageRetentionInHours.Value,
                CleanupPolicy = CleanupPolicyRetentionDescription.Delete
            };
        }
        else if (existingData?.RetentionDescription is not null)
        {
            eventHubData.RetentionDescription = existingData.RetentionDescription;
        }

        if (status is not null)
        {
            eventHubData.Status = status.ToUpperInvariant() switch
            {
                "ACTIVE" => EventHubEntityStatus.Active,
                "DISABLED" => EventHubEntityStatus.Disabled,
                "RESTORING" => EventHubEntityStatus.Restoring,
                "SENDDISABLED" => EventHubEntityStatus.SendDisabled,
                "RECEIVEDISABLED" => EventHubEntityStatus.ReceiveDisabled,
                "CREATING" => EventHubEntityStatus.Creating,
                "DELETING" => EventHubEntityStatus.Deleting,
                "RENAMING" => EventHubEntityStatus.Renaming,
                "UNKNOWN" => EventHubEntityStatus.Unknown,
                _ => throw new ArgumentException(
                    $"Invalid status '{status}'. Valid values: Active, Disabled, Restoring, SendDisabled, ReceiveDisabled, Creating, Deleting, Renaming, Unknown.",
                    nameof(status))
            };
        }
        else if (existingData?.Status is not null)
        {
            eventHubData.Status = existingData.Status;
        }

        var operation = await namespaceResource.Value.GetEventHubs()
            .CreateOrUpdateAsync(WaitUntil.Started, eventHubName, eventHubData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        // The ARM operation's Value can throw immediately after creation (read-after-write lag);
        // fall back to an explicit GET, retried with backoff, rather than failing outright.
        EventHubData? eventHubResourceData;
        try
        {
            eventHubResourceData = operation.Value.Data;
        }
        catch (Exception ex) when (ex is RequestFailedException or InvalidOperationException)
        {
            var refreshed = await GetWithEventualConsistencyRetryAsync(
                async () =>
                {
                    var response = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName, cancellationToken);
                    return response?.Value;
                },
                () => new InvalidOperationException($"Failed to create or update event hub '{eventHubName}'"),
                cancellationToken);
            eventHubResourceData = refreshed.Data;
        }

        return ConvertToEventHub(eventHubResourceData, resourceGroup);
    }

    public async Task<bool> DeleteEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);

        try
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            if (resourceGroupResource?.Value == null)
            {
                _logger.LogInformation("Resource group '{ResourceGroup}' not found, event hub '{EventHubName}' cannot exist", resourceGroup, eventHubName);
                return false;
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetIfExistsAsync(namespaceName, cancellationToken);

            if (namespaceResource?.Value == null)
            {
                _logger.LogInformation("Event Hubs namespace '{NamespaceName}' not found in resource group '{ResourceGroup}', event hub '{EventHubName}' cannot exist", namespaceName, resourceGroup, eventHubName);
                return false;
            }

            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetIfExistsAsync(eventHubName, cancellationToken);

            if (eventHubResource?.Value == null)
            {
                _logger.LogInformation("Event hub '{EventHubName}' not found in namespace '{NamespaceName}', nothing to delete", eventHubName, namespaceName);
                return false;
            }

            var deleteOperation = await eventHubResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForLroCompletionAsync(deleteOperation, cancellationToken);
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

    public async Task<ConsumerGroup> CreateOrUpdateConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? userMetadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(consumerGroupName), consumerGroupName), (nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        // The event hub may have just been created; a near-immediate GET can 404 due to
        // read-after-write lag, so retry briefly instead of failing outright.
        var eventHub = await GetWithEventualConsistencyRetryAsync(
            async () =>
            {
                var response = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName, cancellationToken);
                return response?.Value;
            },
            () => new KeyNotFoundException($"Event Hub '{eventHubName}' not found in namespace '{namespaceName}'."),
            cancellationToken);

        var consumerGroupData = new EventHubsConsumerGroupData();
        if (!string.IsNullOrEmpty(userMetadata))
        {
            consumerGroupData.UserMetadata = userMetadata;
        }

        var operation = await eventHub.GetEventHubsConsumerGroups().CreateOrUpdateAsync(
            WaitUntil.Started,
            consumerGroupName,
            consumerGroupData,
            cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        // Same read-after-write lag can occur when resolving the newly created consumer group.
        EventHubsConsumerGroupResource consumerGroupResource;
        try
        {
            consumerGroupResource = operation.Value;
        }
        catch (Exception ex) when (ex is RequestFailedException or InvalidOperationException)
        {
            consumerGroupResource = await GetWithEventualConsistencyRetryAsync(
                async () =>
                {
                    var response = await eventHub.GetEventHubsConsumerGroups().GetAsync(consumerGroupName, cancellationToken);
                    return response?.Value;
                },
                () => new InvalidOperationException($"Failed to create or update consumer group '{consumerGroupName}'"),
                cancellationToken);
        }

        if (string.IsNullOrEmpty(consumerGroupResource.Id))
        {
            throw new InvalidOperationException("Consumer group resource ID is missing");
        }

        var resourceId = new ResourceIdentifier(consumerGroupResource.Id!);

        return new(
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

    public async Task<bool> DeleteConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(consumerGroupName), consumerGroupName), (nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);

        try
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (resourceGroupResource?.Value == null)
            {
                _logger.LogInformation("Resource group '{ResourceGroup}' not found, consumer group '{ConsumerGroupName}' cannot exist", resourceGroup, consumerGroupName);
                return false;
            }

            var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetIfExistsAsync(namespaceName, cancellationToken);

            if (namespaceResource?.Value == null)
            {
                _logger.LogInformation("Event Hubs namespace '{NamespaceName}' not found in resource group '{ResourceGroup}', consumer group '{ConsumerGroupName}' cannot exist", namespaceName, resourceGroup, consumerGroupName);
                return false;
            }

            var eventHubResource = await namespaceResource.Value.GetEventHubs().GetIfExistsAsync(eventHubName, cancellationToken);

            if (eventHubResource?.Value == null)
            {
                _logger.LogInformation("Event hub '{EventHubName}' not found in namespace '{NamespaceName}', consumer group '{ConsumerGroupName}' cannot exist", eventHubName, namespaceName, consumerGroupName);
                return false;
            }

            var consumerGroupResource = await eventHubResource.Value.GetEventHubsConsumerGroups().GetIfExistsAsync(consumerGroupName, cancellationToken);

            if (consumerGroupResource?.Value == null)
            {
                _logger.LogInformation("Consumer group '{ConsumerGroupName}' not found in event hub '{EventHubName}', nothing to delete", consumerGroupName, eventHubName);
                return false;
            }

            var deleteOperation = await consumerGroupResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForLroCompletionAsync(deleteOperation, cancellationToken);
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

    public async Task<List<ConsumerGroup>> GetConsumerGroupsAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName, cancellationToken);

        if (eventHubResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hub '{eventHubName}' not found in namespace '{namespaceName}'.");
        }

        var consumerGroups = new List<ConsumerGroup>();

        await foreach (var consumerGroup in eventHubResource.Value.GetEventHubsConsumerGroups().WithCancellation(cancellationToken))
        {
            consumerGroups.Add(ConvertToConsumerGroup(consumerGroup.Data, resourceGroup, namespaceName, eventHubName));
        }

        return consumerGroups;
    }

    public async Task<ConsumerGroup?> GetConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(consumerGroupName), consumerGroupName), (nameof(eventHubName), eventHubName), (nameof(namespaceName), namespaceName), (nameof(resourceGroup), resourceGroup), (nameof(subscription), subscription));

        var subscriptionResource = await ResolveSubscriptionResourceAsync(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        if (resourceGroupResource?.Value == null)
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found.");
        }

        var namespaceResource = await resourceGroupResource.Value.GetEventHubsNamespaces().GetAsync(namespaceName, cancellationToken);

        if (namespaceResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hubs namespace '{namespaceName}' not found in resource group '{resourceGroup}'.");
        }

        var eventHubResource = await namespaceResource.Value.GetEventHubs().GetAsync(eventHubName, cancellationToken);

        if (eventHubResource?.Value == null)
        {
            throw new KeyNotFoundException($"Event Hub '{eventHubName}' not found in namespace '{namespaceName}'.");
        }

        var consumerGroupResource = await eventHubResource.Value.GetEventHubsConsumerGroups().GetIfExistsAsync(consumerGroupName, cancellationToken);

        if (consumerGroupResource?.Value == null)
        {
            return null;
        }

        return ConvertToConsumerGroup(consumerGroupResource.Value.Data, resourceGroup, namespaceName, eventHubName);
    }

    private static ConsumerGroup ConvertToConsumerGroup(EventHubsConsumerGroupData consumerGroupData, string resourceGroup, string namespaceName, string eventHubName)
    {
        return new(
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

    /// <summary>
    /// Returns a SubscriptionResource handle for ARM navigation without making an HTTP call.
    /// This avoids the cache-dependent GET /subscriptions/{id} that GetSubscription() makes,
    /// which caused non-deterministic test proxy recordings.
    /// </summary>
    private async Task<ResourceManager.Resources.SubscriptionResource> ResolveSubscriptionResourceAsync(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
            ? subscription
            : await _subscriptionService.GetSubscriptionIdByName(subscription, tenant, retryPolicy, cancellationToken);

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        return armClient.GetSubscriptionResource(
            ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscriptionId));
    }
}
