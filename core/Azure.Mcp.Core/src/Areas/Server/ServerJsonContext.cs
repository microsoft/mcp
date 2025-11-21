// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Metadata;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Core.Areas.Server;

[JsonSerializable(typeof(RegistryRoot))]
[JsonSerializable(typeof(Dictionary<string, RegistryServerInfo>))]
[JsonSerializable(typeof(RegistryServerInfo))]
[JsonSerializable(typeof(ListToolsResult))]
[JsonSerializable(typeof(IList<McpClientTool>))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(List<Tool>))]
[JsonSerializable(typeof(ToolInputSchema))]
[JsonSerializable(typeof(ToolPropertySchema))]
[JsonSerializable(typeof(ToolMetadata))]
[JsonSerializable(typeof(MetadataDefinition))]
[JsonSerializable(typeof(ConsolidatedToolDefinition))]
[JsonSerializable(typeof(List<ConsolidatedToolDefinition>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
)]
internal sealed partial class ServerJsonContext : JsonSerializerContext
{
}
