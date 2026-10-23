// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Cosmos.Commands.Container;
using Azure.Mcp.Tools.Cosmos.Commands.Item;

namespace Azure.Mcp.Tools.Cosmos.Commands;

[JsonSerializable(typeof(CosmosListCommand.CosmosListCommandResult))]
[JsonSerializable(typeof(ItemQueryCommand.ItemQueryCommandResult))]
[JsonSerializable(typeof(ContainerSchemaInferCommand.ContainerSchemaInferCommandResult))]
[JsonSerializable(typeof(ItemListRecentCommand.ItemListRecentCommandResult))]
[JsonSerializable(typeof(ItemGetCommand.ItemGetCommandResult))]
[JsonSerializable(typeof(ItemTextSearchCommand.ItemTextSearchCommandResult))]
[JsonSerializable(typeof(ItemVectorSearchCommand.ItemVectorSearchCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class CosmosJsonContext : JsonSerializerContext
{
}
