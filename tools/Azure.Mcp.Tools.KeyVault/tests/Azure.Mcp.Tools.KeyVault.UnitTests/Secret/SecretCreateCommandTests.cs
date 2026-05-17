// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.KeyVault.Commands;
using Azure.Mcp.Tools.KeyVault.Commands.Secret;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Secret;

public class SecretCreateCommandTests : CommandUnitTestsBase<SecretCreateCommand, IKeyVaultService>
{
    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownSecretName = "knownSecretName";
    private const string _knownSecretValue = "knownSecretValue";
    private readonly KeyVaultSecret _knownKeyVaultSecret = new(_knownSecretName, _knownSecretValue);

    [Fact]
    public async Task ExecuteAsync_CreatesSecret_WhenValidInput()
    {
        // Arrange
        Service.CreateSecret(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSecretName),
            Arg.Is(_knownSecretValue),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(_knownKeyVaultSecret);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--value", _knownSecretValue,
            "--subscription", _knownSubscriptionId);

        // Assert
        var retrievedSecret = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.SecretCreateCommandResult);

        Assert.Equal(_knownSecretName, retrievedSecret.Name);
        Assert.Equal(_knownSecretValue, retrievedSecret.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidObject_IfSecretNameIsEmpty()
    {
        // Arrange & Act - No need to mock service since validation should fail before service is called
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--secret", "",
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

        Service.CreateSecret(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSecretName),
            Arg.Is(_knownSecretValue),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--value", _knownSecretValue,
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
