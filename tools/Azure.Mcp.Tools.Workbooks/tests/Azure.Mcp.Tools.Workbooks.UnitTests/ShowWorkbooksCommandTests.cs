// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        Assert.Contains("resource ID", command.Description, StringComparison.OrdinalIgnoreCase);
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
        Assert.Contains("resource ID", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("JSON", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metadata", description, StringComparison.OrdinalIgnoreCase);
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

        _service.GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Workbook);
        Assert.Equal("Test Workbook", result.Workbook.DisplayName);
        Assert.Equal(workbookId, result.Workbook.WorkbookId);
        Assert.Equal("Test Description", result.Workbook.Description);
        Assert.Contains("Notebook/1.0", result.Workbook.SerializedData);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenWorkbookNotFound()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/nonexistent";

        _service.GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(Task.FromResult<WorkbookInfo?>(null));

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to retrieve workbook", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(Task.FromException<WorkbookInfo?>(new Exception("Service error")));

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

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
        var expectedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test",
            Description: "Test",
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

        _service.GetWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--tenant", "test-tenant"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Is("test-tenant"));
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var expectedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test",
            Description: "Test",
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

        _service.GetWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Is((string?)null));
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var expectedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test",
            Description: "Test",
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

        _service.GetWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--auth-method", "1"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithInvalidWorkbookId_ReturnsValidationError(string invalidWorkbookId)
    {
        // Arrange
        var args = _command.GetCommand().Parse([
            "--workbook-id", invalidWorkbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("workbook", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithMalformedWorkbookId_ReturnsServiceError()
    {
        // Arrange
        var invalidWorkbookId = "invalid-resource-id";
        var args = _command.GetCommand().Parse([
            "--workbook-id", invalidWorkbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
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

        _service.GetWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Workbook);

        var workbook = result.Workbook;
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
