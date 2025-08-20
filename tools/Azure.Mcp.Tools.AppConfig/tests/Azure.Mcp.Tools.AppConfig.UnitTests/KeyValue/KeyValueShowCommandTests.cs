// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Models;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Azure.Mcp.Tools.AppConfig.Commands.KeyValue.KeyValueShowCommand;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue;

[Trait("Area", "AppConfig")]
public class KeyValueShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly ILogger<KeyValueShowCommand> _logger;
    private readonly KeyValueShowCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public KeyValueShowCommandTests()
    {
        _appConfigService = Substitute.For<IAppConfigService>();
        _logger = Substitute.For<ILogger<KeyValueShowCommand>>();

        _command = new(_logger);
        _parser = new(_command.GetCommand());
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appConfigService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSetting_WhenSettingExists()
    {
        // Arrange
        var expectedSetting = new KeyValueSetting
        {
            Key = "my-key",
            Value = "my-value",
            Label = "prod",
            ContentType = "text/plain",
            Locked = false
        };
        _appConfigService.GetKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedSetting);

        var args = _parser.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key",
            "--label", "prod"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueShowResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Setting.Key);
        Assert.Equal("my-value", result.Setting.Value);
        Assert.Equal("prod", result.Setting.Label);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSetting_WhenNoLabelProvided()
    {
        // Arrange
        var expectedSetting = new KeyValueSetting
        {
            Key = "my-key",
            Value = "my-value",
            Label = "",
            ContentType = "text/plain",
            Locked = false
        };
        _appConfigService.GetKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>())
            .Returns(expectedSetting);

        var args = _parser.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueShowResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal("my-key", result.Setting.Key);
        Assert.Equal("my-value", result.Setting.Value);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _appConfigService.GetKeyValue(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string>())
            .ThrowsAsync(new Exception("Setting not found"));

        var args = _parser.Parse([
            "--subscription", "sub123",
            "--account", "account1",
            "--key", "my-key"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Setting not found", response.Message);
    }

    [Theory]
    [InlineData("--account", "account1", "--key", "my-key")] // Missing subscription
    [InlineData("--subscription", "sub123", "--key", "my-key")] // Missing account
    [InlineData("--subscription", "sub123", "--account", "account1")] // Missing key
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange
        var parseResult = _parser.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
