// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Account;

public class AccountCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<AccountCreateCommand> _logger;
    private readonly AccountCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountCreateCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AccountCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account testaccount --resource-group testrg --location eastus --subscription sub123", true)]
    [InlineData("--account testaccount --resource-group testrg --location eastus --sku Standard_GRS --subscription sub123", true)]
    [InlineData("--account testaccount --resource-group testrg --location eastus --kind StorageV2 --access-tier Cool --subscription sub123", true)]
    [InlineData("--resource-group testrg --location eastus --subscription sub123", false)] // Missing account name
    [InlineData("--account testaccount --location eastus --subscription sub123", false)] // Missing resource group
    [InlineData("--account testaccount --resource-group testrg --subscription sub123", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedAccount = new StorageAccountInfo(
                "testaccount",
                "eastus",
                "StorageV2",
                "Standard_LRS",
                "Standard",
                false,
                false,
                true);

            _storageService.CreateStorageAccount(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool?>(),
                Arg.Any<bool?>(),
                Arg.Any<bool?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(expectedAccount);
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

            var json = JsonSerializer.Serialize(response.Results);
            var result = JsonSerializer.Deserialize<AccountCreateResult>(json);
            Assert.NotNull(result);
            Assert.NotNull(result!.Account);
            Assert.Equal("testaccount", result.Account.Name);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesStorageAccountNameAlreadyExists()
    {
        // Arrange
        var conflictException = new RequestFailedException(409, "Storage account name already exists");

        _storageService.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(conflictException);

        var parseResult = _commandDefinition.Parse(["--account", "existingaccount", "--resource-group", "testrg", "--location", "eastus", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(409, response.Status);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesResourceGroupNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException(404, "Resource group not found");

        _storageService.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--resource-group", "nonexistentrg", "--location", "eastus", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var authException = new RequestFailedException(403, "Authorization failed");

        _storageService.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(authException);

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--resource-group", "testrg", "--location", "eastus", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _storageService.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<StorageAccountInfo>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--resource-group", "testrg", "--location", "eastus", "--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedAccount = new StorageAccountInfo(
            "testaccount",
            "eastus",
            "StorageV2",
            "Standard_GRS",
            "Standard",
            true,
            false,
            true);

        _storageService.CreateStorageAccount(
            "testaccount",
            "testrg",
            "eastus",
            "sub123",
            "Standard_GRS",
            "StorageV2",
            "Cool",
            true,
            false,
            true,
            null,
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedAccount);

        var parseResult = _commandDefinition.Parse([
            "--account", "testaccount",
            "--resource-group", "testrg",
            "--location", "eastus",
            "--subscription", "sub123",
            "--sku", "Standard_GRS",
            "--kind", "StorageV2",
            "--access-tier", "Cool",
            "--enable-https-traffic-only", "true",
            "--allow-blob-public-access", "false",
            "--enable-hierarchical-namespace", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        await _storageService.Received(1).CreateStorageAccount(
            "testaccount",
            "testrg",
            "eastus",
            "sub123",
            "Standard_GRS",
            "StorageV2",
            "Cool",
            true,
            false,
            true,
            null,
            Arg.Any<RetryPolicyOptions>());
    }

    private class AccountCreateResult
    {
        [JsonPropertyName("account")]
        public StorageAccountInfo Account { get; set; } = null!;
    }
}
