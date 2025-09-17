// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventGrid.Commands.Subscription;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;

namespace Azure.Mcp.Tools.EventGrid.Commands;

[JsonSerializable(typeof(SubscriptionListCommand.SubscriptionListCommandResult))]
[JsonSerializable(typeof(TopicListCommand.TopicListCommandResult))]
[JsonSerializable(typeof(EventGridSubscriptionInfo))]
[JsonSerializable(typeof(EventGridTopicInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class EventGridJsonContext : JsonSerializerContext
{
}
