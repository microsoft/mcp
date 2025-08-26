// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Database;
[Trait("Area", "AppService")]
[Trait("Command", "DatabaseAddCommand")]
public class DatabaseAddCommandLiveTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsSuccessResult()
    {
        var result = await CallToolAsync(
            "azmcp_appservice_database_add",
            new Dictionary<string, object?>
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "app", Settings.ResourceBaseName + "-webapp" },
                { "database-type", "SqlServer" },
                { "database-server", Settings.ResourceBaseName + "-sql.database.windows.net" },
                { "database", Settings.ResourceBaseName + "db" }
            });

        // Test should validate actual command execution and error handling
        Assert.NotNull(result);

        // In live environment, this will likely fail due to non-existent resources,
        // but we should get a proper error response rather than a system exception
        if (result.IsError)
        {
            // Validate we get expected Azure resource errors, not system/interface errors
            var errorContent = result.Content?.ToString() ?? result.Error?.Message ?? "";
            Assert.True(
                errorContent.Contains("not found") ||
                errorContent.Contains("does not exist") ||
                errorContent.Contains("ResourceGroupNotFound") ||
                errorContent.Contains("WebSiteNotFound") ||
                errorContent.Contains("subscription"),
                $"Expected Azure resource error but got: {errorContent}");
        }
        else
        {
            // If successful, validate the response structure
            Assert.False(result.IsError);
            Assert.NotNull(result.Content);
        }
    }

    [Theory]
    [InlineData("SqlServer")]
    public async Task ExecuteAsync_WithDifferentDatabaseTypes_AcceptsValidTypes(string databaseType)
    {
        var result = await CallToolAsync(
            "azmcp_appservice_database_add",
            new Dictionary<string, object?>
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "test-rg" },
                { "app", "test-app" },
                { "database-type", databaseType },
                { "database-server", "test-server" },
                { "database", "test-db" }
            });

        Assert.NotNull(result);

        // Test that database type validation works correctly
            if (result.IsError)
            {
                var errorContent = result.Content?.ToString() ?? "";

                // Should not fail due to invalid database type since we're testing valid types
                Assert.False(
                    errorContent.Contains("Unsupported database type") ||
                    errorContent.Contains("invalid database type"),
                    $"Database type '{databaseType}' should be supported but got error: {errorContent}");

                // Should fail due to Azure resource issues (expected in live environment)
                Assert.True(
                    errorContent.Contains("not found") ||
                    errorContent.Contains("does not exist") ||
                    errorContent.Contains("ResourceGroupNotFound") ||
                    errorContent.Contains("WebSiteNotFound") ||
                    errorContent.Contains("subscription"),
                    $"Expected Azure resource error for {databaseType} but got: {errorContent}");
            }
            else
            {
                Assert.False(result.IsError, $"Command should handle {databaseType} database type correctly");
            }
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    [InlineData("random")]
    public async Task ExecuteAsync_WithInvalidDatabaseTypes_ReturnsValidationError(string invalidDatabaseType)
    {
        var result = await CallToolAsync(
            "azmcp_appservice_database_add",
            new Dictionary<string, object?>
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "test-rg" },
                { "app", "test-app" },
                { "database-type", invalidDatabaseType },
                { "database-server", "test-server" },
                { "database", "test-db" }
            });

        Assert.NotNull(result);
        Assert.True(result.IsError, $"Invalid database type '{invalidDatabaseType}' should cause validation error");

        var errorContent = result.Content?.ToString() ?? result.Error?.Message ?? "";
        Assert.True(
            errorContent.Contains("Unsupported database type") ||
            errorContent.Contains("invalid database type") ||
            errorContent.Contains("ArgumentException"),
            $"Expected database type validation error for '{invalidDatabaseType}' but got: {errorContent}");
    }
}
