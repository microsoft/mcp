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

public class AccountDetailsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<AccountDetailsCommand> _logger;
    private readonly AccountDetailsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountDetailsCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AccountDetailsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("details", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account mystorageaccount --subscription sub123", true)]
    [InlineData("--subscription sub123 --account mystorageaccount", true)]
    [InlineData("--subscription sub123", false)] // Missing account
    [InlineData("--account mystorageaccount", false)] // Missing subscription
    [InlineData("", false)] // Missing both
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var originalSubscriptionEnv = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        try
        {
            // Clear environment variable for failing test cases to ensure proper validation
            if (!shouldSucceed && !args.Contains("--subscription"))
            {
                Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", null);
            }

            if (shouldSucceed)
            {
                var expectedAccount = new Models.StorageAccountInfo(
                    "mystorageaccount", "eastus", "StorageV2", "Standard_LRS", "Standard", true, true, true);

                _storageService.GetStorageAccountDetails(
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
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", originalSubscriptionEnv);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccountDetails_WhenAccountExists()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var expectedAccount = new Models.StorageAccountInfo(
            account, "eastus", "StorageV2", "Standard_LRS", "Standard", true, true, true);

        _storageService.GetStorageAccountDetails(
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
        var result = JsonSerializer.Deserialize<AccountDetailsResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result!.Account);
        Assert.Equal(expectedAccount.Name, result.Account.Name);
        Assert.Equal(expectedAccount.Location, result.Account.Location);
        Assert.Equal(expectedAccount.Kind, result.Account.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";

        _storageService.GetStorageAccountDetails(
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

        _storageService.GetStorageAccountDetails(
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

        _storageService.GetStorageAccountDetails(
            Arg.Is(account), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new RequestFailedException(403, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Authorization failed accessing the storage account", response.Message);
    }

    private class AccountDetailsResult
    {
        [JsonPropertyName("account")]
        public Models.StorageAccountInfo Account { get; set; } = null!;
    }
}
