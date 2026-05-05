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

public class ListWorkbooksCommandTests : CommandUnitTestsBase<ListWorkbooksCommand, IWorkbooksService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("list", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("List Workbooks", Command.Title);
    }

    [Fact]
    public void Description_VerifyLength()
    {
        var description = Command.Description;
        Assert.NotNull(description);
        Assert.True(Command.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWorkbooks_WhenWorkbooksExist()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Test Workbook 1",
                Description: "Test Description 1",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: "{}",
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            ),
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook2",
                DisplayName: "Test Workbook 2",
                Description: "Test Description 2",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: "{}",
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user2",
                SourceId: "azure monitor"
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, expectedWorkbooks.Count, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.Equal(expectedWorkbooks.Count, result.Workbooks.Count);
        Assert.Collection(result.Workbooks,
            workbook =>
            {
                Assert.Equal("Test Workbook 1", workbook.DisplayName);
                Assert.Contains("workbook1", workbook.WorkbookId);
            },
            workbook =>
            {
                Assert.Equal("Test Workbook 2", workbook.DisplayName);
                Assert.Contains("workbook2", workbook.WorkbookId);
            });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResults_WhenNoWorkbooksExist()
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.Empty(result.Workbooks);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group",
            "--tenant", "test-tenant");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>?>(s => s != null && s.Contains("test-subscription")),
            Arg.Is<IReadOnlyList<string>?>(rg => rg != null && rg.Contains("test-resource-group")),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is("test-tenant"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>?>(s => s != null && s.Contains("test-subscription")),
            Arg.Is<IReadOnlyList<string>?>(rg => rg != null && rg.Contains("test-resource-group")),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is<string?>(t => t == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group",
            "--auth-method", "1");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>?>(s => s != null && s.Contains("test-subscription")),
            Arg.Is<IReadOnlyList<string>?>(rg => rg != null && rg.Contains("test-resource-group")),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSubscription_ReturnsValidationError()
    {
        // Arrange & Act - subscription is required
        var response = await ExecuteCommandAsync([]);

        // Assert - should fail validation without subscription
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexWorkbookData_SerializesCorrectly()
    {
        // Arrange
        var complexSerializedData = @"{
            ""version"": ""Notebook/1.0"",
            ""items"": [
                {
                    ""type"": 1,
                    ""content"": {
                        ""json"": ""# Complex Workbook\n\nThis is a complex test workbook.""
                    }
                }
            ]
        }";

        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/complex",
                DisplayName: "Complex Test Workbook",
                Description: "A workbook with complex data",
                Category: "workbook",
                Location: "westus2",
                Kind: "shared",
                Tags: @"{""environment"": ""test"", ""team"": ""data""}",
                SerializedData: complexSerializedData,
                Version: "2.0",
                TimeModified: new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
                UserId: "complex-user-id",
                SourceId: "azure monitor"
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.Single(result.Workbooks);

        var workbook = result.Workbooks.First();
        Assert.Equal("Complex Test Workbook", workbook.DisplayName);
        Assert.Equal("2.0", workbook.Version);
        Assert.Contains("complex", workbook.WorkbookId);
        Assert.Contains("Notebook/1.0", workbook.SerializedData);
    }

    [Fact]
    public async Task ExecuteAsync_WithKindFilter_PassesCorrectFilter()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Shared Workbook",
                Description: "A shared workbook",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared"),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", "shared");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared"),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFilter_PassesCorrectFilter()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Sentinel Workbook",
                Description: "A sentinel workbook",
                Category: "sentinel",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == "sentinel"),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--category", "sentinel");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == "sentinel"),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithSourceIdFilter_PassesCorrectFilter()
    {
        // Arrange
        var sourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/components/myapp";
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "App Insights Workbook",
                Description: "A workbook linked to App Insights",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: sourceId
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.SourceId == sourceId),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--source-id", sourceId);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.SourceId == sourceId),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleFilters_PassesCorrectFilters()
    {
        // Arrange
        var sourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/components/myapp";
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Filtered Workbook",
                Description: "A workbook with multiple filters",
                Category: "sentinel",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: sourceId
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared" && f.Category == "sentinel" && f.SourceId == sourceId),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", "shared",
            "--category", "sentinel",
            "--source-id", sourceId);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared" && f.Category == "sentinel" && f.SourceId == sourceId),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_PassesEmptyFilters()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Unfiltered Workbook",
                Description: "A workbook without filters",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && !f.HasFilters),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && !f.HasFilters),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("shared")]
    [InlineData("user")]
    public async Task ExecuteAsync_WithValidKind_AcceptsKindValue(string kind)
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == kind),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", kind);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == kind),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("workbook")]
    [InlineData("sentinel")]
    [InlineData("TSG")]
    [InlineData("application")]
    public async Task ExecuteAsync_WithValidCategory_AcceptsCategoryValue(string category)
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == category),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--category", category);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == category),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTotalCount_WhenIncludeTotalCountTrue()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Test Workbook",
                Description: "Test",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 100, null); // TotalCount=100

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Is(true), // includeTotalCount
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.Equal(100, result.TotalCount);
        Assert.Equal(1, result.Returned);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxResults_PassesMaxResultsToService()
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Is(25), // maxResults
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--max-results", "25");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Is(25),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("summary", OutputFormat.Summary)]
    [InlineData("standard", OutputFormat.Standard)]
    [InlineData("full", OutputFormat.Full)]
    public async Task ExecuteAsync_WithOutputFormat_PassesCorrectFormat(string formatStr, OutputFormat expectedFormat)
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Is(expectedFormat),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--output-format", formatStr);

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Is(expectedFormat),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNameContainsFilter_PassesCorrectFilter()
    {
        // Arrange
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.NameContains == "my-workbook"),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--name-contains", "my-workbook");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.NameContains == "my-workbook"),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithModifiedAfterFilter_PassesCorrectFilter()
    {
        // Arrange - Testing the --modified-after semantic filter from optimization plan
        var modifiedAfterDate = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.ModifiedAfter.HasValue && f.ModifiedAfter.Value.Date == modifiedAfterDate.Date),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--modified-after", "2024-06-15");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("2024-06-15")]
    [InlineData("2024-06-15T10:30:00Z")]
    [InlineData("2024-06-15T10:30:00+02:00")]
    public async Task ExecuteAsync_WithVariousDateFormats_ParsesModifiedAfterCorrectly(string dateString)
    {
        // Arrange - Testing various ISO 8601 date formats
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--modified-after", dateString);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidDateFormat_IgnoresModifiedAfterFilter()
    {
        // Arrange - Invalid date should be ignored, not cause an error
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && !f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--modified-after", "invalid-date");

        // Assert - Should succeed but with no modified-after filter applied
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null && !f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1001, 1000)]
    [InlineData(5000, 1000)]
    [InlineData(2000, 1000)]
    public async Task ExecuteAsync_WithMaxResultsOverLimit_CapsAtMaximum(int requestedMaxResults, int expectedMaxResults)
    {
        // Arrange - Per optimization plan, max-results is capped at 1000
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Is(expectedMaxResults),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--max-results", requestedMaxResults.ToString());

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Is(expectedMaxResults),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(-1, 50)]
    public async Task ExecuteAsync_WithZeroOrNegativeMaxResults_UsesDefaultValue(int requestedMaxResults, int expectedMaxResults)
    {
        // Arrange - Zero or negative should default to 50
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Is(expectedMaxResults),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--max-results", requestedMaxResults.ToString());

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Is(expectedMaxResults),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_IncludeTotalCountDefaultsToTrue_WhenNotSpecified()
    {
        // Arrange - Test that includeTotalCount defaults to true when not specified
        var listResult = new WorkbookListResult([], 100, null);

        // Set up mock to accept any call and return our result
        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        // Verify the service was called with includeTotalCount = true (default behavior)
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Is(true),  // Default should be true
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAllSemanticFilters_PassesAllFiltersCorrectly()
    {
        // Arrange - Test combining all semantic filters together per optimization plan
        var sourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.operationalinsights/workspaces/workspace1";

        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Production Dashboard",
                Description: "Production monitoring dashboard",
                Category: "sentinel",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{}",
                Version: "1.0",
                TimeModified: new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
                UserId: "user1",
                SourceId: sourceId
            )
        };

        var listResult = new WorkbookListResult(expectedWorkbooks, 1, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null &&
                f.Kind == "shared" &&
                f.Category == "sentinel" &&
                f.SourceId == sourceId &&
                f.NameContains == "dashboard" &&
                f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", "shared",
            "--category", "sentinel",
            "--source-id", sourceId,
            "--name-contains", "dashboard",
            "--modified-after", "2024-01-01");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is<WorkbookFilters?>(f => f != null &&
                f.Kind == "shared" &&
                f.Category == "sentinel" &&
                f.SourceId == sourceId &&
                f.NameContains == "dashboard" &&
                f.ModifiedAfter.HasValue),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("SUMMARY", OutputFormat.Summary)]
    [InlineData("Summary", OutputFormat.Summary)]
    [InlineData("FULL", OutputFormat.Full)]
    [InlineData("Full", OutputFormat.Full)]
    [InlineData("STANDARD", OutputFormat.Standard)]
    [InlineData("Standard", OutputFormat.Standard)]
    public async Task ExecuteAsync_WithOutputFormatCaseInsensitive_ParsesCorrectly(string formatStr, OutputFormat expectedFormat)
    {
        // Arrange - Output format should be case-insensitive
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Is(expectedFormat),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--output-format", formatStr);

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Is(expectedFormat),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownOutputFormat_DefaultsToStandard()
    {
        // Arrange - Unknown format should default to standard
        var listResult = new WorkbookListResult([], 0, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Is(OutputFormat.Standard),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--output-format", "unknown-format");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Is(OutputFormat.Standard),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsToIncludeTotalCountTrue()
    {
        // Arrange - Per optimization plan, includeTotalCount defaults to true
        var listResult = new WorkbookListResult([], 100, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Is(true), // should default to true
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        await Service.Received(1).ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Is(true),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResults_WhenServiceReturnsNullTotalCount()
    {
        // Arrange - Regression test for null data handling from Resource Graph
        // When Resource Graph returns null data, the service should return empty results
        // instead of throwing "The requested operation requires an element of type 'Object'"
        var listResult = new WorkbookListResult([], null, null);

        Service.ListWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<OutputFormat>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(listResult);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert - Should return OK with empty results, not throw
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.Empty(result.Workbooks);
        Assert.Null(result.TotalCount);
    }
}
