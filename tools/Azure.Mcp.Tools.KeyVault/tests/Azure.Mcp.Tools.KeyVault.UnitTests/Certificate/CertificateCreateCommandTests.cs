// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Certificate;

public class CertificateCreateCommandTests : CommandUnitTestsBase<CertificateCreateCommand, IKeyVaultService>
{
    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownCertificateName = "knownCertificateName";

    [Fact]
    public async Task ExecuteAsync_CallsServiceCorrectly()
    {
        // Arrange
        var expectedError = "Expected test error";

        // TODO (vcolin7): Find a way to mock CertificateOperation
        // We'll test that the service is called correctly, but let it fail since mocking the return is complex
        Service.CreateCertificate(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownCertificateName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--certificate", _knownCertificateName,
            "--subscription", _knownSubscriptionId);

        // Assert - Verify the service was called with correct parameters
        await Service.Received(1).CreateCertificate(
            _knownVaultName,
            _knownCertificateName,
            _knownSubscriptionId,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        // Should handle the exception
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidObject_IfCertificateNameIsEmpty()
    {
        // Arrange & Act - No need to mock service since validation should fail before service is called
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--certificate", "",
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

        Service.CreateCertificate(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownCertificateName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--certificate", _knownCertificateName,
            "--subscription", _knownSubscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
