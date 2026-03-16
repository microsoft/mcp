// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using System.Security;
using Azure.ResourceManager;
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

    #region Sovereign Cloud Tests

    [Theory]
    // Azure China Cloud
    [InlineData("https://myregistry.azurecr.cn", "acr")]
    [InlineData("https://myconfig.azconfig.azure.cn", "appconfig")]
    [InlineData("https://mycomm.communication.azure.cn", "communication")]
    public void ValidateAzureServiceEndpoint_AzureChinaCloud_ValidEndpoints_DoesNotThrow(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzureChina));
        Assert.Null(exception);
    }

    [Theory]
    // Azure US Government
    [InlineData("https://myregistry.azurecr.us", "acr")]
    [InlineData("https://myconfig.azconfig.azure.us", "appconfig")]
    [InlineData("https://mycomm.communication.azure.us", "communication")]
    public void ValidateAzureServiceEndpoint_AzureGovernment_ValidEndpoints_DoesNotThrow(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzureGovernment));
        Assert.Null(exception);
    }

    [Theory]
    // Public cloud endpoint should fail in China cloud
    [InlineData("https://myregistry.azurecr.io", "acr")]
    [InlineData("https://myconfig.azconfig.io", "appconfig")]
    public void ValidateAzureServiceEndpoint_PublicCloudEndpoint_InChinaCloud_Throws(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzureChina));
        Assert.Contains("Azure China Cloud", exception.Message);
        Assert.Contains("not a valid", exception.Message);
    }

    [Theory]
    // Public cloud endpoint should fail in Gov cloud
    [InlineData("https://myregistry.azurecr.io", "acr")]
    [InlineData("https://myconfig.azconfig.io", "appconfig")]
    public void ValidateAzureServiceEndpoint_PublicCloudEndpoint_InGovCloud_Throws(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzureGovernment));
        Assert.Contains("Azure US Government Cloud", exception.Message);
        Assert.Contains("not a valid", exception.Message);
    }

    [Theory]
    // China cloud endpoint should fail in public cloud
    [InlineData("https://myregistry.azurecr.cn", "acr")]
    [InlineData("https://myconfig.azconfig.azure.cn", "appconfig")]
    public void ValidateAzureServiceEndpoint_ChinaCloudEndpoint_InPublicCloud_Throws(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud));
        Assert.Contains("Azure Public Cloud", exception.Message);
        Assert.Contains("not a valid", exception.Message);
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
    [InlineData("https://www.microsoft.com")]
    [InlineData("https://www.google.com")]
    [InlineData("https://github.com")]
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

    [Theory]
    [InlineData("http://localhost")]
    [InlineData("http://LOCALHOST")]
    [InlineData("http://localhost:8080")]
    [InlineData("http://local")]
    [InlineData("http://localtest.me")]  // Common localhost alias
    [InlineData("http://lvh.me")]        // Another localhost variation
    public void ValidatePublicTargetUrl_ReservedHostnames_ThrowsSecurityException(string url)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.Contains("reserved", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("http://127.0.0.1.nip.io")]      // nip.io resolves to 127.0.0.1
    [InlineData("http://127.0.0.1.xip.io")]      // xip.io resolves to 127.0.0.1
    [InlineData("http://127.0.0.1.sslip.io")]    // sslip.io resolves to 127.0.0.1
    [InlineData("http://10.0.0.1.nip.io")]       // Private IP via DNS
    [InlineData("http://192.168.1.1.nip.io")]    // Private IP via DNS
    public void ValidatePublicTargetUrl_DnsResolvesToPrivateIP_ThrowsSecurityException(string url)
    {
        // This test validates that DNS resolution is performed and private IPs are caught
        // Note: These services (nip.io, xip.io, sslip.io) actually resolve to the IPs in the subdomain
        // If DNS resolution fails (e.g., offline), the test will throw SecurityException for different reason

        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));

        // The error should mention either:
        // 1. "resolves to a private or reserved IP" (if DNS succeeded)
        // 2. "Unable to resolve hostname" (if DNS failed - still secure)
        // 3. "reserved" (if hostname matches a known wildcard DNS service)
        Assert.True(
            exception.Message.Contains("private or reserved", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("Unable to resolve hostname", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("reserved", StringComparison.OrdinalIgnoreCase),
            $"Expected error about private IP, DNS resolution, or reserved hostname, but got: {exception.Message}");
    }

    [Fact]
    public void ValidatePublicTargetUrl_UnresolvableHostname_ThrowsSecurityException()
    {
        // Arrange - use a guaranteed non-existent hostname
        var url = "http://this-hostname-definitely-does-not-exist-12345.invalid";

        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.Contains("Unable to resolve hostname", exception.Message);
    }

    #endregion

    #region DNS Bypass Prevention Tests - SDL Security

    [Theory]
    [InlineData("http://169.254.169.254.nip.io")]  // IMDS bypass via nip.io
    [InlineData("http://evil.sslip.io")]            // sslip.io wildcard DNS
    [InlineData("http://evil.xip.io")]              // xip.io wildcard DNS
    public void ValidatePublicTargetUrl_KnownSSRFBypassDomains_ThrowsSecurityException(string url)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.True(
            exception.Message.Contains("reserved", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("private or reserved", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("Unable to resolve hostname", StringComparison.OrdinalIgnoreCase),
            $"Expected security error, but got: {exception.Message}");
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

    #region IsPrivateOrReservedIP - IPv4-mapped IPv6 bypass tests

    [Theory]
    [InlineData("::ffff:169.254.169.254")]  // IMDS
    [InlineData("::ffff:168.63.129.16")]    // Azure WireServer
    [InlineData("::ffff:127.0.0.1")]        // Loopback
    [InlineData("::ffff:10.0.0.1")]         // Private 10.x
    [InlineData("::ffff:172.16.0.1")]       // Private 172.16.x
    [InlineData("::ffff:192.168.1.1")]      // Private 192.168.x
    [InlineData("::ffff:100.64.0.1")]       // CGNAT
    [InlineData("::ffff:0.0.0.0")]          // Reserved
    [InlineData("::ffff:255.255.255.255")]  // Broadcast
    public void IsPrivateOrReservedIP_IPv4MappedIPv6_ReturnsTrue(string address)
    {
        // Arrange
        var ipAddress = IPAddress.Parse(address);
        Assert.True(ipAddress.IsIPv4MappedToIPv6);

        // Act & Assert
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - IPv6 reserved ranges

    [Theory]
    [InlineData("::")]         // Unspecified (equivalent to 0.0.0.0)
    [InlineData("ff02::1")]    // Multicast - all nodes
    [InlineData("ff05::1")]    // Multicast - site-local
    [InlineData("ff0e::1")]    // Multicast - global
    [InlineData("ff01::1")]    // Multicast - interface-local
    public void IsPrivateOrReservedIP_IPv6ReservedRanges_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("2001:db8::1")]        // Documentation prefix (RFC 3849)
    [InlineData("2001:db8:1234::1")]   // Documentation prefix variant
    public void IsPrivateOrReservedIP_DocumentationPrefix_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("100::1")]    // Discard prefix (RFC 6666)
    [InlineData("100::")]     // Discard prefix base
    public void IsPrivateOrReservedIP_DiscardPrefix_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - 6to4 embedded IPv4 bypass

    [Theory]
    [InlineData("2002:a9fe:a9fe::1")]   // 6to4 embedding 169.254.169.254 (IMDS)
    [InlineData("2002:a83f:8110::1")]   // 6to4 embedding 168.63.129.16 (WireServer)
    [InlineData("2002:7f00:0001::1")]   // 6to4 embedding 127.0.0.1 (Loopback)
    [InlineData("2002:0a00:0001::1")]   // 6to4 embedding 10.0.0.1 (Private)
    [InlineData("2002:c0a8:0101::1")]   // 6to4 embedding 192.168.1.1 (Private)
    [InlineData("2002:ac10:0001::1")]   // 6to4 embedding 172.16.0.1 (Private)
    public void IsPrivateOrReservedIP_6to4EmbeddedPrivateIPv4_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_6to4EmbeddedPublicIPv4_ReturnsFalse()
    {
        // 2002:0808:0808::1 embeds 8.8.8.8 (Google DNS - public)
        var ipAddress = IPAddress.Parse("2002:0808:0808::1");
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - Teredo embedded IPv4 bypass

    [Theory]
    [InlineData("2001:0000:4136:e378:8000:63bf:3fff:fdd2")]  // Teredo client IPv4 = 192.0.2.45 → ~bytes = public, but let's test private
    public void IsPrivateOrReservedIP_TeredoPublicIPv4_ReturnsFalse(string address)
    {
        // 3fff:fdd2 XOR ffff = c000:022d = 192.0.2.45 (documentation range 192.0.2.0/24, but not checked as private)
        // Actually 192.0.2.x is TEST-NET-1, not in our private list, so returns false
        var ipAddress = IPAddress.Parse(address);
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_TeredoEmbeddedLoopback_ReturnsTrue()
    {
        // Teredo with client IPv4 127.0.0.1: bytes[12..15] = 127^0xFF, 0^0xFF, 0^0xFF, 1^0xFF = 0x80, 0xFF, 0xFF, 0xFE
        var ipAddress = IPAddress.Parse("2001:0000:4136:e378:8000:63bf:80ff:fffe");
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_TeredoEmbeddedIMDS_ReturnsTrue()
    {
        // Teredo with client IPv4 169.254.169.254: XOR each byte with 0xFF
        // 169^255=86 (0x56), 254^255=1 (0x01), 169^255=86 (0x56), 254^255=1 (0x01)
        var ipAddress = IPAddress.Parse("2001:0000:4136:e378:8000:63bf:5601:5601");
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_TeredoEmbeddedPrivate10x_ReturnsTrue()
    {
        // Teredo with client IPv4 10.0.0.1: XOR each byte with 0xFF
        // 10^255=245 (0xF5), 0^255=255 (0xFF), 0^255=255 (0xFF), 1^255=254 (0xFE)
        var ipAddress = IPAddress.Parse("2001:0000:4136:e378:8000:63bf:f5ff:fffe");
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - IPv4-compatible IPv6 (deprecated)

    [Theory]
    [InlineData("::127.0.0.1")]       // IPv4-compatible loopback
    [InlineData("::10.0.0.1")]        // IPv4-compatible private
    [InlineData("::192.168.1.1")]     // IPv4-compatible private
    [InlineData("::169.254.169.254")] // IPv4-compatible IMDS
    public void IsPrivateOrReservedIP_IPv4CompatibleIPv6_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.Equal(AddressFamily.InterNetworkV6, ipAddress.AddressFamily);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region ValidatePublicTargetUrl - Wildcard DNS services

    [Theory]
    [InlineData("http://anything.nip.io")]
    [InlineData("http://anything.sslip.io")]
    [InlineData("http://anything.xip.io")]
    [InlineData("http://10.0.0.1.nip.io")]
    [InlineData("http://192-168-1-1.sslip.io")]
    public void ValidatePublicTargetUrl_WildcardDnsServices_ThrowsSecurityException(string url)
    {
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.Contains("reserved", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
