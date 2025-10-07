// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;

namespace Azure.Mcp.Tools.EventHubs.Commands;

[JsonSerializable(typeof(EventHubDeleteCommand.EventHubDeleteCommandResult))]
[JsonSerializable(typeof(EventHubGetCommand.EventHubGetCommandResult))]
[JsonSerializable(typeof(EventHubInfo))]
[JsonSerializable(typeof(EventHubsNamespaceData))]
[JsonSerializable(typeof(EventHubsNamespaceSku))]
[JsonSerializable(typeof(EventHubUpdateCommand.EventHubUpdateCommandResult))]
[JsonSerializable(typeof(Models.Namespace))]
[JsonSerializable(typeof(NamespaceGetCommand.NamespaceGetCommandResult))]
[JsonSerializable(typeof(NamespaceGetCommand.NamespacesGetCommandResult))]
[JsonSerializable(typeof(NamespaceUpdateCommand.NamespaceUpdateCommandResult))]
[JsonSerializable(typeof(NamespaceDeleteCommand.NamespaceDeleteCommandResult))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class EventHubsJsonContext : JsonSerializerContext;
