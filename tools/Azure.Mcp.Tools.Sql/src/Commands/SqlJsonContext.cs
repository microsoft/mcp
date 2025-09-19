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

[JsonSerializable(typeof(DatabaseShowCommand.DatabaseShowResult))]
[JsonSerializable(typeof(DatabaseListCommand.DatabaseListResult))]
[JsonSerializable(typeof(DatabaseCreateCommand.DatabaseCreateResult))]
[JsonSerializable(typeof(DatabaseUpdateCommand.DatabaseUpdateResult))]
[JsonSerializable(typeof(DatabaseDeleteCommand.DatabaseDeleteResult))]
[JsonSerializable(typeof(EntraAdminListCommand.EntraAdminListResult))]
[JsonSerializable(typeof(FirewallRuleListCommand.FirewallRuleListResult))]
[JsonSerializable(typeof(FirewallRuleCreateCommand.FirewallRuleCreateResult))]
[JsonSerializable(typeof(FirewallRuleDeleteCommand.FirewallRuleDeleteResult))]
[JsonSerializable(typeof(ServerCreateCommand.ServerCreateResult))]
[JsonSerializable(typeof(ServerDeleteCommand.ServerDeleteResult))]
[JsonSerializable(typeof(ServerShowCommand.ServerShowResult))]
[JsonSerializable(typeof(ElasticPoolListCommand.ElasticPoolListResult))]
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
[JsonSerializable(typeof(SqlElasticPoolData))]
[JsonSerializable(typeof(SqlElasticPoolProperties))]
[JsonSerializable(typeof(SqlElasticPoolPerDatabaseSettings))]
[JsonSerializable(typeof(SqlFirewallRuleData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SqlJsonContext : JsonSerializerContext;
