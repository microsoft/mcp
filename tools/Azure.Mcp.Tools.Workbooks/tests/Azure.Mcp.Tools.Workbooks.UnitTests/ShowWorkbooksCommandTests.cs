// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class ShowWorkbooksCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkbooksService _service;
    private readonly ILogger<ShowWorkbooksCommand> _logger;
    private readonly ShowWorkbooksCommand _command;

    public ShowWorkbooksCommandTests()
    {
        _service = Substitute.For<IWorkbooksService>();
        _logger = Substitute.For<ILogger<ShowWorkbooksCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("workbook", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("show", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get Workbook", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.NotNull(description);
        Assert.Contains("workbook", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("batch", description, StringComparison.OrdinalIgnoreCase);
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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.NotNull(result);
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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId1,
            "--workbook-ids", workbookId2
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.NotNull(result);
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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId1,
            "--workbook-ids", workbookId2
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.NotNull(result);
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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkbookBatchResult>(new Exception("Service error")));

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId,
            "--tenant", "test-tenant"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).GetWorkbooksAsync(
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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).GetWorkbooksAsync(
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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId,
            "--auth-method", "1"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).GetWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutWorkbookIds_ReturnsError()
    {
        // Arrange
        var args = _command.GetCommand().Parse([]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

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

        _service.GetWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.NotNull(result);
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
