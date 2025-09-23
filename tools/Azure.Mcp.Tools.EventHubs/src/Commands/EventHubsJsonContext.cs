// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Services.Models;

namespace Azure.Mcp.Tools.EventHubs.Commands;

[JsonSerializable(typeof(NamespaceGetCommand.NamespaceGetCommandResult))]
[JsonSerializable(typeof(NamespaceGetCommand.NamespaceGetSingleCommandResult))]
[JsonSerializable(typeof(EventHubsNamespaceInfo))]
[JsonSerializable(typeof(EventHubsNamespaceDetails))]
[JsonSerializable(typeof(EventHubsNamespaceSku))]
[JsonSerializable(typeof(EventHubsNamespaceData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class EventHubsJsonContext : JsonSerializerContext;
