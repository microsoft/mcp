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

public class SecretGetCommandTests : CommandUnitTestsBase<SecretGetCommand, IKeyVaultService>
{
    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownSecretName = "knownSecretName";
    private const string _knownSecretValue = "knownSecretValue";
    private readonly KeyVaultSecret _knownKeyVaultSecret = new(_knownSecretName, _knownSecretValue);

    [Fact]
    public async Task ExecuteAsync_ReturnsSecret()
    {
        // Arrange
        Service.GetSecret(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSecretName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(_knownKeyVaultSecret);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--subscription", _knownSubscriptionId);

        // Assert
        var retrievedSecret = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.SecretGetCommandResult);

        Assert.NotNull(retrievedSecret.Secret);
        Assert.Null(retrievedSecret.Secrets);
        Assert.Equal(_knownSecretName, retrievedSecret.Secret.Name);
        Assert.Equal(_knownSecretValue, retrievedSecret.Secret.Value);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.GetSecret(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--secret", _knownSecretName,
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSecretsList_WhenSecretNameNotProvided()
    {
        // Arrange
        var expectedSecrets = new List<string> { "secret1", "secret2", "secret3" };

        Service.ListSecrets(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedSecrets);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.SecretGetCommandResult);

        Assert.NotNull(result.Secrets);
        Assert.Null(result.Secret);
        Assert.Equal(expectedSecrets.Count, result.Secrets.Count);
        Assert.Equal(expectedSecrets, result.Secrets);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_WhenListingSecrets()
    {
        // Arrange
        var expectedError = "List error";

        Service.ListSecrets(
            Arg.Is(_knownVaultName),
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
        Assert.Contains(expectedError, response.Message);
    }
}
