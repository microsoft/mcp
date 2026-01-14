// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Microsoft.Extensions.Configuration;
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
    /// <param name="serviceStartOptions">The service start options (may not be configured yet).</param>
    public AzureCloudConfiguration(IConfiguration configuration, IOptions<ServiceStartOptions>? serviceStartOptions = null)
    {
        // Try to get cloud configuration from various sources in priority order:
        // 1. ServiceStartOptions (--cloud command line argument)
        // 2. Configuration (appsettings.json or environment variables)
        var cloudValue = serviceStartOptions?.Value?.Cloud
            ?? configuration["cloud"] 
            ?? configuration["Cloud"]
            ?? configuration["AZURE_CLOUD"]
            ?? Environment.GetEnvironmentVariable("AZURE_CLOUD");

        AuthorityHost = ParseCloudValue(cloudValue);
    }

    /// <inheritdoc/>
    public Uri AuthorityHost { get; }

    private static Uri ParseCloudValue(string? cloudValue)
    {
        if (string.IsNullOrWhiteSpace(cloudValue))
        {
            return new Uri(DefaultAuthorityHost);
        }

        // Check if it's already a URL
        if (cloudValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(cloudValue);
        }

        // Map common sovereign cloud names to authority hosts
        var authorityHostUrl = cloudValue.ToLowerInvariant() switch
        {
            "azurecloud" or "azurepubliccloud" or "public" => "https://login.microsoftonline.com",
            "azurechinacloud" or "china" => "https://login.chinacloudapi.cn",
            "azureusgovernment" or "azureusgovernmentcloud" or "usgov" or "usgovernment" => "https://login.microsoftonline.us",
            "azuregermanycloud" or "germany" => "https://login.microsoftonline.de",
            _ => DefaultAuthorityHost // Default to public cloud if unknown
        };
        
        return new Uri(authorityHostUrl);
    }
}
