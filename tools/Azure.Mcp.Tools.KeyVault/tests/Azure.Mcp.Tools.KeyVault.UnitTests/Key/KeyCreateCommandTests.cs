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

public class KeyCreateCommandTests : CommandUnitTestsBase<KeyCreateCommand, IKeyVaultService>
{
    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownKeyName = "knownKeyName";
    private readonly KeyType _knownKeyType = KeyType.Rsa;
    private readonly KeyVaultKey _knownKeyVaultKey;

    public KeyCreateCommandTests()
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
    public async Task ExecuteAsync_CreatesKey_WithValidInput()
    {
        // Arrange
        Service.CreateKey(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownKeyName),
            Arg.Is(_knownKeyType.ToString()),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(_knownKeyVaultKey);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--key", _knownKeyName,
            "--key-type", _knownKeyType.ToString(),
            "--subscription", _knownSubscriptionId);

        // Assert
        var retrievedKey = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.KeyCreateCommandResult);

        Assert.Equal(_knownKeyName, retrievedKey.Name);
        Assert.Equal(_knownKeyType.ToString(), retrievedKey.KeyType);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidObject_IfKeyNameIsEmpty()
    {
        // Arrange & Act - No need to mock service since validation should fail before service is called
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--key", "",
            "--key-type", _knownKeyType.ToString(),
            "--subscription", _knownSubscriptionId);

        // Assert - Should return validation error response
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.CreateKey(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownKeyName),
            Arg.Is(_knownKeyType.ToString()),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--key", _knownKeyName,
            "--key-type", _knownKeyType.ToString(),
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
