// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.LiveTests;

public class WorkbooksCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";
    public override List<UriRegexSanitizer> UriRegexSanitizers => new List<UriRegexSanitizer>
    {
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "workbooks\\/([^?\\/]+)",
            Value = EmptyGuid,
            GroupForReplace = "1"
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "resource[gG]roups\\/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        })
    };

    public override List<BodyKeySanitizer> BodyKeySanitizers => new List<BodyKeySanitizer>()
    {
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.query")
        {
            Regex = "\\r\\n",
            Value = "\\n"
        })
    };

    protected override async ValueTask LoadSettingsAsync()
    {
        await base.LoadSettingsAsync();

        // we're inserting this in front of others to ensure it runs first
        GeneralRegexSanitizers.Insert(0, new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = Settings.ResourceGroupName,
            Value = "Sanitized",
        }));
    }

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
    [CustomMatcher(compareBody: false)]
    public async Task Should_list_workbooks()
    {
        var result = await CallToolAsync(
            "workbooks_list",
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
    [CustomMatcher(compareBody: false)]
    public async Task Should_list_workbooks_with_total_count()
    {
        // Test total count feature from optimization plan
        var result = await CallToolAsync(
            "workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "include-total-count", "true" }
            });

        var workbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, workbooks.ValueKind);

        // Total count should be present
        var totalCount = result.AssertProperty("TotalCount");
        Assert.True(totalCount.ValueKind == JsonValueKind.Number || totalCount.ValueKind == JsonValueKind.Null);

        // Returned count should be present
        var returned = result.AssertProperty("Returned");
        Assert.Equal(JsonValueKind.Number, returned.ValueKind);
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_list_workbooks_with_name_contains_filter()
    {
        // Test the --name-contains semantic filter from optimization plan
        var result = await CallToolAsync(
            "workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name-contains", "Test" }  // Bicep template creates workbooks with "Test" in name
            });

        var workbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, workbooks.ValueKind);

        // All returned workbooks should contain "Test" in display name
        foreach (var workbook in workbooks.EnumerateArray())
        {
            var displayName = workbook.AssertProperty("DisplayName").GetString();
            Assert.Contains("Test", displayName, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_list_workbooks_with_max_results()
    {
        // Test max-results feature from optimization plan
        var result = await CallToolAsync(
            "workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "max-results", "1" }
            });

        var workbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, workbooks.ValueKind);

        // Should return at most 1 workbook
        var workbooksArray = workbooks.EnumerateArray().ToArray();
        Assert.True(workbooksArray.Length <= 1);

        // Returned count should match actual count
        var returned = result.AssertProperty("Returned");
        Assert.Equal(workbooksArray.Length, returned.GetInt32());
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_list_workbooks_with_summary_output_format()
    {
        // Test output-format=summary from optimization plan (minimal tokens)
        var result = await CallToolAsync(
            "workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "output-format", "summary" }
            });

        var workbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, workbooks.ValueKind);

        // Verify workbooks have basic properties
        foreach (var workbook in workbooks.EnumerateArray())
        {
            workbook.AssertProperty("WorkbookId");
            workbook.AssertProperty("DisplayName");
        }
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_list_workbooks_without_subscription_for_cross_scope_discovery()
    {
        // Test optional subscription parameter from optimization plan
        // When omitted, should return all accessible workbooks across tenants
        var result = await CallToolAsync(
            "workbooks_list",
            new()
            {
                { "max-results", "5" }  // Limit results for test speed
            });

        var workbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, workbooks.ValueKind);

        // Should return workbooks (may be from any accessible subscription)
        // This tests the cross-scope discovery feature
        Assert.True(workbooks.ValueKind == JsonValueKind.Array, "Workbooks property should be an array");
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_show_workbook_details()
    {
        // First get the list of workbooks to find a valid ID
        var listResult = await CallToolAsync(
            "workbooks_list",
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
            "workbooks_show",
            new()
            {
                { "workbook-ids", workbookId.GetString()! }
            });

        var showWorkbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, showWorkbooks.ValueKind);
        var showWorkbooksArray = showWorkbooks.EnumerateArray().ToArray();
        Assert.NotEmpty(showWorkbooksArray);

        var workbook = showWorkbooksArray[0];
        workbook.AssertProperty("WorkbookId");
        workbook.AssertProperty("DisplayName");

        // SerializedData property must be present (but may be null due to Azure API limitation)
        workbook.AssertProperty("SerializedData");
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_show_multiple_workbooks_in_batch()
    {
        // Test batch show operations from optimization plan
        // First get the list of workbooks to find valid IDs
        var listResult = await CallToolAsync(
            "workbooks_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "max-results", "2" }
            });

        var workbooks = listResult.AssertProperty("Workbooks");
        var workbooksArray = workbooks.EnumerateArray().ToArray();

        if (workbooksArray.Length < 2)
        {
            // Not enough workbooks for batch test, skip
            return;
        }

        // Get IDs of first two workbooks
        var workbookId1 = workbooksArray[0].GetProperty("WorkbookId").GetString()!;
        var workbookId2 = workbooksArray[1].GetProperty("WorkbookId").GetString()!;

        // Batch show - multiple workbook IDs in single call
        var result = await CallToolAsync(
            "workbooks_show",
            new()
            {
                { "workbook-ids", new[] { workbookId1, workbookId2 } }
            });

        var showWorkbooks = result.AssertProperty("Workbooks");
        Assert.Equal(JsonValueKind.Array, showWorkbooks.ValueKind);
        var showWorkbooksArray = showWorkbooks.EnumerateArray().ToArray();

        // Should have returned both workbooks
        Assert.Equal(2, showWorkbooksArray.Length);

        // Verify both workbooks have required properties
        foreach (var workbook in showWorkbooksArray)
        {
            workbook.AssertProperty("WorkbookId");
            workbook.AssertProperty("DisplayName");
        }

        // Errors array should be empty (all succeeded)
        var errors = result.AssertProperty("Errors");
        Assert.Empty(errors.EnumerateArray());
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_perform_basic_crud_operations()
    {
        var workbookName = RegisterOrRetrieveVariable("workbookName", $"Test Workbook {Guid.NewGuid():N}");
        string? workbookId = null;

        try
        {
            // CREATE
            var createResult = await CallToolAsync(
                "workbooks_create",
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
            var updatedName = RegisterOrRetrieveVariable("updatedName", $"Updated {workbookName}");
            var updateResult = await CallToolAsync(
                "workbooks_update",
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
                "workbooks_show",
                new()
                {
                    { "workbook-ids", workbookId }
                });

            var shownWorkbooks = showResult.AssertProperty("Workbooks");
            Assert.Equal(JsonValueKind.Array, shownWorkbooks.ValueKind);
            var shownWorkbooksArray = shownWorkbooks.EnumerateArray().ToArray();
            Assert.NotEmpty(shownWorkbooksArray);

            var shownWorkbook = shownWorkbooksArray[0];
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
                    "workbooks_delete",
                    new()
                    {
                        { "workbook-ids", workbookId }
                    });

                Assert.NotNull(deleteResult);
            }
        }
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_delete_workbook()
    {
        var workbookName = RegisterOrRetrieveVariable("WorkBookName", $"Delete Test Workbook {Guid.NewGuid():N}");

        // Create a workbook to delete
        var createResult = await CallToolAsync(
            "workbooks_create",
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
        workbookId = RetrieveSanitizedVariable("WorkbookId", workbookId);

        // Delete the workbook
        var deleteResult = await CallToolAsync(
            "workbooks_delete",
            new()
            {
                { "workbook-ids", workbookId }
            });

        // Verify delete operation succeeded
        Assert.NotNull(deleteResult);
        var succeeded = deleteResult.AssertProperty("Succeeded");
        Assert.Equal(JsonValueKind.Array, succeeded.ValueKind);
        var succeededArray = succeeded.EnumerateArray().ToArray();
        Assert.Single(succeededArray);
        Assert.Equal(workbookId, succeededArray[0].GetString());
        var errors = deleteResult.AssertProperty("Errors");
        Assert.Empty(errors.EnumerateArray());

        // Verify workbook no longer exists by trying to show it (should return empty or error)
        var showResult = await CallToolAsync(
            "workbooks_show",
            new()
            {
                { "workbook-ids", workbookId }
            });

        // Should return a response with empty Workbooks array or an error
        Assert.NotNull(showResult);
        // After deletion, show might return an error or empty results
        // depending on the implementation - check if it has errors or empty workbooks
        if (showResult.Value.TryGetProperty("Errors", out var showErrors) && showErrors.GetArrayLength() > 0)
        {
            // Has errors - expected when workbook not found
            var errorItem = showErrors.EnumerateArray().First();
            var errorMessage = errorItem.GetProperty("Message").GetString();
            Assert.Contains("not found", errorMessage, StringComparison.OrdinalIgnoreCase);
        }
        else if (showResult.Value.TryGetProperty("message", out var errorMsg))
        {
            // Response has error message at top level
            Assert.Contains("not found", errorMsg.GetString()!, StringComparison.OrdinalIgnoreCase);
        }
    }

    private string RetrieveSanitizedVariable(string name, string value)
    {
        if (this.TestMode == Azure.Mcp.Tests.Helpers.TestMode.Live)
        {
            return value;
        }
        else if (this.TestMode == Azure.Mcp.Tests.Helpers.TestMode.Record)
        {
            RegisterVariable(name, value.Replace(Settings.SubscriptionId, EmptyGuid).Replace(Settings.ResourceGroupName.ToLower(), "Sanitized"));
            return value;
        }
        else
        {
            return TestVariables[name];
        }
    }
}

