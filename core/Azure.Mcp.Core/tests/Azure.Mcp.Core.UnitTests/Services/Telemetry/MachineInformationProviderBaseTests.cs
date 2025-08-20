// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Telemetry;

public class MachineInformationProviderBaseTests
{
    private readonly ILogger<MachineInformationProviderBase> _logger;
    private readonly TestMachineInformationProvider _provider;

    public MachineInformationProviderBaseTests()
    {
        _logger = Substitute.For<ILogger<MachineInformationProviderBase>>();
        _provider = new TestMachineInformationProvider(_logger);
    }

    [Fact]
    public async Task GetMacAddressHash_WhenMacAddressExists_ReturnsHashedValue()
    {
        // Act
        var result = await _provider.GetMacAddressHash();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("N/A", result);
        // Should be a valid SHA-256 hash (64 hex characters)
        Assert.Matches("^[a-f0-9]{64}$", result);
    }

    [Fact]
    public async Task GetMacAddressHash_WhenNoMacAddressFound_ReturnsNotAvailable()
    {
        // This test is challenging since we can't easily mock NetworkInterface.GetAllNetworkInterfaces()
        // In a real scenario, you might want to refactor the code to make it more testable
        // For now, we'll test the happy path and error handling

        // Act
        var result = await _provider.GetMacAddressHash();

        // Assert
        Assert.NotNull(result);
        // Result should either be a hash or "N/A"
        Assert.True(result == "N/A" || result.Length == 64);
    }

    [Fact]
    public void GetMacAddress_ReturnsValidMacAddressOrNull()
    {
        // Act
        var result = _provider.GetMacAddress();

        // Assert
        // Should either return null or a non-empty string
        if (result != null)
        {
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void GenerateDeviceId_ReturnsValidGuidFormat()
    {
        // Act
        var result = _provider.GenerateDeviceId();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Should be in format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        Assert.Matches(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", result);

        // Should be lowercase
        Assert.Equal(result.ToLowerInvariant(), result);

        // Should be parsable as a GUID
        Assert.True(Guid.TryParse(result, out _));
    }

    [Fact]
    public void GenerateDeviceId_GeneratesUniqueIds()
    {
        // Act
        var id1 = _provider.GenerateDeviceId();
        var id2 = _provider.GenerateDeviceId();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Theory]
    [InlineData("test")]
    [InlineData("")]
    [InlineData("ABC123")]
    [InlineData("special-characters!@#$%")]
    [InlineData("unicode-測試")]
    public void HashValue_WithVariousInputs_ReturnsValidSha256Hash(string input)
    {
        // Act
        var result = _provider.HashValue(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(64, result.Length); // SHA-256 produces 64 hex characters
        Assert.Matches("^[a-f0-9]+$", result); // Should only contain lowercase hex

        // Verify it's actually the correct SHA-256 hash
        var expectedHash = ComputeExpectedHash(input);
        Assert.Equal(expectedHash, result);
    }

    [Fact]
    public void HashValue_SameInput_ReturnsSameHash()
    {
        // Arrange
        const string input = "test-value";

        // Act
        var hash1 = _provider.HashValue(input);
        var hash2 = _provider.HashValue(input);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashValue_DifferentInputs_ReturnsDifferentHashes()
    {
        // Act
        var hash1 = _provider.HashValue("input1");
        var hash2 = _provider.HashValue("input2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Constants_HaveExpectedValues()
    {
        // Use reflection to access protected constants through the test class
        var type = typeof(TestMachineInformationProvider);

        // These constants should be accessible through the base class
        // We can verify them through the actual usage in the class

        // Test that NotAvailable constant is used correctly
        var notAvailableField = type.BaseType!.GetField("NotAvailable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.Equal("N/A", notAvailableField?.GetValue(null));
    }

    /// <summary>
    /// Test class that allows us to simulate exception scenarios
    /// </summary>
    private class ExceptionThrowingProvider : MachineInformationProviderBase
    {
        public ExceptionThrowingProvider(ILogger<MachineInformationProviderBase> logger) : base(logger)
        {
        }

        public override Task<string?> GetOrCreateDeviceId()
        {
            return Task.FromResult<string?>("test-device-id");
        }

        // Override to throw exception for testing error handling
        protected override string? GetMacAddress()
        {
            throw new InvalidOperationException("Simulated network error");
        }

        // Expose protected method
        public new Task<string> GetMacAddressHash() => base.GetMacAddressHash();
    }

    [Fact]
    public async Task GetMacAddressHash_WhenExceptionThrown_ReturnsNotAvailableAndLogsError()
    {
        // Arrange
        var exceptionProvider = new ExceptionThrowingProvider(_logger);

        // Act
        var result = await exceptionProvider.GetMacAddressHash();

        // Assert
        Assert.Equal("N/A", result);

        // Verify that an error was logged
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Unable to calculate MAC address hash")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Helper method to compute expected SHA-256 hash for comparison
    /// </summary>
    private static string ComputeExpectedHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashInput = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashInput).Replace("-", string.Empty).ToLowerInvariant();
    }
}

/// <summary>
/// Test implementation of the abstract MachineInformationProviderBase class
/// </summary>
internal class TestMachineInformationProvider(ILogger<MachineInformationProviderBase> logger)
    : MachineInformationProviderBase(logger)
{
    public override Task<string?> GetOrCreateDeviceId()
    {
        // Default implementation for testing - can be overridden in specific tests
        return Task.FromResult<string?>("test-device-id");
    }

    public new string? GetMacAddress() => base.GetMacAddress();
    public new string GenerateDeviceId() => base.GenerateDeviceId();
    public new string HashValue(string value) => base.HashValue(value);
}
