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

public class BackupVaultUpdateCommandTests : SubscriptionCommandUnitTestsBase<BackupVaultUpdateCommand, INetAppFilesBackupVaultService>
{
    private static readonly NetAppFilesBackupVault UpdatedBackupVault = new(
        "vault1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/backupVaults/vault1",
        "eastus",
        "Succeeded");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesBackupVaultTags()
    {
        Service.UpdateBackupVaultAsync(
            "account1",
            "vault1",
            Arg.Is<IDictionary<string, string>>(tags => tags.Count == 1 && tags["environment"] == "test"),
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(UpdatedBackupVault);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--tags", "{\"environment\":\"test\"}",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupVaultUpdateResult);
        Assert.Equal(UpdatedBackupVault, result.BackupVault);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTags_UpdatesWithEmptyDictionary()
    {
        Service.UpdateBackupVaultAsync(
            "account1",
            "vault1",
            Arg.Is<IDictionary<string, string>>(tags => tags.Count == 0),
            "rg",
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedBackupVault);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-vault", "vault1",
            "--tags", "{}",
            "--resource-group", "rg",
            "--subscription", "sub");

        ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupVaultUpdateResult);
    }

    [Theory]
    [InlineData("--account _account --backup-vault vault1 --tags {} --resource-group rg --subscription sub", "--account")]
    [InlineData("--account account1 --backup-vault _vault --tags {} --resource-group rg --subscription sub", "--backup-vault")]
    [InlineData("--account account1 --backup-vault vault/name --tags {} --resource-group rg --subscription sub", "--backup-vault")]
    [InlineData("--account account1 --backup-vault vault1 --tags [] --resource-group rg --subscription sub", "--tags")]
    [InlineData("--account account1 --backup-vault vault1 --tags {invalid} --resource-group rg --subscription sub", "--tags")]
    [InlineData("--account account1 --backup-vault vault1 --tags {\"key\":1} --resource-group rg --subscription sub", "--tags")]
    public async Task ExecuteAsync_InvalidOption_ReturnsBadRequest(string args, string expectedMessage)
    {
        var response = await ExecuteCommandAsync(args);

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
            "--tags", "{}",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-64", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-vault vault1 --tags {} --resource-group rg --subscription sub")]
    [InlineData("--account account1 --tags {} --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --tags {} --subscription sub")]
    [InlineData("--account account1 --backup-vault vault1 --tags {} --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.UpdateBackupVaultAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, string>>(),
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
            "--tags", "{}",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
