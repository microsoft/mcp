// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Commands.Table;

namespace Azure.Mcp.Tools.Postgres.Commands;

[JsonSerializable(typeof(DatabaseListCommand.DatabaseListCommandResult))]
[JsonSerializable(typeof(DatabaseQueryCommand.DatabaseQueryCommandResult))]
[JsonSerializable(typeof(ServerConfigGetCommand.ServerConfigGetCommandResult))]
[JsonSerializable(typeof(ServerParamGetCommand.ServerParamGetCommandResult))]
[JsonSerializable(typeof(ServerParamSetCommand.ServerParamSetCommandResult))]
[JsonSerializable(typeof(ServerListCommand.ServerListCommandResult))]
[JsonSerializable(typeof(TableSchemaGetCommand.TableSchemaGetCommandResult))]
[JsonSerializable(typeof(TableListCommand.TableListCommandResult))]

internal sealed partial class PostgresJsonContext : JsonSerializerContext
{
}
