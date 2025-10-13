// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventGrid.Commands.Events;
using Azure.Mcp.Tools.EventGrid.Commands.Subscription;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;
using Azure.Mcp.Tools.EventGrid.Models;

namespace Azure.Mcp.Tools.EventGrid.Commands;

// JsonSerializable attributes for all types used in EventGrid command responses and event serialization
[JsonSerializable(typeof(EventGridPublishCommand.EventGridPublishCommandResult))]
[JsonSerializable(typeof(SubscriptionListCommand.SubscriptionListCommandResult))]
[JsonSerializable(typeof(TopicListCommand.TopicListCommandResult))]
[JsonSerializable(typeof(EventGridSubscriptionInfo))]
[JsonSerializable(typeof(EventGridTopicInfo))]
[JsonSerializable(typeof(EventPublishResult))]
[JsonSerializable(typeof(EventGridEventSchema))] // For individual event serialization to EventGrid
[JsonSerializable(typeof(CloudEvent))] // For CloudEvent schema input deserialization
[JsonSerializable(typeof(EventGridEventInput))] // For EventGrid schema input deserialization
[JsonSerializable(typeof(CustomEvent))] // For custom event schema input deserialization
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class EventGridJsonContext : JsonSerializerContext
{
}
