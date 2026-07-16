// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services.Models;

namespace Azure.Mcp.Tools.Sql.Commands;

[JsonSerializable(typeof(DatabaseGetCommand.DatabaseGetCommandResult))]
[JsonSerializable(typeof(DatabaseCreateCommand.DatabaseCreateCommandResult))]
[JsonSerializable(typeof(DatabaseUpdateCommand.DatabaseUpdateCommandResult))]
[JsonSerializable(typeof(DatabaseRenameCommand.DatabaseRenameCommandResult))]
[JsonSerializable(typeof(DatabaseDeleteCommand.DatabaseDeleteCommandResult))]
[JsonSerializable(typeof(EntraAdminListCommand.EntraAdminListCommandResult))]
[JsonSerializable(typeof(FirewallRuleListCommand.FirewallRuleListCommandResult))]
[JsonSerializable(typeof(FirewallRuleCreateCommand.FirewallRuleCreateCommandResult))]
[JsonSerializable(typeof(FirewallRuleDeleteCommand.FirewallRuleDeleteCommandResult))]
[JsonSerializable(typeof(ServerGetCommand.ServerGetCommandResult))]
[JsonSerializable(typeof(ServerCreateCommand.ServerCreateCommandResult))]
[JsonSerializable(typeof(ServerDeleteCommand.ServerDeleteCommandResult))]
[JsonSerializable(typeof(ElasticPoolListCommand.ElasticPoolListCommandResult))]
[JsonSerializable(typeof(SqlDatabase))]
[JsonSerializable(typeof(SqlServer))]
[JsonSerializable(typeof(SqlServerEntraAdministrator))]
[JsonSerializable(typeof(SqlServerFirewallRule))]
[JsonSerializable(typeof(SqlElasticPool))]
[JsonSerializable(typeof(DatabaseSku))]
[JsonSerializable(typeof(ElasticPoolSku))]
[JsonSerializable(typeof(ElasticPoolPerDatabaseSettings))]
[JsonSerializable(typeof(SqlDatabaseData))]
[JsonSerializable(typeof(SqlDatabaseProperties))]
[JsonSerializable(typeof(SqlServerAadAdministratorData))]
[JsonSerializable(typeof(SqlFirewallRuleData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SqlJsonContext : JsonSerializerContext;
