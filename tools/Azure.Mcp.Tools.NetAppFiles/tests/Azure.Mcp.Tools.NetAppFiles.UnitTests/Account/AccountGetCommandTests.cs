// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Account;

public class AccountGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<AccountGetCommand> _logger;
    private readonly AccountGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<AccountGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoAccountParameter_ReturnsAllAccounts()
    {
        // Arrange
        var subscription = "sub123";
        var expectedAccounts = new ResourceQueryResults<NetAppAccountInfo>(
        [
            new("anfaccount1", "eastus", "rg1", "Succeeded", null, "Microsoft.NetApp", null),
            new("anfaccount2", "westus", "rg2", "Succeeded", null, "Microsoft.NetApp", false)
        ], false);

        _netAppFilesService.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedAccounts));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Accounts);
        Assert.Equal(expectedAccounts.Results.Count, result.Accounts.Count);
        Assert.Equal(expectedAccounts.Results.Select(a => a.Name), result.Accounts.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccounts()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<NetAppAccountInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Accounts);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123", true)] // Account is optional
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedAccount = new ResourceQueryResults<NetAppAccountInfo>(
                [new("myanfaccount", "eastus", "rg1", "Succeeded", null, "Microsoft.NetApp", null)],
                false);

            _netAppFilesService.GetAccountDetails(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedAccount));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccountDetails_WhenAccountExists()
    {
        // Arrange
        var account = "myanfaccount";
        var subscription = "sub123";
        var expectedAccount = new ResourceQueryResults<NetAppAccountInfo>(
            [new(account, "eastus", "rg1", "Succeeded", "ad-id-1", "Microsoft.NetApp", false)],
            false);

        _netAppFilesService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedAccount));

        var args = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Accounts);

        Assert.Equal(expectedAccount.Results[0].Name, result.Accounts[0].Name);
        Assert.Equal(expectedAccount.Results[0].Location, result.Accounts[0].Location);
        Assert.Equal(expectedAccount.Results[0].ResourceGroup, result.Accounts[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "myanfaccount";
        var subscription = "sub123";

        _netAppFilesService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var account = "nonexistentaccount";
        var subscription = "sub123";

        _netAppFilesService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "NetApp Files account not found"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("NetApp Files account not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "myanfaccount";
        var subscription = "sub123";

        _netAppFilesService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
