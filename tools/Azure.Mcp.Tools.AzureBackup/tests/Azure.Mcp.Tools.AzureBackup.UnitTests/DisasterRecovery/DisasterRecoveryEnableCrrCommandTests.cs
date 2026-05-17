// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.DisasterRecovery;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.DisasterRecovery;

public class DisasterRecoveryEnableCrrCommandTests : CommandUnitTestsBase<DisasterRecoveryEnableCrrCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("enable-crr", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_EnablesCrr_Successfully()
    {
        // Arrange
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Is("v"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Cross-region restore enabled"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.DisasterRecoveryEnableCrrCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Is("v"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Is("v"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Vault not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestForNonGrsVault()
    {
        // Arrange
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Is("v"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(400, "CRR not supported"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Bad request enabling Cross-Region Restore", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictWhenAlreadyEnabled()
    {
        // Arrange
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Already enabled"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already enabled", response.Message);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    [InlineData("backup")]
    [InlineData("recovery")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", vaultType);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--vault-type must be", response.Message);
    }

    [Theory]
    [InlineData("rsv")]
    [InlineData("dpp")]
    [InlineData("RSV")]
    [InlineData("DPP")]
    public async Task ExecuteAsync_AcceptsValidVaultType(string vaultType)
    {
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(vaultType),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", vaultType);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ConfigureCrossRegionRestoreAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new OperationResult("Succeeded", null, null));
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
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var options = Command.GetCommand().Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Cross-region restore enabled"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.DisasterRecoveryEnableCrrCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("Cross-region restore enabled", result.Result.Message);
    }
}
