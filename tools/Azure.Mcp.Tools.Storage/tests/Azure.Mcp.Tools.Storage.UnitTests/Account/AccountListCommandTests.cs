// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Account;

public class AccountListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<AccountListCommand> _logger;
    private readonly AccountListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountListCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AccountListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Arrange
        var subscription = "sub123";
        var expectedAccounts = new List<Models.StorageAccountInfo>
        {
            new("account1", "eastus", "StorageV2", "Standard_LRS", "Standard", true, true, true),
            new("account2", "westus", "StorageV2", "Standard_GRS", "Standard", false, false, true)
        };

        _storageService.GetStorageAccounts(Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedAccounts));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<AccountListResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result!.Accounts);
        Assert.Equal(expectedAccounts.Count, result.Accounts!.Count);
        Assert.Equal(expectedAccounts.Select(a => a.Name), result.Accounts.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoAccounts()
    {
        // Arrange
        var subscription = "sub123";

        _storageService.GetStorageAccounts(Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(new List<Models.StorageAccountInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _storageService.GetStorageAccounts(Arg.Is(subscription), null, Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class AccountListResult
    {
        [JsonPropertyName("accounts")]
        public List<Models.StorageAccountInfo>? Accounts { get; set; }
    }
}
