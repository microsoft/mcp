// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Azure.Mcp.Tools.AppConfig.Commands.Account.AccountListCommand;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.Account;

[Trait("Area", "AppConfig")]
public class AccountListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly ILogger<AccountListCommand> _logger;
    private readonly AccountListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountListCommandTests()
    {
        _appConfigService = Substitute.For<IAppConfigService>();
        _logger = Substitute.For<ILogger<AccountListCommand>>();

        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appConfigService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccounts_WhenAccountsExist()
    {
        // Arrange
        var expectedAccounts = new List<AppConfigurationAccount>
        {
            new() { Name = "account1", Location = "East US", Endpoint = "https://account1.azconfig.io" },
            new() { Name = "account2", Location = "West US", Endpoint = "https://account2.azconfig.io" }
        };
        _appConfigService.GetAppConfigAccounts(
            "sub123",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedAccounts);

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<AccountListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Accounts.Count);
        Assert.Equal("account1", result.Accounts[0].Name);
        Assert.Equal("account2", result.Accounts[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccountsExist()
    {
        // Arrange
        _appConfigService.GetAppConfigAccounts(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<AccountListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Empty(result.Accounts);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _appConfigService.GetAppConfigAccounts(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<AppConfigurationAccount>>(new Exception("Service error")));

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenSubscriptionIsMissing()
    {
        // Arrange && Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([]));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns503_WhenServiceIsUnavailable()
    {
        // Arrange
        _appConfigService.GetAppConfigAccounts(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new HttpRequestException("Service Unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable));

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(503, response.Status);
        Assert.Contains("Service Unavailable", response.Message);
    }
}
