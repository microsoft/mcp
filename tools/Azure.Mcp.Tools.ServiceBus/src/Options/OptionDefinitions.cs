// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ServiceBus.Options;

public static class ServiceBusOptionDefinitions
{
    public const string NamespaceName = "namespace";
    public const string QueueName = "queue";
    public const string MaxMessagesName = "max-messages";
    public const string TopicName = "topic";
    public const string SubscriptionName = "subscription-name";

    public static readonly Option<string> Namespace = new(
        $"--{NamespaceName}"
    )
    {
        Description = "The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)",
        Required = true
    };

    public static readonly Option<string> Queue = new(
        $"--{QueueName}"
    )
    {
        Description = "The queue name to peek messages from.",
        Required = true
    };

    public static readonly Option<string> Subscription = new(
        $"--{SubscriptionName}"
    )
    {
        Description = "The name of subscription to peek messages from.",
        Required = true
    };

    public static readonly Option<string> Topic = new(
        $"--{TopicName}"
    )
    {
        Description = "The name of the topic containing the subscription.",
        Required = true
    };

    public static readonly Option<int> MaxMessages = new($"--{MaxMessagesName}")
    {
        Description = "The maximum number of messages to return.",
        DefaultValueFactory = _ => 1,
        Required = false
    };
}
