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

public class CertificateGetCommandTests : CommandUnitTestsBase<CertificateGetCommand, IKeyVaultService>
{
    private const string _knownSubscriptionId = "knownSubscription";
    private const string _knownVaultName = "knownVaultName";
    private const string _knownCertificateName = "knownCertificateName";

    [Fact]
    public async Task ExecuteAsync_CallsServiceCorrectly()
    {
        // Arrange
        var expectedError = "Expected test error";

        // TODO (vcolin7): Find a way to mock KeyVaultCertificateWithPolicy
        // We'll test that the service is called correctly, but let it fail since mocking the return is complex
        Service.GetCertificate(
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
        await Service.Received(1).GetCertificate(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownCertificateName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        // Should handle the exception
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.GetCertificate(
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

    [Fact]
    public async Task ExecuteAsync_ReturnsCertificatesList_WhenCertificateNameNotProvided()
    {
        // Arrange
        var expectedCertificates = new List<string> { "cert1", "cert2", "cert3" };

        Service.ListCertificates(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedCertificates);

        // Act
        var response = await ExecuteCommandAsync(
            "--vault", _knownVaultName,
            "--subscription", _knownSubscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, Commands.KeyVaultJsonContext.Default.CertificateGetCommandResult);

        Assert.NotNull(result.Certificates);
        Assert.Null(result.Certificate);
        Assert.Equal(expectedCertificates.Count, result.Certificates.Count);
        Assert.Equal(expectedCertificates, result.Certificates);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_WhenListingCertificates()
    {
        // Arrange
        var expectedError = "List error";

        Service.ListCertificates(
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
        Assert.StartsWith(expectedError, response.Message);
    }
}
