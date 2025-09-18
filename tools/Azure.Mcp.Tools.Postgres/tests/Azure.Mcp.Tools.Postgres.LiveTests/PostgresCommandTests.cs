// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.LiveTests;

public class PostgresCommandTests : CommandTestsBase
{
    private const string ServersKey = "servers";
    private const string DatabasesKey = "databases";
    private const string TablesKey = "tables";
    private const string ConfigKey = "config";
    private const string QueryResultKey = "queryResult";

    private readonly PostgresService _postgresService;

    public PostgresCommandTests(ITestOutputHelper output) : base(output)
    {
        var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        var cacheService = new CacheService(memoryCache);
        var tenantService = new TenantService(cacheService);
        var subscriptionService = new SubscriptionService(cacheService, tenantService);
        var resourceGroupService = new ResourceGroupService(cacheService, subscriptionService);
        _postgresService = new PostgresService(resourceGroupService);
    }

    [Fact]
    public async Task Should_list_postgres_servers()
    {
        // act
        var result = await CallToolAsync(
            "azmcp_postgres_server_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        // assert
        var serversArray = result.AssertProperty(ServersKey);
        Assert.Equal(JsonValueKind.Array, serversArray.ValueKind);
        Assert.NotEmpty(serversArray.EnumerateArray());

        // Check that our test server is in the list
        var serverName = $"{Settings.ResourceBaseName}-postgres";
        Assert.Contains(serversArray.EnumerateArray(), server =>
            server.GetString()?.Contains(serverName) == true);
    }

    [Fact]
    public async Task Should_list_postgres_databases()
    {
        var serverName = $"{Settings.ResourceBaseName}-postgres";

        // act
        var result = await CallToolAsync(
            "azmcp_postgres_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName }
            });

        // assert
        var databasesArray = result.AssertProperty(DatabasesKey);
        Assert.Equal(JsonValueKind.Array, databasesArray.ValueKind);
        Assert.NotEmpty(databasesArray.EnumerateArray());

        // Check that our test database is in the list
        Assert.Contains(databasesArray.EnumerateArray(), db =>
            db.GetString() == "testdb");
    }

    [Fact]
    public async Task Should_get_postgres_server_config()
    {
        var serverName = $"{Settings.ResourceBaseName}-postgres";

        // act
        var result = await CallToolAsync(
            "azmcp_postgres_server_config_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName }
            });

        // assert
        var configProperty = result.AssertProperty(ConfigKey);
        Assert.Equal(JsonValueKind.Object, configProperty.ValueKind);

        // Verify some expected configuration properties exist
        Assert.True(configProperty.TryGetProperty("sku", out _));
        Assert.True(configProperty.TryGetProperty("properties", out _));
    }

    [Fact]
    public async Task Should_execute_simple_database_query()
    {
        var serverName = $"{Settings.ResourceBaseName}-postgres";
        var databaseName = "testdb";

        // act - Execute a simple query to test connectivity
        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "SELECT version();" }
            });

        // assert
        var queryResult = result.AssertProperty(QueryResultKey);
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);
        Assert.NotEmpty(queryResult.EnumerateArray());

        // Verify the result contains version information
        var firstRow = queryResult.EnumerateArray().First();
        Assert.True(firstRow.TryGetProperty("version", out var versionProperty));
        Assert.Contains("PostgreSQL", versionProperty.GetString()!);
    }

    [Fact]
    public async Task Should_list_tables_in_database()
    {
        var serverName = $"{Settings.ResourceBaseName}-postgres";
        var databaseName = "testdb";

        // First create a test table
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "CREATE TABLE IF NOT EXISTS test_table (id SERIAL PRIMARY KEY, name VARCHAR(100));" }
            });

        // act - List tables
        var result = await CallToolAsync(
            "azmcp_postgres_table_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName }
            });

        // assert
        var tablesArray = result.AssertProperty(TablesKey);
        Assert.Equal(JsonValueKind.Array, tablesArray.ValueKind);

        // Check that our test table is in the list
        Assert.Contains(tablesArray.EnumerateArray(), table =>
            table.GetString() == "test_table");
    }

    [Fact]
    public async Task Should_get_table_schema()
    {
        var serverName = $"{Settings.ResourceBaseName}-postgres";
        var databaseName = "testdb";
        var tableName = "test_table";

        // Ensure test table exists
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "CREATE TABLE IF NOT EXISTS test_table (id SERIAL PRIMARY KEY, name VARCHAR(100));" }
            });

        // act
        var result = await CallToolAsync(
            "azmcp_postgres_table_schema_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "table", tableName }
            });

        // assert
        var schemaProperty = result.AssertProperty("schema");
        Assert.Equal(JsonValueKind.Array, schemaProperty.ValueKind);
        Assert.NotEmpty(schemaProperty.EnumerateArray());

        // Verify expected columns are present
        var columns = schemaProperty.EnumerateArray().ToList();
        Assert.Contains(columns, col =>
            col.TryGetProperty("column_name", out var nameProperty) &&
            nameProperty.GetString() == "id");
        Assert.Contains(columns, col =>
            col.TryGetProperty("column_name", out var nameProperty) &&
            nameProperty.GetString() == "name");
    }

    [Fact]
    public async Task Should_insert_and_query_data()
    {
        var serverName = $"{Settings.ResourceBaseName}-postgres";
        var databaseName = "testdb";

        // Create test table
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", "CREATE TABLE IF NOT EXISTS live_test_data (id SERIAL PRIMARY KEY, test_name VARCHAR(100), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);" }
            });

        // Insert test data
        var testId = Guid.NewGuid().ToString();
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", $"INSERT INTO live_test_data (test_name) VALUES ('test_{testId}');" }
            });

        // act - Query the inserted data
        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", $"SELECT * FROM live_test_data WHERE test_name = 'test_{testId}';" }
            });

        // assert
        var queryResult = result.AssertProperty(QueryResultKey);
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);
        Assert.NotEmpty(queryResult.EnumerateArray());

        var firstRow = queryResult.EnumerateArray().First();
        Assert.True(firstRow.TryGetProperty("test_name", out var testNameProperty));
        Assert.Equal($"test_{testId}", testNameProperty.GetString());

        // Clean up
        await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceBaseName },
                { "server", serverName },
                { "database", databaseName },
                { "query", $"DELETE FROM live_test_data WHERE test_name = 'test_{testId}';" }
            });
    }
}