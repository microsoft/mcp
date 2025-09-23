// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Services;

public interface IEventGridService
{
    Task<List<EventGridTopicInfo>> GetTopicsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<EventGridSubscriptionInfo>> GetSubscriptionsAsync(
        string subscription,
        string? resourceGroup = null,
        string? topicName = null,
        string? location = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
