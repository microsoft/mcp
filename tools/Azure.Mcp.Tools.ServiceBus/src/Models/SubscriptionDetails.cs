// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ServiceBus.Models;

public class SubscriptionDetails
{
    [JsonPropertyName("activeMessageCount")]
    public long ActiveMessageCount { get; set; }
    [JsonPropertyName("deadLetteringOnMessageExpiration")]
    public bool DeadLetteringOnMessageExpiration { get; set; }
    [JsonPropertyName("deadLetterMessageCount")]
    public long DeadLetterMessageCount { get; set; }
    [JsonPropertyName("enableBatchedOperations")]
    public bool EnableBatchedOperations { get; set; }
    [JsonPropertyName("forwardDeadLetteredMessagesTo")]
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    [JsonPropertyName("forwardTo")]
    public string? ForwardTo { get; set; }
    [JsonPropertyName("lockDuration")]
    public TimeSpan LockDuration { get; set; }
    [JsonPropertyName("maxDeliveryCount")]
    public int MaxDeliveryCount { get; set; }
    [JsonPropertyName("requiresSession")]
    public bool RequiresSession { get; set; }
    [JsonPropertyName("subscriptionName")]
    public string? SubscriptionName { get; set; }
    [JsonPropertyName("topicName")]
    public string? TopicName { get; set; }
    [JsonPropertyName("totalMessageCount")]
    public long TotalMessageCount { get; set; }
    [JsonPropertyName("transferDeadLetterMessageCount")]
    public long TransferDeadLetterMessageCount { get; set; }
    [JsonPropertyName("transferMessageCount")]
    public long TransferMessageCount { get; set; }
}
