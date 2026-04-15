// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.Account;

public class AccountListCommandTests : CommandUnitTestsBase<AccountListCommand, IAppConfigService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsAccounts_WhenAccountsExist()
    {
        // Arrange
        var expectedAccounts = new ResourceQueryResults<AppConfigurationAccount>(
        [
            new() { Name = "account1", Location = "East US", Endpoint = "https://account1.azconfig.io" },
            new() { Name = "account2", Location = "West US", Endpoint = "https://account2.azconfig.io" }
        ], false);
        Service.GetAppConfigAccounts(
            "sub123",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.AccountListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Accounts.Count);
        Assert.Equal("account1", result.Accounts[0].Name);
        Assert.Equal("account2", result.Accounts[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccountsExist()
    {
        // Arrange
        Service.GetAppConfigAccounts(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<AppConfigurationAccount>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppConfigJsonContext.Default.AccountListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Accounts);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        Service.GetAppConfigAccounts(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenSubscriptionIsMissing()
    {
        // Arrange && Act
        var response = await ExecuteCommandAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns503_WhenServiceIsUnavailable()
    {
        // Arrange
        Service.GetAppConfigAccounts(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("Service Unavailable", response.Message);
    }
}
