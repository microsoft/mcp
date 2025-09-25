// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.LiveTests;

public class WorkbooksCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
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
            workbook.AssertProperty("WorkbookId");
            workbook.AssertProperty("DisplayName");
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
        var workbookId = firstWorkbook.AssertProperty("WorkbookId");

        // Now get the detailed workbook
        var result = await CallToolAsync(
            "azmcp_workbooks_show",
            new()
            {
                { "workbook-id", workbookId.GetString()! }
            });

        var workbook = result.AssertProperty("Workbook");
        workbook.AssertProperty("WorkbookId");
        workbook.AssertProperty("DisplayName");

        // SerializedData property must be present (but may be null due to Azure API limitation)
        workbook.AssertProperty("SerializedData");
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
            var workbookIdProperty = createdWorkbook.AssertProperty("WorkbookId");
            workbookId = workbookIdProperty.GetString();
            Assert.NotNull(workbookId);

            var displayNameProperty = createdWorkbook.AssertProperty("DisplayName");
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
            var updatedDisplayName = updatedWorkbook.AssertProperty("DisplayName");
            Assert.Equal(updatedName, updatedDisplayName.GetString());

            // READ (verify exists)
            var showResult = await CallToolAsync(
                "azmcp_workbooks_show",
                new()
                {
                    { "workbook-id", workbookId }
                });

            var shownWorkbook = showResult.AssertProperty("Workbook");
            shownWorkbook.AssertProperty("WorkbookId");
            var shownDisplayName = shownWorkbook.AssertProperty("DisplayName");
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
        var workbookIdProperty = createdWorkbook.AssertProperty("WorkbookId");
        string? workbookId = workbookIdProperty.GetString();
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
        var deletedWorkbookId = deleteResult.AssertProperty("WorkbookId");
        Assert.Equal(workbookId, deletedWorkbookId.GetString());
        var deleteMessage = deleteResult.AssertProperty("Message");
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
        var errorMessage = showResult.AssertProperty("message");
        var errorType = showResult.AssertProperty("type");
        Assert.Equal("Exception", errorType.GetString());
        Assert.Contains("not found", errorMessage.GetString(), StringComparison.OrdinalIgnoreCase);
    }


}

