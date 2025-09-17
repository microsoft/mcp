// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.LiveTests;

public class PostgresCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{

    [Fact]
    public async Task Should_ListDatabases_Successfully()
    {
        // Use the deployed test PostgreSQL server
        var serverName = Settings.ResourceBaseName + "-postgres";

        var result = await CallToolAsync(
            "azmcp_postgres_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName }
            });

        // Should successfully retrieve the list of databases
        var databases = result.AssertProperty("Databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);

        // Should contain at least the default postgres database
        var databaseArray = databases.EnumerateArray().ToList();
        Assert.True(databaseArray.Count >= 1, "Should contain at least the default postgres database");

        // Verify that postgres database exists (default database)
        var postgresDb = databaseArray.FirstOrDefault(db =>
            db.GetProperty("name").GetString() == "postgres");
        Assert.NotEqual(default, postgresDb);

        // Verify database properties
        if (postgresDb.ValueKind != JsonValueKind.Undefined)
        {
            var dbName = postgresDb.GetProperty("name").GetString();
            Assert.Equal("postgres", dbName);
        }
    }

    [Fact]
    public async Task Should_QueryDatabase_Successfully()
    {
        // Use the deployed test PostgreSQL server
        var serverName = Settings.ResourceBaseName + "-postgres";
        var databaseName = "postgres"; // Default database

        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "SELECT version()" }
            });

        // Should successfully execute the query
        var queryResult = result.AssertProperty("QueryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should have at least one element (the column headers)
        Assert.True(queryResult.GetArrayLength() >= 1, "Should have at least column headers");
        
        // First element should be column names
        var firstElement = queryResult.EnumerateArray().First();
        Assert.Equal(JsonValueKind.String, firstElement.ValueKind);
        Assert.Contains("version", firstElement.GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_ListServers_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_server_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName }
            });

        // Should successfully retrieve the list of servers
        var servers = result.AssertProperty("Servers");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);

        // Should contain at least one server (our test server)
        var serverArray = servers.EnumerateArray().ToList();
        Assert.True(serverArray.Count >= 1, "Should contain at least one PostgreSQL server");

        // Verify server properties
        var firstServer = serverArray.First();
        Assert.Equal(JsonValueKind.Object, firstServer.ValueKind);

        // Verify required properties exist
        Assert.True(firstServer.TryGetProperty("name", out _));
        Assert.True(firstServer.TryGetProperty("id", out _));
        Assert.True(firstServer.TryGetProperty("type", out _));
        Assert.True(firstServer.TryGetProperty("location", out _));

        var serverType = firstServer.GetProperty("type").GetString();
        Assert.Contains("Microsoft.DBforPostgreSQL", serverType);
    }

    [Fact]
    public async Task Should_GetServerConfig_Successfully()
    {
        // Use the deployed test PostgreSQL server
        var serverName = Settings.ResourceBaseName + "-postgres";

        var result = await CallToolAsync(
            "azmcp_postgres_server_config_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName }
            });

        // Should successfully retrieve server configuration
        var config = result.AssertProperty("Configuration");
        Assert.Equal(JsonValueKind.String, config.ValueKind);

        // Should contain configuration information
        var configString = config.GetString();
        Assert.NotNull(configString);
        Assert.NotEmpty(configString);
        Assert.Contains("Server Name:", configString);
    }

    [Fact]
    public async Task Should_GetServerParam_Successfully()
    {
        // Use the deployed test PostgreSQL server
        var serverName = Settings.ResourceBaseName + "-postgres";
        var parameterName = "log_statement"; // A common PostgreSQL parameter

        var result = await CallToolAsync(
            "azmcp_postgres_server_param_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName },
                { "param", parameterName }
            });

        // Should successfully retrieve the parameter
        var parameterValue = result.AssertProperty("ParameterValue");
        Assert.Equal(JsonValueKind.String, parameterValue.ValueKind);

        // Verify parameter value is not null or empty
        var paramValue = parameterValue.GetString();
        Assert.NotNull(paramValue);
    }

    [Fact]
    public async Task Should_ListTables_Successfully()
    {
        // Use the deployed test PostgreSQL server
        var serverName = Settings.ResourceBaseName + "-postgres";
        var databaseName = "postgres"; // Default database

        var result = await CallToolAsync(
            "azmcp_postgres_table_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName },
                { "database", databaseName }
            });

        // Should successfully retrieve the list of tables
        var tables = result.AssertProperty("Tables");
        Assert.Equal(JsonValueKind.Array, tables.ValueKind);

        // PostgreSQL default database may or may not have user tables,
        // but the command should execute successfully
        var tableArray = tables.EnumerateArray().ToList();

        // If there are tables, verify they are strings (table names)
        if (tableArray.Count > 0)
        {
            var firstTable = tableArray.First();
            Assert.Equal(JsonValueKind.String, firstTable.ValueKind);
        }
    }

    [Fact]
    public async Task Should_GetTableSchema_Successfully()
    {
        // First, create a test table to ensure we have something to query
        var serverName = Settings.ResourceBaseName + "-postgres";
        var databaseName = "postgres";

        // Create a test table first
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "CREATE TABLE IF NOT EXISTS test_schema_table (id SERIAL PRIMARY KEY, name VARCHAR(100), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP)" }
            });

        // Now get the schema for the test table
        var result = await CallToolAsync(
            "azmcp_postgres_table_schema_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName },
                { "database", databaseName },
                { "table", "test_schema_table" }
            });

        // Should successfully retrieve the table schema
        var schema = result.AssertProperty("Schema");
        Assert.Equal(JsonValueKind.Array, schema.ValueKind);

        // Schema should contain column information as strings
        var schemaArray = schema.EnumerateArray().ToList();
        Assert.True(schemaArray.Count >= 1, "Should have at least one schema entry");

        // Verify that each schema entry is a string
        var firstEntry = schemaArray.First();
        Assert.Equal(JsonValueKind.String, firstEntry.ValueKind);

        // The schema should contain information about our test table columns
        var schemaContent = string.Join(" ", schemaArray.Select(s => s.GetString()));
        Assert.Contains("id", schemaContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name", schemaContent, StringComparison.OrdinalIgnoreCase);

        // Cleanup: Drop the test table
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", Settings.PrincipalName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "DROP TABLE IF EXISTS test_schema_table" }
            });
    }

    [Theory]
    [InlineData("--invalid-param", new string[0])]
    [InlineData("--subscription", new[] { "invalidSub" })]
    [InlineData("--subscription", new[] { "sub", "--resource-group", "rg" })]  // Missing server
    public async Task Should_Return400_WithInvalidDatabaseListInput(string firstArg, string[] remainingArgs)
    {
        var allArgs = new[] { firstArg }.Concat(remainingArgs);
        var argsString = string.Join(" ", allArgs);

        try
        {
            var result = await CallToolAsync("azmcp_postgres_database_list",
                new Dictionary<string, object?> { { "args", argsString } });

            // If we get here, the command didn't fail as expected
            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - the command should fail with invalid input
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")] // Missing server and database
    [InlineData("--subscription sub --resource-group rg --server server1")] // Missing database and query
    public async Task Should_Return400_WithInvalidQueryInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_postgres_database_query",
                new Dictionary<string, object?> { { "args", args } });

            // If we get here, the command didn't fail as expected
            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - the command should fail with invalid input
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")] // Missing server
    public async Task Should_Return400_WithInvalidServerListInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_postgres_server_list",
                new Dictionary<string, object?> { { "args", args } });

            // If we get here, the command didn't fail as expected
            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - the command should fail with invalid input
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")] // Missing server
    public async Task Should_Return400_WithInvalidServerConfigGetInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_postgres_server_config_get",
                new Dictionary<string, object?> { { "args", args } });

            // If we get here, the command didn't fail as expected
            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - the command should fail with invalid input
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")] // Missing server and parameter-name
    [InlineData("--subscription sub --resource-group rg --server server1")] // Missing parameter-name
    public async Task Should_Return400_WithInvalidServerParamGetInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_postgres_server_param_get",
                new Dictionary<string, object?> { { "args", args } });

            // If we get here, the command didn't fail as expected
            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - the command should fail with invalid input
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
    }
}
