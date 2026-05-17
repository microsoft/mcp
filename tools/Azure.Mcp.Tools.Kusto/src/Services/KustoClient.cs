// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Core;

namespace Azure.Mcp.Tools.Kusto.Services;

public sealed class KustoClient(
    string clusterUri,
    TokenCredential tokenCredential,
    string userAgent,
    IHttpClientFactory httpClientFactory)
{
    // Valid Kusto cluster domain suffixes from official Kusto endpoints configuration
    private static readonly string[] s_validKustoDomainSuffixes =
    [
        // Public cloud
        ".dxp.aad.azure.com",
        ".dxp-dev.aad.azure.com",
        ".kusto.azuresynapse.net",
        ".kusto.windows.net",
        ".kustodev.azuresynapse-dogfood.net",
        ".kustodev.windows.net",
        ".kustomfa.windows.net",
        ".kusto.data.microsoft.com",
        ".kusto.fabric.microsoft.com",
        ".adx.loganalytics.azure.com",
        ".adx.applicationinsights.azure.com",
        ".adx.monitor.azure.com",
        // US Government
        ".kusto.usgovcloudapi.net",
        ".kustomfa.usgovcloudapi.net",
        ".adx.loganalytics.azure.us",
        ".adx.applicationinsights.azure.us",
        ".adx.monitor.azure.us",
        // China
        ".kusto.azuresynapse.azure.cn",
        ".kusto.chinacloudapi.cn",
        ".kustomfa.chinacloudapi.cn",
        ".adx.loganalytics.azure.cn",
        ".adx.applicationinsights.azure.cn",
        ".adx.monitor.azure.cn",
        // Germany
        ".kusto.sovcloud-api.de",
        ".kustomfa.sovcloud-api.de"
    ];

    // Exact hostnames that are valid Kusto endpoints
    private static readonly HashSet<string> s_validKustoHostnames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Public cloud
        "kusto.aria.microsoft.com",
        "eu.kusto.aria.microsoft.com",
        "ade.applicationinsights.io",
        "ade.loganalytics.io",
        "adx.aimon.applicationinsights.azure.com",
        "adx.applicationinsights.azure.com",
        "adx.loganalytics.azure.com",
        "adx.monitor.azure.com",
        // US Government
        "adx.applicationinsights.azure.us",
        "adx.loganalytics.azure.us",
        "adx.monitor.azure.us",
        // China
        "adx.applicationinsights.azure.cn",
        "adx.loganalytics.azure.cn",
        "adx.monitor.azure.cn",
        // Germany
        "adx.applicationinsights.azure.de",
        "adx.loganalytics.azure.de",
        "adx.monitor.azure.de"
    };

    private readonly string _clusterUri = ValidateAndNormalizeClusterUri(clusterUri);
    private readonly TokenCredential _tokenCredential = tokenCredential;
    private readonly string _userAgent = userAgent;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private static readonly TimeSpan s_httpClientTimeout = TimeSpan.FromSeconds(240);
    private static readonly string s_application = "AzureMCP";
    private static readonly string s_clientRequestIdPrefix = "AzMcp";

    /// <summary>
    /// Validates that the cluster URI is a valid Azure Data Explorer endpoint.
    /// Prevents SSRF attacks by rejecting arbitrary URLs.
    /// </summary>
    private static string ValidateAndNormalizeClusterUri(string clusterUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clusterUri, nameof(clusterUri));

        // Normalize: remove trailing slash
        var normalizedUri = clusterUri.TrimEnd('/');

        if (!Uri.TryCreate(normalizedUri, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException(
                $"Invalid Kusto cluster URI format: '{clusterUri}'",
                nameof(clusterUri));
        }

        // Must be HTTPS
        if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Kusto cluster URI must use HTTPS scheme.",
                nameof(clusterUri));
        }

        // Validate it's a legitimate Kusto cluster domain using simple string matching
        if (!IsValidKustoHost(uri.Host))
        {
            throw new ArgumentException(
                $"Invalid Kusto cluster URI. Must be a valid Azure Data Explorer cluster endpoint (e.g., https://clustername.region.kusto.windows.net). Received host: '{uri.Host}'",
                nameof(clusterUri));
        }

        return normalizedUri;
    }

    /// <summary>
    /// Checks if the host is a valid Kusto cluster domain.
    /// Uses simple string matching instead of regex to avoid ReDoS attacks.
    /// </summary>
    private static bool IsValidKustoHost(string host)
    {
        // Check for exact hostname match first (O(1) lookup with HashSet)
        if (s_validKustoHostnames.Contains(host))
        {
            return true;
        }

        // Check if host ends with one of the valid Kusto domain suffixes
        var matchedSuffix = Array.Find(s_validKustoDomainSuffixes,
            suffix => host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

        if (matchedSuffix == null)
        {
            return false;
        }

        // Extract the cluster name part (everything before the suffix)
        var clusterPart = host[..^matchedSuffix.Length];

        // Cluster part must not be empty
        if (string.IsNullOrEmpty(clusterPart))
        {
            return false;
        }

        // Validate cluster part contains only valid hostname characters
        // Split by dots and validate each segment
        var segments = clusterPart.Split('.');

        // Validate each segment contains only valid characters
        foreach (var segment in segments)
        {
            if (!IsValidHostnameSegment(segment))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates a hostname segment contains only alphanumeric characters and hyphens,
    /// starts with an alphanumeric character, and is not empty.
    /// </summary>
    private static bool IsValidHostnameSegment(string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return false;
        }

        // Must start with alphanumeric
        if (!char.IsLetterOrDigit(segment[0]))
        {
            return false;
        }

        // All characters must be alphanumeric or hyphen
        foreach (var c in segment)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines the appropriate Kusto token scope based on the cluster URI domain.
    /// </summary>
    private string GetKustoScope()
    {
        var uri = new Uri(_clusterUri);
        var host = uri.Host;

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

    public Task<KustoResult> ExecuteQueryCommandAsync(string database, string text, CancellationToken cancellationToken)
        => ExecuteCommandAsync("/v1/rest/query", database, text, cancellationToken);

    public Task<KustoResult> ExecuteControlCommandAsync(string database, string text, CancellationToken cancellationToken)
        => ExecuteCommandAsync("/v1/rest/mgmt", database, text, cancellationToken);

    private async Task<KustoResult> ExecuteCommandAsync(string endpoint, string database, string text, CancellationToken cancellationToken)
    {
        var uri = _clusterUri + endpoint;
        var httpRequest = await GenerateRequestAsync(uri, database, text, cancellationToken).ConfigureAwait(false);
        var client = _httpClientFactory.CreateClient();
        client.Timeout = s_httpClientTimeout;
        return await SendRequestAsync(client, httpRequest, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpRequestMessage> GenerateRequestAsync(string uri, string database, string text, CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        var scopes = new string[]
        {
            GetKustoScope()
        };
        var clientRequestId = s_clientRequestIdPrefix + Guid.NewGuid().ToString();
        var tokenRequestContext = new TokenRequestContext(scopes, clientRequestId);
        var accessToken = await _tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken.Token);
        httpRequest.Headers.Add("User-Agent", _userAgent);
        httpRequest.Headers.Add("x-ms-client-request-id", clientRequestId);
        httpRequest.Headers.Add("x-ms-app", s_application);
        httpRequest.Headers.Add("x-ms-client-version", "Kusto.Client.Light");
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var body = new JsonObject
        {
            { "db", database },
            { "csl", text }
        };
        var properties = new JsonObject
        {
            { "ClientRequestId", clientRequestId }
        };
        body.Add("properties", properties);
        var bodyStr = body.ToJsonString();
        httpRequest.Content = new StringContent(bodyStr);
        httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json", "utf-8");
        return httpRequest;
    }

    private async Task<KustoResult> SendRequestAsync(HttpClient httpClient, HttpRequestMessage httpRequest, CancellationToken cancellationToken)
    {
        var httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed with status code {httpResponse.StatusCode}: {errorContent}");
        }
        return KustoResult.FromHttpResponseMessage(httpResponse);
    }
}
