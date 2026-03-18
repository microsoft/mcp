// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

namespace Azure.Mcp.Tools.Cosmos.Commands;

[JsonSerializable(typeof(CosmosListCommand.CosmosListCommandResult))]
[JsonSerializable(typeof(ItemQueryCommand.ItemQueryCommandResult))]
[JsonSerializable(typeof(CopyJobResult))]
[JsonSerializable(typeof(CopyJobListResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class CosmosJsonContext : JsonSerializerContext
{
}
