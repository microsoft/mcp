// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Backup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.Backup;

public class BackupGetCommandTests : SubscriptionCommandUnitTestsBase<BackupGetCommand, INetAppFilesBackupService>
{
    private static readonly NetAppFilesBackup ExistingBackup = new(
        "backup1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/backupVaults/vault1/backups/backup1",
        "Succeeded",
        "Manual",
        "on-demand-backup",
        1024,
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/capacityPools/pool1/volumes/volume1");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsBackup()
    {
        Service.GetBackupAsync(
            "account1",
            "vault1",
            "backup1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(ExistingBackup);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--backup", "backup1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupGetResult);
        Assert.Equal(ExistingBackup, result.Backup);
    }

    [Theory]
    [InlineData("--account account/name --backup-vault vault1 --backup backup1 --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --backup-vault vault.name --backup backup1 --resource-group rg --subscription sub", "--backup-vault")]
    [InlineData("--account account1 --backup-vault vault1 --backup .backup --resource-group rg --subscription sub", "--backup")]
    public async Task ExecuteAsync_InvalidOption_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("backup-vault", 65)]
    [InlineData("backup", 257)]
    public async Task ExecuteAsync_OversizedName_ReturnsBadRequest(string option, int length)
    {
        var backupVault = option == "backup-vault" ? new string('a', length) : "vault1";
        var backup = option == "backup" ? new string('a', length) : "backup1";

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", backupVault,
            "--backup", backup,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"--{option}", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-vault vault1 --backup backup1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup backup1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --backup backup1 --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --backup backup1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_BackupNotFound_ReturnsSafeMessage()
    {
        Service.GetBackupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal backup metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--backup", "backup1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("backup was not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
