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

    internal const string NamespaceDescription = "The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)";
    internal const string SubscriptionNameDescription = "The Service Bus subscription name.";
    internal const string MaxMessagesDescription = "The maximum number of messages to return. Default is 1.";
}
