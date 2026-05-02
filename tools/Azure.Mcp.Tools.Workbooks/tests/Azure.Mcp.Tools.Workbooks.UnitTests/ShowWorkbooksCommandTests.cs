// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Workbooks.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class ShowWorkbooksCommandTests : CommandUnitTestsBase<ShowWorkbooksCommand, IWorkbooksService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("show", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("show", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get Workbook", Command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = Command.Description;
        Assert.NotNull(description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWorkbook_WhenWorkbookExists()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var expectedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test Workbook",
            Description: "Test Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{\"version\":\"Notebook/1.0\",\"items\":[]}",
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        var batchResult = new WorkbookBatchResult([expectedWorkbook], []);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.Single(result.Workbooks);
        Assert.Empty(result.Errors);
        Assert.Equal("Test Workbook", result.Workbooks[0].DisplayName);
        Assert.Equal(workbookId, result.Workbooks[0].WorkbookId);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBatchResults_WhenMultipleWorkbooksRequested()
    {
        // Arrange
        var workbookId1 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var workbookId2 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook2";

        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: workbookId1,
                DisplayName: "Test Workbook 1",
                Description: "Test Description 1",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: "{}",
                SerializedData: "{}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            ),
            new(
                WorkbookId: workbookId2,
                DisplayName: "Test Workbook 2",
                Description: "Test Description 2",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: "{}",
                SerializedData: "{}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user2",
                SourceId: "azure monitor"
            )
        };

        var batchResult = new WorkbookBatchResult(expectedWorkbooks, []);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId1, "--workbook-ids", workbookId2);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.Equal(2, result.Workbooks.Count);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPartialResults_WhenSomeWorkbooksNotFound()
    {
        // Arrange
        var workbookId1 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var workbookId2 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/notfound";

        var expectedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId1,
            DisplayName: "Test Workbook 1",
            Description: "Test Description 1",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{}",
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        var error = new WorkbookError(workbookId2, 404, "Resource not found");

        var batchResult = new WorkbookBatchResult([expectedWorkbook], [error]);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId1, "--workbook-ids", workbookId2);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.Single(result.Workbooks);
        Assert.Single(result.Errors);
        Assert.Equal(workbookId2, result.Errors[0].WorkbookId);
        Assert.Equal(404, result.Errors[0].StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var batchResult = new WorkbookBatchResult([], []);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync("--workbook-ids", workbookId, "--tenant", "test-tenant");

        // Assert
        await Service.Received(1).GetWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is("test-tenant"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var batchResult = new WorkbookBatchResult([], []);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        await Service.Received(1).GetWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is<string?>(t => t == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var batchResult = new WorkbookBatchResult([], []);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync("--workbook-ids", workbookId, "--auth-method", "1");

        // Assert
        await Service.Received(1).GetWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutWorkbookIds_ReturnsError()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync([]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("workbook", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexWorkbookData_SerializesCorrectly()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/complex";
        var complexSerializedData = @"{
            ""version"": ""Notebook/1.0"",
            ""items"": [
                {
                    ""type"": 1,
                    ""content"": {
                        ""json"": ""# Complex Workbook\n\nThis is a complex test workbook with markdown.""
                    }
                },
                {
                    ""type"": 3,
                    ""content"": {
                        ""version"": ""KqlItem/1.0"",
                        ""query"": ""AzureActivity | summarize count() by ActivityStatus"",
                        ""size"": 0,
                        ""title"": ""Activity Summary"",
                        ""timeContext"": {
                            ""durationMs"": 86400000
                        },
                        ""queryType"": 0,
                        ""resourceType"": ""microsoft.operationalinsights/workspaces"",
                        ""visualization"": ""piechart""
                    }
                }
            ],
            ""styleSettings"": {},
            ""$schema"": ""https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json""
        }";

        var complexTags = @"{
            ""environment"": ""production"",
            ""team"": ""data-analytics"",
            ""version"": ""2.1"",
            ""custom"": ""true""
        }";

        var expectedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Complex Analytics Dashboard",
            Description: "A comprehensive dashboard with multiple KQL queries and visualizations",
            Category: "workbook",
            Location: "westus2",
            Kind: "shared",
            Tags: complexTags,
            SerializedData: complexSerializedData,
            Version: "2.1",
            TimeModified: new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.Zero),
            UserId: "complex-user-id-12345",
            SourceId: "azure monitor"
        );

        var batchResult = new WorkbookBatchResult([expectedWorkbook], []);

        Service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.Single(result.Workbooks);
        Assert.Empty(result.Errors);

        var workbook = result.Workbooks[0];
        Assert.Equal("Complex Analytics Dashboard", workbook.DisplayName);
        Assert.Equal("2.1", workbook.Version);
        Assert.Equal("westus2", workbook.Location);
        Assert.Contains("data-analytics", workbook.Tags);
        Assert.Contains("KqlItem/1.0", workbook.SerializedData);
        Assert.Contains("AzureActivity", workbook.SerializedData);
        Assert.Contains("piechart", workbook.SerializedData);
        Assert.Equal("complex-user-id-12345", workbook.UserId);
    }
}
