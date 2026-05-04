// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.KeyVault.Commands;
using Azure.Mcp.Tools.KeyVault.Commands.Key;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Keys;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Key;

public class KeyGetCommandTests : CommandUnitTestsBase<KeyGetCommand, IKeyVaultService>
{
    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownKeyName = "knownKeyName";
    private readonly KeyType _knownKeyType = KeyType.Rsa;
    private readonly KeyVaultKey _knownKeyVaultKey;

    public KeyGetCommandTests()
    {
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
    public async Task ExecuteAsync_ReturnsKey()
    {
        // Arrange
        Service.GetKey(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownKeyName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(_knownKeyVaultKey);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--key", _knownKeyName,
            "--subscription", _knownSubscriptionId);

        // Assert
        var retrievedKey = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.KeyGetCommandResult);

        Assert.NotNull(retrievedKey.Key);
        Assert.Null(retrievedKey.Keys);
        Assert.Equal(_knownKeyName, retrievedKey.Key.Name);
        Assert.Equal(_knownKeyType.ToString(), retrievedKey.Key.KeyType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.GetKey(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownKeyName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--key", _knownKeyName,
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKeysList_WhenKeyNameNotProvided()
    {
        // Arrange
        var expectedKeys = new List<string> { "key1", "key2", "key3" };

        Service.ListKeys(
            Arg.Is(_knownVaultName),
            Arg.Is(false),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedKeys);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.KeyGetCommandResult);

        Assert.NotNull(result.Keys);
        Assert.Null(result.Key);
        Assert.Equal(expectedKeys.Count, result.Keys.Count);
        Assert.Equal(expectedKeys, result.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKeysList_WithManagedKeys()
    {
        // Arrange
        var expectedKeys = new List<string> { "key1", "key2", "managed-key1" };

        Service.ListKeys(
            Arg.Is(_knownVaultName),
            Arg.Is(true),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedKeys);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId,
            "--include-managed", "true");

        // Assert
        var result = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.KeyGetCommandResult);

        Assert.NotNull(result.Keys);
        Assert.Null(result.Key);
        Assert.Equal(expectedKeys.Count, result.Keys.Count);
        Assert.Equal(expectedKeys, result.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_WhenListingKeys()
    {
        // Arrange
        var expectedError = "List error";

        Service.ListKeys(
            Arg.Is(_knownVaultName),
            Arg.Any<bool>(),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
