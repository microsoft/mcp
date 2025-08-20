// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ServiceBus.Models.utilities;
using Azure.Messaging.ServiceBus.Administration;

namespace Azure.Mcp.Tools.ServiceBus.Models;

public class QueueDetails
{
    [JsonPropertyName("activeMessageCount")]
    public long ActiveMessageCount { get; set; }
    [JsonPropertyName("deadLetteringOnMessageExpiration")]
    public bool DeadLetteringOnMessageExpiration { get; set; }
    [JsonPropertyName("deadLetterMessageCount")]
    public long DeadLetterMessageCount { get; set; }
    [JsonPropertyName("defaultMessageTimeToLive")]
    public TimeSpan DefaultMessageTimeToLive { get; set; }
    [JsonPropertyName("enablePartitioning")]
    public bool EnablePartitioning { get; set; }
    [JsonPropertyName("forwardDeadLetteredMessagesTo")]
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    [JsonPropertyName("forwardTo")]
    public string? ForwardTo { get; set; }
    [JsonPropertyName("lockDuration")]
    public TimeSpan LockDuration { get; set; }
    [JsonPropertyName("maxDeliveryCount")]
    public int MaxDeliveryCount { get; set; }
    [JsonPropertyName("maxMessageSizeInKilobytes")]
    public long? MaxMessageSizeInKilobytes { get; set; }
    [JsonPropertyName("maxSizeInMegabytes")]
    public long MaxSizeInMegabytes { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("requiresSession")]
    public bool RequiresSession { get; set; }
    [JsonPropertyName("scheduledMessageCount")]
    public long ScheduledMessageCount { get; set; }
    [JsonPropertyName("sizeInBytes")]
    public long SizeInBytes { get; set; }
    [JsonPropertyName("status")]
    [JsonConverter(typeof(EntityStatusConverter))]
    public EntityStatus Status { get; set; }
    [JsonPropertyName("totalMessageCount")]
    public long TotalMessageCount { get; set; }
    [JsonPropertyName("transferDeadLetterMessageCount")]
    public long TransferDeadLetterMessageCount { get; set; }
    [JsonPropertyName("transferMessageCount")]
    public long TransferMessageCount { get; set; }
}
