// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models;

namespace Microsoft.Mcp.Core.Commands;

[JsonSerializable(typeof(ExceptionResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<JsonNode>))]
[JsonSerializable(typeof(AzureCredentials))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
internal partial class JsonSourceGenerationContext : JsonSerializerContext;
