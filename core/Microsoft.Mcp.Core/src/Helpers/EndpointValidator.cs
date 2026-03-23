// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using System.Security;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Validates Azure service endpoints.
/// </summary>
public static class EndpointValidator
{
    private static readonly string[] s_reservedHosts =
    [
        "localhost",
        "local",
        "localtest.me",          // Common localhost alias
        "lvh.me",                // localhost variations
        "traefik.me",            // Resolves to 127.0.0.1
        "localho.st",            // Resolves to 127.0.0.1
        "nip.io",                // Wildcard DNS - resolves to embedded IP
        "sslip.io",              // Wildcard DNS - resolves to embedded IP
        "xip.io",                // Wildcard DNS - resolves to embedded IP
    ];

    /// <summary>
    /// Gets the allowed domain suffixes for Azure services based on the cloud environment.
    /// </summary>
    /// <param name="armEnvironment">The ARM environment (cloud) to get suffixes for.</param>
    /// <returns>Dictionary mapping service types to their allowed domain suffixes.</returns>
    private static Dictionary<string, string[]> GetAllowedDomainSuffixes(ArmEnvironment armEnvironment)
    {
        // Determine which cloud we're in
        var isPublicCloud = armEnvironment.Equals(ArmEnvironment.AzurePublicCloud);
        var isChinaCloud = armEnvironment.Equals(ArmEnvironment.AzureChina);
        var isGovCloud = armEnvironment.Equals(ArmEnvironment.AzureGovernment);
        var isGermanyCloud = armEnvironment.Equals(ArmEnvironment.AzureGermany);

        // Build cloud-specific suffixes for services that require validation
        var acrSuffix = isPublicCloud ? "azurecr.io"
            : isChinaCloud ? "azurecr.cn"
            : isGovCloud ? "azurecr.us"
            : isGermanyCloud ? "azurecr.de"
            : "azurecr.io";

        var appConfigSuffix = isPublicCloud ? "azconfig.io"
            : isChinaCloud ? "azconfig.azure.cn"
            : isGovCloud ? "azconfig.azure.us"
            : isGermanyCloud ? "azconfig.azure.de"
            : "azconfig.io";

        var commSuffix = isPublicCloud ? "communication.azure.com"
            : isChinaCloud ? "communication.azure.cn"
            : isGovCloud ? "communication.azure.us"
            : isGermanyCloud ? "communication.azure.de"
            : "communication.azure.com";

        var foundrySuffix = isPublicCloud ? "services.ai.azure.com"
            : isChinaCloud ? "services.ai.azure.cn"
            : isGovCloud ? "services.ai.azure.us"
            : isGermanyCloud ? "services.ai.azure.de"
            : "services.ai.azure.com";

        var openaiSuffix = isPublicCloud ? "openai.azure.com"
            : isChinaCloud ? "openai.azure.cn"
            : isGovCloud ? "openai.azure.us"
            : isGermanyCloud ? "openai.azure.de"
            : "openai.azure.com";

        var cognitiveServicesSuffix = isPublicCloud ? "cognitiveservices.azure.com"
            : isChinaCloud ? "cognitiveservices.azure.cn"
            : isGovCloud ? "cognitiveservices.azure.us"
            : isGermanyCloud ? "cognitiveservices.azure.de"
            : "cognitiveservices.azure.com";

        return new Dictionary<string, string[]>
        {
            // Azure Communication Services
            { "communication", [$".{commSuffix}"] },

            // Azure App Configuration
            { "appconfig", [$".{appConfigSuffix}"] },

            // Azure Container Registry
            { "acr", [$".{acrSuffix}"] },

            // Microsoft Foundry (AI Services project endpoints)
            { "foundry", [$".{foundrySuffix}"] },

            // Azure OpenAI
            { "azure-openai", [$".{openaiSuffix}", $".{cognitiveServicesSuffix}"] },
        };
    }

    /// <summary>
    /// Validates that an endpoint belongs to an allowed Azure service domain.
    /// Uses Azure Public Cloud domains by default.
    /// </summary>
    /// <param name="endpoint">The endpoint URL to validate.</param>
    /// <param name="serviceType">The type of Azure service (e.g., "storage-blob", "keyvault").</param>
    public static void ValidateAzureServiceEndpoint(string endpoint, string serviceType)
    {
        ValidateAzureServiceEndpoint(endpoint, serviceType, ArmEnvironment.AzurePublicCloud);
    }

    /// <summary>
    /// Validates that an endpoint belongs to an allowed Azure service domain for the specified cloud environment.
    /// </summary>
    /// <param name="endpoint">The endpoint URL to validate.</param>
    /// <param name="serviceType">The type of Azure service (e.g., "storage-blob", "keyvault").</param>
    /// <param name="armEnvironment">The Azure cloud environment (Public, China, Government, etc.).</param>
    public static void ValidateAzureServiceEndpoint(string endpoint, string serviceType, ArmEnvironment armEnvironment)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            throw new SecurityException($"Invalid endpoint format: {endpoint}");
        }

        // Ensure HTTPS
        if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException(
                $"Endpoint must use HTTPS protocol. Got: {uri.Scheme}");
        }

        var allowedDomainSuffixes = GetAllowedDomainSuffixes(armEnvironment);

        if (!allowedDomainSuffixes.TryGetValue(serviceType, out var allowedSuffixes))
        {
            throw new ArgumentException($"Unknown service type: {serviceType}", nameof(serviceType));
        }

        // Validate domain: must exactly match suffix or be a proper subdomain
        var isValid = allowedSuffixes.Any(suffix =>
        {
            // Exact match (e.g., "azconfig.io")
            if (uri.Host.Equals(suffix.TrimStart('.'), StringComparison.OrdinalIgnoreCase))
                return true;

            // Proper subdomain match (e.g., "myconfig.azconfig.io" matches ".azconfig.io")
            // Ensure the suffix starts with a dot, then check if host ends with it
            if (suffix.StartsWith('.') && uri.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                // Ensure there's a subdomain portion and it doesn't contain path separators
                // This prevents path components from being interpreted as subdomains (e.g., "azconfig.io/evil")
                // Note: Multi-level subdomains like "sub.myconfig.azconfig.io" are valid and allowed
                var domainBeforeSuffix = uri.Host.Substring(0, uri.Host.Length - suffix.Length);
                return !string.IsNullOrEmpty(domainBeforeSuffix) && !domainBeforeSuffix.Contains('/');
            }

            return false;
        });

        if (!isValid)
        {
            var cloudName = armEnvironment.Equals(ArmEnvironment.AzurePublicCloud) ? "Azure Public Cloud"
                : armEnvironment.Equals(ArmEnvironment.AzureChina) ? "Azure China Cloud"
                : armEnvironment.Equals(ArmEnvironment.AzureGovernment) ? "Azure US Government Cloud"
                : armEnvironment.Equals(ArmEnvironment.AzureGermany) ? "Azure Germany Cloud"
                : "configured Azure cloud";

            throw new SecurityException(
                $"Endpoint host '{uri.Host}' is not a valid {serviceType} domain for {cloudName}. " +
                $"Expected domains: {string.Join(", ", allowedSuffixes)}");
        }
    }

    /// <summary>
    /// Validates that a URL is from an allowed external domain (GitHub, etc.)
    /// </summary>
    public static void ValidateExternalUrl(string url, string[] allowedHosts)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new SecurityException($"Invalid URL format: {url}");
        }

        // Ensure HTTPS for external URLs
        if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException(
                $"External URL must use HTTPS protocol. Got: {uri.Scheme}");
        }

        var isAllowed = allowedHosts.Any(host =>
            uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase));

        if (!isAllowed)
        {
            throw new SecurityException(
                $"URL host '{uri.Host}' is not in the allowed list. " +
                $"Allowed hosts: {string.Join(", ", allowedHosts)}");
        }
    }

    /// <summary>
    /// Validates that a target URL (for load testing, etc.) isn't pointing to internal resources.
    /// Performs DNS resolution to detect hostnames that resolve to private/reserved IPs.
    /// </summary>
    public static void ValidatePublicTargetUrl(string url, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new SecurityException($"Invalid URL format: {url}");
        }

        // Only allow HTTP and HTTPS schemes to prevent protocol-specific SSRF
        // (e.g., gopher://, file://, dict:// attacks)
        if (!uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
            !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException(
                $"Target URL must use HTTP or HTTPS protocol. Got: {uri.Scheme}");
        }

        // Check if host is a literal IP address
        if (IPAddress.TryParse(uri.Host, out var ipAddress))
        {
            if (IsPrivateOrReservedIP(ipAddress))
            {
                throw new SecurityException(
                    "Target URL uses a private or reserved IP address. " +
                    "Targeting internal endpoints is not permitted.");
            }
        }
        else
        {
            // Normalize FQDN trailing dot (e.g., "nip.io." -> "nip.io")
            // to prevent blocklist bypass via DNS absolute form
            var normalizedHost = uri.Host.TrimEnd('.');

            // Check for reserved hostnames (catches localhost variations)

            if (s_reservedHosts.Any(reserved =>
                normalizedHost.Equals(reserved, StringComparison.OrdinalIgnoreCase) ||
                normalizedHost.EndsWith($".{reserved}", StringComparison.OrdinalIgnoreCase)))
            {
                throw new SecurityException(
                    $"Target URL hostname '{uri.Host}' is reserved and cannot be targeted.");
            }

            // Resolve DNS and validate all resolved IPs
            try
            {
                var hostEntry = Dns.GetHostEntry(uri.Host);

                // Fail-closed: reject if DNS returns no addresses (prevents
                // bypass via empty AddressList where downstream resolves differently)
                if (hostEntry.AddressList.Length == 0)
                {
                    throw new SecurityException(
                        $"Target URL hostname '{uri.Host}' resolved to no IP addresses. " +
                        "Ensure the hostname resolves to a public IP address.");
                }

                foreach (var resolvedIp in hostEntry.AddressList)
                {
                    if (IsPrivateOrReservedIP(resolvedIp))
                    {
                        throw new SecurityException(
                            $"Target URL hostname '{uri.Host}' resolves to a private or reserved IP address. " +
                            "Targeting internal endpoints is not permitted.");
                    }
                }
            }
            catch (SecurityException)
            {
                throw; // Re-throw SecurityException from private IP check
            }
            catch (Exception ex)
            {
                // DNS resolution failure - treat as invalid for security
                logger?.LogWarning(ex, "DNS resolution failed for '{Host}': {Message}", uri.Host, ex.Message);
                throw new SecurityException(
                    $"Unable to resolve hostname '{uri.Host}' for security validation. " +
                    "Ensure the hostname is publicly resolvable.");
            }
        }
    }

    /// <summary>
    /// Checks if an IP address is private, reserved, or otherwise non-routable
    /// </summary>
    public static bool IsPrivateOrReservedIP(IPAddress ipAddress)
    {
        // Normalize IPv4-mapped IPv6 addresses (::ffff:0:0/96) to their IPv4 equivalent
        // so they are validated against IPv4 private/reserved ranges. Without this,
        // an attacker can bypass SSRF protection using DNS AAAA records like ::ffff:169.254.169.254.
        if (ipAddress.IsIPv4MappedToIPv6)
        {
            ipAddress = ipAddress.MapToIPv4();
        }

        var bytes = ipAddress.GetAddressBytes();

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // Loopback: 127.0.0.0/8
            if (bytes[0] == 127)
            {
                return true;
            }

            // Private: 10.0.0.0/8
            if (bytes[0] == 10)
            {
                return true;
            }

            // Private: 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                return true;
            }

            // Private: 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }

            // Link-local: 169.254.0.0/16 (includes IMDS at 169.254.169.254)
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                return true;
            }

            // WireServer: 168.63.129.16
            if (bytes[0] == 168 && bytes[1] == 63 && bytes[2] == 129 && bytes[3] == 16)
            {
                return true;
            }

            // Shared address space: 100.64.0.0/10
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
            {
                return true;
            }

            // Broadcast: 255.255.255.255
            if (bytes[0] == 255 && bytes[1] == 255 && bytes[2] == 255 && bytes[3] == 255)
            {
                return true;
            }

            // Reserved ranges
            if (bytes[0] == 0)
            {
                return true;  // 0.0.0.0/8
            }

            if (bytes[0] >= 224)
            {
                return true;  // Multicast (224.0.0.0/4) and Reserved (240.0.0.0/4)
            }

            // TEST-NET-1: 192.0.2.0/24 (RFC 5737 https://datatracker.ietf.org/doc/html/rfc5737) - documentation, non-routable
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2)
            {
                return true;
            }

            // TEST-NET-2: 198.51.100.0/24 (RFC 5737 https://datatracker.ietf.org/doc/html/rfc5737) - documentation, non-routable
            if (bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
            {
                return true;
            }

            // TEST-NET-3: 203.0.113.0/24 (RFC 5737 https://datatracker.ietf.org/doc/html/rfc5737) - documentation, non-routable
            if (bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)
            {
                return true;
            }

            // Benchmarking: 198.18.0.0/15 (RFC 2544 https://datatracker.ietf.org/doc/html/rfc2544) - testing, non-routable
            if (bytes[0] == 198 && (bytes[1] == 18 || bytes[1] == 19))
            {
                return true;
            }

            // IANA special: 192.0.0.0/24 (RFC 6890 https://datatracker.ietf.org/doc/html/rfc6890)
            if (bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0)
            {
                return true;
            }

            // 6to4 relay: 192.88.99.0/24 (RFC 7526 https://datatracker.ietf.org/doc/html/rfc7526, deprecated)
            if (bytes[0] == 192 && bytes[1] == 88 && bytes[2] == 99)
            {
                return true;
            }
        }
        else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Loopback: ::1
            if (ipAddress.Equals(IPAddress.IPv6Loopback))
            {
                return true;
            }

            // Unspecified: :: (equivalent to 0.0.0.0)
            if (ipAddress.Equals(IPAddress.IPv6Any))
            {
                return true;
            }

            // Private: fc00::/7
            if ((bytes[0] & 0xfe) == 0xfc)
            {
                return true;
            }

            // Link-local: fe80::/10
            if (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80)
            {
                return true;
            }

            // Site-local: fec0::/10 (RFC 3879 https://datatracker.ietf.org/doc/html/rfc3879, deprecated but may still be routed)
            if (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0xc0)
            {
                return true;
            }

            // Multicast: ff00::/8
            if (bytes[0] == 0xff)
            {
                return true;
            }

            // Discard prefix: 0100::/64 (RFC 6666 https://datatracker.ietf.org/doc/html/rfc6666)
            if (bytes[0] == 0x01 && bytes[1] == 0x00 &&
                bytes[2] == 0x00 && bytes[3] == 0x00 &&
                bytes[4] == 0x00 && bytes[5] == 0x00 &&
                bytes[6] == 0x00 && bytes[7] == 0x00)
            {
                return true;
            }

            // Documentation: 2001:db8::/32 (RFC 3849 https://datatracker.ietf.org/doc/html/rfc3849) - non-routable
            if (bytes[0] == 0x20 && bytes[1] == 0x01 &&
                bytes[2] == 0x0d && bytes[3] == 0xb8)
            {
                return true;
            }

            // BMWG benchmarking: 2001:0002::/48 (RFC 5180 https://datatracker.ietf.org/doc/html/rfc5180) - non-routable
            if (bytes[0] == 0x20 && bytes[1] == 0x01 &&
                bytes[2] == 0x00 && bytes[3] == 0x02 &&
                bytes[4] == 0x00 && bytes[5] == 0x00)
            {
                return true;
            }

            // NAT64: 64:ff9b::/96 (RFC 6052 https://datatracker.ietf.org/doc/html/rfc6052) - embeds IPv4 in last 32 bits
            // On NAT64 infrastructure, 64:ff9b::a9fe:a9fe translates to 169.254.169.254
            if (bytes[0] == 0x00 && bytes[1] == 0x64 &&
                bytes[2] == 0xff && bytes[3] == 0x9b &&
                bytes[4] == 0x00 && bytes[5] == 0x00 &&
                bytes[6] == 0x00 && bytes[7] == 0x00 &&
                bytes[8] == 0x00 && bytes[9] == 0x00 &&
                bytes[10] == 0x00 && bytes[11] == 0x00)
            {
                var embeddedIpv4 = new IPAddress([bytes[12], bytes[13], bytes[14], bytes[15]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // NAT64 v2: 64:ff9b:1::/48 (RFC 8215 https://datatracker.ietf.org/doc/html/rfc8215 + RFC 6052 https://datatracker.ietf.org/doc/html/rfc6052#section-2.2 Section 2.2)
            // For /48 prefix, IPv4 is split: first 16 bits in bytes[6-7],
            // last 16 bits in bytes[9-10], with byte[8] reserved ("u" byte).
            if (bytes[0] == 0x00 && bytes[1] == 0x64 &&
                bytes[2] == 0xff && bytes[3] == 0x9b &&
                bytes[4] == 0x00 && bytes[5] == 0x01)
            {
                var embeddedIpv4 = new IPAddress([bytes[6], bytes[7], bytes[9], bytes[10]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // 6to4: 2002::/16 - embeds IPv4 in bytes[2..5]; validate the embedded address
            if (bytes[0] == 0x20 && bytes[1] == 0x02)
            {
                var embeddedIpv4 = new IPAddress([bytes[2], bytes[3], bytes[4], bytes[5]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // Teredo: 2001:0000::/32 - client IPv4 in bytes[12..15] XOR'd with 0xFF
            if (bytes[0] == 0x20 && bytes[1] == 0x01 &&
                bytes[2] == 0x00 && bytes[3] == 0x00)
            {
                var embeddedIpv4 = new IPAddress([
                    (byte)(bytes[12] ^ 0xff),
                    (byte)(bytes[13] ^ 0xff),
                    (byte)(bytes[14] ^ 0xff),
                    (byte)(bytes[15] ^ 0xff)]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // IPv4-compatible (deprecated): ::x.x.x.x - validate embedded IPv4
            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0 &&
                bytes[4] == 0 && bytes[5] == 0 && bytes[6] == 0 && bytes[7] == 0 &&
                bytes[8] == 0 && bytes[9] == 0 && bytes[10] == 0 && bytes[11] == 0)
            {
                var embeddedIpv4 = new IPAddress([bytes[12], bytes[13], bytes[14], bytes[15]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // IPv4-translated: ::ffff:0:x.x.x.x (RFC 2765 https://datatracker.ietf.org/doc/html/rfc2765 / RFC 6145 https://datatracker.ietf.org/doc/html/rfc6145, SIIT)
            // Different from IPv4-mapped (::ffff:x.x.x.x) — bytes[8-9]=FF:FF instead of [10-11].
            // .NET's IsIPv4MappedToIPv6 does NOT recognize this prefix!
            // On SIIT/NAT64 infrastructure, ::ffff:0:a9fe:a9fe translates to 169.254.169.254.
            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0 &&
                bytes[4] == 0 && bytes[5] == 0 && bytes[6] == 0 && bytes[7] == 0 &&
                bytes[8] == 0xff && bytes[9] == 0xff &&
                bytes[10] == 0 && bytes[11] == 0)
            {
                var embeddedIpv4 = new IPAddress([bytes[12], bytes[13], bytes[14], bytes[15]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // ISATAP (RFC 5214 https://datatracker.ietf.org/doc/html/rfc5214): embeds IPv4 in last 4 bytes with interface ID
            // pattern ::0:5efe:x.x.x.x (modified EUI-64 with 5efe marker).
            // Link-local ISATAP (fe80::5efe:...) is already blocked by fe80::/10 above,
            // but global-prefix ISATAP (e.g., 2001:db8::5efe:a9fe:a9fe) would bypass.
            if (bytes[8] == 0x00 && bytes[9] == 0x00 &&
                bytes[10] == 0x5e && bytes[11] == 0xfe)
            {
                var embeddedIpv4 = new IPAddress([bytes[12], bytes[13], bytes[14], bytes[15]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }

            // Also check ISATAP with u/l bit set (::200:5efe:x.x.x.x)
            if (bytes[8] == 0x02 && bytes[9] == 0x00 &&
                bytes[10] == 0x5e && bytes[11] == 0xfe)
            {
                var embeddedIpv4 = new IPAddress([bytes[12], bytes[13], bytes[14], bytes[15]]);
                return IsPrivateOrReservedIP(embeddedIpv4);
            }
        }

        return false;
    }
}
