// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests.Account;

public class AccountGetCommandTests : SubscriptionCommandUnitTestsBase<AccountGetCommand, INetAppFilesAccountService>
{
    private static readonly NetAppFilesAccount ExistingAccount = new(
        "account1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1",
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
    public async Task ExecuteAsync_GetsAccount()
    {
        Service.GetAccountAsync(
            "account1",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(ExistingAccount);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.AccountGetResult);
        Assert.Equal(ExistingAccount, result.Account);
    }

    [Theory]
    [InlineData("_account")]
    [InlineData("-account")]
    [InlineData("account/name")]
    [InlineData("account.name")]
    public async Task ExecuteAsync_InvalidAccountName_ReturnsBadRequest(string account)
    {
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--account", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_OversizedAccountName_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", new string('a', 129),
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-128", response.Message);
    }

    [Theory]
    [InlineData("--resource-group rg --subscription sub")]
    [InlineData("--account account1 --subscription sub")]
    [InlineData("--account account1 --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AccountNotFound_ReturnsSafeMessage()
    {
        Service.GetAccountAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "backend payload containing internal account metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("account not found", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}