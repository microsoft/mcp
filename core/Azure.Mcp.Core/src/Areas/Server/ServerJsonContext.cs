// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas.Server.Models;
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
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
)]
internal sealed partial class ServerJsonContext : JsonSerializerContext
{
}
