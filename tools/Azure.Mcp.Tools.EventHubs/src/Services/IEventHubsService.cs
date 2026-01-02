// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Models;

namespace Azure.Mcp.Tools.EventHubs.Services;

public interface IEventHubsService
{
    Task<List<Namespace>> GetNamespacesAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<Namespace> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<Namespace> CreateOrUpdateNamespaceAsync(
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
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<bool> DeleteNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<List<EventHub>> GetEventHubsAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<EventHub?> GetEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<EventHub> CreateOrUpdateEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        int? partitionCount = null,
        long? messageRetentionInHours = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<bool> DeleteEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<ConsumerGroup> CreateOrUpdateConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? userMetadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<bool> DeleteConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<List<ConsumerGroup>> GetConsumerGroupsAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);

    Task<ConsumerGroup?> GetConsumerGroupAsync(
        string consumerGroupName,
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default,
        Uri? authorityHost = null);
}
