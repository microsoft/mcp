// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.BackupPolicy;

public class BackupPolicyCreateCommandTests : SubscriptionCommandUnitTestsBase<BackupPolicyCreateCommand, INetAppFilesBackupPolicyService>
{
    private static readonly NetAppFilesBackupPolicy CreatedBackupPolicy = new(
        "policy1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/backupPolicies/policy1",
        "eastus",
        "Succeeded",
        2,
        1,
        1,
        true);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesBackupPolicy()
    {
        Service.CreateBackupPolicyAsync(
            "account1",
            "policy1",
            "eastus",
            2,
            1,
            1,
            true,
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedBackupPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--location", "eastus",
            "--daily-backups-to-keep", "2",
            "--weekly-backups-to-keep", "1",
            "--monthly-backups-to-keep", "1",
            "--enabled", "true",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupPolicyCreateResult);
        Assert.Equal(CreatedBackupPolicy, result.BackupPolicy);
    }

    [Theory]
    [InlineData("_account", "policy1", "--account")]
    [InlineData("account1", "_policy", "--backup-policy")]
    [InlineData("account1", "policy/name", "--backup-policy")]
    [InlineData("account1", "policy.name", "--backup-policy")]
    public async Task ExecuteAsync_InvalidName_ReturnsBadRequest(
        string account,
        string backupPolicy,
        string expectedMessage)
    {
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--backup-policy", backupPolicy,
            "--location", "eastus",
            "--daily-backups-to-keep", "2",
            "--weekly-backups-to-keep", "1",
            "--monthly-backups-to-keep", "1",
            "--enabled", "true",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(1, -1, 1)]
    [InlineData(1, 1, -1)]
    [InlineData(0, 0, 0)]
    public async Task ExecuteAsync_InvalidRetention_ReturnsBadRequest(
        int dailyBackupsToKeep,
        int weeklyBackupsToKeep,
        int monthlyBackupsToKeep)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--location", "eastus",
            "--daily-backups-to-keep", dailyBackupsToKeep.ToString(),
            "--weekly-backups-to-keep", weeklyBackupsToKeep.ToString(),
            "--monthly-backups-to-keep", monthlyBackupsToKeep.ToString(),
            "--enabled", "true",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("backup", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-policy policy1 --location eastus --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --location eastus --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --location eastus --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --location eastus --daily-backups-to-keep 2 --monthly-backups-to-keep 1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --location eastus --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --location eastus --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --location eastus --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --enabled true --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --location eastus --daily-backups-to-keep 2 --weekly-backups-to-keep 1 --monthly-backups-to-keep 1 --enabled true --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateBackupPolicyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<bool>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal backup policy metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--location", "eastus",
            "--daily-backups-to-keep", "2",
            "--weekly-backups-to-keep", "1",
            "--monthly-backups-to-keep", "1",
            "--enabled", "true",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}