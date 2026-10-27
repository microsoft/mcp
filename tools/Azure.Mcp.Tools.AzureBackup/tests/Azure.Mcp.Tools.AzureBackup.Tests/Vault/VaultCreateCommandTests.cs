// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Vault;

public class VaultCreateCommandTests : SubscriptionCommandUnitTestsBase<VaultCreateCommand, IAzureBackupService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--enable-public-network-access true", true)]
    public async Task ExecuteAsync_PublicAccessRequiresOptIn(string options, bool publicAccess)
    {
        Service.CreateVaultAsync(
            "myVault", "rg", "sub", "rsv", "eastus",
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), publicAccess, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new VaultCreateResult("id1", "myVault", "rsv", "eastus", "Succeeded"));

        var response = await ExecuteCommandAsync($"--subscription sub --vault myVault --resource-group rg --vault-type rsv --location eastus {options}");
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(publicAccess, Assert.Single(Service.ReceivedCalls()).GetArguments()[8]);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsPublicAccessForUnsupportedVaultType()
    {
        var response = await ExecuteCommandAsync("--subscription sub --vault myVault --resource-group rg --vault-type dpp --location eastus --enable-public-network-access true");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("rsv")]
    [InlineData("dpp")]
    public async Task ExecuteAsync_ExistingVault_ReturnsConflictWithUpdateGuidance(string vaultType)
    {
        const string serviceMessage =
            "The vault already exists. Use 'azurebackup_vault_update' ('azurebackup vault update') to modify it, " +
            "or choose a different name for a new vault.";
        Service.CreateVaultAsync(
            "v", "rg", "sub", vaultType, "eastus",
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, serviceMessage));

        var response = await ExecuteCommandAsync(
            $"--subscription sub --vault v --resource-group rg --vault-type {vaultType} --location eastus");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains(serviceMessage, response.Message);
    }

    [Theory]
    [InlineData(409, HttpStatusCode.Conflict)]
    [InlineData(403, HttpStatusCode.Forbidden)]
    [InlineData(400, HttpStatusCode.BadRequest)]
    public async Task ExecuteAsync_StorageRedundancyFailsAfterCreate_PreservesPartialSuccessGuidance(int status, HttpStatusCode expectedStatus)
    {
        const string serviceMessage =
            "Vault 'v' was created, but setting storage redundancy to 'LocallyRedundant' failed. " +
            "Before configuring backup, retry with 'azurebackup vault update --subscription \"sub\" --resource-group \"rg\" " +
            "--vault \"v\" --vault-type rsv --redundancy LocallyRedundant'. Details: Storage config update failed.";
        Service.CreateVaultAsync(
            "v", "rg", "sub", "rsv", "eastus",
            Arg.Any<string?>(), "LocallyRedundant", Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status, serviceMessage, "BMSUserErrorStorageConfig", null));

        var response = await ExecuteCommandAsync(
            "--subscription sub --vault v --resource-group rg --vault-type rsv --location eastus --storage-type LocallyRedundant");

        Assert.Equal(expectedStatus, response.Status);
        Assert.Contains(serviceMessage, response.Message);
        Assert.DoesNotContain("choose a different name", response.Message);
    }

    [Theory]
    [InlineData("rsv", "ReadAccessGeoZoneRedundant")]
    [InlineData("rsv", "Invalid")]
    [InlineData("dpp", "Invalid")]
    public async Task ExecuteAsync_InvalidStorageType_ReturnsBadRequestWithoutCallingService(string vaultType, string storageType)
    {
        var response = await ExecuteCommandAsync(
            $"--subscription sub --vault v --resource-group rg --vault-type {vaultType} --location eastus --storage-type {storageType}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--storage-type must be 'GeoRedundant', 'LocallyRedundant', or 'ZoneRedundant'.", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesVault_Successfully()
    {
        // Arrange
        Service.CreateVaultAsync(
            Arg.Is("myVault"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("rsv"), Arg.Is("eastus"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new VaultCreateResult("id1", "myVault", "rsv", "eastus", "Succeeded"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "myVault",
            "--resource-group", "rg",
            "--vault-type", "rsv",
            "--location", "eastus");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.VaultCreateCommandResult);

        Assert.Equal("myVault", result.Vault.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.CreateVaultAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("rsv"), Arg.Is("eastus"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", "rsv",
            "--location", "eastus");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --vault-type rsv --location eastus", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing vault-type and location
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateVaultAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("rsv"), Arg.Is("eastus"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), cancellationToken: Arg.Any<CancellationToken>())
                .Returns(new VaultCreateResult("id", "v", "rsv", "eastus", "Succeeded"));
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
        var options = CommandDefinition.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--location");
        Assert.Contains(options, o => o.Name == "--sku");
        Assert.Contains(options, o => o.Name == "--storage-type");
    }
}
