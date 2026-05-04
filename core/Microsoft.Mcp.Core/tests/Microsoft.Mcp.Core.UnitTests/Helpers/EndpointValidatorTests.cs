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
    [InlineData("https://my-foundry.services.ai.azure.com", "foundry")]
    [InlineData("https://my-foundry.services.ai.azure.com/api/projects/my-project", "foundry")]
    [InlineData("https://my-resource.openai.azure.com", "azure-openai")]
    [InlineData("https://my-resource.cognitiveservices.azure.com", "azure-openai")]
    [InlineData("https://mynamespace.servicebus.windows.net", "servicebus")]
    [InlineData("https://my-ns.servicebus.windows.net", "servicebus")]
    public void ValidateAzureServiceEndpoint_ValidEndpoints_DoesNotThrow(string endpoint, string serviceType)
    {
        // Act & Assert
        var exception = Record.Exception(() => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://evil.com", "communication", "not a valid communication domain")]
    [InlineData("https://evil.com/.communication.azure.com", "communication", "not a valid communication domain")]
    [InlineData("http://mycomm.communication.azure.com", "communication", "must use HTTPS")]
    [InlineData("ftp://myconfig.azconfig.io", "appconfig", "must use HTTPS")]
    [InlineData("https://evil.com", "foundry", "not a valid foundry domain")]
    [InlineData("http://my-foundry.services.ai.azure.com", "foundry", "must use HTTPS")]
    [InlineData("https://my-foundry.services.ai.azure.com.evil.com", "foundry", "not a valid foundry domain")]
    [InlineData("https://evil.com", "azure-openai", "not a valid azure-openai domain")]
    [InlineData("http://my-resource.openai.azure.com", "azure-openai", "must use HTTPS")]
    [InlineData("https://my-resource.openai.azure.com.evil.com", "azure-openai", "not a valid azure-openai domain")]
    [InlineData("https://attacker.dssldrf.net", "servicebus", "not a valid servicebus domain")]
    [InlineData("http://mynamespace.servicebus.windows.net", "servicebus", "must use HTTPS")]
    [InlineData("https://mynamespace.servicebus.windows.net.evil.com", "servicebus", "not a valid servicebus domain")]
    [InlineData("https://evil.com/.servicebus.windows.net", "servicebus", "not a valid servicebus domain")]
    public void ValidateAzureServiceEndpoint_InvalidEndpoints_ThrowsSecurityException(
        string endpoint,
        string serviceType,
        string expectedMessagePart)
    {
        // Act & Assert
        var exception = Assert.Throws<SecurityException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud));
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
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud));
    }

    [Fact]
    public void ValidateAzureServiceEndpoint_NullEndpoint_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(null!, "communication", ArmEnvironment.AzurePublicCloud));
    }

    [Fact]
    public void ValidateAzureServiceEndpoint_InvalidUriFormat_ThrowsSecurityException()
    {
        // Arrange
        var invalidEndpoint = "not-a-valid-uri";

        // Act & Assert
        var exception = Assert.Throws<SecurityException>(
            () => EndpointValidator.ValidateAzureServiceEndpoint(invalidEndpoint, "communication", ArmEnvironment.AzurePublicCloud));
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
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, unknownServiceType, ArmEnvironment.AzurePublicCloud));
        Assert.Contains("Unknown service type", exception.Message);
    }

    #endregion

    #region Sovereign Cloud Tests

    [Theory]
    // Azure China Cloud
    [InlineData("https://myregistry.azurecr.cn", "acr")]
    [InlineData("https://myconfig.azconfig.azure.cn", "appconfig")]
    [InlineData("https://mycomm.communication.azure.cn", "communication")]
    [InlineData("https://my-foundry.services.ai.azure.cn", "foundry")]
    [InlineData("https://my-resource.openai.azure.cn", "azure-openai")]
    [InlineData("https://my-resource.cognitiveservices.azure.cn", "azure-openai")]
    [InlineData("https://mynamespace.servicebus.chinacloudapi.cn", "servicebus")]
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
    [InlineData("https://my-foundry.services.ai.azure.us", "foundry")]
    [InlineData("https://my-resource.openai.azure.us", "azure-openai")]
    [InlineData("https://my-resource.cognitiveservices.azure.us", "azure-openai")]
    [InlineData("https://mynamespace.servicebus.usgovcloudapi.net", "servicebus")]
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
        Assert.Throws<ArgumentException>(() => EndpointValidator.ValidateExternalUrl(null!, ["github.com"]));
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
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud));
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
            () => EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud));
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
    [InlineData("2001:0000:4136:e378:8000:63bf:f7f7:f7f7")]  // Teredo client XOR → 8.8.8.8 (public)
    public void IsPrivateOrReservedIP_TeredoPublicIPv4_ReturnsFalse(string address)
    {
        // Teredo with client IPv4 8.8.8.8: bytes[12..15] = 0xf7 XOR 0xff = 0x08 → 8.8.8.8 (public)
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

    #region IsPrivateOrReservedIP - NAT64 embedded IPv4 bypass

    [Theory]
    [InlineData("64:ff9b::a9fe:a9fe")]   // NAT64 embedding 169.254.169.254 (IMDS)
    [InlineData("64:ff9b::a83f:8110")]   // NAT64 embedding 168.63.129.16 (WireServer)
    [InlineData("64:ff9b::7f00:1")]      // NAT64 embedding 127.0.0.1 (Loopback)
    [InlineData("64:ff9b::a00:1")]       // NAT64 embedding 10.0.0.1 (Private)
    [InlineData("64:ff9b::c0a8:101")]    // NAT64 embedding 192.168.1.1 (Private)
    public void IsPrivateOrReservedIP_NAT64EmbeddedPrivateIPv4_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_NAT64EmbeddedPublicIPv4_ReturnsFalse()
    {
        // 64:ff9b::808:808 embeds 8.8.8.8 (Google DNS - public)
        var ipAddress = IPAddress.Parse("64:ff9b::808:808");
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("64:ff9b:1:a9fe:a9:fe00::")]   // NAT64v2 /48: IMDS 169.254.169.254 in bytes[6-7,9-10]
    [InlineData("64:ff9b:1:7f00:0:100::")]      // NAT64v2 /48: Loopback 127.0.0.1 in bytes[6-7,9-10]
    [InlineData("64:ff9b:1:a9fe:a9:fe00:808:808")] // NAT64v2 EXPLOIT: IMDS in correct pos, public IP in suffix
    public void IsPrivateOrReservedIP_NAT64v2EmbeddedPrivateIPv4_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_NAT64v2EmbeddedPublicIPv4_ReturnsFalse()
    {
        // NAT64v2 /48: 8.8.8.8 in bytes[6-7,9-10] → public
        var ipAddress = IPAddress.Parse("64:ff9b:1:808:8:800::");
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - Site-local IPv6 (deprecated)

    [Theory]
    [InlineData("fec0::1")]        // Site-local
    [InlineData("feff::1")]        // Site-local upper bound
    public void IsPrivateOrReservedIP_SiteLocalIPv6_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - IPv4 TEST-NETs and reserved ranges

    [Theory]
    [InlineData("192.0.2.1")]      // TEST-NET-1 (RFC 5737)
    [InlineData("198.51.100.1")]   // TEST-NET-2 (RFC 5737)
    [InlineData("203.0.113.1")]    // TEST-NET-3 (RFC 5737)
    [InlineData("198.18.0.1")]     // Benchmarking (RFC 2544)
    [InlineData("198.19.255.255")] // Benchmarking upper bound
    [InlineData("192.0.0.1")]      // IANA special (RFC 6890)
    [InlineData("192.88.99.1")]    // 6to4 relay (RFC 7526)
    public void IsPrivateOrReservedIP_IPv4ReservedRanges_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - BMWG benchmarking IPv6

    [Theory]
    [InlineData("2001:2::1")]          // BMWG benchmarking (RFC 5180)
    [InlineData("2001:2:0:ffff::1")]   // BMWG upper bound within /48
    public void IsPrivateOrReservedIP_BMWGIPv6_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region ValidatePublicTargetUrl - Trailing dot FQDN bypass

    [Theory]
    [InlineData("http://nip.io./path")]
    [InlineData("http://sslip.io./path")]
    [InlineData("http://xip.io./path")]
    [InlineData("http://169.254.169.254.nip.io./path")]
    [InlineData("http://localhost./path")]
    public void ValidatePublicTargetUrl_TrailingDotReservedHost_ThrowsSecurityException(string url)
    {
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.Contains("reserved", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region IsPrivateOrReservedIP - IPv4-translated (SIIT) bypass

    [Theory]
    [InlineData("::ffff:0:a9fe:a9fe")]   // IPv4-translated IMDS 169.254.169.254
    [InlineData("::ffff:0:7f00:1")]      // IPv4-translated loopback 127.0.0.1
    [InlineData("::ffff:0:a00:1")]       // IPv4-translated 10.0.0.1
    [InlineData("::ffff:0:c0a8:101")]    // IPv4-translated 192.168.1.1
    [InlineData("::ffff:0:a83f:8110")]   // IPv4-translated WireServer 168.63.129.16
    public void IsPrivateOrReservedIP_IPv4TranslatedPrivateIPv4_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Fact]
    public void IsPrivateOrReservedIP_IPv4TranslatedPublicIPv4_ReturnsFalse()
    {
        // ::ffff:0:808:808 embeds 8.8.8.8 (public)
        var ipAddress = IPAddress.Parse("::ffff:0:808:808");
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IsPrivateOrReservedIP - IPv6 BMWG benchmarking prefix

    [Fact]
    public void IsPrivateOrReservedIP_IPv6Benchmarking_ReturnsTrue()
    {
        // 2001:2::/48 (RFC 5180) - benchmarking, non-routable
        var ipAddress = IPAddress.Parse("2001:2::1");
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region ValidatePublicTargetUrl - Scheme enforcement

    [Theory]
    [InlineData("ftp://8.8.8.8/")]
    [InlineData("gopher://8.8.8.8/")]
    [InlineData("file:///etc/passwd")]
    [InlineData("dict://8.8.8.8:11211/")]
    public void ValidatePublicTargetUrl_NonHttpScheme_ThrowsSecurityException(string url)
    {
        var exception = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
        Assert.Contains("HTTP or HTTPS", exception.Message);
    }

    [Theory]
    [InlineData("http://8.8.8.8/")]
    [InlineData("https://8.8.8.8/")]
    public void ValidatePublicTargetUrl_HttpSchemes_Allowed(string url)
    {
        // Should not throw for HTTP/HTTPS with public IPs
        EndpointValidator.ValidatePublicTargetUrl(url);
    }

    #endregion

    #region Additional Coverage Tests (MQ Wave 1)

    [Theory]
    [InlineData("8.8.8.8")]            // Google DNS
    [InlineData("1.1.1.1")]            // Cloudflare DNS
    [InlineData("13.107.42.14")]       // Microsoft public
    public void IsPrivateOrReservedIP_PublicIPv4_ReturnsFalse(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("2607:f8b0:4004:800::200e")]   // Google IPv6
    [InlineData("2606:4700:4700::1111")]        // Cloudflare IPv6
    public void IsPrivateOrReservedIP_PublicIPv6_ReturnsFalse(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("::ffff:8.8.8.8")]     // IPv4-mapped public
    [InlineData("::ffff:1.1.1.1")]     // IPv4-mapped public
    public void IsPrivateOrReservedIP_IPv4MappedPublic_ReturnsFalse(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("::8.8.8.8")]          // IPv4-compatible public
    [InlineData("::1.1.1.1")]          // IPv4-compatible public
    public void IsPrivateOrReservedIP_IPv4CompatiblePublic_ReturnsFalse(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("fe80::1")]            // Link-local
    [InlineData("fe80::abcd:ef01:2345:6789")]
    public void IsPrivateOrReservedIP_IPv6LinkLocal_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("172.15.255.255", false)]   // Just below 172.16/12
    [InlineData("172.16.0.0", true)]        // Range start
    [InlineData("172.31.255.255", true)]     // Range end
    [InlineData("172.32.0.0", false)]        // Just above range
    [InlineData("100.63.255.255", false)]    // Just below CGNAT
    [InlineData("100.64.0.0", true)]         // CGNAT start
    [InlineData("100.127.255.255", true)]    // CGNAT end
    [InlineData("100.128.0.0", false)]       // Just above CGNAT
    public void IsPrivateOrReservedIP_IPv4Boundaries(string address, bool expected)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.Equal(expected, EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("224.0.0.1")]          // Multicast
    [InlineData("239.255.255.255")]    // Multicast upper bound
    [InlineData("240.0.0.1")]          // Future reserved
    [InlineData("255.255.255.254")]    // Future reserved upper
    public void IsPrivateOrReservedIP_IPv4MulticastAndReserved_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("http://local.")]
    [InlineData("http://localtest.me.")]
    [InlineData("http://lvh.me.")]
    public void ValidatePublicTargetUrl_TrailingDotReservedHosts_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    [Theory]
    [InlineData("http://[::ffff:169.254.169.254]")]     // IPv4-mapped IMDS
    [InlineData("http://[::ffff:127.0.0.1]")]           // IPv4-mapped loopback
    [InlineData("http://[2002:a9fe:a9fe::1]")]          // 6to4 IMDS
    [InlineData("http://[::1]")]                         // IPv6 loopback
    public void ValidatePublicTargetUrl_IPv6TransitionInUrl_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    [Theory]
    [InlineData("http://evil.localhost")]
    [InlineData("http://sub.lvh.me")]
    [InlineData("http://any.sub.nip.io")]
    public void ValidatePublicTargetUrl_SubdomainOfReservedHost_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    [Fact]
    public void ValidatePublicTargetUrl_SanitizedErrorMessages_NoIPLeak()
    {
        // Verify that error messages for literal private IPs don't leak the address value
        var ex = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl("http://127.0.0.1/test"));
        Assert.DoesNotContain("127.0.0.1", ex.Message);
    }

    [Fact]
    public void ValidatePublicTargetUrl_DnsError_NoDetailLeak()
    {
        // Verify DNS error messages don't leak internal resolver details
        var ex = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl("http://this-host-does-not-exist-12345.invalid/test"));
        Assert.DoesNotContain("Details:", ex.Message);
    }

    #endregion

    #region ISATAP and Empty DNS Tests (MQ Wave 4)

    [Theory]
    [InlineData("2001:db8:1::5efe:a9fe:a9fe")]    // ISATAP global prefix with IMDS IPv4
    [InlineData("2001:db8:1::5efe:7f00:0001")]     // ISATAP global prefix with loopback
    [InlineData("2001:db8:1::5efe:0a00:0001")]     // ISATAP global prefix with 10.0.0.1
    public void IsPrivateOrReservedIP_IsatapEmbeddedPrivate_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("2607:f8b0::5efe:0808:0808")]      // ISATAP with 8.8.8.8 (public)
    public void IsPrivateOrReservedIP_IsatapEmbeddedPublic_ReturnsFalse(string address)
    {
        // ISATAP with public embedded IPv4 should not be blocked
        // Note: 2607:f8b0::/32 is Google's prefix — not in any blocked range
        // The ISATAP check extracts 8.8.8.8 which is public → returns false
        var ipAddress = IPAddress.Parse(address);
        Assert.False(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    [InlineData("2001:db8:1:0:200:5efe:a9fe:a9fe")]  // ISATAP u/l bit set with IMDS
    [InlineData("2001:db8:1:0:200:5efe:7f00:0001")]   // ISATAP u/l bit set with loopback
    public void IsPrivateOrReservedIP_IsatapULBitEmbeddedPrivate_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    #endregion

    #region IPv6 Loopback/Localhost Bypass Representations

    [Theory]
    // Alternate compressed and expanded forms of ::1
    [InlineData("0:0:0:0:0:0:0:1")]                    // Full expanded form of ::1
    [InlineData("0000:0000:0000:0000:0000:0000:0000:0001")] // Zero-padded full form
    [InlineData("0::0:1")]                              // Alternate compressed representation
    // Expanded IPv4-mapped loopback forms
    [InlineData("0:0:0:0:0:ffff:127.0.0.1")]           // Expanded IPv4-mapped form
    public void IsPrivateOrReservedIP_LoopbackAlternateRepresentations_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    // Various ::1 representations in URLs
    [InlineData("http://[0:0:0:0:0:0:0:1]/")]                      // Full expanded form
    [InlineData("http://[0000:0000:0000:0000:0000:0000:0000:0001]/")] // Zero-padded full form
    [InlineData("http://[0::0:1]/")]                                // Alternate compressed
    // Expanded IPv4-mapped loopback in URLs
    [InlineData("http://[0:0:0:0:0:ffff:127.0.0.1]/")]             // Expanded IPv4-mapped form
    public void ValidatePublicTargetUrl_LoopbackAlternateRepresentations_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    #endregion

    #region IPv6 Zone ID / Encoding Obfuscation Bypass

    [Theory]
    // Zone IDs with various encodings — .NET Uri parser strips zone IDs,
    // so the IP is still validated correctly as ::1 or fe80::1
    [InlineData("http://[::1%25eth0]/")]                // Zone ID with interface name
    [InlineData("http://[fe80::1%25eth0]/")]            // Link-local with zone ID
    public void ValidatePublicTargetUrl_IPv6WithZoneId_Blocked(string url)
    {
        // .NET's Uri parser handles %25 as literal % in zone IDs.
        // The underlying IP (::1 or fe80::1) is still private/reserved.
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    #endregion

    #region IPv6 Internal Network Targeting via IPv4-Mapped Addresses

    [Theory]
    // IPv4-mapped private IPs in URL form
    [InlineData("http://[::ffff:10.0.0.1]/")]          // IPv4-mapped internal IP
    [InlineData("http://[::ffff:192.168.1.1]/")]        // IPv4-mapped private range
    [InlineData("http://[::ffff:172.16.0.1]/")]         // IPv4-mapped private 172.16.x
    [InlineData("http://[::ffff:100.64.0.1]/")]         // IPv4-mapped CGNAT
    // Cloud metadata endpoint
    [InlineData("http://[::ffff:169.254.169.254]/")]    // IPv4-mapped cloud metadata endpoint
    public void ValidatePublicTargetUrl_IPv4MappedPrivateInUrl_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    [Theory]
    // Unique local (private) addresses
    [InlineData("http://[fd00:ec2::254]/")]             // Unique local (fc00::/7 private range)
    public void ValidatePublicTargetUrl_IPv6UniqueLocalAddresses_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    #endregion

    #region IPv6 Parser Confusion and Authority Bypass

    [Fact]
    public void ValidatePublicTargetUrl_CredentialSectionBypass_Blocked()
    {
        // http://attacker.com@[::1]/ — .NET Uri parser treats attacker.com as userinfo
        // and [::1] as the actual host, which is loopback
        var url = "http://attacker.com@[::1]/";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // If .NET parses it, the host should be ::1 (loopback) → blocked
            Assert.Throws<SecurityException>(() =>
                EndpointValidator.ValidatePublicTargetUrl(url));
        }
        // If .NET rejects the URI entirely, that's also safe (no request possible)
    }

    [Theory]
    // Non-standard ports targeting internal services
    [InlineData("http://[::1]:8080/")]                  // Non-standard port on loopback
    [InlineData("http://[::1]:80/")]                    // Standard port on loopback
    [InlineData("http://[::1]:443/")]                   // HTTPS port on loopback
    [InlineData("http://[::1]:3000/")]                  // Common dev server port
    public void ValidatePublicTargetUrl_LoopbackNonStandardPorts_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    [Fact]
    public void ValidatePublicTargetUrl_BracketlessIPv6_Rejected()
    {
        // http://::1/ without brackets — invalid per RFC 3986.
        // .NET's Uri.TryCreate should reject this, preventing any request.
        var url = "http://::1/";
        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            // If somehow parsed, it must still be blocked
            Assert.Throws<SecurityException>(() =>
                EndpointValidator.ValidatePublicTargetUrl(url));
        }
        else
        {
            // Invalid URI → safe (ValidatePublicTargetUrl would throw SecurityException)
            Assert.Throws<SecurityException>(() =>
                EndpointValidator.ValidatePublicTargetUrl(url));
        }
    }

    [Fact]
    public void ValidatePublicTargetUrl_AuthorityConfusion_ParsedCorrectly()
    {
        // http://[::1]:80@attacker.com/ — .NET Uri parser correctly identifies
        // attacker.com as the host (userinfo = [::1]:80), so this is NOT a bypass.
        // The request would go to attacker.com, not ::1.
        var url = "http://[::1]:80@attacker.com/";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // .NET correctly parses the host as attacker.com, not ::1
            Assert.Equal("attacker.com", uri.Host);
        }
        // If .NET rejects the URI entirely, that's also safe
    }

    #endregion

    #region IPv6 Cloud Metadata Bypass Variants

    [Theory]
    // Various ways to reach 169.254.169.254 (IMDS) via IPv6
    [InlineData("::ffff:169.254.169.254")]                    // IPv4-mapped (canonical)
    [InlineData("0:0:0:0:0:ffff:169.254.169.254")]           // IPv4-mapped expanded
    [InlineData("0000:0000:0000:0000:0000:ffff:a9fe:a9fe")]  // IPv4-mapped hex fully expanded
    // Various ways to reach 10.0.0.1 via IPv6
    [InlineData("0:0:0:0:0:ffff:10.0.0.1")]                  // Expanded IPv4-mapped
    [InlineData("0000:0000:0000:0000:0000:ffff:0a00:0001")]  // Fully expanded hex
    // Various ways to reach 192.168.1.1 via IPv6
    [InlineData("0:0:0:0:0:ffff:192.168.1.1")]               // Expanded IPv4-mapped
    public void IsPrivateOrReservedIP_CloudMetadataIPv6Variants_ReturnsTrue(string address)
    {
        var ipAddress = IPAddress.Parse(address);
        Assert.True(EndpointValidator.IsPrivateOrReservedIP(ipAddress));
    }

    [Theory]
    // URL-level tests for cloud metadata via IPv6
    [InlineData("http://[::ffff:169.254.169.254]/latest/meta-data/")]  // IMDS with path
    [InlineData("http://[::ffff:168.63.129.16]/machine")]              // Azure WireServer via IPv6
    public void ValidatePublicTargetUrl_CloudMetadataIPv6InUrl_Blocked(string url)
    {
        Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidatePublicTargetUrl(url));
    }

    #endregion
}
