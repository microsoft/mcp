// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Commands;

[JsonSerializable(typeof(CosmosListCommand.CosmosListCommandResult))]
[JsonSerializable(typeof(ItemQueryCommand.ItemQueryCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class CosmosJsonContext : JsonSerializerContext
{
}
