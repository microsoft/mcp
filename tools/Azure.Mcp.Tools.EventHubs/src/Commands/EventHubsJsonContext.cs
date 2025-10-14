// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.ResourceManager.EventHubs.Models;

namespace Azure.Mcp.Tools.EventHubs.Commands;

[JsonSerializable(typeof(Models.ConsumerGroup))]
[JsonSerializable(typeof(ConsumerGroupDeleteCommand.ConsumerGroupDeleteCommandResult))]
[JsonSerializable(typeof(ConsumerGroupGetCommand.ConsumerGroupGetCommandResult))]
[JsonSerializable(typeof(ConsumerGroupUpdateCommand.ConsumerGroupUpdateCommandResult))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(EventHubDeleteCommand.EventHubDeleteCommandResult))]
[JsonSerializable(typeof(EventHubGetCommand.EventHubGetCommandResult))]
[JsonSerializable(typeof(Models.EventHub))]
[JsonSerializable(typeof(EventHubUpdateCommand.EventHubUpdateCommandResult))]
[JsonSerializable(typeof(Models.Namespace))]
[JsonSerializable(typeof(Models.EventHubsSku))]
[JsonSerializable(typeof(NamespaceDeleteCommand.NamespaceDeleteCommandResult))]
[JsonSerializable(typeof(NamespaceGetCommand.NamespaceGetCommandResult))]
[JsonSerializable(typeof(NamespaceGetCommand.NamespacesGetCommandResult))]
[JsonSerializable(typeof(NamespaceUpdateCommand.NamespaceUpdateCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class EventHubsJsonContext : JsonSerializerContext;
