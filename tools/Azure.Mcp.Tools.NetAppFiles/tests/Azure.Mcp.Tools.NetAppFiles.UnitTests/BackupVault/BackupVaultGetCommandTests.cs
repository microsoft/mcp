// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.BackupVault;

public class BackupVaultGetCommandTests : SubscriptionCommandUnitTestsBase<BackupVaultGetCommand, INetAppFilesService>
{
    [Fact]
    public async Task ExecuteAsync_NoBackupVaultParameter_ReturnsAllBackupVaults()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVaults = new ResourceQueryResults<BackupVaultInfo>(
        [
            new("account1/vault1", "eastus", "rg1", "Succeeded"),
            new("account1/vault2", "westus", "rg2", "Succeeded")
        ], false);

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVaults));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupVaultGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.BackupVaults);
        Assert.Equal(expectedVaults.Results.Count, result.BackupVaults.Count);
        Assert.Equal(expectedVaults.Results.Select(v => v.Name), result.BackupVaults.Select(v => v.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBackupVaults()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<BackupVaultInfo>([], false));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupVaultGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.BackupVaults);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --resource-group rg1", true)]
    [InlineData("--subscription sub123 --ids /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.NetApp/netAppAccounts/account1/backupVaults/vault1", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --backupVault myvault", true)]
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVaults = new ResourceQueryResults<BackupVaultInfo>(
                [new("account1/vault1", "eastus", "rg1", "Succeeded")],
                false);

            Service.GetBackupVaultDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedVaults));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBackupVaultDetails_WhenVaultExists()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var subscription = "sub123";
        var expectedVaults = new ResourceQueryResults<BackupVaultInfo>(
            [new($"{account}/{backupVault}", "eastus", "rg1", "Succeeded")],
            false);

        Service.GetBackupVaultDetails(
            Arg.Is(account), Arg.Is(backupVault), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVaults));

        // Act
        var response = await ExecuteCommandAsync(["--account", account, "--backupVault", backupVault, "--subscription", subscription]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupVaultGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.BackupVaults);
        Assert.Equal($"{account}/{backupVault}", result.BackupVaults[0].Name);
        Assert.Equal("eastus", result.BackupVaults[0].Location);
        Assert.Equal("rg1", result.BackupVaults[0].ResourceGroup);
        Assert.Equal("Succeeded", result.BackupVaults[0].ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var backupVault = "nonexistentvault";

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(), Arg.Is(backupVault), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup vault not found"));

        var response = await ExecuteCommandAsync(["--backupVault", backupVault, "--subscription", subscription]);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Backup vault not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVaults = new ResourceQueryResults<BackupVaultInfo>(
            [new("account1/vault1", "eastus", "rg1", "Succeeded")],
            false);

        Service.GetBackupVaultDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVaults));

        // Act
        var response = await ExecuteCommandAsync(["--subscription", subscription]);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupVaultGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.BackupVaults);
        var vaultInfo = result.BackupVaults[0];
        Assert.Equal("account1/vault1", vaultInfo.Name);
        Assert.Equal("eastus", vaultInfo.Location);
        Assert.Equal("rg1", vaultInfo.ResourceGroup);
        Assert.Equal("Succeeded", vaultInfo.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_PassesResourceGroupAndIdsToService()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var ids = new[] { "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.NetApp/netAppAccounts/account1/backupVaults/vault1" };
        var expectedVaults = new ResourceQueryResults<BackupVaultInfo>(
            [new("account1/vault1", "eastus", "rg1", "Succeeded")],
            false);

        Service.GetBackupVaultDetails(
            Arg.Is<string?>(v => v == null),
            Arg.Is<string?>(v => v == null),
            Arg.Is(resourceGroup),
            Arg.Is<IReadOnlyList<string>?>(v => v != null && v.Count == 1 && v[0] == ids[0]),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVaults));

        var response = await ExecuteCommandAsync(["--subscription", subscription, "--resource-group", resourceGroup, "--ids", ids[0]]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).GetBackupVaultDetails(
            Arg.Is<string?>(v => v == null),
            Arg.Is<string?>(v => v == null),
            Arg.Is(resourceGroup),
            Arg.Is<IReadOnlyList<string>?>(v => v != null && v.Count == 1 && v[0] == ids[0]),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
