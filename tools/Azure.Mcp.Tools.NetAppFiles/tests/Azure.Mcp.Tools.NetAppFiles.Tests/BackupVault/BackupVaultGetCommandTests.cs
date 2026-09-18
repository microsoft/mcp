// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.BackupVault;

public class BackupVaultGetCommandTests : SubscriptionCommandUnitTestsBase<BackupVaultGetCommand, INetAppFilesBackupVaultService>
{
    private static readonly NetAppFilesBackupVault BackupVault = new(
        "vault1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/backupVaults/vault1",
        "eastus",
        "Succeeded");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsBackupVault()
    {
        Service.GetBackupVaultAsync(
            "account1",
            "vault1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(BackupVault);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupVaultGetResult);
        Assert.Equal(BackupVault, result.BackupVault);
    }

    [Theory]
    [InlineData("_account", "vault1", "--account")]
    [InlineData("account1", "_vault", "--backup-vault")]
    [InlineData("account1", "vault/name", "--backup-vault")]
    [InlineData("account1", "vault.name", "--backup-vault")]
    public async Task ExecuteAsync_InvalidName_ReturnsBadRequest(
        string account,
        string backupVault,
        string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--backup-vault", backupVault,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_OversizedBackupVaultName_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", new string('a', 65),
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-64", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-vault vault1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ReturnsSafeMessage()
    {
        Service.GetBackupVaultAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal backup vault metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("was not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
