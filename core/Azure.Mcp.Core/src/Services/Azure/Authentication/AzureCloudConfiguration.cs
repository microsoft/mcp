// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.ResourceManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Implementation of <see cref="IAzureCloudConfiguration"/> that reads from configuration.
/// </summary>
public class AzureCloudConfiguration : IAzureCloudConfiguration
{
    private const string DefaultAuthorityHost = "https://login.microsoftonline.com";

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureCloudConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to read from.</param>
    /// <param name="serviceStartOptions">Optional service start options that can provide the cloud configuration.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AzureCloudConfiguration(
        IConfiguration configuration,
        IOptions<ServiceStartOptions>? serviceStartOptions = null,
        ILogger<AzureCloudConfiguration>? logger = null)
    {
        // Try to get cloud configuration from various sources in priority order:
        // 1. ServiceStartOptions (--cloud command line argument)
        // 2. Configuration (appsettings.json or environment variables)
        var cloudValue = serviceStartOptions?.Value?.Cloud
            ?? configuration["cloud"]
            ?? configuration["Cloud"]
            ?? configuration["AZURE_CLOUD"]
            ?? Environment.GetEnvironmentVariable("AZURE_CLOUD");

        (AuthorityHost, ArmEnvironment) = ParseCloudValue(cloudValue);

        logger?.LogDebug(
            "Azure cloud configuration initialized. Cloud value: '{CloudValue}', AuthorityHost: '{AuthorityHost}', ArmEnvironment: '{ArmEnvironment}'",
            cloudValue ?? "(not specified)",
            AuthorityHost,
            ArmEnvironment);
    }

    /// <inheritdoc/>
    public Uri AuthorityHost { get; }

    /// <inheritdoc/>
    public ArmEnvironment ArmEnvironment { get; }

    private static (Uri authorityHost, ArmEnvironment armEnvironment) ParseCloudValue(string? cloudValue)
    {
        if (string.IsNullOrWhiteSpace(cloudValue))
        {
            return (new Uri(DefaultAuthorityHost), ArmEnvironment.AzurePublicCloud);
        }

        // Check if it's already a URL - in this case we only have authority host
        // and must default to public cloud for ARM (custom cloud scenario requires
        // additional configuration not currently supported)
        if (cloudValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return (new Uri(cloudValue), ArmEnvironment.AzurePublicCloud);
        }

        // Map common sovereign cloud names to authority hosts and ARM environments
        return cloudValue.ToLowerInvariant() switch
        {
            "azurecloud" or "azurepubliccloud" or "public" =>
                (new Uri("https://login.microsoftonline.com"), ArmEnvironment.AzurePublicCloud),
            "azurechinacloud" or "china" =>
                (new Uri("https://login.chinacloudapi.cn"), ArmEnvironment.AzureChina),
            "azureusgovernment" or "azureusgovernmentcloud" or "usgov" or "usgovernment" =>
                (new Uri("https://login.microsoftonline.us"), ArmEnvironment.AzureGovernment),
            _ => (new Uri(DefaultAuthorityHost), ArmEnvironment.AzurePublicCloud) // Default to public cloud if unknown
        };
    }
}
