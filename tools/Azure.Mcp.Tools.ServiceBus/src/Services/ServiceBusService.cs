// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Azure.Mcp.Tools.ServiceBus.Services;

public class ServiceBusService(ITenantService tenantService) : BaseAzureService(tenantService), IServiceBusService
{
    public async Task<QueueDetails> GetQueueDetails(
        string namespaceName,
        string queueName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(cancellationToken);
        var client = new ServiceBusAdministrationClient(namespaceName, credential);
        var runtimeProperties = (await client.GetQueueRuntimePropertiesAsync(queueName, cancellationToken)).Value;
        var properties = (await client.GetQueueAsync(queueName, cancellationToken)).Value;

        return new QueueDetails
        {
            DefaultMessageTimeToLive = properties.DefaultMessageTimeToLive,
            EnablePartitioning = properties.EnablePartitioning,
            MaxMessageSizeInKilobytes = properties.MaxMessageSizeInKilobytes,
            MaxSizeInMegabytes = properties.MaxSizeInMegabytes,
            Name = properties.Name,
            Status = properties.Status,

            ActiveMessageCount = runtimeProperties.ActiveMessageCount,
            DeadLetteringOnMessageExpiration = properties.DeadLetteringOnMessageExpiration,
            DeadLetterMessageCount = runtimeProperties.DeadLetterMessageCount,
            ForwardDeadLetteredMessagesTo = properties.ForwardDeadLetteredMessagesTo,
            ForwardTo = properties.ForwardTo,
            LockDuration = properties.LockDuration,
            MaxDeliveryCount = properties.MaxDeliveryCount,
            RequiresSession = properties.RequiresSession,
            ScheduledMessageCount = runtimeProperties.ScheduledMessageCount,
            SizeInBytes = runtimeProperties.SizeInBytes,
            TotalMessageCount = runtimeProperties.TotalMessageCount,
            TransferDeadLetterMessageCount = runtimeProperties.TransferDeadLetterMessageCount,
            TransferMessageCount = runtimeProperties.TransferMessageCount,
        };
    }

    public async Task<SubscriptionDetails> GetSubscriptionDetails(
        string namespaceName,
        string topicName,
        string subscriptionName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(cancellationToken);
        var client = new ServiceBusAdministrationClient(namespaceName, credential);
        var runtimeProperties = (await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName, cancellationToken)).Value;
        var properties = (await client.GetSubscriptionAsync(topicName, subscriptionName, cancellationToken)).Value;

        return new SubscriptionDetails
        {
            ActiveMessageCount = runtimeProperties.ActiveMessageCount,
            DeadLetteringOnMessageExpiration = properties.DeadLetteringOnMessageExpiration,
            DeadLetterMessageCount = runtimeProperties.DeadLetterMessageCount,
            EnableBatchedOperations = properties.EnableBatchedOperations,
            ForwardDeadLetteredMessagesTo = properties.ForwardDeadLetteredMessagesTo,
            ForwardTo = properties.ForwardTo,
            LockDuration = properties.LockDuration,
            MaxDeliveryCount = properties.MaxDeliveryCount,
            RequiresSession = properties.RequiresSession,
            TotalMessageCount = runtimeProperties.TotalMessageCount,
            SubscriptionName = runtimeProperties.SubscriptionName,
            TopicName = runtimeProperties.TopicName,
            TransferMessageCount = runtimeProperties.TransferMessageCount,
            TransferDeadLetterMessageCount = runtimeProperties.TransferDeadLetterMessageCount,
        };
    }

    public async Task<TopicDetails> GetTopicDetails(
        string namespaceName,
        string topicName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(cancellationToken);
        var client = new ServiceBusAdministrationClient(namespaceName, credential);
        var runtimeProperties = (await client.GetTopicRuntimePropertiesAsync(topicName, cancellationToken)).Value;
        var properties = (await client.GetTopicAsync(topicName, cancellationToken)).Value;

        return new TopicDetails
        {
            DefaultMessageTimeToLive = properties.DefaultMessageTimeToLive,
            EnablePartitioning = properties.EnablePartitioning,
            MaxMessageSizeInKilobytes = properties.MaxMessageSizeInKilobytes,
            MaxSizeInMegabytes = properties.MaxSizeInMegabytes,
            Name = properties.Name,
            Status = properties.Status,

            SubscriptionCount = runtimeProperties.SubscriptionCount,
            SizeInBytes = runtimeProperties.SizeInBytes,
            ScheduledMessageCount = runtimeProperties.ScheduledMessageCount,
        };
    }

    public async Task<List<ServiceBusReceivedMessage>> PeekQueueMessages(
        string namespaceName,
        string queueName,
        int maxMessages,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(cancellationToken);

        await using (var client = new ServiceBusClient(namespaceName, credential))
        await using (var receiver = client.CreateReceiver(queueName))
        {
            var messages = await receiver.PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken);

            return [.. messages];
        }
    }

    public async Task<List<ServiceBusReceivedMessage>> PeekSubscriptionMessages(
        string namespaceName,
        string topicName,
        string subscriptionName,
        int maxMessages,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(cancellationToken);

        await using (var client = new ServiceBusClient(namespaceName, credential))
        await using (var receiver = client.CreateReceiver(topicName, subscriptionName))
        {
            var messages = await receiver.PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken);

            return [.. messages];
        }
    }
}
