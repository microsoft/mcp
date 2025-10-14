// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Npgsql;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.LiveTests;

public class PostgresCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    private const string TestDatabaseName = "testdb";

    private string ServerName => Settings.DeploymentOutputs["POSTGRESSERVERNAME"];
    private string ServerFqdn => Settings.DeploymentOutputs["POSTGRESSERVERFQDN"];
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
        var tokenCredential = new DefaultAzureCredential();
        var tokenRequestContext = new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]);
        AccessToken accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

        string connectionString = $"Host={ServerFqdn};Database={TestDatabaseName};Username={AdminUsername};Password={accessToken.Token};SSL Mode=Require;Trust Server Certificate=true;";

        Output.WriteLine($"Connecting to PostgreSQL...");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        Output.WriteLine($"Connected successfully!");

        // Create employees table
        string createEmployeesTable = @"
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
        string insertEmployees = @"
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
        string createDepartmentsTable = @"
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
        string insertDepartments = @"
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
        JsonElement? result = await CallToolAsync(
            "azmcp_postgres_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "user", AdminUsername }
            });

        // Should successfully retrieve the list of databases
        JsonElement databases = result.AssertProperty("Databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);

        // Should contain at least our test databases
        List<JsonElement> databaseList = databases.EnumerateArray().ToList();
        Assert.True(databaseList.Count >= 2, $"Should contain at least {TestDatabaseName} and postgres databases");

        // Verify that our test databases exist
        List<string?> testDbNames = databaseList.Select(db => db.GetString()).ToList();
        Assert.Contains(TestDatabaseName, testDbNames);
        Assert.Contains("postgres", testDbNames);
    }

    [Fact]
    public async Task Should_ListTables_Successfully()
    {
        JsonElement? result = await CallToolAsync(
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
        JsonElement tables = result.AssertProperty("Tables");
        Assert.Equal(JsonValueKind.Array, tables.ValueKind);

        // Should contain our test tables
        List<JsonElement> tableList = tables.EnumerateArray().ToList();
        Assert.True(tableList.Count >= 2, "Should contain at least employees and departments tables");

        List<string?> tableNames = tableList.Select(t => t.GetString()).ToList();
        Assert.Contains("employees", tableNames);
        Assert.Contains("departments", tableNames);
    }

    [Fact]
    public async Task Should_GetTableSchema_Successfully()
    {
        JsonElement? result = await CallToolAsync(
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
        JsonElement schema = result.AssertProperty("Schema");
        Assert.Equal(JsonValueKind.Array, schema.ValueKind);

        // Should contain all columns from the employees table
        List<JsonElement> schemaArray = schema.EnumerateArray().ToList();
        Assert.True(schemaArray.Count >= 8, "Should contain at least 8 columns");

        // Verify that key columns exist in the schema
        string schemaJson = schema.ToString();
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
        JsonElement? result = await CallToolAsync(
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
        JsonElement queryResult = result.AssertProperty("QueryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should return at least the employees we inserted
        List<JsonElement> resultList = queryResult.EnumerateArray().ToList();
        Assert.True(resultList.Count >= 2, "Should return at least 2 Engineering employees");

        // Verify the result contains expected data
        string resultJson = queryResult.ToString();
        Assert.Contains("John", resultJson);
        Assert.Contains("Alice", resultJson);
    }

    [Fact]
    public async Task Should_ExecuteQuery_WithAggregation_Successfully()
    {
        // Test a query with COUNT aggregation
        JsonElement? result = await CallToolAsync(
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
        JsonElement queryResult = result.AssertProperty("QueryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should return aggregated results
        List<JsonElement> resultArray = queryResult.EnumerateArray().ToList();
        Assert.True(resultArray.Count >= 3, "Should return at least 3 departments");
    }

    [Fact]
    public async Task Should_ExecuteQuery_WithJoin_Successfully()
    {
        // Test a query with JOIN
        JsonElement? result = await CallToolAsync(
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
        JsonElement queryResult = result.AssertProperty("QueryResult");
        Assert.Equal(JsonValueKind.Array, queryResult.ValueKind);

        // Should return employees in Seattle
        List<JsonElement> resultArray = queryResult.EnumerateArray().ToList();
        Assert.True(resultArray.Count >= 1, "Should return at least 1 employee in Seattle");
    }

    [Fact]
    public async Task Should_ListServerConfigs_Successfully()
    {
        JsonElement? result = await CallToolAsync(
            "azmcp_postgres_server_config_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "user", AdminUsername },
            });

        // Should successfully retrieve server configurations
        JsonElement config = result.AssertProperty("Configuration");
        Assert.Equal(JsonValueKind.String, config.ValueKind);

        string[] configLines = config
            .GetString()?
            .Trim()
            .Split("\n") ?? Array.Empty<string>();

        // Should contain multiple configuration parameters
        Assert.True(configLines.Length > 0, "Should return at least one configuration parameter");

        List<KeyValuePair<string, string>> configList = configLines
            .Select(line =>
            {
                string[] parts = line.Split(":", 2);
                return new KeyValuePair<string, string>(
                    parts[0].Trim(),
                    parts.Length > 1 ? parts[1].Trim() : string.Empty);
            })
            .ToList();

        // Should contain at least the "Server Name" parameter
        KeyValuePair<string, string> serverConfig = configList.FirstOrDefault(c => c.Key == "Server Name");
        Assert.Equal("Server Name", serverConfig.Key);
        Assert.Equal(ServerName, serverConfig.Value);
    }

    [Fact]
    public async Task Should_GetServerParameter_Successfully()
    {
        // Get a specific server parameter (max_connections is a common one)
        JsonElement? result = await CallToolAsync(
            "azmcp_postgres_server_param_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", ServerName },
                { "user", AdminUsername },
                { "param", "max_connections" }
            });

        // Should successfully retrieve the parameter
        JsonElement parameter = result.AssertProperty("ParameterValue");
        Assert.Equal(JsonValueKind.String, parameter.ValueKind);

        int maxConnections = int.Parse(parameter.GetString() ?? "0");
        Assert.True(maxConnections > 0, "max_connections should be greater than 0");
    }

    [Fact]
    public async Task Should_ListServers_Successfully()
    {
        JsonElement? result = await CallToolAsync(
            "azmcp_postgres_server_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "user", AdminUsername },
            });

        // Should successfully retrieve the list of servers
        List<string?> servers = result.AssertProperty("Servers")
            .EnumerateArray()
            .Select(s => s.GetString())
            .ToList();
        Assert.True(servers.Count >= 1, $"Should contain at least the {ServerName} server");

        // Verify that our test server exists
        Assert.Contains(ServerName, servers);
    }

    [Fact]
    public async Task Should_RejectNonSelectQuery_WithValidationError()
    {
        JsonElement? result = await CallToolAsync(
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

        // no exception is thrown and response is not propagated here to check the following:
        // "status": 400,
        // "message": "Only single read-only SELECT statements are allowed.",
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_HandleInvalidServerName_Gracefully()
    {
        JsonElement? result = await CallToolAsync(
                "azmcp_postgres_database_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", Guid.NewGuid().ToString() }, // <-- nonexistent_server
                    { "database", TestDatabaseName },
                    { "user", AdminUsername }
                });

        // Should handle the error gracefully
        JsonElement errorMessageProperty = result.Value.GetProperty("message");
        Assert.Equal(JsonValueKind.String, errorMessageProperty.ValueKind);
        Assert.Equal("No such host is known.", errorMessageProperty.GetString());
    }

    [Fact]
    public async Task Should_HandleInvalidDatabaseName_Gracefully()
    {
        string nonexistentDatabase = Guid.NewGuid().ToString();

        JsonElement? result = await CallToolAsync(
                "azmcp_postgres_table_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", ServerName },
                    { "database", nonexistentDatabase },
                    { "user", AdminUsername }
                });

        // Should handle the error gracefully
        JsonElement errorMessageProperty = result.Value.GetProperty("message");
        Assert.Equal(JsonValueKind.String, errorMessageProperty.ValueKind);
        Assert.Contains($"\"{nonexistentDatabase}\" does not exist", errorMessageProperty.GetString());
    }

    [Fact]
    public async Task Should_HandleInvalidTableName_Gracefully()
    {
        JsonElement? result = await CallToolAsync(
            "azmcp_postgres_table_schema_get",
            new()
            {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", ServerName },
                    { "database", TestDatabaseName },
                    { "user", AdminUsername },
                    { "table", Guid.NewGuid().ToString() } // <-- nonexistent_table
            });

        JsonElement schema = result.AssertProperty("Schema");
        Assert.Equal(JsonValueKind.Array, schema.ValueKind);

        // Schema should be empty for nonexistent table
        List<JsonElement> schemaList = schema.EnumerateArray().ToList();
        Assert.Empty(schemaList);
    }
}
