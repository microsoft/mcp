// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppConfig.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue;

[Trait("Area", "AppConfig")]
public class KeyValueDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly ILogger<KeyValueDeleteCommand> _logger;
    private readonly KeyValueDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public KeyValueDeleteCommandTests()
    {
        _appConfigService = Substitute.For<IAppConfigService>();
        _logger = Substitute.For<ILogger<KeyValueDeleteCommand>>();

        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appConfigService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesKeyValue_WhenValidParametersProvided()
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
        await _appConfigService.Received(1).DeleteKeyValue(
            "account1",
            "my-key",
            "sub123",
            null,
            Arg.Any<RetryPolicyOptions>(),
            null);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesKeyValueWithLabel_WhenLabelProvided()
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
        await _appConfigService.Received(1).DeleteKeyValue(
            "account1",
            "my-key",
            "sub123", null,
            Arg.Any<RetryPolicyOptions>(),
            "prod");

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppConfigJsonContext.Default.KeyValueDeleteCommandResult);

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Key);
        Assert.Equal("prod", result.Label);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _appConfigService.DeleteKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>())
            .ThrowsAsync(new Exception("Failed to delete key-value"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Failed to delete key-value", response.Message);
    }

    [Theory]
    [InlineData("--account", "account1", "--key", "my-key")] // Missing subscription
    [InlineData("--subscription", "sub123", "--key", "my-key")] // Missing account
    [InlineData("--subscription", "sub123", "--account", "account1")] // Missing key
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
