// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;

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
        CustomCloud,
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureCloudConfiguration"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to read from.</param>
    /// <param name="runtimeConfiguration">Optional runtime configurations that can provide the cloud configuration.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AzureCloudConfiguration(
        IConfiguration configuration,
        IOptions<ServerRuntimeConfiguration>? runtimeConfiguration = null,
        ILogger<AzureCloudConfiguration>? logger = null)
    {
        // Try to get cloud configuration from various sources in priority order:
        // 1. ServerRuntimeConfiguration (--cloud command line argument)
        // 2. Configuration (appsettings.json or environment variables)
        var cloudValue = runtimeConfiguration?.Value?.Cloud
            ?? configuration["AZURE_CLOUD"]
            ?? configuration["azure_cloud"]
            ?? configuration["cloud"]
            ?? configuration["Cloud"]
            ?? Environment.GetEnvironmentVariable("AZURE_CLOUD");

        var customCloudConfig = runtimeConfiguration?.Value?.CustomCloudConfig
            ?? configuration["CUSTOM_CLOUD_CONFIG"]
            ?? Environment.GetEnvironmentVariable("CUSTOM_CLOUD_CONFIG");

        var profile = ParseCloudValue(cloudValue, customCloudConfig);
        AuthorityHost = profile.AuthorityHost;
        ArmEnvironment = profile.ArmEnvironment;
        CloudType = profile.CloudType;
        LogAnalytics = profile.LogAnalytics;
        Kusto = profile.Kusto;

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

    public CloudServiceConfiguration? LogAnalytics { get; }

    public KustoCloudConfiguration? Kusto { get; }

    private static AzureCloudProfile ParseCloudValue(
        string? cloudValue,
        string? customCloudConfig)
    {
        if (string.IsNullOrWhiteSpace(cloudValue))
        {
            return CreateBuiltIn(
                AzureAuthorityHosts.AzurePublicCloud,
                ArmEnvironment.AzurePublicCloud,
                AzureCloud.AzurePublicCloud,
                "https://api.loganalytics.io");
        }

        // Map common sovereign cloud names to authority hosts and ARM environments
        return cloudValue.ToLowerInvariant() switch
        {
            "azurecloud" or "azurepubliccloud" or "public" or "azurepublic" =>
                CreateBuiltIn(AzureAuthorityHosts.AzurePublicCloud, ArmEnvironment.AzurePublicCloud, AzureCloud.AzurePublicCloud, "https://api.loganalytics.io"),
            "azurechinacloud" or "china" or "azurechina" =>
                CreateBuiltIn(AzureAuthorityHosts.AzureChina, ArmEnvironment.AzureChina, AzureCloud.AzureChinaCloud, "https://api.loganalytics.azure.cn"),
            "azureusgovernment" or "azureusgovernmentcloud" or "usgov" or "usgovernment" =>
                CreateBuiltIn(AzureAuthorityHosts.AzureGovernment, ArmEnvironment.AzureGovernment, AzureCloud.AzureUSGovernmentCloud, "https://api.loganalytics.us"),
            "custom" => LoadCustomCloud(customCloudConfig),
            _ => throw new ArgumentException(
                $"Unrecognized cloud value '{cloudValue}'. Supported values are: AzureCloud, AzurePublicCloud, Public, AzurePublic, AzureChinaCloud, China, AzureChina, AzureUSGovernment, AzureUSGovernmentCloud, USGov, USGovernment, custom.",
                nameof(cloudValue))
        };
    }

    private static AzureCloudProfile CreateBuiltIn(
        Uri authorityHost,
        ArmEnvironment armEnvironment,
        AzureCloud cloudType,
        string logAnalyticsEndpoint)
    {
        var endpoint = new Uri(logAnalyticsEndpoint);
        return new(
            authorityHost,
            armEnvironment,
            cloudType,
            new(endpoint, endpoint.AbsoluteUri.TrimEnd('/')),
            null);
    }

    private static AzureCloudProfile LoadCustomCloud(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A custom cloud configuration file is required when cloud is 'custom'.", nameof(path));
        }

        var metadata = JsonSerializer.Deserialize(
            File.ReadAllText(path),
            CustomCloudMetadataJsonContext.Default.CustomCloudMetadata)
            ?? throw new ArgumentException("The custom cloud configuration file is empty.", nameof(path));

        var authorityHost = ParseHttpsOrigin(metadata.AuthorityHost, nameof(metadata.AuthorityHost));
        if (metadata.Arm == null)
        {
            throw new ArgumentException("Custom cloud metadata must specify the arm capability.", nameof(path));
        }

        var arm = ParseServiceCapability(metadata.Arm, "arm");
        var logAnalytics = metadata.LogAnalytics == null
            ? null
            : ParseServiceCapability(metadata.LogAnalytics, "logAnalytics");
        var kusto = metadata.Kusto == null
            ? null
            : ParseKustoCapability(metadata.Kusto);

        return new(
            authorityHost,
            new ArmEnvironment(arm.Endpoint, arm.Audience),
            AzureCloud.CustomCloud,
            logAnalytics,
            kusto);
    }

    private static CloudServiceConfiguration ParseServiceCapability(
        CustomCloudServiceMetadata metadata,
        string propertyName)
    {
        var endpoint = ParseHttpsOrigin(metadata.Endpoint, $"{propertyName}.endpoint");
        var audience = ParseHttpsAudience(metadata.Audience, $"{propertyName}.audience");
        return new(endpoint, audience);
    }

    private static KustoCloudConfiguration ParseKustoCapability(CustomCloudKustoMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.EndpointSuffix))
        {
            throw new ArgumentException(
                "Custom cloud metadata property 'kusto.endpointSuffix' must be specified.",
                nameof(metadata.EndpointSuffix));
        }

        return new(
            NormalizeKustoEndpointSuffix(metadata.EndpointSuffix),
            ParseHttpsAudience(metadata.Audience, "kusto.audience"));
    }

    private static string NormalizeKustoEndpointSuffix(string value)
    {
        if (!value.Equals(value.Trim(), StringComparison.Ordinal) ||
            value.Any(c => c > 127) ||
            value.Contains("*", StringComparison.Ordinal) ||
            value.Contains("%", StringComparison.Ordinal) ||
            value.Contains("/", StringComparison.Ordinal) ||
            value.Contains("\\", StringComparison.Ordinal) ||
            value.Contains(":", StringComparison.Ordinal) ||
            value.Contains("@", StringComparison.Ordinal) ||
            value.Contains("?", StringComparison.Ordinal) ||
            value.Contains("#", StringComparison.Ordinal) ||
            value.EndsWith(".", StringComparison.Ordinal))
        {
            throw new ArgumentException("Custom cloud metadata property 'kusto.endpointSuffix' must be a DNS suffix.", nameof(value));
        }

        var host = value.TrimStart('.');
        if (Uri.CheckHostName(host) != UriHostNameType.Dns || host.Split('.').Length < 2)
        {
            throw new ArgumentException("Custom cloud metadata property 'kusto.endpointSuffix' must be a multi-label DNS suffix.", nameof(value));
        }

        foreach (var label in host.Split('.'))
        {
            if (label.Length is 0 or > 63 ||
                !char.IsLetterOrDigit(label[0]) ||
                !char.IsLetterOrDigit(label[^1]) ||
                label.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
            {
                throw new ArgumentException("Custom cloud metadata property 'kusto.endpointSuffix' contains an invalid DNS label.", nameof(value));
            }
        }

        return $".{host.ToLowerInvariant()}";
    }

    private static string ParseHttpsAudience(string? value, string propertyName)
    {
        return ParseHttpsOrigin(value, propertyName).AbsoluteUri.TrimEnd('/');
    }

    private static bool HasExplicitPort(string value)
    {
        var authorityStart = Uri.UriSchemeHttps.Length + Uri.SchemeDelimiter.Length;
        var authorityEnd = value.IndexOf('/', authorityStart);
        var authority = authorityEnd < 0 ? value[authorityStart..] : value[authorityStart..authorityEnd];
        return authority.Contains(":", StringComparison.Ordinal);
    }

    private static Uri ParseHttpsOrigin(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !value.Equals(value.Trim(), StringComparison.Ordinal) ||
            value.Any(c => c > 127) ||
            value.Contains("%", StringComparison.Ordinal) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            Uri.CheckHostName(uri.Host) != UriHostNameType.Dns ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            HasExplicitPort(value) ||
            uri.AbsolutePath != "/")
        {
            throw new ArgumentException(
                $"Custom cloud metadata property '{propertyName}' must be a canonical HTTPS origin.",
                propertyName);
        }

        return uri;
    }
}
