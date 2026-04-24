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

public class DeleteWorkbooksCommandTests : CommandUnitTestsBase<DeleteWorkbooksCommand, IWorkbooksService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("delete", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Delete Workbook", Command.Title);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenWorkbookDeletedSuccessfully()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

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

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId1, "--workbook-ids", workbookId2);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

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

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId1, "--workbook-ids", workbookId2);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

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

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.Empty(result.Succeeded);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        Service.DeleteWorkbooksAsync(
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
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync(":--workbook-ids", workbookId, "--tenant", "test-tenant");

        // Assert
        await Service.Received(1).DeleteWorkbooksAsync(
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

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync("--workbook-ids", workbookId);

        // Assert
        await Service.Received(1).DeleteWorkbooksAsync(
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

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync("--workbook-ids", workbookId, "--auth-method", "1");

        // Assert
        await Service.Received(1).DeleteWorkbooksAsync(
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
    public async Task ExecuteAsync_WithValidResourceId_ProcessesCorrectly()
    {
        // Arrange
        var validWorkbookId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/my-rg/providers/microsoft.insights/workbooks/my-workbook-guid";

        var batchResult = new WorkbookDeleteBatchResult([validWorkbookId], []);

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        var response = await ExecuteCommandAsync("--workbook-ids", validWorkbookId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);

        Assert.Single(result.Succeeded);
        Assert.Contains(validWorkbookId, result.Succeeded);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_PassesRetryOptions()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        var batchResult = new WorkbookDeleteBatchResult([workbookId], []);

        Service.DeleteWorkbooksAsync(
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(batchResult);

        // Act
        await ExecuteCommandAsync("--workbook-ids", workbookId, "--retry-max-retries", "5", "--retry-delay", "2");

        // Assert
        await Service.Received(1).DeleteWorkbooksAsync(
            Arg.Is<IReadOnlyList<string>>(ids => ids.Contains(workbookId)),
            Arg.Is<RetryPolicyOptions?>(options =>
                options != null &&
                options.MaxRetries == 5 &&
                options.DelaySeconds == 2),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
