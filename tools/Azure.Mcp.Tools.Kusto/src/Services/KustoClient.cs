// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;

namespace Azure.Mcp.Tools.Kusto.Services;

internal sealed class KustoClient(
    KustoEndpoint endpoint,
    string? tenant,
    string userAgent,
    IAzureService azureService)
{
    internal const string HttpClientName = "Azure.Mcp.Tools.Kusto";

    private readonly KustoEndpoint _endpoint = endpoint;
    private readonly string? _tenant = tenant;
    private readonly TokenCredential? _testTokenCredential;
    // Resolve credentials per request and reuse tokens only for the same credential instance.
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private TokenCredential? _cachedTokenCredential;
    private AccessToken _accessToken;
    private readonly string _userAgent = userAgent;
    private readonly IAzureService _azureService = azureService;
    private static readonly TimeSpan s_httpClientTimeout = TimeSpan.FromSeconds(240);
    private static readonly TimeSpan s_tokenRefreshBuffer = TimeSpan.FromMinutes(5);
    private static readonly string s_application = "AzureMCP";
    private static readonly string s_clientRequestIdPrefix = "AzMcp";

    internal KustoClient(
        KustoEndpoint endpoint,
        TokenCredential testTokenCredential,
        string userAgent,
        IAzureService azureService)
        : this(endpoint, (string?)null, userAgent, azureService)
    {
        _testTokenCredential = testTokenCredential;
    }

    public Task<KustoResult> ExecuteQueryCommandAsync(string database, string text, CancellationToken cancellationToken)
        => ExecuteCommandAsync("/v1/rest/query", database, text, cancellationToken);

    public Task<KustoResult> ExecuteControlCommandAsync(string database, string text, CancellationToken cancellationToken)
        => ExecuteCommandAsync("/v1/rest/mgmt", database, text, cancellationToken);

    private async Task<KustoResult> ExecuteCommandAsync(string endpoint, string database, string text, CancellationToken cancellationToken)
    {
        var uri = _endpoint.ClusterUri + endpoint;
        using var httpRequest = await GenerateRequestAsync(uri, database, text, cancellationToken).ConfigureAwait(false);
        using var client = _azureService.GetClient(HttpClientName);
        client.Timeout = s_httpClientTimeout;
        return await SendRequestAsync(client, httpRequest, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpRequestMessage> GenerateRequestAsync(string uri, string database, string text, CancellationToken cancellationToken)
    {
        var clientRequestId = s_clientRequestIdPrefix + Guid.NewGuid().ToString();
        var accessToken = await GetAccessTokenAsync(clientRequestId, cancellationToken).ConfigureAwait(false);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequest.Headers.Authorization = new("bearer", accessToken.Token);
        httpRequest.Headers.Add("User-Agent", _userAgent);
        httpRequest.Headers.Add("x-ms-client-request-id", clientRequestId);
        httpRequest.Headers.Add("x-ms-app", s_application);
        httpRequest.Headers.Add("x-ms-client-version", "Kusto.Client.Light");
        httpRequest.Headers.Accept.Add(new("application/json"));

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
        httpRequest.Content.Headers.ContentType = new("application/json", "utf-8");
        return httpRequest;
    }

    private async Task<AccessToken> GetAccessTokenAsync(string clientRequestId, CancellationToken cancellationToken)
    {
        var tokenCredential = _testTokenCredential;
        if (tokenCredential == null)
        {
            var resolvedTenant = await _azureService.ResolveTenantIdAsync(_tenant, cancellationToken).ConfigureAwait(false);
            tokenCredential = await _azureService.GetTokenCredentialAsync(resolvedTenant, cancellationToken).ConfigureAwait(false);
        }

        await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var now = DateTimeOffset.UtcNow;
            if (!ReferenceEquals(_cachedTokenCredential, tokenCredential) ||
                _accessToken.ExpiresOn <= now + s_tokenRefreshBuffer ||
                _accessToken.RefreshOn <= now)
            {
                var tokenRequestContext = new TokenRequestContext([_endpoint.Scope], clientRequestId);
                _accessToken = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);
                _cachedTokenCredential = tokenCredential;
            }

            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static async Task<KustoResult> SendRequestAsync(HttpClient httpClient, HttpRequestMessage httpRequest, CancellationToken cancellationToken)
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
