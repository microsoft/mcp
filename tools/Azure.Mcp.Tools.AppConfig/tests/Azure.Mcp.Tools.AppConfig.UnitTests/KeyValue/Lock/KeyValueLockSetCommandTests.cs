// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue.Lock;

[Trait("Area", "AppConfig")]
public class KeyValueLockSetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly ILogger<KeyValueLockSetCommand> _logger;
    private readonly KeyValueLockSetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public KeyValueLockSetCommandTests()
    {
        _appConfigService = Substitute.For<IAppConfigService>();
        _logger = Substitute.For<ILogger<KeyValueLockSetCommand>>();

        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appConfigService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_LocksKeyValue_WhenValidParametersProvided()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--lock"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _appConfigService.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            true,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.True(result.Locked);
    }

    [Fact]
    public async Task ExecuteAsync_LocksKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod",
            "--lock"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _appConfigService.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            true,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            "prod");

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
        Assert.True(result.Locked);
    }

    [Fact]
    public async Task ExecuteAsync_UnlocksKeyValue_WhenValidParametersProvided()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _appConfigService.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            false,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.False(result.Locked);
    }

    [Fact]
    public async Task ExecuteAsync_UnlocksKeyValueWithLabel_WhenLabelProvided()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _appConfigService.Received(1).SetKeyValueLockState(
            "account1",
            "my-key",
            false,
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            "prod");
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.KeyValueLockSetCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException(bool locked)
    {
        // Arrange
        _appConfigService.SetKeyValueLockState(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>())
            .ThrowsAsync(new Exception("Failed to lock key-value"));

        var argsToParse = locked
            ? new List<string> { "--subscription", "sub123", "--account", "account1", "--key", "my-key", "--lock" }
            : new List<string> { "--subscription", "sub123", "--account", "account1", "--key", "my-key" };
        var args = _commandDefinition.Parse(argsToParse);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Failed to lock key-value", response.Message);
    }

    [Theory]
    [InlineData("")] // No parameters
    [InlineData("--subscription sub123")] // Missing account and key
    [InlineData("--subscription sub123 --account account1")] // Missing key
    [InlineData("--account account1 --key my-key")] // Missing subscription
    [InlineData("--subscription sub123 --key my-key")] // Missing account
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(string args)
    {
        // Arrange
        var parsedArgs = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
