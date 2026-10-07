// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public class FabricCoreService(HttpClient httpClient, TokenCredential? credential = null) : IFabricCoreService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? new DefaultAzureCredential();
    private const string UserAgentHeaderName = "User-Agent";
    private const string UserAgentHeaderValue = "Fabric Core MCP";

    public async Task<FabricItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items";
        var jsonContent = JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<FabricItem>(response, CoreJsonContext.Default.FabricItem, cancellationToken) ?? new FabricItem();
    }

    /// <inheritdoc />
    public async Task<FabricCapacityMetadata> GetCapacityAsync(string capacityId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(capacityId, out var parsedCapacityId) || parsedCapacityId == Guid.Empty)
        {
            throw new ArgumentException("Capacity ID must be a nonempty UUID.", nameof(capacityId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/capacities/{parsedCapacityId:D}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url,
            completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new FabricCapacityGetThrottledException(retryAfter);
            }

            var statusCode = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
            throw new HttpRequestException("Unable to retrieve Fabric capacity metadata.", null, statusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var capacity = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.FabricCapacityMetadata, cancellationToken);

        if (!FabricCapacityMetadata.IsValid(capacity) || capacity.Id != parsedCapacityId)
        {
            throw new JsonException("Fabric returned invalid capacity metadata.");
        }

        return capacity;
    }

    public async Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/catalog/search";
        var jsonContent = JsonSerializer.Serialize(request, CoreJsonContext.Default.CatalogSearchRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<CatalogSearchResponse>(response, CoreJsonContext.Default.CatalogSearchResponse, cancellationToken) ?? new CatalogSearchResponse();
    }

    private async Task<Stream> SendFabricApiRequestAsync(
        HttpMethod method,
        string url,
        string? jsonContent = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendFabricHttpRequestAsync(method, url, jsonContent, headers, cancellationToken: cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> SendFabricHttpRequestAsync(
        HttpMethod method,
        string url,
        string? jsonContent = null,
        Dictionary<string, string>? headers = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var tokenRequestContext = new TokenRequestContext(FabricEndpoints.FabricScopes);
        var accessToken = await _credential.GetTokenAsync(tokenRequestContext, cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        request.Headers.Add(UserAgentHeaderName, UserAgentHeaderValue);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (jsonContent != null)
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        return await _httpClient.SendAsync(request, completionOption, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Fabric API request failed with status {(int)response.StatusCode} ({response.StatusCode}): {content}");
        }
    }
}
