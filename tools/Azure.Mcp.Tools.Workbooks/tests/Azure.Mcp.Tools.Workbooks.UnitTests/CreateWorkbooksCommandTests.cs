// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class CreateWorkbooksCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkbooksService _service;
    private readonly ILogger<CreateWorkbooksCommand> _logger;
    private readonly CreateWorkbooksCommand _command;

    public CreateWorkbooksCommandTests()
    {
        _service = Substitute.For<IWorkbooksService>();
        _logger = Substitute.For<ILogger<CreateWorkbooksCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("workbook", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource group", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("create", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create Workbook", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.NotNull(description);
        Assert.Contains("workbook", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource group", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subscription", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("display name", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("serialized", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesWorkbook_WhenValidParametersProvided()
    {
        // Arrange
        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/test-id",
            DisplayName: "Test Workbook",
            Description: null,
            Category: "workbook",
            Location: "West US 2",
            Kind: "shared",
            Tags: null,
            SerializedData: """{"items":[{"type":"text","content":"Test content"}]}""",
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: "azure monitor"
        );

        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""");

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.NotNull(result.Results);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await _service.Received(1).CreateWorkbook(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[{"type":"text","content":"Test content"}]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesProvidedSourceId_WhenSpecified()
    {
        // Arrange
        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/test-id",
            DisplayName: "Test Workbook",
            Description: null,
            Category: null,
            Location: "West US 2",
            Kind: null,
            Tags: null,
            SerializedData: """{"items":[{"type":"text","content":"Test content"}]}""",
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: null
        );

        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""",
            "--source-id", "custom-source");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).CreateWorkbook(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[{"type":"text","content":"Test content"}]}""",
            "custom-source",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceReturnsNull()
    {
        // Arrange
        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns((WorkbookInfo?)null);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""");

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<WorkbookInfo?>(new InvalidOperationException("Service error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""");

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/test-id",
            DisplayName: null,
            Description: null,
            Category: null,
            Location: null,
            Kind: null,
            Tags: null,
            SerializedData: null,
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: null
        );

        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-subscription",
            "--resource-group", "test-resource-group",
            "--display-name", "My Test Workbook",
            "--serialized-content", """{"version": "Notebook/1.0","items": [{"type": "1","content": "Hello World"}]}""");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).CreateWorkbook(
            "test-subscription",
            "test-resource-group",
            "My Test Workbook",
            """{"version": "Notebook/1.0","items": [{"type": "1","content": "Hello World"}]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var workbook = new WorkbookInfo("test-id", null, null, null, null, null, null, null, null, null, null, null);
        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).CreateWorkbook(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbook = new WorkbookInfo("test-id", null, null, null, null, null, null, null, null, null, null, null);
        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""",
            "--auth-method", "1",
            "--tenant", "test-tenant");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).CreateWorkbook(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>(),
            "test-tenant");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidDisplayName_ReturnsValidationError(string? invalidDisplayName)
    {
        // Arrange
        var context = new CommandContext(_serviceProvider);
        var args = new List<string> { "--subscription", "test-sub", "--resource-group", "test-rg", "--serialized-content", """{"items":[]}""" };

        if (invalidDisplayName != null)
        {
            args.AddRange(["--display-name", invalidDisplayName]);
        }

        var parseResult = CreateParseResult([.. args]);

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidSerializedContent_ReturnsValidationError(string? invalidSerializedContent)
    {
        // Arrange
        var context = new CommandContext(_serviceProvider);
        var args = new List<string> { "--subscription", "test-sub", "--resource-group", "test-rg", "--display-name", "Test Workbook" };

        if (invalidSerializedContent != null)
        {
            args.AddRange(["--serialized-content", invalidSerializedContent]);
        }

        var parseResult = CreateParseResult([.. args]);

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexSerializedContent_HandlesCorrectly()
    {
        // Arrange
        var complexSerializedData = """
        {
            "version": "Notebook/1.0",
            "items": [
                {
                    "type": 1,
                    "content": "# Azure Workbook Dashboard\n\nThis is a test workbook with complex content."
                },
                {
                    "type": 3,
                    "content": {
                        "version": "KqlItem/1.0",
                        "query": "AzureActivity | summarize count() by ActivityStatus",
                        "size": 1,
                        "queryType": 0,
                        "resourceType": "microsoft.operationalinsights/workspaces"
                    }
                }
            ]
        }
        """;

        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/complex-id",
            DisplayName: "Complex Test Workbook",
            Description: null,
            Category: "workbook",
            Location: "West US 2",
            Kind: "shared",
            Tags: """{"Environment": "Test", "Team": "DevOps"}""",
            SerializedData: complexSerializedData,
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: "azure monitor"
        );

        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Complex Test Workbook",
            "--serialized-content", complexSerializedData);

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryOptions_PassesCorrectParameters()
    {
        // Arrange
        var workbook = new WorkbookInfo("test-id", null, null, null, null, null, null, null, null, null, null, null);
        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(workbook);

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""",
            "--retry-max-retries", "5",
            "--retry-delay", "2.5",
            "--retry-max-delay", "30",
            "--retry-mode", "1");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).CreateWorkbook(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[]}""",
            "azure monitor",
            Arg.Is<RetryPolicyOptions?>(opts =>
                opts != null &&
                opts.MaxRetries == 5 &&
                opts.DelaySeconds == 2.5 &&
                opts.MaxDelaySeconds == 30));
    }

    [Fact]
    public async Task ExecuteAsync_HandlesExceptionCorrectly_WhenExceptionOccurs()
    {
        // Arrange
        _service.CreateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<WorkbookInfo?>(new ArgumentException("Invalid workbook data")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = CreateParseResult("--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""");

        // Act
        var result = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(context.Response, result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
    }

    private ParseResult CreateParseResult(params string[] args)
    {
        var command = _command.GetCommand();
        return command.Parse(args);
    }
}
