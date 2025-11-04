// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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

public class CertificateImportCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<CertificateImportCommand> _logger;
    private readonly CertificateImportCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscription = "knownSubscription";
    private const string _knownVault = "knownVault";
    private const string _knownCertName = "knownCertificate";
    // Generate a deterministic base64 string from readable words to avoid cspell warnings on opaque text.
    private static readonly string _fakePfxBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("sample certificate data"));

    public CertificateImportCommandTests()
    {
        _keyVaultService = Substitute.For<IKeyVaultService>();
        _logger = Substitute.For<ILogger<CertificateImportCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_keyVaultService);
        _serviceProvider = services.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_CallsService_WithExpectedParameters()
    {
        // Arrange
        _keyVaultService.ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error")); // force exception to avoid building return object

        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", _fakePfxBase64,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        await _keyVaultService.Received(1).ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status); // due to forced exception
    }

    public static IEnumerable<object[]> InvalidArgumentCases()
    {
        // Build scenarios with missing required parameters
        yield return new object[] { "" };
        yield return new object[] { "--vault knownVault" };
        yield return new object[] { "--vault knownVault --certificate knownCertificate" };
        yield return new object[] { "--vault knownVault --certificate knownCertificate --subscription knownSubscription" };
        yield return new object[] { $"--vault knownVault --certificate knownCertificate --certificate-data {_fakePfxBase64}" };
    }

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_RejectsInvalidArguments(string argLine)
    {
        // Arrange
        var args = _commandDefinition.Parse(argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Configure mock
        _keyVaultService.ImportCertificate(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error - service should not be called for invalid inputs"));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await _keyVaultService.DidNotReceive().ImportCertificate(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsValidArguments()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", _fakePfxBase64,
            "--subscription", _knownSubscription
        ]);


        _keyVaultService.ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotEqual(HttpStatusCode.BadRequest, response.Status);
        await _keyVaultService.Received(1).ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        var expected = "boom";
        _keyVaultService.ImportCertificate(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>()).ThrowsAsync(new Exception(expected));

        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", _fakePfxBase64,
            "--subscription", _knownSubscription
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expected, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsService_WithPemData()
    {
        // Arrange - minimal mock PEM (not a valid cert, but exercises the code path)
        var pem = "-----BEGIN CERTIFICATE-----\nABCDEF123456\n-----END CERTIFICATE-----";

        _keyVaultService.ImportCertificate(
            _knownVault,
            _knownCertName,
            pem,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", pem,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert - ensure the PEM (with header) was passed through untouched
        await _keyVaultService.Received(1).ImportCertificate(
            _knownVault,
            _knownCertName,
            pem,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CallsService_WithPassword()
    {
        var password = "P@ssw0rd!";

        _keyVaultService.ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            password,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", _fakePfxBase64,
            "--password", password,
            "--subscription", _knownSubscription
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        await _keyVaultService.Received(1).ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            password,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CallsService_WithFilePath()
    {
        // Arrange - create temp file to simulate file path input
        var tempPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempPath, [1, 2, 3, 4], TestContext.Current.CancellationToken);
            _keyVaultService.ImportCertificate(
                _knownVault,
                _knownCertName,
                tempPath,
                null,
                _knownSubscription,
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>())
                .ThrowsAsync(new Exception("Test error"));
            var args = _commandDefinition.Parse([
                "--vault", _knownVault,
                "--certificate", _knownCertName,
                "--certificate-data", tempPath,
                "--subscription", _knownSubscription
            ]);
            // Act
            var response = await _command.ExecuteAsync(_context, args);
            // Assert - ensure the raw path was passed through
            await _keyVaultService.Received(1).ImportCertificate(
                _knownVault,
                _knownCertName,
                tempPath,
                null,
                _knownSubscription,
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>());
            Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_OnInvalidCertificateData()
    {
        // Simulate service throwing the wrapped invalid data message
        var invalidData = "not-valid-base64-or-path";
        var errorMessage = $"Error importing certificate '{_knownCertName}' into vault {_knownVault}: The provided certificate-data is neither a file path, raw PEM, nor base64 encoded content.";

        _keyVaultService.ImportCertificate(
            _knownVault,
            _knownCertName,
            invalidData,
            null,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(errorMessage));

        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", invalidData,
            "--subscription", _knownSubscription
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(errorMessage, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_OnInvalidPassword()
    {
        // Simulate password mismatch scenario
        var password = "WrongPassword";
        var mismatchMessage = $"Error importing certificate '{_knownCertName}' into vault {_knownVault}: Invalid password or certificate data.";

        _keyVaultService.ImportCertificate(
            _knownVault,
            _knownCertName,
            _fakePfxBase64,
            password,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(mismatchMessage));

        var args = _commandDefinition.Parse([
            "--vault", _knownVault,
            "--certificate", _knownCertName,
            "--certificate-data", _fakePfxBase64,
            "--password", password,
            "--subscription", _knownSubscription
        ]);

        var response = await _command.ExecuteAsync(_context, args);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(mismatchMessage, response.Message);
    }
}
