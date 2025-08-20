// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Keys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Key;

public class KeyCreateCommandTests
{

    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<KeyCreateCommand> _logger;
    private readonly KeyCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownKeyName = "knownKeyName";
    private readonly KeyType _knownKeyType = KeyType.Rsa;
    private readonly KeyVaultKey _knownKeyVaultKey;

    public KeyCreateCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<KeyCreateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_keyVaultService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());

        _knownKeyVaultKey = new KeyVaultKey(_knownKeyName);

        var jsonWebKey = new JsonWebKey([KeyOperation.Encrypt])
        {
            KeyType = _knownKeyType
        };

        // Use reflection to set the internal Key property, which holds KeyType and is required in KeyVaultKey
        var keyProperty = typeof(KeyVaultKey).GetProperty("Key", System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        keyProperty?.SetValue(_knownKeyVaultKey, jsonWebKey);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesKey_WithValidInput()
    {
        // Arrange
        _keyVaultService.CreateKey(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownKeyName),
            Arg.Is(_knownKeyType.ToString()),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(_knownKeyVaultKey);

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--key", _knownKeyName,
            "--key-type", _knownKeyType.ToString(),
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var retrievedKey = JsonSerializer.Deserialize<KeyCreateResult>(json);

        Assert.NotNull(retrievedKey);
        Assert.Equal(_knownKeyName, retrievedKey.Name);
        Assert.Equal(_knownKeyType.ToString(), retrievedKey.KeyType);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidObject_IfKeyNameIsEmpty()
    {
        // Arrange - No need to mock service since validation should fail before service is called
        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--key", "",
            "--key-type", _knownKeyType.ToString(),
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert - Should return validation error response
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _keyVaultService.CreateKey(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownKeyName),
            Arg.Is(_knownKeyType.ToString()),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--vault", _knownVaultName,
            "--key", _knownKeyName,
            "--key-type", _knownKeyType.ToString(),
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class KeyCreateResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("keyType")]
        public string KeyType { get; set; } = null!;

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("notBefore")]
        public DateTimeOffset? NotBefore { get; set; }

        [JsonPropertyName("expiresOn")]
        public DateTimeOffset? ExpiresOn { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonPropertyName("updatedOn")]
        public DateTimeOffset? UpdatedOn { get; set; }
    }
}
