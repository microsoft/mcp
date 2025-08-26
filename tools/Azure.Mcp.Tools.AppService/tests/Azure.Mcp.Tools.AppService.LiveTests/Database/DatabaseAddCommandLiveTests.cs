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
public class DatabaseAddCommandLiveTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output),
    IClassFixture<LiveTestFixture>
{
    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsSuccessResult()
    {
        var result = await CallToolAsync(
            "azmcp_appservice_database_add",
            new Dictionary<string, object?>
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "test-rg" },
                { "app", "test-app" },
                { "database-type", "SqlServer" },
                { "database-server", "test-server.database.windows.net" },
                { "database", "test-db" }
            });

        // Test should validate actual command execution and error handling
        Assert.NotNull(result);

        // In live environment, this will likely fail due to non-existent resources,
        // but we should get a proper error response rather than a system exception
        if (result.IsError())
        {
            // Validate we get expected Azure resource errors, not system/interface errors
            var errorContent = result.Content()?.ToString() ?? "";
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
            Assert.True(result.IsSuccess());
            Assert.NotNull(result.Content());
        }
    }

    [Theory]
    [InlineData("basic", null, null, null, null)]
    [InlineData("custom-connection-string", "Server=custom;Database=custom;UserId=user;Password=pass;", null, null, null)]
    [InlineData("tenant", null, "test-tenant-id", null, null)]
    [InlineData("retry-policy", null, null, 3, 1.0)]
    public async Task ExecuteAsync_WithVariousParameters_AcceptsParameters(
        string scenario,
        string? connectionString,
        string? tenant,
        int? retryMaxRetries,
        double? retryDelay)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "subscription", Settings.SubscriptionId },
            { "resource-group", "test-rg" },
            { "app", "test-app" },
            { "database-type", "SqlServer" },
            { "database-server", "test-server.database.windows.net" },
            { "database", "test-db" }
        };

        // Add optional parameters based on scenario
        if (connectionString != null)
            parameters.Add("connection-string", connectionString);

        if (tenant != null)
            parameters.Add("tenant", tenant);

        if (retryMaxRetries.HasValue)
            parameters.Add("retry-max-retries", retryMaxRetries.Value);

        if (retryDelay.HasValue)
            parameters.Add("retry-delay", retryDelay.Value);

        var result = await CallToolAsync("azmcp_appservice_database_add", parameters);

        // Test actual command execution and proper error handling
        Assert.NotNull(result);

        // Validate that parameters are correctly passed and processed
        if (result.IsError())
        {
            var errorContent = result.Content()?.ToString() ?? "";

            // Should not fail due to parameter validation issues for valid scenarios
            Assert.False(
                errorContent.Contains("required parameter") ||
                errorContent.Contains("invalid parameter") ||
                errorContent.Contains("ArgumentException"),
                $"[{scenario}] Parameter validation failed: {errorContent}");

            // Should fail due to Azure resource issues, which is expected in live tests
            Assert.True(
                errorContent.Contains("not found") ||
                errorContent.Contains("does not exist") ||
                errorContent.Contains("ResourceGroupNotFound") ||
                errorContent.Contains("WebSiteNotFound") ||
                errorContent.Contains("subscription"),
                $"[{scenario}] Expected Azure resource error but got: {errorContent}");
        }
        else
        {
            // If successful, validate the response has expected structure
            Assert.True(result.IsSuccess(), $"[{scenario}] Command should succeed or fail with proper Azure error");
        }
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("MySQL")]
    [InlineData("PostgreSQL")]
    [InlineData("CosmosDB")]
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
        if (result.IsError())
        {
            var errorContent = result.Content()?.ToString() ?? "";

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
            Assert.True(result.IsSuccess(), $"Command should handle {databaseType} database type correctly");
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
        Assert.True(result.IsError(), $"Invalid database type '{invalidDatabaseType}' should cause validation error");

        var errorContent = result.Content()?.ToString() ?? "";
        Assert.True(
            errorContent.Contains("Unsupported database type") ||
            errorContent.Contains("invalid database type") ||
            errorContent.Contains("ArgumentException"),
            $"Expected database type validation error for '{invalidDatabaseType}' but got: {errorContent}");
    }
}
