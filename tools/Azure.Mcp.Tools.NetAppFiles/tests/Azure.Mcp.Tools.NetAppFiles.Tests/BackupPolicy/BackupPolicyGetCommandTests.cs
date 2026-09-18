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

public class BackupPolicyGetCommandTests : SubscriptionCommandUnitTestsBase<BackupPolicyGetCommand, INetAppFilesBackupPolicyService>
{
    private static readonly NetAppFilesBackupPolicy BackupPolicy = new(
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

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_GetsBackupPolicy()
    {
        Service.GetBackupPolicyAsync(
            "account1",
            "policy1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(BackupPolicy);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.BackupPolicyGetResult);
        Assert.Equal(BackupPolicy, result.BackupPolicy);
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
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-64", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--backup-policy policy1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --subscription sub")]
    [InlineData("--account account1 --backup-policy policy1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ReturnsSafeMessage()
    {
        Service.GetBackupPolicyAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal backup policy metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--backup-policy", "policy1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("was not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
