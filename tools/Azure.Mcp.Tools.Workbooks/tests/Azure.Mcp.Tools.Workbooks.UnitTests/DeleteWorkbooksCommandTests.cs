// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class DeleteWorkbooksCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkbooksService _service;
    private readonly ILogger<DeleteWorkbooksCommand> _logger;
    private readonly DeleteWorkbooksCommand _command;

    public DeleteWorkbooksCommandTests()
    {
        _service = Substitute.For<IWorkbooksService>();
        _logger = Substitute.For<ILogger<DeleteWorkbooksCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("delete", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Delete Workbook", _command.Title);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenWorkbookDeletedSuccessfully()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.DeleteWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DeleteWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(workbookId, result.WorkbookId);
        Assert.Equal("Successfully deleted", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenWorkbookDeletionFails()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.DeleteWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(false);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains($"Failed to delete workbook with ID '{workbookId}'", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.DeleteWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(Task.FromException<bool>(new Exception("Service error")));

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        _service.DeleteWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--tenant", "test-tenant"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).DeleteWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Is("test-tenant"));
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        _service.DeleteWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).DeleteWorkbook(
            Arg.Is(workbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Is((string?)null));
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        _service.DeleteWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--auth-method", "1"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).DeleteWorkbook(
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
        Assert.Equal(400, response.Status);
        Assert.Contains("workbook", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingWorkbookId_ReturnsValidationError()
    {
        // Arrange - Parse without required workbook-id parameter
        var parseResult = _command.GetCommand().Parse([]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("workbook", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidResourceId_ProcessesCorrectly()
    {
        // Arrange
        var validWorkbookId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/my-rg/providers/microsoft.insights/workbooks/my-workbook-guid";

        _service.DeleteWorkbook(
            Arg.Is(validWorkbookId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", validWorkbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DeleteWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(validWorkbookId, result.WorkbookId);
        Assert.Equal("Successfully deleted", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithDisplayNameAsWorkbookId_ProcessesCorrectly()
    {
        // Arrange
        var displayName = "My Test Workbook";

        _service.DeleteWorkbook(
            Arg.Is(displayName),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", displayName
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DeleteWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(displayName, result.WorkbookId);
        Assert.Equal("Successfully deleted", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_PassesRetryOptions()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.DeleteWorkbook(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(true);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--retry-max-retries", "5",
            "--retry-delay", "2"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).DeleteWorkbook(
            Arg.Is(workbookId),
            Arg.Is<RetryPolicyOptions>(options =>
                options.MaxRetries == 5 &&
                options.DelaySeconds == 2),
            Arg.Any<string?>());
    }

    private class DeleteWorkbooksCommandResult
    {
        public string WorkbookId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
