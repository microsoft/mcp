// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class AccountListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<AccountListCommand> _logger;

    public AccountListCommandTests()
    {
        _cosmosService = Substitute.For<ICosmosService>();
        _logger = Substitute.For<ILogger<AccountListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_cosmosService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccounts_WhenAccountsExist()
    {
        // Arrange
        var expectedAccounts = new List<string> { "account1", "account2" };
        _cosmosService.GetCosmosAccounts(Arg.Is("sub123"), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedAccounts);

        var command = new AccountListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<AccountListResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedAccounts, result.Accounts);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoAccounts()
    {
        // Arrange
        _cosmosService.GetCosmosAccounts("sub123", null, null)
            .Returns([]);

        var command = new AccountListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "sub123";

        _cosmosService.GetCosmosAccounts(subscriptionId, null, Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new AccountListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class AccountListResult
    {
        [JsonPropertyName("accounts")]
        public List<string> Accounts { get; set; } = [];
    }
}
