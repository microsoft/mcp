// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ServiceBus.Commands.Queue;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;

namespace Azure.Mcp.Tools.ServiceBus.Commands;

[JsonSerializable(typeof(QueuePeekCommand.QueuePeekCommandResult))]
[JsonSerializable(typeof(QueueDetailsCommand.QueueDetailsCommandResult))]
[JsonSerializable(typeof(SubscriptionPeekCommand.SubscriptionPeekCommandResult))]
[JsonSerializable(typeof(SubscriptionDetailsCommand.SubscriptionDetailsCommandResult))]
[JsonSerializable(typeof(TopicDetailsCommand.TopicDetailsCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class ServiceBusJsonContext : JsonSerializerContext
{
}
