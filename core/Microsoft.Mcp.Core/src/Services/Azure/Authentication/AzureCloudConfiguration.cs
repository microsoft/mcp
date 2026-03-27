// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Implementation of <see cref="IAzureCloudConfiguration"/> that reads from configuration.
/// </summary>
public class AzureCloudConfiguration : IAzureCloudConfiguration
{

    public enum AzureCloud
    {
        AzurePublicCloud,
        AzureChinaCloud,
        AzureUSGovernmentCloud,
    }

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
            ?? configuration["AZURE_CLOUD"]
            ?? configuration["azure_cloud"]
            ?? configuration["cloud"]
            ?? configuration["Cloud"]
            ?? Environment.GetEnvironmentVariable("AZURE_CLOUD");

        (AuthorityHost, ArmEnvironment, CloudType) = ParseCloudValue(cloudValue);

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

    public AzureCloud CloudType { get; }

    private static (Uri authorityHost, ArmEnvironment armEnvironment, AzureCloud cloudType) ParseCloudValue(string? cloudValue)
    {
        if (string.IsNullOrWhiteSpace(cloudValue))
        {
            return (AzureAuthorityHosts.AzurePublicCloud, ArmEnvironment.AzurePublicCloud, AzureCloud.AzurePublicCloud);
        }

        // Map common sovereign cloud names to authority hosts and ARM environments
        return cloudValue.ToLowerInvariant() switch
        {
            "azurecloud" or "azurepubliccloud" or "public" or "azurepublic" =>
                (AzureAuthorityHosts.AzurePublicCloud, ArmEnvironment.AzurePublicCloud, AzureCloud.AzurePublicCloud),
            "azurechinacloud" or "china" or "azurechina" =>
                (AzureAuthorityHosts.AzureChina, ArmEnvironment.AzureChina, AzureCloud.AzureChinaCloud),
            "azureusgovernment" or "azureusgovernmentcloud" or "usgov" or "usgovernment" =>
                (AzureAuthorityHosts.AzureGovernment, ArmEnvironment.AzureGovernment, AzureCloud.AzureUSGovernmentCloud),
            _ => throw new ArgumentException(
                $"Unrecognized cloud value '{cloudValue}'. Supported values are: AzureCloud, AzurePublicCloud, Public, AzurePublic, AzureChinaCloud, China, AzureChina, AzureUSGovernment, AzureUSGovernmentCloud, USGov, USGovernment.",
                nameof(cloudValue))
        };
    }
}
