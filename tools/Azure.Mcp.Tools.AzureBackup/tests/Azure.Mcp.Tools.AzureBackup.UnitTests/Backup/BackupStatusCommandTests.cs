// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Backup;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Backup;

public class BackupStatusCommandTests : CommandUnitTestsBase<BackupStatusCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("status", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProtectedStatus_WhenResourceIsProtected()
    {
        // Arrange
        var expectedDatasourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1";
        var expected = new BackupStatusResult(
            expectedDatasourceId,
            "Protected",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1",
            "DefaultPolicy",
            null,
            null,
            null);

        Service.GetBackupStatusAsync(
            Arg.Is(expectedDatasourceId),
            Arg.Is("sub1"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--datasource-id", expectedDatasourceId,
            "--location", "eastus");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.BackupStatusCommandResult);

        Assert.Equal("Protected", result.Status.ProtectionStatus);
        Assert.NotNull(result.Status.VaultId);
        Assert.Equal("DefaultPolicy", result.Status.PolicyName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotProtectedStatus_WhenResourceIsNotProtected()
    {
        // Arrange
        var expectedDatasourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm2";
        var expected = new BackupStatusResult(
            expectedDatasourceId,
            "NotProtected",
            null,
            null,
            null,
            null,
            null);

        Service.GetBackupStatusAsync(
            Arg.Is(expectedDatasourceId),
            Arg.Is("sub1"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--datasource-id", expectedDatasourceId,
            "--location", "eastus");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.BackupStatusCommandResult);

        Assert.Equal("NotProtected", result.Status.ProtectionStatus);
        Assert.Null(result.Status.VaultId);
        Assert.Null(result.Status.PolicyName);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        Service.GetBackupStatusAsync(
            Arg.Is("ds1"),
            Arg.Is("sub1"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--datasource-id", "ds1",
            "--location", "eastus");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.GetBackupStatusAsync(
            Arg.Is("ds1"),
            Arg.Is("sub1"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--datasource-id", "ds1",
            "--location", "eastus");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        Service.GetBackupStatusAsync(
            Arg.Is("ds1"),
            Arg.Is("sub1"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--datasource-id", "ds1",
            "--location", "eastus");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub1 --datasource-id ds1 --location eastus", true)]
    [InlineData("--subscription sub1", false)] // Missing datasource-id and location
    [InlineData("--subscription sub1 --datasource-id ds1", false)] // Missing location
    [InlineData("--subscription sub1 --location eastus", false)] // Missing datasource-id
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetBackupStatusAsync(
                Arg.Is("ds1"),
                Arg.Is("sub1"),
                Arg.Is("eastus"),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new BackupStatusResult("ds1", "Protected", "v1", "pol", null, null, null));
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
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedDatasourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1";
        var expected = new BackupStatusResult(
            expectedDatasourceId,
            "Protected",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1",
            "DefaultPolicy",
            DateTimeOffset.UtcNow.AddHours(-1),
            "Completed",
            "Healthy");

        Service.GetBackupStatusAsync(
            Arg.Is(expectedDatasourceId),
            Arg.Is("sub1"),
            Arg.Is("eastus"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--datasource-id", expectedDatasourceId,
            "--location", "eastus");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.BackupStatusCommandResult);

        Assert.Equal("Protected", result.Status.ProtectionStatus);
        Assert.Equal("DefaultPolicy", result.Status.PolicyName);
        Assert.Equal("Completed", result.Status.LastBackupStatus);
        Assert.Equal("Healthy", result.Status.HealthStatus);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Act - use reflection or test via ExecuteAsync
        // The binding is tested implicitly through ExecuteAsync tests above
        // Verify the command has the right options registered
        var command = Command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--datasource-id");
        Assert.Contains(options, o => o.Name == "--location");
        Assert.Contains(options, o => o.Name == "--subscription");
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParametersToService()
    {
        // Arrange
        var expectedDatasourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1";
        var expectedSubscription = "sub1";
        var expectedLocation = "eastus";

        Service.GetBackupStatusAsync(
            Arg.Is(expectedDatasourceId),
            Arg.Is(expectedSubscription),
            Arg.Is(expectedLocation),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new BackupStatusResult(expectedDatasourceId, "Protected", null, null, null, null, null));

        // Act
        await ExecuteCommandAsync(
            "--subscription", expectedSubscription,
            "--datasource-id", expectedDatasourceId,
            "--location", expectedLocation);

        // Assert
        await Service.Received(1).GetBackupStatusAsync(
            expectedDatasourceId,
            expectedSubscription,
            expectedLocation,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
