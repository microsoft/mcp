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

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        _service.DeleteWorkbooksAsync(
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
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Succeeded);
        Assert.Contains(workbookId, result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBatchResults_WhenMultipleWorkbooksDeleted()
    {
        // Arrange
        var workbookId1 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var workbookId2 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook2";

        var batchResult = new WorkbookDeleteBatchResult([workbookId1, workbookId2], []);

        _service.DeleteWorkbooksAsync(
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
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Succeeded.Count);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPartialResults_WhenSomeDeletionsFail()
    {
        // Arrange
        var workbookId1 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var workbookId2 = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook2";

        var error = new WorkbookError(workbookId2, 404, "Resource not found");
        var batchResult = new WorkbookDeleteBatchResult([workbookId1], [error]);

        _service.DeleteWorkbooksAsync(
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
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Succeeded);
        Assert.Contains(workbookId1, result.Succeeded);
        Assert.Single(result.Errors);
        Assert.Equal(workbookId2, result.Errors[0].WorkbookId);
        Assert.Equal(404, result.Errors[0].StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenAllDeletionsFail()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        var error = new WorkbookError(workbookId, 403, "Access denied");
        var batchResult = new WorkbookDeleteBatchResult([], [error]);

        _service.DeleteWorkbooksAsync(
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
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Succeeded);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkbookDeleteBatchResult>(new Exception("Service error")));

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
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        _service.DeleteWorkbooksAsync(
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
        await _service.Received(1).DeleteWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is("test-tenant"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        _service.DeleteWorkbooksAsync(
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
        await _service.Received(1).DeleteWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is<string?>(t => t == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        _service.DeleteWorkbooksAsync(
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
        await _service.Received(1).DeleteWorkbooksAsync(
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
    public async Task ExecuteAsync_WithValidResourceId_ProcessesCorrectly()
    {
        // Arrange
        var validWorkbookId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/my-rg/providers/microsoft.insights/workbooks/my-workbook-guid";

        var batchResult = new WorkbookDeleteBatchResult([validWorkbookId], []);

        _service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", validWorkbookId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Succeeded);
        Assert.Contains(validWorkbookId, result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_PassesRetryOptions()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        _service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        var args = _command.GetCommand().Parse([
            "--workbook-ids", workbookId,
            "--retry-max-retries", "5",
            "--retry-delay", "2"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        await _service.Received(1).DeleteWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Is<RetryPolicyOptions?>(options =>
                options != null &&
                options.MaxRetries == 5 &&
                options.DelaySeconds == 2),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
