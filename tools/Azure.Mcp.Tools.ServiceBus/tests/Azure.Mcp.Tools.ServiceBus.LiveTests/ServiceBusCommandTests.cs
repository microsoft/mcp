// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.ServiceBus.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.LiveTests
{
    public class ServiceBusCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
    {
        private const string QueueName = "queue1";
        private const string TopicName = "topic1";
        private const string SubscriptionName = "subscription1";

        /// <summary>
        /// Builds the fully qualified Service Bus namespace name using the cloud-specific
        /// DNS suffix so live tests work in sovereign clouds (Public, Gov, China).
        /// </summary>
        private string NamespaceFqdn => $"{Settings.ResourceBaseName}{ServiceBusSuffix(Settings.Cloud)}";

        private static string ServiceBusSuffix(AzureCloud cloud) => cloud switch
        {
            AzureCloud.AzureUSGovernmentCloud => ".servicebus.usgovcloudapi.net",
            AzureCloud.AzureChinaCloud => ".servicebus.chinacloudapi.cn",
            _ => ".servicebus.windows.net",
        };

        [Fact(Skip = "The command for this test has been commented out until we know how to surface binary data.")]
        public async Task Queue_peek_messages()
        {
            var numberOfMessages = 2;

            await SendTestMessages(QueueName, numberOfMessages);

            var result = await CallToolAsync(
                "servicebus_queue_peek",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.QueueName, QueueName },
                    { ServiceBusOptionDefinitions.NamespaceName, NamespaceFqdn},
                    { ServiceBusOptionDefinitions.MaxMessagesName, numberOfMessages.ToString() }
                });

            var messages = result.AssertProperty("messages");
            Assert.Equal(JsonValueKind.Array, messages.ValueKind);
            Assert.Equal(numberOfMessages, messages.GetArrayLength());
        }

        [Fact(Skip = "The command for this test has been commented out until we know how to surface binary data.")]
        public async Task Topic_subscription_peek_messages()
        {
            var numberOfMessages = 2;

            await SendTestMessages(TopicName, numberOfMessages);

            var result = await CallToolAsync(
                "servicebus_topic_subscription_peek",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.NamespaceName, NamespaceFqdn},
                    { ServiceBusOptionDefinitions.TopicName, TopicName },
                    { ServiceBusOptionDefinitions.SubscriptionName, SubscriptionName },
                    { ServiceBusOptionDefinitions.MaxMessagesName, numberOfMessages.ToString() }
                });

            var messages = result.AssertProperty("messages");
            Assert.Equal(JsonValueKind.Array, messages.ValueKind);
            Assert.Equal(numberOfMessages, messages.GetArrayLength());
        }

        [Fact]
        public async Task Queue_details()
        {
            var result = await CallToolAsync(
                "servicebus_queue_details",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.QueueName, QueueName },
                    { ServiceBusOptionDefinitions.NamespaceName, NamespaceFqdn},
                });

            var details = result.AssertProperty("queueDetails");
            Assert.Equal(JsonValueKind.Object, details.ValueKind);
        }

        [Fact]
        public async Task Topic_details()
        {
            var result = await CallToolAsync(
                "servicebus_topic_details",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.TopicName, TopicName },
                    { ServiceBusOptionDefinitions.NamespaceName, NamespaceFqdn},
                });

            var details = result.AssertProperty("topicDetails");
            Assert.Equal(JsonValueKind.Object, details.ValueKind);
        }

        [Fact]
        public async Task Subscription_details()
        {
            var result = await CallToolAsync(
                "servicebus_topic_subscription_details",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.TopicName, TopicName },
                    { ServiceBusOptionDefinitions.SubscriptionName, SubscriptionName },
                    { ServiceBusOptionDefinitions.NamespaceName, NamespaceFqdn},
                });

            var details = result.AssertProperty("subscriptionDetails");
            Assert.Equal(JsonValueKind.Object, details.ValueKind);
        }

        private async Task SendTestMessages(string queueOrTopicName, int numberOfMessages)
        {
            var tokenProvider = new SingleIdentityTokenCredentialProvider(NullLoggerFactory.Instance);
            TokenCredential credentials = await tokenProvider.GetTokenCredentialAsync(Settings.TenantId, default);
            await using (var client = new ServiceBusClient(NamespaceFqdn, credentials))
            await using (var sender = client.CreateSender(queueOrTopicName))
            {
                var batch = await sender.CreateMessageBatchAsync(TestContext.Current.CancellationToken);

                for (int i = 0; i < numberOfMessages; i++)
                {
                    Assert.True(batch.TryAddMessage(new ServiceBusMessage("Message " + i)),
                        $"Unable to add message #{i} to batch.");
                }

                await sender.SendMessagesAsync(batch, TestContext.Current.CancellationToken);
            }
        }
    }
}
