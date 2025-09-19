// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.ServiceBus.Options;
using Azure.Messaging.ServiceBus;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.LiveTests
{
    public class ServiceBusCommandTests : CommandTestsBase
    {
        private const string QueueName = "queue1";
        private const string TopicName = "topic1";
        private const string SubscriptionName = "subscription1";

        public ServiceBusCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "The command for this test has been commented out until we know how to surface binary data.")]
        public async Task Queue_peek_messages()
        {
            var numberOfMessages = 2;

            await SendTestMessages(QueueName, numberOfMessages);

            var result = await CallToolAsync(
                "azmcp_servicebus_queue_peek",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.QueueName, QueueName },
                    { ServiceBusOptionDefinitions.NamespaceName, $"{Settings.ResourceBaseName}.servicebus.windows.net"},
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
                "azmcp_servicebus_topic_subscription_peek",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.NamespaceName, $"{Settings.ResourceBaseName}.servicebus.windows.net"},
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
                "azmcp_servicebus_queue_details",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.QueueName, QueueName },
                    { ServiceBusOptionDefinitions.NamespaceName, $"{Settings.ResourceBaseName}.servicebus.windows.net"},
                });

            var details = result.AssertProperty("queueDetails");
            Assert.Equal(JsonValueKind.Object, details.ValueKind);
        }

        [Fact]
        public async Task Topic_details()
        {
            var result = await CallToolAsync(
                "azmcp_servicebus_topic_details",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.TopicName, TopicName },
                    { ServiceBusOptionDefinitions.NamespaceName, $"{Settings.ResourceBaseName}.servicebus.windows.net"},
                });

            var details = result.AssertProperty("topicDetails");
            Assert.Equal(JsonValueKind.Object, details.ValueKind);
        }

        [Fact]
        public async Task Subscription_details()
        {
            var result = await CallToolAsync(
                "azmcp_servicebus_topic_subscription_details",
                new()
                {
                    { OptionDefinitions.Common.SubscriptionName, Settings.SubscriptionId },
                    { ServiceBusOptionDefinitions.TopicName, TopicName },
                    { ServiceBusOptionDefinitions.SubscriptionName, SubscriptionName },
                    { ServiceBusOptionDefinitions.NamespaceName, $"{Settings.ResourceBaseName}.servicebus.windows.net"},
                });

            var details = result.AssertProperty("subscriptionDetails");
            Assert.Equal(JsonValueKind.Object, details.ValueKind);
        }

        private async Task SendTestMessages(string queueOrTopicName, int numberOfMessages)
        {
            var credentials = new CustomChainedCredential(Settings.TenantId);
            await using (var client = new ServiceBusClient($"{Settings.ResourceBaseName}.servicebus.windows.net", credentials))
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
