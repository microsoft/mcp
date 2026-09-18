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

public class BackupPolicyUpdateCommandTests : SubscriptionCommandUnitTestsBase<BackupPolicyUpdateCommand, INetAppFilesBackupPolicyService>
{
    private static readonly NetAppFilesBackupPolicy UpdatedBackupPolicy = new(
        "policy1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1/backupPolicies/policy1",
        "eastus",
        "Succeeded",
        3,
        2,
        1,
        false);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesBackupPolicy()
    {
        Service.UpdateBackupPolicyAsync(
            "account1",
            "policy1",
            3,
            2,
            1,
            false,
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(UpdatedBackupPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--daily-backups-to-keep", "3",
            "--weekly-backups-to-keep", "2",
            "--monthly-backups-to-keep", "1",
            "--enabled", "false",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupPolicyUpdateResult);
        Assert.Equal(UpdatedBackupPolicy, result.BackupPolicy);
    }

    [Fact]
    public async Task ExecuteAsync_ExplicitFalse_PreservesOmittedValues()
    {
        Service.UpdateBackupPolicyAsync(
            "account1",
            "policy1",
            null,
            null,
            null,
            false,
            "rg",
            "sub",
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedBackupPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--enabled", "false",
            "--resource-group", "rg",
            "--subscription", "sub");

        ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupPolicyUpdateResult);
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
            "--enabled", "true",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_OversizedBackupPolicyName_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", new string('a', 65),
            "--enabled", "true",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-64", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--daily-backups-to-keep")]
    [InlineData("--weekly-backups-to-keep")]
    [InlineData("--monthly-backups-to-keep")]
    public async Task ExecuteAsync_NegativeRetention_ReturnsBadRequest(string option)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            option, "-1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("zero or greater", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_NoUpdateProperty_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-policy policy1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --enabled true --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --enabled true --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --enabled true --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.UpdateBackupPolicyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
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
            "--daily-backups-to-keep", "3",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
