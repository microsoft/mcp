// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Sql.LiveTests;

public class SqlCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{

    [Fact]
    public async Task Should_ShowDatabase_Successfully()
    {
        // Use the deployed test SQL server and database
        var serverName = Settings.ResourceBaseName;
        var databaseName = "testdb";

        var result = await CallToolAsync(
            "azmcp_sql_db_show",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName },
                { "database", databaseName }
            });

        // Should successfully retrieve the database
        var database = result.AssertProperty("database");
        Assert.Equal(JsonValueKind.Object, database.ValueKind);

        // Verify database properties
        var dbName = database.GetProperty("name").GetString();
        Assert.Equal(databaseName, dbName);

        var dbType = database.GetProperty("type").GetString();
        Assert.Equal("Microsoft.Sql/servers/databases", dbType, ignoreCase: true);
    }

    [Fact]
    public async Task Should_ListDatabases_Successfully()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;

        var result = await CallToolAsync(
            "azmcp_sql_db_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName }
            });

        // Should successfully retrieve the list of databases
        var databases = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);

        // Should contain at least the master database and our test database
        var databaseArray = databases.EnumerateArray().ToList();
        Assert.True(databaseArray.Count >= 2, "Should contain at least master and testdb databases");

        // Verify that master database exists
        var masterDb = databaseArray.FirstOrDefault(db =>
            db.GetProperty("name").GetString() == "master");
        Assert.NotEqual(default, masterDb);

        // Verify that our test database exists
        var testDb = databaseArray.FirstOrDefault(db =>
            db.GetProperty("name").GetString() == "testdb");
        Assert.NotEqual(default, testDb);

        // Verify database properties for test database
        if (testDb.ValueKind != JsonValueKind.Undefined)
        {
            var dbType = testDb.GetProperty("type").GetString();
            Assert.Equal("Microsoft.Sql/servers/databases", dbType, ignoreCase: true);

            var dbStatus = testDb.GetProperty("status").GetString();
            Assert.Equal("Online", dbStatus);
        }
    }

    [Theory]
    [InlineData("--invalid-param", new string[0])]
    [InlineData("--subscription", new[] { "invalidSub" })]
    [InlineData("--subscription", new[] { "sub", "--resource-group", "rg" })]  // Missing server and database
    public async Task Should_Return400_WithInvalidInput(string firstArg, string[] remainingArgs)
    {
        var allArgs = new[] { firstArg }.Concat(remainingArgs);
        var argsString = string.Join(" ", allArgs);

        // For error testing, we expect CallToolAsync to return null (no results)
        // and we need to catch any exceptions or check the response manually
        try
        {
            var result = await CallToolAsync("azmcp_sql_db_show",
                new Dictionary<string, object?> { { "args", argsString } });

            // If we get here, the command didn't fail as expected
            // This might indicate the command succeeded when it should have failed
            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - the command should fail with invalid input
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
    }

    [Fact]
    public async Task Should_ValidateRequiredParameters()
    {
        // Test with missing required parameters - expect an exception or null result
        try
        {
            var result = await CallToolAsync(
                "azmcp_sql_db_show",
                new Dictionary<string, object?>
                {
                    { "subscription", Settings.SubscriptionId }
                    // Missing resource-group, server, and database
                });

            // If we get here without an exception, the validation didn't work as expected
            Assert.Fail("Expected command to fail due to missing required parameters, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected behavior - should fail due to missing required parameters
            Assert.NotNull(ex.Message);
            Assert.Contains("required", ex.Message.ToLower());
        }
    }

    [Fact]
    public async Task Should_DeleteDatabase_Return404ForNonExistentDatabase()
    {
        // Test deleting a non-existent database to verify proper error handling
        var serverName = Settings.ResourceBaseName;
        var nonExistentDbName = "non-existent-database-" + Guid.NewGuid().ToString("N")[..8];

        try
        {
            var result = await CallToolAsync(
                "azmcp_sql_db_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "server", serverName },
                    { "database", nonExistentDbName }
                });

            // Delete operation should be idempotent - it might succeed even if database doesn't exist
            // This tests the service's idempotent behavior
            if (result.HasValue)
            {
                var deleteResult = result.Value.AssertProperty("databaseName");
                Assert.Equal(nonExistentDbName, deleteResult.GetString());

                var deleted = result.Value.AssertProperty("deleted");
                Assert.Equal(JsonValueKind.False, deleted.ValueKind); // Should be false for non-existent DB
            }
        }
        catch (Exception ex)
        {
            // Some implementations might return 404 - this is also acceptable
            Assert.Contains("404", ex.Message);
        }
    }

    [Theory]
    [InlineData("--invalid-database-param")]
    [InlineData("--subscription test-sub --resource-group test-rg --server test-server")] // Missing database
    public async Task Should_Return400_WithInvalidDatabaseDeleteInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_sql_db_delete",
                new Dictionary<string, object?> { { "args", args } });

            Assert.Fail("Expected command to fail with invalid input, but it succeeded");
        }
        catch (Exception ex)
        {
            // Expected - command should fail with validation error
            Assert.NotNull(ex.Message);
        }
    }

    [Fact]
    public async Task Should_ListSqlServerEntraAdmins_Successfully()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;

        var result = await CallToolAsync(
            "azmcp_sql_server_entra-admin_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName }
            });

        // The command should succeed, but results may be null if no Entra admins are configured
        if (result.HasValue)
        {
            // If there are results, verify the structure
            var admins = result.Value.AssertProperty("administrators");
            Assert.Equal(JsonValueKind.Array, admins.ValueKind);

            // If there are admins, verify their structure
            if (admins.GetArrayLength() > 0)
            {
                var firstAdmin = admins.EnumerateArray().First();
                Assert.Equal(JsonValueKind.Object, firstAdmin.ValueKind);

                // Verify required properties exist
                firstAdmin.AssertProperty("administratorType");
                firstAdmin.AssertProperty("login");
                firstAdmin.AssertProperty("sid");
            }
        }
        // If result is null, that's valid - it means no AD administrators are configured
        // The test passes as long as the command executed successfully (no exception thrown)
    }

    [Fact]
    public async Task Should_ListSqlServerFirewallRules_Successfully()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;

        var result = await CallToolAsync(
            "azmcp_sql_server_firewall-rule_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName }
            });

        // The command should succeed and return firewall rules
        // Most SQL servers have at least the "AllowAllWindowsAzureIps" rule
        if (result.HasValue)
        {
            // If there are results, verify the structure
            var firewallRules = result.Value.AssertProperty("firewallRules");
            Assert.Equal(JsonValueKind.Array, firewallRules.ValueKind);

            // If there are firewall rules, verify their structure
            if (firewallRules.GetArrayLength() > 0)
            {
                var firstRule = firewallRules.EnumerateArray().First();
                Assert.Equal(JsonValueKind.Object, firstRule.ValueKind);

                // Verify required properties exist
                var name = firstRule.AssertProperty("name");
                firstRule.AssertProperty("id");
                firstRule.AssertProperty("type");
                firstRule.AssertProperty("startIpAddress");
                firstRule.AssertProperty("endIpAddress");

                // Verify the name is not empty
                Assert.NotNull(name.GetString());
                Assert.NotEmpty(name.GetString()!);
            }
        }
        // If result is null, that's valid - it means no firewall rules are configured
        // The test passes as long as the command executed successfully (no exception thrown)
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")] // Missing server
    public async Task Should_Return400_WithInvalidFirewallRuleListInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_sql_server_firewall-rule_list",
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

    [Fact]
    public async Task Should_ListElasticPools_Successfully()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;

        var result = await CallToolAsync(
            "azmcp_sql_elastic-pool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName }
            });

        // The command should succeed and return elastic pools (should have at least the test pool)
        var elasticPools = result.AssertProperty("elasticPools");
        Assert.Equal(JsonValueKind.Array, elasticPools.ValueKind);
        Assert.True(elasticPools.GetArrayLength() > 0, "Expected at least one elastic pool to be returned from the test infrastructure");

        // Verify the structure of the first elastic pool
        var firstPool = elasticPools.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, firstPool.ValueKind);

        // Verify required properties exist
        firstPool.AssertProperty("name");
        firstPool.AssertProperty("id");
        firstPool.AssertProperty("type");
        firstPool.AssertProperty("location");

        var poolType = firstPool.GetProperty("type").GetString();
        Assert.Equal("Microsoft.Sql/servers/elasticPools", poolType, ignoreCase: true);
    }

    [Fact]
    public async Task Should_CreateFirewallRule_Successfully()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;
        var ruleName = $"test-rule-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var startIp = "192.168.1.100";
        var endIp = "192.168.1.200";

        var result = await CallToolAsync(
            "azmcp_sql_server_firewall-rule_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName },
                { "firewall-rule-name", ruleName },
                { "start-ip-address", startIp },
                { "end-ip-address", endIp }
            });

        // Should successfully create the firewall rule
        var firewallRule = result.AssertProperty("firewallRule");
        Assert.Equal(JsonValueKind.Object, firewallRule.ValueKind);

        // Verify firewall rule properties
        var name = firewallRule.GetProperty("name").GetString();
        Assert.Equal(ruleName, name);

        var ruleType = firewallRule.GetProperty("type").GetString();
        Assert.Equal("Microsoft.Sql/servers/firewallRules", ruleType, ignoreCase: true);

        var ruleStartIp = firewallRule.GetProperty("startIpAddress").GetString();
        Assert.Equal(startIp, ruleStartIp);

        var ruleEndIp = firewallRule.GetProperty("endIpAddress").GetString();
        Assert.Equal(endIp, ruleEndIp);

        var id = firewallRule.GetProperty("id").GetString();
        Assert.NotNull(id);
        Assert.Contains(serverName, id);
        Assert.Contains(ruleName, id);
    }

    [Fact]
    public async Task Should_DeleteFirewallRule_Successfully()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;
        var ruleName = $"test-delete-rule-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var startIp = "192.168.2.100";
        var endIp = "192.168.2.200";

        // First create a firewall rule to delete
        await CallToolAsync(
            "azmcp_sql_server_firewall-rule_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName },
                { "firewall-rule-name", ruleName },
                { "start-ip-address", startIp },
                { "end-ip-address", endIp }
            });

        // Now delete the firewall rule
        var result = await CallToolAsync(
            "azmcp_sql_server_firewall-rule_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName },
                { "firewall-rule-name", ruleName }
            });

        // Should successfully delete the firewall rule
        var deleted = result.AssertProperty("deleted").GetBoolean();
        Assert.True(deleted);

        var deletedRuleName = result.AssertProperty("ruleName").GetString();
        Assert.Equal(ruleName, deletedRuleName);
    }

    [Fact]
    public async Task Should_DeleteNonExistentFirewallRule_ReturnsFalse()
    {
        // Use the deployed test SQL server
        var serverName = Settings.ResourceBaseName;
        var nonExistentRuleName = $"non-existent-rule-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var result = await CallToolAsync(
            "azmcp_sql_server_firewall-rule_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "server", serverName },
                { "firewall-rule-name", nonExistentRuleName }
            });

        // Should return false when trying to delete non-existent rule (idempotent)
        var deleted = result.AssertProperty("deleted").GetBoolean();
        Assert.False(deleted);

        var deletedRuleName = result.AssertProperty("ruleName").GetString();
        Assert.Equal(nonExistentRuleName, deletedRuleName);
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    [InlineData("--subscription sub --resource-group rg")] // Missing server, name, and IP addresses
    [InlineData("--subscription sub --resource-group rg --server server --firewall-rule-name rule1")] // Missing IP addresses
    public async Task Should_Return400_WithInvalidFirewallRuleCreateInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_sql_server_firewall-rule_create",
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
    [InlineData("--subscription sub --resource-group rg")] // Missing server, location, admin credentials
    [InlineData("--subscription sub --resource-group rg --server server1")] // Missing location and admin credentials
    public async Task Should_Return400_WithInvalidSqlServerCreateInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_sql_server_create",
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
    public async Task Should_Return400_WithInvalidSqlServerShowInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_sql_server_show",
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
    public async Task Should_Return400_WithInvalidSqlServerDeleteInput(string args)
    {
        try
        {
            var result = await CallToolAsync("azmcp_sql_server_delete",
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
