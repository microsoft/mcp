// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;

namespace Azure.Mcp.Tools.EventHubs.Commands;

[JsonSerializable(typeof(NamespaceListCommand.NamespaceListCommandResult))]
[JsonSerializable(typeof(EventHubsNamespaceInfo))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class EventHubsJsonContext : JsonSerializerContext;
