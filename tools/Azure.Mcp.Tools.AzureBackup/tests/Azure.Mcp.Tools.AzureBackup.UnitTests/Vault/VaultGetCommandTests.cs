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

public class VaultGetCommandTests : CommandUnitTestsBase<VaultGetCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsVaults_WhenNoVaultSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVaults = new List<BackupVaultInfo>
        {
            new("id1", "vault1", "rsv", "eastus", "rg1", "Succeeded", "Standard", "GeoRedundant", null, null, null, null, null, null),
            new("id2", "vault2", "dpp", "westus", "rg2", "Succeeded", "Standard", "LocallyRedundant", null, null, null, null, null, null)
        };

        Service.ListVaultsAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedVaults);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.VaultGetCommandResult);

        Assert.Equal(2, result.Vaults.Count);
        Assert.Equal("vault1", result.Vaults[0].Name);
        Assert.Equal("vault2", result.Vaults[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleVault_WhenVaultSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vaultName = "myVault";
        var resourceGroup = "myRg";
        var expectedVault = new BackupVaultInfo("id1", vaultName, "rsv", "eastus", resourceGroup, "Succeeded", "Standard", "GeoRedundant", null, null, null, null, null, null);

        Service.GetVaultAsync(
            Arg.Is(vaultName),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vaultName,
            "--resource-group", resourceGroup);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.VaultGetCommandResult);

        Assert.Single(result.Vaults);
        Assert.Equal(vaultName, result.Vaults[0].Name);
        Assert.Equal("eastus", result.Vaults[0].Location);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoVaultsExist()
    {
        // Arrange
        var subscription = "sub123";

        Service.ListVaultsAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.VaultGetCommandResult);

        Assert.Empty(result.Vaults);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var subscription = "sub123";

        Service.ListVaultsAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var vaultName = "nonexistent";
        var resourceGroup = "myRg";

        Service.GetVaultAsync(
            Arg.Is(vaultName),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Vault not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vaultName,
            "--resource-group", resourceGroup);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    [InlineData("backup")]
    [InlineData("recovery")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--vault-type", vaultType);
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
        Service.ListVaultsAsync(
            Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--vault-type", vaultType);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --vault myVault --resource-group myRg", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListVaultsAsync(
                Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);

            Service.GetVaultAsync(
                Arg.Is("myVault"), Arg.Is("myRg"), Arg.Is("sub123"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new BackupVaultInfo("id1", "myVault", "rsv", "eastus", "myRg", "Succeeded", "Standard", "GeoRedundant", null, null, null, null, null, null));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
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
        var options = CommandDefinition.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
    }
}
