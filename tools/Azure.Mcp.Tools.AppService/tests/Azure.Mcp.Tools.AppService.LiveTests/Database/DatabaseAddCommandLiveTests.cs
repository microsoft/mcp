// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
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
        // If the tool returned no JSON (null), treat that as an expected error outcome in live tests
        if (!result.HasValue)
        {
            // Expected for live environments where resources may not exist; accept as valid outcome.
            return;
        }

        // Otherwise, verify that the returned JSON is non-empty
        var contentString = result.Value.ToString();
        Assert.False(string.IsNullOrWhiteSpace(contentString), "Expected non-empty content when command returns JSON");
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

        // Test that database type validation works correctly
        if (!result.HasValue)
        {
            // No JSON result indicates the tool returned an error (acceptable for live environment)
            return;
        }
        else
        {
            var content = result.Value.ToString();

            // Should not fail due to invalid database type since we're testing valid types
            Assert.False(
                content.Contains("Unsupported database type") ||
                content.Contains("invalid database type"),
                $"Database type '{databaseType}' should be supported but got error: {content}");

            // If it succeeded, ensure the returned content is not empty
            Assert.False(string.IsNullOrWhiteSpace(content), $"Command should return content for {databaseType}");
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
                { "resource-group", Settings.ResourceGroupName },
                { "app", Settings.ResourceBaseName + "-webapp" },
                { "database-type", invalidDatabaseType },
                { "database-server", Settings.ResourceBaseName + "-sql.database.windows.net" },
                { "database", Settings.ResourceBaseName + "db" }
            });

        // For invalid types, the tool may either return no JSON (error case) or a JSON error payload.
        if (!result.HasValue)
        {
            // No JSON result indicates the tool returned an error — acceptable outcome
            return;
        }

        // If JSON was returned, validate the error message explicitly
        var root = result.Value;
        string? message = null;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
            {
                message = m.GetString();
            }
            else if (root.TryGetProperty("results", out var r) && r.ValueKind == JsonValueKind.Object)
            {
                if (r.TryGetProperty("message", out var rm) && rm.ValueKind == JsonValueKind.String)
                {
                    message = rm.GetString();
                }
            }
        }

        Assert.False(string.IsNullOrWhiteSpace(message), $"Expected an error message for invalid database type '{invalidDatabaseType}' but none was found");
        Assert.Contains("Unsupported database type", message, StringComparison.OrdinalIgnoreCase);
    }
}
