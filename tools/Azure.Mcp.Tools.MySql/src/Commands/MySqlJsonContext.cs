// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;

namespace Azure.Mcp.Tools.MySql.Commands;

[JsonSerializable(typeof(DatabaseListCommand.DatabaseListCommandResult))]
[JsonSerializable(typeof(DatabaseQueryCommand.DatabaseQueryCommandResult))]
[JsonSerializable(typeof(ServerConfigGetCommand.ServerConfigGetCommandResult))]
[JsonSerializable(typeof(ServerParamGetCommand.ServerParamGetCommandResult))]
[JsonSerializable(typeof(ServerParamSetCommand.ServerParamSetCommandResult))]
[JsonSerializable(typeof(ServerListCommand.ServerListCommandResult))]
[JsonSerializable(typeof(TableSchemaGetCommand.TableSchemaGetCommandResult))]
[JsonSerializable(typeof(TableListCommand.TableListCommandResult))]
[JsonSerializable(typeof(MySqlService.ServerConfigGetResult))]

internal sealed partial class MySqlJsonContext : JsonSerializerContext
{
}
