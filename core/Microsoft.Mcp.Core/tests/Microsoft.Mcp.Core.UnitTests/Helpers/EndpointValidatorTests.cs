// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Helpers;

public class EndpointValidatorTests
{
    #region ValidateAzureServiceEndpoint Tests

    [Theory]
    [InlineData("https://mycomm.communication.azure.com", "communication")]
    [InlineData("https://myconfig.azconfig.io", "appconfig")]
    [InlineData("https://myregistry.azurecr.io", "acr")]
    [InlineData("https://myai.cognitiveservices.azure.com", "ai")]
    [InlineData("https://myai.openai.azure.com", "ai")]
    [InlineData("https://myproject.services.ai.azure.com", "ai")]
    [InlineData("https://mystorage.blob.core.windows.net", "storage-blob")]
    [InlineData("https://myvault.vault.azure.net", "keyvault")]
    [InlineData("https://mydb.database.windows.net", "sql")]
    public void ValidateAzureServiceEndpoint_ValidEndpoints_DoesNotThrow(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Record.Exception(() => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://evil.com", "communication", "not a valid communication domain")]
    [InlineData("https://evil.com/.communication.azure.com", "communication", "not a valid communication domain")]
    [InlineData("http://mycomm.communication.azure.com", "communication", "must use HTTPS")]
    [InlineData("ftp://myconfig.azconfig.io", "appconfig", "must use HTTPS")]
    public void ValidateAzureServiceEndpoint_InvalidEndpoints_ThrowsSecurityException(
        string endpoint,
        string serviceType,
        string expectedMessagePart)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType));
        Assert.Contains(expectedMessagePart, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("", "communication")]
    [InlineData("   ", "communication")]
    public void ValidateAzureServiceEndpoint_NullOrEmptyEndpoint_ThrowsArgumentException(
        string endpoint,
        string serviceType)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType));
    }

    [Fact]
    public void ValidateAzureServiceEndpoint_NullEndpoint_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(null!, "communication"));
    }

    [Fact]
    public void ValidateAzureServiceEndpoint_InvalidUriFormat_ThrowsSecurityException()
    {
        // Arrange
        var invalidEndpoint = "not-a-valid-uri";

        // Act & Assert
        var exception = Assert.Throws<SecurityException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(invalidEndpoint, "communication"));
        Assert.Contains("Invalid endpoint format", exception.Message);
    }

    [Fact]
    public void ValidateAzureServiceEndpoint_UnknownServiceType_ThrowsArgumentException()
    {
        // Arrange
        var endpoint = "https://example.com";
        var unknownServiceType = "unknown-service";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, unknownServiceType));
        Assert.Contains("Unknown service type", exception.Message);
    }

    #endregion

    #region ValidateExternalUrl Tests

    [Theory]
    [InlineData("https://raw.githubusercontent.com/user/repo/main/file.txt", new[] { "raw.githubusercontent.com", "github.com" })]
    [InlineData("https://github.com/user/repo", new[] { "raw.githubusercontent.com", "github.com" })]
    [InlineData("https://example.com/path", new[] { "example.com" })]
    public void ValidateExternalUrl_AllowedHost_DoesNotThrow(string url, string[] allowedHosts)
    {
        // Act & Assert
        var exception = Record.Exception(() => EndpointValidator.ValidateExternalUrl(url, allowedHosts));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://evil.com/malicious", new[] { "github.com" }, "not in the allowed list")]
    [InlineData("http://github.com/repo", new[] { "github.com" }, "must use HTTPS")]
    public void ValidateExternalUrl_InvalidHost_ThrowsSecurityException(
        string url,
        string[] allowedHosts,
        string expectedMessagePart)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(
            () => EndpointValidator.ValidateExternalUrl(url, allowedHosts));
        Assert.Contains(expectedMessagePart, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("", new[] { "github.com" })]
    [InlineData("   ", new[] { "github.com" })]
    public void ValidateExternalUrl_NullOrEmptyUrl_ThrowsArgumentException(string url, string[] allowedHosts)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => EndpointValidator.ValidateExternalUrl(url, allowedHosts));
    }

    [Fact]
    public void ValidateExternalUrl_NullUrl_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => EndpointValidator.ValidateExternalUrl(null!, new[] { "github.com" }));
    }

    #endregion

    #region ValidatePublicTargetUrl Tests - SDL Exit Criteria

    [Theory]
    [InlineData("https://myapp.azurewebsites.net")]
    [InlineData("https://example.com")]
    [InlineData("https://api.example.com/endpoint")]
    [InlineData("https://8.8.8.8")]  // Public IP (Google DNS)
    [InlineData("https://1.1.1.1")]  // Public IP (Cloudflare DNS)
    public void ValidatePublicTargetUrl_PublicEndpoints_DoesNotThrow(string url)
    {
        // Act & Assert
        var exception = Record.Exception(() => EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.Null(exception);
    }

    [Theory]
    // IMDS and WireServer (Critical)
    [InlineData("http://169.254.169.254")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    [InlineData("http://168.63.129.16")]
    [InlineData("http://168.63.129.16/machine?comp=goalstate")]

    // Loopback addresses
    [InlineData("http://127.0.0.1")]
    [InlineData("http://127.0.200.8")]
    [InlineData("http://127.255.255.255")]
    [InlineData("http://[::1]")]

    // Private networks (RFC1918)
    [InlineData("http://10.0.0.1")]
    [InlineData("http://10.255.255.255")]
    [InlineData("http://172.16.0.1")]
    [InlineData("http://172.16.0.99")]
    [InlineData("http://172.31.255.255")]
    [InlineData("http://192.168.0.1")]
    [InlineData("http://192.168.0.101")]
    [InlineData("http://192.168.255.255")]

    // Shared address space (CGNAT)
    [InlineData("http://100.64.0.1")]
    [InlineData("http://100.64.0.123")]
    [InlineData("http://100.127.255.255")]

    // Link-local (APIPA)
    [InlineData("http://169.254.0.1")]
    [InlineData("http://169.254.255.255")]

    // Reserved/Special addresses
    [InlineData("http://0.0.0.0")]
    [InlineData("http://255.255.255.255")]

    // IPv6 private
    [InlineData("http://[fc00::1]")]
    [InlineData("http://[fd00::1]")]

    // Reserved hostnames
    [InlineData("http://localhost")]
    [InlineData("http://local")]
    public void ValidatePublicTargetUrl_PrivateOrReservedAddresses_ThrowsSecurityException(string url)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        // The error message varies: "private or reserved" for IPs, "reserved" for hostnames
        Assert.True(
            exception.Message.Contains("private or reserved", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("reserved", StringComparison.OrdinalIgnoreCase),
            $"Expected error message to contain 'private or reserved' or 'reserved', but got: {exception.Message}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidatePublicTargetUrl_NullOrEmptyUrl_ThrowsArgumentException(string url)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => EndpointValidator.ValidatePublicTargetUrl(url));
    }

    [Fact]
    public void ValidatePublicTargetUrl_NullUrl_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => EndpointValidator.ValidatePublicTargetUrl(null!));
    }

    [Fact]
    public void ValidatePublicTargetUrl_InvalidUriFormat_ThrowsSecurityException()
    {
        // Arrange
        var invalidUrl = "not-a-valid-uri";

        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(invalidUrl));
        Assert.Contains("Invalid URL format", exception.Message);
    }

    #endregion

    #region Edge Cases and Security Scenarios

    [Theory]
    [InlineData("https://myconfig.azconfig.io/", "appconfig")]  // Trailing slash
    [InlineData("https://myconfig.azconfig.io:443", "appconfig")]  // Explicit port
    [InlineData("https://MYCONFIG.AZCONFIG.IO", "appconfig")]  // Mixed case
    public void ValidateAzureServiceEndpoint_EdgeCases_DoesNotThrow(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Record.Exception(
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://evil.com/.azconfig.io", "appconfig")]  // Domain suffix attack
    [InlineData("https://azconfig.io.evil.com", "appconfig")]  // Domain prefix attack
    [InlineData("https://myconfig-azconfig.io", "appconfig")]  // Typosquatting
    public void ValidateAzureServiceEndpoint_DomainSpoofingAttempts_ThrowsSecurityException(
        string endpoint,
        string serviceType)
    {
        // Act & Assert
        Assert.Throws<SecurityException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType));
    }

    [Fact]
    public void ValidateExternalUrl_CaseInsensitiveHostMatching_Works()
    {
        // Arrange
        var url = "https://GITHUB.COM/repo";
        var allowedHosts = new[] { "github.com" };

        // Act & Assert
        var exception = Record.Exception(() => EndpointValidator.ValidateExternalUrl(url, allowedHosts));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("http://192.168.1.1/admin")]  // Private network admin panel
    [InlineData("http://10.0.0.1/api")]  // Private API endpoint
    [InlineData("http://localhost:8080/health")]  // Local service health check
    public void ValidatePublicTargetUrl_CommonSSRFTargets_ThrowsSecurityException(string url)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.True(
            exception.Message.Contains("private or reserved", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("reserved", StringComparison.OrdinalIgnoreCase),
            $"Expected error message about private or reserved addresses, but got: {exception.Message}");
    }

    #endregion
}
