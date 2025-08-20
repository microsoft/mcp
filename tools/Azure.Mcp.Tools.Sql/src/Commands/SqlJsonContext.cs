// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Models;

namespace Azure.Mcp.Tools.Sql.Commands;

[JsonSerializable(typeof(DatabaseShowCommand.DatabaseShowResult))]
[JsonSerializable(typeof(DatabaseListCommand.DatabaseListResult))]
[JsonSerializable(typeof(EntraAdminListCommand.EntraAdminListResult))]
[JsonSerializable(typeof(FirewallRuleListCommand.FirewallRuleListResult))]
[JsonSerializable(typeof(ElasticPoolListCommand.ElasticPoolListResult))]
[JsonSerializable(typeof(SqlDatabase))]
[JsonSerializable(typeof(SqlServerEntraAdministrator))]
[JsonSerializable(typeof(SqlServerFirewallRule))]
[JsonSerializable(typeof(SqlElasticPool))]
[JsonSerializable(typeof(DatabaseSku))]
[JsonSerializable(typeof(ElasticPoolSku))]
[JsonSerializable(typeof(ElasticPoolPerDatabaseSettings))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SqlJsonContext : JsonSerializerContext;
