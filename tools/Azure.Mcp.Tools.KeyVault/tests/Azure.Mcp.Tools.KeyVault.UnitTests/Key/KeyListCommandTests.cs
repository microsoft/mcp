// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Key;

public class KeyListCommandTests
{

    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<KeyListCommand> _logger;
    private readonly KeyListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    private const string _knownSubscriptionId = "knownSubscriptionId";
    private const string _knownVaultName = "knownVaultName";

    public KeyListCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<KeyListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_keyVaultService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKeys_WhenKeysExist()
    {
        // Arrange
        var expectedKeys = new List<string> { "key1", "key2" };

        _keyVaultService.ListKeys(
            Arg.Is(_knownVaultName),
            Arg.Any<bool>(),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedKeys);

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<KeyListResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedKeys, result.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoKeys()
    {
        // Arrange
        _keyVaultService.ListKeys(
            Arg.Is(_knownVaultName),
            Arg.Any<bool>(),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId
        ]);

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

        _keyVaultService.ListKeys(
            Arg.Is(_knownVaultName),
            Arg.Any<bool>(),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class KeyListResult
    {
        [JsonPropertyName("keys")]
        public List<string> Keys { get; set; } = [];
    }
}
