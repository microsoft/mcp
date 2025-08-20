// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Core.Services.Http;

namespace Azure.Mcp.Tools.Kusto.Services;

public class KustoClient(
    string clusterUri,
    TokenCredential tokenCredential,
    string userAgent,
    IHttpClientService httpClientService)
{
    private readonly string _clusterUri = clusterUri;
    private readonly TokenCredential _tokenCredential = tokenCredential;
    private readonly string _userAgent = userAgent;
    private readonly HttpClient _httpClient = httpClientService.CreateClient(new Uri(clusterUri));
    private static readonly string s_application = "AzureMCP";
    private static readonly string s_clientRequestIdPrefix = "AzMcp";
    private static readonly string s_default_scope = "https://kusto.kusto.windows.net/.default";

    public Task<KustoResult> ExecuteQueryCommandAsync(string database, string text, CancellationToken cancellationToken)
        => ExecuteCommandAsync("/v1/rest/query", database, text, cancellationToken);

    public Task<KustoResult> ExecuteControlCommandAsync(string database, string text, CancellationToken cancellationToken)
        => ExecuteCommandAsync("/v1/rest/mgmt", database, text, cancellationToken);

    private async Task<KustoResult> ExecuteCommandAsync(string endpoint, string database, string text, CancellationToken cancellationToken)
    {
        var uri = _clusterUri + endpoint;
        var httpRequest = await GenerateRequestAsync(uri, database, text, cancellationToken).ConfigureAwait(false);
        return await SendRequestAsync(_httpClient, httpRequest, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpRequestMessage> GenerateRequestAsync(string uri, string database, string text, CancellationToken cancellationToken = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        var scopes = new string[]
        {
            s_default_scope
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

    private async Task<KustoResult> SendRequestAsync(HttpClient httpClient, HttpRequestMessage httpRequest, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorContent = await httpResponse.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed with status code {httpResponse.StatusCode}: {errorContent}");
        }
        return KustoResult.FromHttpResponseMessage(httpResponse);
    }
}
