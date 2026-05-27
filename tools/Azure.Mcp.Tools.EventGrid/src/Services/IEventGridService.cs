// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventGrid.Services;

public interface IEventGridService
{
    Task<List<EventGridTopicInfo>> GetTopicsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<EventGridSubscriptionInfo>> GetSubscriptionsAsync(
        string subscription,
        string? resourceGroup = null,
        string? topicName = null,
        string? location = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<EventPublishResult> PublishEventAsync(
        string subscription,
        string? resourceGroup,
        string topicName,
        string eventData,
        string? eventSchema = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<EventGridTopicInfo?> CreateTopicAsync(
        string topic,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
