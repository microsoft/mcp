// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Account;

public class AccountGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<AccountGetCommand> _logger;
    private readonly AccountGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountGetCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AccountGetCommand>>();

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
        var expectedAccounts = new List<Models.AccountInfo>
        {
            new("account1", "eastus", "StorageV2", "Standard_LRS", "Standard", true, true, true),
            new("account2", "westus", "StorageV2", "Standard_GRS", "Standard", false, false, true)
        };

        _storageService.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedAccounts));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.AccountGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Accounts);
        Assert.Equal(expectedAccounts.Count, result.Accounts.Count);
        Assert.Equal(expectedAccounts.Select(a => a.Name), result.Accounts.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccounts()
    {
        // Arrange
        var subscription = "sub123";

        _storageService.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.AccountGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Accounts);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _storageService.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
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
    [InlineData("--account mystorageaccount --subscription sub123", true)]
    [InlineData("--subscription sub123 --account mystorageaccount", true)]
    [InlineData("--subscription sub123", true)] // Account is optional
    [InlineData("--account mystorageaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var originalSubscriptionEnv = EnvironmentHelpers.GetAzureSubscriptionId();

        try
        {
            // Clear environment variable for failing test cases to ensure proper validation
            if (!shouldSucceed && !args.Contains("--subscription"))
            {
                EnvironmentHelpers.SetAzureSubscriptionId(null);
            }

            if (shouldSucceed)
            {
                var expectedAccount = new List<Models.AccountInfo> {
                    new ("mystorageaccount", "eastus", "StorageV2", "Standard_LRS", "Standard", true, true, true)
                };

                _storageService.GetAccountDetails(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                    .Returns(Task.FromResult(expectedAccount));
            }

            var parseResult = _commandDefinition.Parse(args);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult);

            // Assert
            Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
        finally
        {
            // Restore original environment variable
            EnvironmentHelpers.SetAzureSubscriptionId(originalSubscriptionEnv);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccountDetails_WhenAccountExists()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var expectedAccount = new List<Models.AccountInfo> {
            new (account, "eastus", "StorageV2", "Standard_LRS", "Standard", true, true, true)
        };

        _storageService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedAccount));

        var args = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.AccountGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Accounts);

        Assert.Equal(expectedAccount[0].Name, result.Accounts[0].Name);
        Assert.Equal(expectedAccount[0].Location, result.Accounts[0].Location);
        Assert.Equal(expectedAccount[0].Kind, result.Accounts[0].Kind);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";

        _storageService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var account = "nonexistentaccount";
        var subscription = "sub123";

        _storageService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new RequestFailedException(404, "Storage account not found"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("Storage account not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";

        _storageService.GetAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new RequestFailedException(403, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
