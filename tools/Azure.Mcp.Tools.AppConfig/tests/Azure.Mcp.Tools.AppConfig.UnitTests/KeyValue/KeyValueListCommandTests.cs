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
using static Azure.Mcp.Tools.AppConfig.Commands.KeyValue.KeyValueListCommand;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.KeyValue;

[Trait("Area", "AppConfig")]
public class KeyValueListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly ILogger<KeyValueListCommand> _logger;
    private readonly KeyValueListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public KeyValueListCommandTests()
    {
        _appConfigService = Substitute.For<IAppConfigService>();
        _logger = Substitute.For<ILogger<KeyValueListCommand>>();

        _command = new(_logger);
        _parser = new(_command.GetCommand());
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appConfigService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSettings_WhenSettingsExist()
    {
        // Arrange
        var expectedSettings = new List<KeyValueSetting>
        {
            new() { Key = "key1", Value = "value1", Label = "prod" },
            new() { Key = "key2", Value = "value2", Label = "dev" }
        };
        _appConfigService.ListKeyValues(
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<RetryPolicyOptions>())
          .Returns(expectedSettings);

        var args = _parser.Parse(["--subscription", "sub123", "--account", "account1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Settings.Count);
        Assert.Equal("key1", result.Settings[0].Key);
        Assert.Equal("key2", result.Settings[1].Key);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredSettings_WhenKeyFilterProvided()
    {
        // Arrange
        var expectedSettings = new List<KeyValueSetting>
        {
            new() { Key = "key1", Value = "value1", Label = "prod" }
        };
        _appConfigService.ListKeyValues(
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<RetryPolicyOptions>())
          .Returns(expectedSettings);

        var args = _parser.Parse(["--subscription", "sub123", "--account", "account1", "--key", "key1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Single(result.Settings);
        Assert.Equal("key1", result.Settings[0].Key);
        Assert.Equal("value1", result.Settings[0].Value);
        Assert.Equal("prod", result.Settings[0].Label);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredSettings_WhenLabelFilterProvided()
    {
        // Arrange
        var expectedSettings = new List<KeyValueSetting>
        {
            new() { Key = "key1", Value = "value1", Label = "prod" }
        };
        _appConfigService.ListKeyValues(
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<string>(),
          Arg.Any<RetryPolicyOptions>())
          .Returns(expectedSettings);

        var args = _parser.Parse(["--subscription", "sub123", "--account", "account1", "--label", "prod"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyValueListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.Single(result.Settings);
        Assert.Equal("key1", result.Settings[0].Key);
        Assert.Equal("value1", result.Settings[0].Value);
        Assert.Equal("prod", result.Settings[0].Label);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _appConfigService.ListKeyValues(Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Service error"));

        var args = _parser.Parse(["--subscription", "sub123", "--account", "account1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Theory]
    [InlineData("--account", "account1")] // Missing subscription
    [InlineData("--subscription", "sub123")] // Missing account
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
