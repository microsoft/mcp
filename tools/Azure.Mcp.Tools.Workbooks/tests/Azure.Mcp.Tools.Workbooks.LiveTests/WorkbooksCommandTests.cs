// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.LiveTests;

public class WorkbooksCommandTests(LiveTestFixture fixture, ITestOutputHelper output)
    : CommandTestsBase(fixture, output), IClassFixture<LiveTestFixture>
{
    // Test workbook content for CRUD operations
    private const string TestWorkbookContent = """
        {
            "version": "Notebook/1.0",
            "items": [
                {
                    "type": 1,
                    "content": {
                        "json": "# Test Workbook\n\nThis is a test workbook created by Azure MCP live tests."
                    }
                }
            ],
            "styleSettings": {},
            "$schema": "https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json"
        }
        """;

    [Fact]
    public async Task Should_list_workbooks()
    {
        var result = await CallToolAsync(
            "azmcp_workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var workbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, workbooks.ValueKind);

        // Should have workbooks from bicep template
        var workbooksArray = workbooks.EnumerateArray();
        Assert.NotEmpty(workbooksArray);

        // Verify basic properties exist
        foreach (var workbook in workbooksArray)
        {
            Assert.True(workbook.TryGetProperty("WorkbookId", out _));
            Assert.True(workbook.TryGetProperty("DisplayName", out _));
        }
    }

    [Fact]
    public async Task Should_show_workbook_details()
    {
        // First get the list of workbooks to find a valid ID
        var listResult = await CallToolAsync(
            "azmcp_workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var workbooks = listResult.AssertProperty("Workbooks");
        var workbooksArray = workbooks.EnumerateArray().ToArray();
        Assert.NotEmpty(workbooksArray);

        // Use the first workbook
        var firstWorkbook = workbooksArray[0];
        Assert.True(firstWorkbook.TryGetProperty("WorkbookId", out var workbookId));

        // Now get the detailed workbook
        var result = await CallToolAsync(
            "azmcp_workbooks_show",
            new()
            {
                { "workbook-id", workbookId.GetString()! }
            });

        var workbook = result.AssertProperty("Workbook");
        Assert.True(workbook.TryGetProperty("WorkbookId", out _));
        Assert.True(workbook.TryGetProperty("DisplayName", out var displayName));

        // SerializedData property must be present (but may be null due to Azure API limitation)
        Assert.True(workbook.TryGetProperty("SerializedData", out _));
    }

    [Fact]
    public async Task Should_perform_basic_crud_operations()
    {
        var workbookName = $"Test Workbook {Guid.NewGuid():N}";
        string? workbookId = null;

        try
        {
            // CREATE
            var createResult = await CallToolAsync(
                "azmcp_workbooks_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "display-name", workbookName },
                    { "serialized-content", TestWorkbookContent },
                    { "source-id", "azure monitor" }
                });

            var createdWorkbook = createResult.AssertProperty("Workbook");
            Assert.True(createdWorkbook.TryGetProperty("WorkbookId", out var workbookIdProperty));
            workbookId = workbookIdProperty.GetString();
            Assert.NotNull(workbookId);

            Assert.True(createdWorkbook.TryGetProperty("DisplayName", out var displayNameProperty));
            Assert.Equal(workbookName, displayNameProperty.GetString());

            // UPDATE
            var updatedName = $"Updated {workbookName}";
            var updateResult = await CallToolAsync(
                "azmcp_workbooks_update",
                new()
                {
                    { "workbook-id", workbookId },
                    { "display-name", updatedName }
                });

            var updatedWorkbook = updateResult.AssertProperty("Workbook");
            Assert.True(updatedWorkbook.TryGetProperty("DisplayName", out var updatedDisplayName));
            Assert.Equal(updatedName, updatedDisplayName.GetString());

            // READ (verify exists)
            var showResult = await CallToolAsync(
                "azmcp_workbooks_show",
                new()
                {
                    { "workbook-id", workbookId }
                });

            var shownWorkbook = showResult.AssertProperty("Workbook");
            Assert.True(shownWorkbook.TryGetProperty("WorkbookId", out _));
            Assert.True(shownWorkbook.TryGetProperty("DisplayName", out var shownDisplayName));
            Assert.Equal(updatedName, shownDisplayName.GetString());
        }
        finally
        {
            // DELETE
            if (!string.IsNullOrEmpty(workbookId))
            {
                var deleteResult = await CallToolAsync(
                    "azmcp_workbooks_delete",
                    new()
                    {
                        { "workbook-id", workbookId }
                    });

                Assert.NotNull(deleteResult);
            }
        }
    }

    [Fact]
    public async Task Should_delete_workbook()
    {
        var workbookName = $"Delete Test Workbook {Guid.NewGuid():N}";
        string? workbookId = null;

        // Create a workbook to delete
        var createResult = await CallToolAsync(
            "azmcp_workbooks_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "display-name", workbookName },
                { "serialized-content", TestWorkbookContent },
                { "source-id", "azure monitor" }
            });

        var createdWorkbook = createResult.AssertProperty("Workbook");
        Assert.True(createdWorkbook.TryGetProperty("WorkbookId", out var workbookIdProperty));
        workbookId = workbookIdProperty.GetString();
        Assert.NotNull(workbookId);

        // Delete the workbook
        var deleteResult = await CallToolAsync(
            "azmcp_workbooks_delete",
            new()
            {
                { "workbook-id", workbookId }
            });

        // Verify delete operation succeeded
        Assert.NotNull(deleteResult);
        Assert.True(deleteResult.Value.TryGetProperty("WorkbookId", out var deletedWorkbookId));
        Assert.Equal(workbookId, deletedWorkbookId.GetString());
        Assert.True(deleteResult.Value.TryGetProperty("Message", out var deleteMessage));
        Assert.Equal("Successfully deleted", deleteMessage.GetString());

        // Verify workbook no longer exists by trying to show it (should return error)
        var showResult = await CallToolAsync(
            "azmcp_workbooks_show",
            new()
            {
                { "workbook-id", workbookId }
            });

        // Should return an error response (500) since workbook was deleted
        Assert.NotNull(showResult);
        Assert.True(showResult.Value.TryGetProperty("message", out var errorMessage));
        Assert.True(showResult.Value.TryGetProperty("type", out var errorType));
        Assert.Equal("Exception", errorType.GetString());
        Assert.Contains("not found", errorMessage.GetString(), StringComparison.OrdinalIgnoreCase);
    }


}
