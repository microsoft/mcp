// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;

namespace Azure.Mcp.Tools.EventGrid.Commands;

[JsonSerializable(typeof(TopicListCommand.TopicListCommandResult))]
[JsonSerializable(typeof(EventGridTopicInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class EventGridJsonContext : JsonSerializerContext
{
}
