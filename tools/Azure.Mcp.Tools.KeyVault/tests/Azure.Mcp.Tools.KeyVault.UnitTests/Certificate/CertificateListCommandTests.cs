// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.KeyVault.Commands.Certificate;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Certificate;

public class CertificateListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<CertificateListCommand> _logger;
    private readonly CertificateListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    private const string _knownSubscriptionId = "knownSubscriptionId";
    private const string _knownVaultName = "knownVaultName";

    public CertificateListCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<CertificateListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_keyVaultService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCertificates_WhenCertificatesExist()
    {
        // Arrange
        var expectedCertificates = new List<string> { "cert1", "cert2" };

        _keyVaultService.ListCertificates(
            Arg.Is(_knownVaultName),
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCertificates);

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
        var result = JsonSerializer.Deserialize<CertificateListResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedCertificates, result.Certificates);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoCertificates()
    {
        // Arrange
        _keyVaultService.ListCertificates(
            Arg.Is(_knownVaultName),
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

        _keyVaultService.ListCertificates(
            Arg.Is(_knownVaultName),
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

    private class CertificateListResult
    {
        [JsonPropertyName("certificates")]
        public List<string> Certificates { get; set; } = [];
    }
}
