// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Kusto.Services;

internal sealed record KustoEndpoint(string ClusterUri, string Scope)
{
    private static readonly string[] s_validKustoDomainSuffixes =
    [
        ".dxp.aad.azure.com",
        ".dxp-dev.aad.azure.com",
        ".kusto.azuresynapse.net",
        ".kusto.windows.net",
        ".kustodev.azuresynapse-dogfood.net",
        ".kustodev.windows.net",
        ".kustomfa.windows.net",
        ".kusto.data.microsoft.com",
        ".kusto.fabric.microsoft.com",
        ".arg.core.windows.net",
        ".adx.loganalytics.azure.com",
        ".adx.applicationinsights.azure.com",
        ".adx.monitor.azure.com",
        ".kusto.usgovcloudapi.net",
        ".kustomfa.usgovcloudapi.net",
        ".adx.loganalytics.azure.us",
        ".adx.applicationinsights.azure.us",
        ".adx.monitor.azure.us",
        ".kusto.azuresynapse.azure.cn",
        ".kusto.chinacloudapi.cn",
        ".kustomfa.chinacloudapi.cn",
        ".adx.loganalytics.azure.cn",
        ".adx.applicationinsights.azure.cn",
        ".adx.monitor.azure.cn",
        ".kusto.sovcloud-api.de",
        ".kustomfa.sovcloud-api.de"
    ];

    private static readonly HashSet<string> s_validKustoHostnames = new(StringComparer.OrdinalIgnoreCase)
    {
        "kusto.aria.microsoft.com",
        "eu.kusto.aria.microsoft.com",
        "ade.applicationinsights.io",
        "ade.loganalytics.io",
        "adx.aimon.applicationinsights.azure.com",
        "adx.applicationinsights.azure.com",
        "adx.loganalytics.azure.com",
        "adx.monitor.azure.com",
        "adx.applicationinsights.azure.us",
        "adx.loganalytics.azure.us",
        "adx.monitor.azure.us",
        "adx.applicationinsights.azure.cn",
        "adx.loganalytics.azure.cn",
        "adx.monitor.azure.cn",
        "adx.applicationinsights.azure.de",
        "adx.loganalytics.azure.de",
        "adx.monitor.azure.de"
    };

    public static KustoEndpoint Create(string clusterUri, IAzureCloudConfiguration cloudConfiguration)
    {
        ArgumentNullException.ThrowIfNull(cloudConfiguration);
        EnsureCustomCloudConfigured(cloudConfiguration);

        var isCustomCloud = cloudConfiguration.CloudType == AzureCloudConfiguration.AzureCloud.CustomCloud;
        var uri = ParseClusterUri(clusterUri, isCustomCloud);
        var host = uri.Host.ToLowerInvariant();
        var scope = cloudConfiguration.CloudType == AzureCloudConfiguration.AzureCloud.CustomCloud
            ? GetCustomCloudScope(host, cloudConfiguration.Kusto!)
            : GetBuiltInScope(host);

        return new(uri.AbsoluteUri.TrimEnd('/'), scope);
    }

    public static void EnsureCustomCloudConfigured(IAzureCloudConfiguration cloudConfiguration)
    {
        ArgumentNullException.ThrowIfNull(cloudConfiguration);

        if (cloudConfiguration.CloudType == AzureCloudConfiguration.AzureCloud.CustomCloud &&
            cloudConfiguration.Kusto == null)
        {
            throw new InvalidOperationException(
                "Custom cloud Kusto data-plane operations require the kusto capability.");
        }
    }

    private static Uri ParseClusterUri(string clusterUri, bool requireOrigin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clusterUri);

        if (!clusterUri.Equals(clusterUri.Trim(), StringComparison.Ordinal) ||
            clusterUri.Any(c => c > 127) ||
            clusterUri.Contains("\\", StringComparison.Ordinal) ||
            clusterUri.Contains("%", StringComparison.Ordinal) ||
            !Uri.TryCreate(clusterUri, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            Uri.CheckHostName(uri.Host) != UriHostNameType.Dns ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            HasExplicitPort(clusterUri) ||
            (requireOrigin && uri.AbsolutePath != "/" && !string.IsNullOrEmpty(uri.AbsolutePath)) ||
            uri.Host.EndsWith(".", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                requireOrigin
                    ? "Kusto cluster URI must be a canonical HTTPS origin."
                    : "Kusto cluster URI must be a canonical HTTPS endpoint.",
                nameof(clusterUri));
        }

        return uri;
    }

    private static string GetCustomCloudScope(string host, KustoCloudConfiguration configuration)
    {
        var suffix = configuration.EndpointSuffix;
        if (!IsValidSuffixHost(host, suffix))
        {
            throw new ArgumentException(
                $"Invalid Kusto cluster URI. Host '{host}' is not within the configured custom cloud Kusto endpoint suffix.",
                nameof(host));
        }

        return configuration.DefaultScope;
    }

    private static string GetBuiltInScope(string host)
    {
        if (!IsValidBuiltInHost(host))
        {
            throw new ArgumentException(
                $"Invalid Kusto cluster URI. Host '{host}' is not a recognized Azure Data Explorer endpoint.",
                nameof(host));
        }

        if (host.EndsWith(".chinacloudapi.cn", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".azure.cn", StringComparison.OrdinalIgnoreCase))
        {
            return "https://kusto.kusto.chinacloudapi.cn/.default";
        }

        if (host.EndsWith(".usgovcloudapi.net", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".azure.us", StringComparison.OrdinalIgnoreCase))
        {
            return "https://kusto.kusto.usgovcloudapi.net/.default";
        }

        return "https://kusto.kusto.windows.net/.default";
    }

    private static bool IsValidBuiltInHost(string host)
    {
        if (s_validKustoHostnames.Contains(host))
        {
            return true;
        }

        return Array.Exists(s_validKustoDomainSuffixes, suffix => IsValidSuffixHost(host, suffix));
    }

    private static bool IsValidSuffixHost(string host, string suffix)
    {
        if (!host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var clusterPart = host[..^suffix.Length];
        if (string.IsNullOrEmpty(clusterPart))
        {
            return false;
        }

        return clusterPart.Split('.').All(IsValidHostnameSegment);
    }

    private static bool IsValidHostnameSegment(string segment) =>
        segment.Length is > 0 and <= 63 &&
        char.IsLetterOrDigit(segment[0]) &&
        char.IsLetterOrDigit(segment[^1]) &&
        segment.All(c => char.IsLetterOrDigit(c) || c == '-');

    private static bool HasExplicitPort(string value)
    {
        var authorityStart = Uri.UriSchemeHttps.Length + Uri.SchemeDelimiter.Length;
        var authorityEnd = value.IndexOf('/', authorityStart);
        var authority = authorityEnd < 0 ? value[authorityStart..] : value[authorityStart..authorityEnd];
        return authority.Contains(":", StringComparison.Ordinal);
    }
}
