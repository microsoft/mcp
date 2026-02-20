// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using System.Security;
using Azure.ResourceManager;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Validates Azure service endpoints.
/// </summary>
public static class EndpointValidator
{
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

        // Build cloud-specific suffixes for services that require validation
        var acrSuffix = isPublicCloud ? "azurecr.io"
            : isChinaCloud ? "azurecr.cn"
            : isGovCloud ? "azurecr.us"
            : "azurecr.io";

        var appConfigSuffix = isPublicCloud ? "azconfig.io"
            : isChinaCloud ? "azconfig.azure.cn"
            : isGovCloud ? "azconfig.azure.us"
            : "azconfig.io";

        var commSuffix = isPublicCloud ? "communication.azure.com"
            : isChinaCloud ? "communication.azure.cn"
            : isGovCloud ? "communication.azure.us"
            : "communication.azure.com";

        return new Dictionary<string, string[]>
        {
            // Azure Communication Services
            { "communication", [$".{commSuffix}"] },

            // Azure App Configuration
            { "appconfig", [$".{appConfigSuffix}"] },

            // Azure Container Registry
            { "acr", [$".{acrSuffix}"] },
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
    public static void ValidatePublicTargetUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new SecurityException($"Invalid URL format: {url}");
        }

        // Check if host is a literal IP address
        if (IPAddress.TryParse(uri.Host, out var ipAddress))
        {
            if (IsPrivateOrReservedIP(ipAddress))
            {
                throw new SecurityException(
                    $"Target URL '{url}' uses a private or reserved IP address ({ipAddress}). " +
                    "Targeting internal endpoints is not permitted.");
            }
        }
        else
        {
            // Check for reserved hostnames (catches localhost variations)
            var reservedHosts = new[]
            {
                "localhost",
                "local",
                "localtest.me",          // Common localhost alias
                "lvh.me",                // localhost variations
                "169.254.169.254.nip.io"  // IMDS bypass attempt
            };

            if (reservedHosts.Any(reserved =>
                uri.Host.Equals(reserved, StringComparison.OrdinalIgnoreCase) ||
                uri.Host.EndsWith($".{reserved}", StringComparison.OrdinalIgnoreCase)))
            {
                throw new SecurityException(
                    $"Target URL hostname '{uri.Host}' is reserved and cannot be targeted.");
            }

            // Resolve DNS and validate all resolved IPs
            try
            {
                var hostEntry = Dns.GetHostEntry(uri.Host);
                foreach (var resolvedIp in hostEntry.AddressList)
                {
                    if (IsPrivateOrReservedIP(resolvedIp))
                    {
                        throw new SecurityException(
                            $"Target URL hostname '{uri.Host}' resolves to a private or reserved IP address ({resolvedIp}). " +
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
                throw new SecurityException(
                    $"Unable to resolve hostname '{uri.Host}' for security validation. " +
                    $"Ensure the hostname is publicly resolvable. Details: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Checks if an IP address is private, reserved, or otherwise non-routable
    /// </summary>
    public static bool IsPrivateOrReservedIP(IPAddress ipAddress)
    {
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
        }
        else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Loopback: ::1
            if (ipAddress.Equals(IPAddress.IPv6Loopback))
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
        }

        return false;
    }
}
