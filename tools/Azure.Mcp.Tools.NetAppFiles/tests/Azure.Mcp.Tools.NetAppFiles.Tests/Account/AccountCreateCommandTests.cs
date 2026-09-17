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

public class AccountCreateCommandTests : SubscriptionCommandUnitTestsBase<AccountCreateCommand, INetAppFilesAccountService>
{
    private static readonly NetAppFilesAccount CreatedAccount = new(
        "account1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1",
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
    public async Task ExecuteAsync_CreatesAccount()
    {
        Service.CreateAccountAsync(
            "account1",
            "eastus",
            "rg",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreatedAccount);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tenant", "tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            NetAppFilesJsonContext.Default.AccountCreateResult);
        Assert.Equal(CreatedAccount, result.Account);
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
            "--location", "eastus",
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
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("1-128", response.Message);
    }

    [Theory]
    [InlineData("--location eastus --resource-group rg --subscription sub")]
    [InlineData("--account account1 --resource-group rg --subscription sub")]
    [InlineData("--account account1 --location eastus --subscription sub")]
    [InlineData("--account account1 --location eastus --resource-group rg")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceConflict_ReturnsSafeMessage()
    {
        Service.CreateAccountAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Conflict,
                "backend payload containing internal account metadata"));

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--location", "eastus",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("resource conflict", response.Message);
        Assert.DoesNotContain("backend payload", response.Message);
    }
}
