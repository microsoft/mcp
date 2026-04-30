// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.ProtectedItem;

public class ProtectedItemUndeleteCommandTests : CommandUnitTestsBase<ProtectedItemUndeleteCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("undelete", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UndeletesItem_Successfully()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is("/subscriptions/00000000/resourceGroups/rg-prod/providers/Microsoft.Compute/virtualMachines/my-vm"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Accepted", null, "Soft-deleted protected item restored"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/00000000/resourceGroups/rg-prod/providers/Microsoft.Compute/virtualMachines/my-vm");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemUndeleteCommandResult);

        Assert.Equal("Accepted", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Accepted", "job-456", "Item restored successfully"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemUndeleteCommandResult);

        Assert.Equal("Accepted", result.Result.Status);
        Assert.Equal("job-456", result.Result.JobId);
        Assert.Equal("Item restored successfully", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --datasource-id ds1", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing datasource-id
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.UndeleteProtectedItemAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new OperationResult("Accepted", null, "Restored"));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not Found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Soft-deleted protected item not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Conflict"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("not in a soft-deleted state", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesKeyNotFoundException()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Item not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Soft-deleted protected item not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UndeletesItem_DppVault_Successfully()
    {
        // Arrange - DPP vaults also support undelete via the deleted instances API
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is("/subscriptions/00000000/resourceGroups/rg/providers/Microsoft.Compute/disks/my-disk"),
            Arg.Is("dpp"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Accepted", null, "Soft-deleted backup instance restored"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/00000000/resourceGroups/rg/providers/Microsoft.Compute/disks/my-disk",
            "--vault-type", "dpp");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemUndeleteCommandResult);

        Assert.Equal("Accepted", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        // Arrange
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid datasource ID format"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1");

        // Assert
        Assert.Contains("Invalid datasource ID format", response.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var options = CommandDefinition.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--datasource-id");
        Assert.Contains(options, o => o.Name == "--container");
    }

    [Fact]
    public async Task ExecuteAsync_PassesContainerToService()
    {
        // Arrange - verify --container flows through to UndeleteProtectedItemAsync
        Service.UndeleteProtectedItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"),
            Arg.Any<string?>(), Arg.Is("IaasVMContainer;iaasvmcontainerv2;rg;myvm"), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Accepted", null, "Restored"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "ds1",
            "--container", "IaasVMContainer;iaasvmcontainerv2;rg;myvm");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UndeleteProtectedItemAsync(
            "v", "rg", "sub", "ds1",
            Arg.Any<string?>(), "IaasVMContainer;iaasvmcontainerv2;rg;myvm", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
