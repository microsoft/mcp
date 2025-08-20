// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ServiceBus.Models.utilities;
using Azure.Messaging.ServiceBus.Administration;

namespace Azure.Mcp.Tools.ServiceBus.Models;

public class TopicDetails
{
    [JsonPropertyName("defaultMessageTimeToLive")]
    public TimeSpan DefaultMessageTimeToLive { get; set; }
    [JsonPropertyName("enablePartitioning")]
    public bool EnablePartitioning { get; set; }
    [JsonPropertyName("maxMessageSizeInKilobytes")]
    public long? MaxMessageSizeInKilobytes { get; set; }
    [JsonPropertyName("maxSizeInMegabytes")]
    public long MaxSizeInMegabytes { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("scheduledMessageCount")]
    public long ScheduledMessageCount { get; set; }
    [JsonPropertyName("sizeInBytes")]
    public long SizeInBytes { get; set; }
    [JsonPropertyName("subscriptionCount")]
    public int SubscriptionCount { get; set; }
    [JsonPropertyName("status")]
    [JsonConverter(typeof(EntityStatusConverter))]
    public EntityStatus Status { get; set; }
}
