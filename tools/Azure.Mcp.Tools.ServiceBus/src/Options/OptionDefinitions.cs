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
        $"--{NamespaceName}",
        "The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Queue = new(
        $"--{QueueName}",
        "The queue name to peek messages from."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Subscription = new(
        $"--{SubscriptionName}",
        "The name of subscription to peek messages from."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Topic = new(
        $"--{TopicName}",
        "The name of the topic containing the subscription."
    )
    {
        IsRequired = true
    };

    public static readonly Option<int> MaxMessages = new(
        $"--{MaxMessagesName}",
        () => 1,
        "The maximum number of messages to return."
    )
    {
        IsRequired = false
    };
}
