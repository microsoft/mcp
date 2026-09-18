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

public class BackupVaultCreateCommandTests : SubscriptionCommandUnitTestsBase<BackupVaultCreateCommand, INetAppFilesBackupVaultService>
{
    private static readonly NetAppFilesBackupVault CreatedBackupVault = new(
        "vault1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/backupVaults/vault1",
        "eastus",
        "Succeeded");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesBackupVault()
    {
        Service.CreateBackupVaultAsync(
            "account1",
            "vault1",
            "eastus",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedBackupVault);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupVaultCreateResult);
        Assert.Equal(CreatedBackupVault, result.BackupVault);
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
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("vault-")]
    [InlineData("vault_")]
    public async Task ExecuteAsync_ValidTrailingCharacter_CreatesBackupVault(string backupVault)
    {
        Service.CreateBackupVaultAsync(
            "account1",
            backupVault,
            "eastus",
            "rg",
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(CreatedBackupVault);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", backupVault,
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_OversizedBackupVaultName_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", new string('a', 65),
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-64", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-vault vault1 --location eastus --resource-group rg --subscription sub")]
    [InlineData("--account account1 --location eastus --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --location eastus --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --location eastus --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateBackupVaultAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal backup vault metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
