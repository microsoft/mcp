// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Commands;

[JsonSerializable(typeof(ContainerListCommand.ContainerListCommandResult))]
[JsonSerializable(typeof(AccountListCommand.AccountListCommandResult))]
[JsonSerializable(typeof(DatabaseListCommand.DatabaseListCommandResult))]
[JsonSerializable(typeof(ItemQueryCommand.ItemQueryCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class CosmosJsonContext : JsonSerializerContext
{
}
