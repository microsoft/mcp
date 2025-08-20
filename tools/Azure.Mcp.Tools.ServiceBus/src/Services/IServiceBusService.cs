// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Messaging.ServiceBus;

namespace Azure.Mcp.Tools.ServiceBus.Services;

public interface IServiceBusService
{
    /// <summary>
    /// Gets the details of a Service Bus subscription.
    /// </summary>
    /// <param name="namespaceName">The Service Bus namespace name</param>
    /// <param name="topicName">The topic name containing the subscription</param>
    /// <param name="subscriptionName">The subscription name to get details for</param>
    /// <param name="tenantId">Optional tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy</param>
    /// <returns>Subscription details</returns>
    /// <exception cref="RequestFailedException">When the service request fails</exception>
    Task<SubscriptionDetails> GetSubscriptionDetails(
        string namespaceName,
        string topicName,
        string subscriptionName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets the details of a Service Bus queue.
    /// </summary>
    /// <param name="namespaceName">The Service Bus namespace name</param>
    /// <param name="queueName">The queue name to get details for</param>
    /// <param name="subscription">Subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy</param>
    /// <returns>Queue details</returns>
    /// <exception cref="RequestFailedException">When the service request fails</exception>
    Task<QueueDetails> GetQueueDetails(
        string namespaceName,
        string queueName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets the details of a Service Bus topic.
    /// </summary>
    /// <param name="namespaceName">The Service Bus namespace name</param>
    /// <param name="topicName">The topic name to get details for</param>
    /// <param name="tenantId">Optional tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy</param>
    /// <returns>Topic details</returns>
    /// <exception cref="RequestFailedException">When the service request fails</exception>
    Task<TopicDetails> GetTopicDetails(
        string namespaceName,
        string topicName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Peeks messages from a Service Bus queue without removing them.
    /// </summary>
    /// <param name="namespaceName">The Service Bus namespace name</param>
    /// <param name="queueName">The queue name to peek messages from</param>
    /// <param name="maxMessages">Maximum number of messages to peek (default: 1)</param>
    /// <param name="subscription">Subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy</param>
    /// <returns>List of peeked messages</returns>
    /// <exception cref="RequestFailedException">When the service request fails</exception>
    Task<List<ServiceBusReceivedMessage>> PeekQueueMessages(
        string namespaceName,
        string queueName,
        int maxMessages,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Peeks messages from a Service Bus subscription without removing them.
    /// </summary>
    /// <param name="namespaceName">The Service Bus namespace name</param>
    /// <param name="topicName">The topic name containing the subscription</param>
    /// <param name="subscriptionName">The subscription name to peek messages from</param>
    /// <param name="maxMessages">Maximum number of messages to peek (default: 1)</param>
    /// <param name="subscription">Subscription ID or name</param>
    /// <param name="tenantId">Optional tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy</param>
    /// <returns>List of peeked messages</returns>
    /// <exception cref="RequestFailedException">When the service request fails</exception>
    Task<List<ServiceBusReceivedMessage>> PeekSubscriptionMessages(
        string namespaceName,
        string topicName,
        string subscriptionName,
        int maxMessages,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);
}
