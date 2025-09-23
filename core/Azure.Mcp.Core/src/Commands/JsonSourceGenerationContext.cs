// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;

namespace Azure.Mcp;

[JsonSerializable(typeof(ExceptionResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<JsonNode>))]
[JsonSerializable(typeof(AzureCredentials))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class JsonSourceGenerationContext : JsonSerializerContext
{

}
