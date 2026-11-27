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

public class AccountUpdateCommandTests : SubscriptionCommandUnitTestsBase<AccountUpdateCommand, INetAppFilesAccountService>
{
    private static readonly NetAppFilesAccount UpdatedAccount = new(
        "account1",
        "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.NetApp/netAppAccounts/account1",
        "eastus",
        null,
        "Succeeded");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTags_ClearsTags()
    {
        Service.UpdateAccountAsync(
            "account1",
            "rg",
            "sub",
            Arg.Is<IReadOnlyDictionary<string, string>>(tags => tags.Count == 0),
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdatedAccount);

        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", "{}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("{invalid-json}")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("{\"environment\":1}")]
    public async Task ExecuteAsync_InvalidTags_ReturnsBadRequest(string tags)
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--resource-group", "rg",
            "--subscription", "sub",
            "--tags", tags);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--tags", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_NoUpdateProperty_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--account", "account1",
            "--resource-group", "rg",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("The --tags property must be provided.", response.Message);
        Assert.Empty(Service.ReceivedCalls());
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
            "--subscription", "sub",
            "--tags", "{}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--account", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--resource-group rg --subscription sub --tags {}")]
    [InlineData("--account account1 --subscription sub --tags {}")]
    [InlineData("--account account1 --resource-group rg --tags {}")]
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsBadRequest(string args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
