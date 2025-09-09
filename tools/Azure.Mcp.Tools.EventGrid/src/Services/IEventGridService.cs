// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventGrid.Models;

namespace Azure.Mcp.Tools.EventGrid.Services;

public interface IEventGridService
{
    Task<List<EventGridTopicInfo>> GetTopicsAsync(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<EventGridSubscriptionInfo>> GetSubscriptionsAsync(
        string subscription,
        string? resourceGroup = null,
        string? topicName = null,
        string? location = null,
        RetryPolicyOptions? retryPolicy = null);
}
