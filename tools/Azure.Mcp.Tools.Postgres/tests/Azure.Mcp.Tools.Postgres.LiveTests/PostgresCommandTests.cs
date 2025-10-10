// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Npgsql;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.LiveTests;

public class PostgresCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    private string ServerName => Settings.ResourceBaseName;
    private string ServerFqdn => Settings.DeploymentOutputs.GetValueOrDefault("postgresServerFqdn")
        ?? $"{ServerName}.postgres.database.azure.com";
    private const string TestDatabaseName = "testdb";
    private const string TestDatabase2Name = "testdb2";
    private string AdminUsername => Settings.PrincipalName ?? string.Empty;
    private static bool _testDataInitialized = false;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        // Only initialize test data once for all tests
        if (_testDataInitialized)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_testDataInitialized)
            {
                return;
            }

            Output.WriteLine("Initializing test data...");
            await CreateTestDataAsync();
            _testDataInitialized = true;
            Output.WriteLine("Test data initialized successfully");
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task CreateTestDataAsync()
    {
        Output.WriteLine($"ServerFqdn: {ServerFqdn}");
        Output.WriteLine($"AdminUsername: {AdminUsername}");
        Output.WriteLine($"TestDatabaseName: {TestDatabaseName}");

        // Get Entra ID access token for PostgreSQL
        var tokenCredential = new Azure.Identity.DefaultAzureCredential();
        var tokenRequestContext = new Azure.Core.TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]);
        var accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

        var connectionString = $"Host={ServerFqdn};Database={TestDatabaseName};Username={AdminUsername};Password={accessToken.Token};SSL Mode=Require;Trust Server Certificate=true;";

        Output.WriteLine($"Connecting to PostgreSQL...");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        Output.WriteLine($"Connected successfully!");

        // Create employees table
        var createEmployeesTable = @"
CREATE TABLE IF NOT EXISTS employees (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    department VARCHAR(50),
    salary DECIMAL(10, 2),
    hire_date DATE DEFAULT CURRENT_DATE,
    is_active BOOLEAN DEFAULT true
);";

        await using (var cmd = new NpgsqlCommand(createEmployeesTable, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Insert employee data
        var insertEmployees = @"
INSERT INTO employees (first_name, last_name, email, department, salary, hire_date, is_active)
VALUES 
    ('John', 'Doe', 'john.doe@example.com', 'Engineering', 75000.00, '2023-01-15', true),
    ('Jane', 'Smith', 'jane.smith@example.com', 'Marketing', 65000.00, '2023-02-20', true),
    ('Bob', 'Johnson', 'bob.johnson@example.com', 'Sales', 70000.00, '2023-03-10', true),
    ('Alice', 'Williams', 'alice.williams@example.com', 'Engineering', 80000.00, '2023-04-05', true),
    ('Charlie', 'Brown', 'charlie.brown@example.com', 'HR', 60000.00, '2023-05-12', false)
ON CONFLICT (email) DO NOTHING;";

        await using (var cmd = new NpgsqlCommand(insertEmployees, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Create departments table
        var createDepartmentsTable = @"
CREATE TABLE IF NOT EXISTS departments (
    dept_id SERIAL PRIMARY KEY,
    dept_name VARCHAR(50) NOT NULL UNIQUE,
    location VARCHAR(100),
    budget DECIMAL(12, 2)
);";

        await using (var cmd = new NpgsqlCommand(createDepartmentsTable, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // Insert department data
        var insertDepartments = @"
INSERT INTO departments (dept_name, location, budget)
VALUES 
    ('Engineering', 'Seattle', 1000000.00),
    ('Marketing', 'New York', 500000.00),
    ('Sales', 'San Francisco', 750000.00),
    ('HR', 'Austin', 300000.00)
ON CONFLICT (dept_name) DO NOTHING;";

        await using (var cmd = new NpgsqlCommand(insertDepartments, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        Output.WriteLine("Test tables and data created successfully");
    }

    [Fact]
    public async Task Should_ListDatabases_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "user", AdminUsername }
            });

        // Should successfully retrieve the list of databases
        var databases = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);

        // Should contain at least our test databases
        var databaseArray = databases.EnumerateArray().ToList();
        Assert.True(databaseArray.Count >= 2, "Should contain at least testdb and testdb2 databases");

        // Verify that our test databases exist
        var testDbNames = databaseArray.Select(db => db.GetString()).ToList();
        Assert.Contains("testdb", testDbNames);
        Assert.Contains("testdb2", testDbNames);

        // PostgreSQL system databases should also be present
        Assert.Contains("postgres", testDbNames);
    }

    [Fact]
    public async Task Should_ListTables_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_table_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "database", TestDatabaseName },
                { "user", AdminUsername }
            });

        // Should successfully retrieve the list of tables
        var tables = result.AssertProperty("tables");
        Assert.Equal(JsonValueKind.Array, tables.ValueKind);

        // Should contain our test tables
        var tableArray = tables.EnumerateArray().ToList();
        Assert.True(tableArray.Count >= 2, "Should contain at least employees and departments tables");

        var tableNames = tableArray.Select(t => t.GetString()).ToList();
        Assert.Contains("employees", tableNames);
        Assert.Contains("departments", tableNames);
    }

    [Fact]
    public async Task Should_GetTableSchema_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_table_schema_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "database", TestDatabaseName },
                { "user", AdminUsername },
                { "table", "employees" }
            });

        // Should successfully retrieve the table schema
        var schema = result.AssertProperty("schema");
        Assert.Equal(JsonValueKind.Array, schema.ValueKind);

        // Should contain all columns from the employees table
        var schemaArray = schema.EnumerateArray().ToList();
        Assert.True(schemaArray.Count >= 8, "Should contain at least 8 columns");

        // Verify that key columns exist in the schema
        var schemaJson = schema.ToString();
        Assert.Contains("id", schemaJson);
        Assert.Contains("first_name", schemaJson);
        Assert.Contains("last_name", schemaJson);
        Assert.Contains("email", schemaJson);
        Assert.Contains("department", schemaJson);
        Assert.Contains("salary", schemaJson);
        Assert.Contains("hire_date", schemaJson);
        Assert.Contains("is_active", schemaJson);
    }

    [Fact]
    public async Task Should_ExecuteQuery_Successfully()
    {
        // Test a simple SELECT query
        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "database", TestDatabaseName },
                { "user", AdminUsername },
                { "query", "SELECT first_name, last_name FROM employees WHERE department = 'Engineering';" }
            });

        // Should successfully execute the query
        var queryResult = result.AssertProperty("queryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should return at least the employees we inserted
        var resultArray = queryResult.EnumerateArray().ToList();
        Assert.True(resultArray.Count >= 2, "Should return at least 2 Engineering employees");

        // Verify the result contains expected data
        var resultJson = queryResult.ToString();
        Assert.Contains("John", resultJson);
        Assert.Contains("Alice", resultJson);
    }

    [Fact]
    public async Task Should_ExecuteQuery_WithAggregation_Successfully()
    {
        // Test a query with COUNT aggregation
        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "database", TestDatabaseName },
                { "user", AdminUsername },
                { "query", "SELECT department, COUNT(*) as emp_count FROM employees GROUP BY department ORDER BY department;" }
            });

        // Should successfully execute the query
        var queryResult = result.AssertProperty("queryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should return aggregated results
        var resultArray = queryResult.EnumerateArray().ToList();
        Assert.True(resultArray.Count >= 3, "Should return at least 3 departments");
    }

    [Fact]
    public async Task Should_ExecuteQuery_WithJoin_Successfully()
    {
        // Test a query with JOIN
        var result = await CallToolAsync(
            "azmcp_postgres_database_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "database", TestDatabaseName },
                { "user", AdminUsername },
                { "query", @"SELECT e.first_name, e.last_name, d.location 
                             FROM employees e 
                             JOIN departments d ON e.department = d.dept_name 
                             WHERE d.location = 'Seattle';" }
            });

        // Should successfully execute the join query
        var queryResult = result.AssertProperty("queryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should return employees in Seattle
        var resultArray = queryResult.EnumerateArray().ToList();
        Assert.True(resultArray.Count >= 1, "Should return at least 1 employee in Seattle");
    }

    [Fact]
    public async Task Should_ListServerConfigs_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_server_config_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName }
            });

        // Should successfully retrieve server configurations
        var configs = result.AssertProperty("configurations");
        Assert.Equal(JsonValueKind.Array, configs.ValueKind);

        // Should contain multiple configuration parameters
        var configArray = configs.EnumerateArray().ToList();
        Assert.True(configArray.Count > 0, "Should return at least one configuration parameter");
    }

    [Fact]
    public async Task Should_GetServerParameter_Successfully()
    {
        // Get a specific server parameter (max_connections is a common one)
        var result = await CallToolAsync(
            "azmcp_postgres_server_param_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "parameter", "max_connections" }
            });

        // Should successfully retrieve the parameter
        var parameter = result.AssertProperty("parameter");
        Assert.Equal(JsonValueKind.Object, parameter.ValueKind);

        // Verify parameter properties
        var paramName = parameter.GetProperty("name").GetString();
        Assert.Equal("max_connections", paramName);

        var paramValue = parameter.GetProperty("value");
        Assert.NotEqual(JsonValueKind.Undefined, paramValue.ValueKind);
    }

    [Fact]
    public async Task Should_ListServers_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_postgres_server_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        // Should successfully retrieve the list of servers
        var servers = result.AssertProperty("servers");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);

        // Should contain at least our test server
        var serverArray = servers.EnumerateArray().ToList();
        Assert.True(serverArray.Count >= 1, "Should contain at least the test PostgreSQL server");

        // Verify our test server is in the list
        var testServer = serverArray.FirstOrDefault(s =>
            s.GetProperty("name").GetString()?.Contains(ServerName) == true);
        Assert.NotEqual(default, testServer);

        // Verify server properties
        if (testServer.ValueKind != JsonValueKind.Undefined)
        {
            var serverType = testServer.GetProperty("type").GetString();
            Assert.Contains("Microsoft.DBforPostgreSQL/flexibleServers", serverType, StringComparison.OrdinalIgnoreCase);

            var serverState = testServer.GetProperty("state").GetString();
            Assert.Equal("Ready", serverState, ignoreCase: true);
        }
    }

    [Fact]
    public async Task Should_RejectNonSelectQuery_WithValidationError()
    {
        // Test that non-SELECT queries are rejected
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await CallToolAsync(
                "azmcp_postgres_database_query",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", ServerName },
                    { "database", TestDatabaseName },
                    { "user", AdminUsername },
                    { "query", "DELETE FROM employees WHERE id = 1;" }
                });
        });

        // Should reject with validation error
        Assert.Contains("validation", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_HandleInvalidServerName_Gracefully()
    {
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await CallToolAsync(
                "azmcp_postgres_database_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", "nonexistent-server-12345" },
                    { "user", AdminUsername }
                });
        });

        // Should handle the error gracefully
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task Should_HandleInvalidDatabaseName_Gracefully()
    {
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await CallToolAsync(
                "azmcp_postgres_table_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", ServerName },
                    { "database", "nonexistent_database" },
                    { "user", AdminUsername }
                });
        });

        // Should handle the error gracefully
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task Should_HandleInvalidTableName_Gracefully()
    {
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await CallToolAsync(
                "azmcp_postgres_table_schema_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", ServerName },
                    { "database", TestDatabaseName },
                    { "user", AdminUsername },
                    { "table", "nonexistent_table" }
                });
        });

        // Should handle the error gracefully
        Assert.NotNull(exception);
    }
}
