// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.ServiceBus.Services;

public sealed class ServiceBusService(IAzureService azureService)
    : BaseAzureService(azureService), IServiceBusService
{
    /// <summary>
    /// Authorizes a bare Service Bus namespace host for the configured cloud and returns its parsed host.
    /// </summary>
    /// <param name="namespaceName">
    /// The fully qualified DNS hostname of the Service Bus namespace, without a scheme, port, path,
    /// query, fragment, or user information.
    /// </param>
    /// <returns>
    /// The parsed ASCII hostname to pass to <see cref="ServiceBusAdministrationClient"/> or <see cref="ServiceBusClient"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="namespaceName"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the namespace is empty, whitespace, not a DNS hostname, or the allow-list is unavailable.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when the namespace cannot form a valid endpoint or is outside the configured cloud's Service Bus domains.
    /// </exception>
    private string GetValidatedNamespace(string namespaceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namespaceName);

        // ServiceBusAdministrationClient and ServiceBusClient document fullyQualifiedNamespace as a namespace hostname:
        // https://learn.microsoft.com/dotnet/api/azure.messaging.servicebus.administration.servicebusadministrationclient.-ctor
        // https://learn.microsoft.com/dotnet/api/azure.messaging.servicebus.servicebusclient.-ctor
        // Check the entire input as a DNS hostname before URI construction so URL components cannot be parsed
        // separately and then discarded by IdnHost. CheckHostName checks syntax without a DNS lookup;
        // EndpointValidator below still authorizes the cloud-specific domain.
        // https://learn.microsoft.com/dotnet/api/system.uri.checkhostname
        if (Uri.CheckHostName(namespaceName) != UriHostNameType.Dns)
        {
            throw new ArgumentException(
                $"Namespace name must be a bare DNS hostname (e.g. 'mynamespace.servicebus.windows.net'). Received: '{namespaceName}'.",
                nameof(namespaceName));
        }

        if (!Uri.TryCreate($"https://{namespaceName}/", UriKind.Absolute, out Uri? endpoint))
        {
            throw new SecurityException($"Service Bus namespace is not a valid hostname. Received: '{namespaceName}'.");
        }

        // HTTPS is an authorization-only representation here, not a transport choice for ServiceBusClient.
        // Reuse the shared cloud and namespace-bypass policy, then pass only the parsed host to
        // ServiceBusAdministrationClient or ServiceBusClient so each receives the authorized destination.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: endpoint.AbsoluteUri,
            serviceType: "servicebus",
            armEnvironment: AzureService.CloudConfiguration.ArmEnvironment,
            executingToolNamespaceName: "servicebus");

        return endpoint.IdnHost;
    }

    private async Task<ServiceBusAdministrationClient> CreateAdministrationClient(
        string namespaceName,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(tenantId, cancellationToken);
        var options = AddDefaultPolicies(new ServiceBusAdministrationClientOptions());
        options.Transport = new HttpClientTransport(AzureService.GetClient());
        return new ServiceBusAdministrationClient(namespaceName, credential, options);
    }

    public async Task<QueueDetails> GetQueueDetails(
        string namespaceName,
        string queueName,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        namespaceName = GetValidatedNamespace(namespaceName);
        var client = await CreateAdministrationClient(namespaceName, tenantId, cancellationToken);
        var runtimeProperties = (await client.GetQueueRuntimePropertiesAsync(queueName, cancellationToken)).Value;
        var properties = (await client.GetQueueAsync(queueName, cancellationToken)).Value;

        return new()
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
        CancellationToken cancellationToken = default)
    {
        namespaceName = GetValidatedNamespace(namespaceName);
        var client = await CreateAdministrationClient(namespaceName, tenantId, cancellationToken);
        var runtimeProperties = (await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName, cancellationToken)).Value;
        var properties = (await client.GetSubscriptionAsync(topicName, subscriptionName, cancellationToken)).Value;

        return new()
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
        CancellationToken cancellationToken = default)
    {
        namespaceName = GetValidatedNamespace(namespaceName);
        var client = await CreateAdministrationClient(namespaceName, tenantId, cancellationToken);
        var runtimeProperties = (await client.GetTopicRuntimePropertiesAsync(topicName, cancellationToken)).Value;
        var properties = (await client.GetTopicAsync(topicName, cancellationToken)).Value;

        return new()
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
        CancellationToken cancellationToken = default)
    {
        namespaceName = GetValidatedNamespace(namespaceName);
        var credential = await GetCredential(tenantId, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        namespaceName = GetValidatedNamespace(namespaceName);
        var credential = await GetCredential(tenantId, cancellationToken);

        await using (var client = new ServiceBusClient(namespaceName, credential))
        await using (var receiver = client.CreateReceiver(topicName, subscriptionName))
        {
            var messages = await receiver.PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken);

            return [.. messages];
        }
    }
}
