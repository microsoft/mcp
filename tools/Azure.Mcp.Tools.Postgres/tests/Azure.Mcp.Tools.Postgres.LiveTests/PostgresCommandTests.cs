// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.LiveTests;

public class PostgresCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    private string? _serverName;
    private string? _databaseName;
    private string? _tableName;
    private string? _userPrincipal;

    private string ServerName => _serverName ??= ResolveDeploymentValue("POSTGRES_SERVER_NAME", Settings.ResourceBaseName);
    private string DatabaseName => _databaseName ??= ResolveDeploymentValue("POSTGRES_DATABASE_NAME", "sampledb");
    private string TableName => _tableName ??= ResolveDeploymentValue("POSTGRES_TABLE_NAME", "inventory");
    private string UserPrincipal => _userPrincipal ??= ResolveDeploymentValue("POSTGRES_AAD_PRINCIPAL", Settings.PrincipalName);

    private string ResolveDeploymentValue(string key, string fallback)
    {
        if (Settings.DeploymentOutputs.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    [Fact]
    public async Task Should_list_postgres_servers()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_server_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal }
            });

        var servers = result.AssertProperty("servers");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);
        Assert.Contains(
            servers.EnumerateArray(),
            element => string.Equals(element.GetString(), ServerName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_get_postgres_server_configuration()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_server_config_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal },
                { "server", ServerName }
            });

        var configuration = result.AssertProperty("configuration");
        Assert.Equal(JsonValueKind.String, configuration.ValueKind);
        var configText = configuration.GetString();
        Assert.False(string.IsNullOrWhiteSpace(configText));
        Assert.Contains(ServerName, configText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_postgres_server_parameter()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_server_param_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal },
                { "server", ServerName },
                { "param", "log_checkpoints" }
            });

        var parameterValue = result.AssertProperty("parameterValue");
        Assert.Equal(JsonValueKind.String, parameterValue.ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(parameterValue.GetString()));
    }

    [Fact]
    public async Task Should_list_postgres_databases()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal },
                { "server", ServerName }
            });

        var databases = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);
        Assert.Contains(
            databases.EnumerateArray(),
            element => string.Equals(element.GetString(), DatabaseName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_list_postgres_tables()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_table_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal },
                { "server", ServerName },
                { "database", DatabaseName }
            });

        var tables = result.AssertProperty("tables");
        Assert.Equal(JsonValueKind.Array, tables.ValueKind);
        Assert.Contains(
            tables.EnumerateArray(),
            element => string.Equals(element.GetString(), TableName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_get_postgres_table_schema()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_table_schema_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal },
                { "server", ServerName },
                { "database", DatabaseName },
                { "table", TableName }
            });

        var schema = result.AssertProperty("schema");
        Assert.Equal(JsonValueKind.Array, schema.ValueKind);
        var schemaEntries = schema.EnumerateArray().Select(element => element.GetString() ?? string.Empty).ToList();
        Assert.Contains(schemaEntries, entry => entry.Contains("name", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(schemaEntries, entry => entry.Contains("quantity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_query_postgres_table()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", UserPrincipal },
                { "server", ServerName },
                { "database", DatabaseName },
                { "query", $"SELECT name, quantity FROM public.{TableName} ORDER BY name;" }
            });

        var queryResult = result.AssertProperty("queryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);
        using var enumerator = queryResult.EnumerateArray();
        Assert.True(enumerator.MoveNext(), "Query results should include a header row.");
        var header = enumerator.Current.GetString();
        Assert.False(string.IsNullOrWhiteSpace(header));
        Assert.Contains("name", header!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("quantity", header!, StringComparison.OrdinalIgnoreCase);
        Assert.True(enumerator.MoveNext(), "Query results should contain at least one data row.");
    }
}
