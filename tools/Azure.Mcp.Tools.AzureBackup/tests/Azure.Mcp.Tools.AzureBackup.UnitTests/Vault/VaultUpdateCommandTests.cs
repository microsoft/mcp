// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Vault;

public class VaultUpdateCommandTests : CommandUnitTestsBase<VaultUpdateCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVault_Successfully()
    {
        // Arrange
        Service.UpdateVaultAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Is("SystemAssigned"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Vault updated successfully"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--identity-type", "SystemAssigned");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.VaultUpdateCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.UpdateVaultAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Is("SystemAssigned"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--identity-type", "SystemAssigned");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUpdateWithNoChanges()
    {
        // Arrange & Act - no update options provided, only required base options
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one update option must be provided", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.UpdateVaultAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Is("SystemAssigned"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--identity-type", "SystemAssigned");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Vault not found", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --identity-type SystemAssigned", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --soft-delete On", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --tags {}", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --redundancy GeoRedundant", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // No update options
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.UpdateVaultAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
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

    [Theory]
    [InlineData("SystemManaged")] // common typo
    [InlineData("Managed")]
    [InlineData("system")]
    [InlineData("invalid")]
    public async Task ExecuteAsync_RejectsInvalidIdentityType(string identityType)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--identity-type", identityType);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--identity-type", response.Message);
    }

    [Theory]
    [InlineData("SystemAssigned")]
    [InlineData("UserAssigned")]
    [InlineData("None")]
    [InlineData("SystemAssigned,UserAssigned")]
    public async Task ExecuteAsync_AcceptsValidIdentityType(string identityType)
    {
        // Arrange
        Service.UpdateVaultAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Is(identityType), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--identity-type", identityType);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("0")]   // below 14
    [InlineData("13")]  // below 14
    [InlineData("181")] // above 180
    [InlineData("abc")] // non-numeric
    public async Task ExecuteAsync_RejectsInvalidSoftDeleteRetentionDays(string retentionDays)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete-retention-days", retentionDays);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--soft-delete-retention-days", response.Message);
    }

    [Theory]
    [InlineData("14")]
    [InlineData("90")]
    [InlineData("180")]
    public async Task ExecuteAsync_AcceptsValidSoftDeleteRetentionDays(string retentionDays)
    {
        // Arrange
        Service.UpdateVaultAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--soft-delete-retention-days", retentionDays);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
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
        Assert.Contains(options, o => o.Name == "--redundancy");
        Assert.Contains(options, o => o.Name == "--soft-delete");
        Assert.Contains(options, o => o.Name == "--soft-delete-retention-days");
        Assert.Contains(options, o => o.Name == "--immutability-state");
        Assert.Contains(options, o => o.Name == "--identity-type");
        Assert.Contains(options, o => o.Name == "--tags");
    }
}
