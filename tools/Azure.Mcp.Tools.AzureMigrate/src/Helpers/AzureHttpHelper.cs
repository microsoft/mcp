// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureMigrate.Constants;

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Helper for making authenticated HTTP requests to Azure APIs.
/// </summary>
public sealed class AzureHttpHelper(IHttpClientFactory httpClientFactory, ITenantService tenantService)
{
    /// <summary>
    /// Gets an HTTP client with Azure authentication headers.
    /// </summary>
    public async Task<HttpClient> GetAuthenticatedClientAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient();
        var credential = await tenantService.GetTokenCredentialAsync(null, cancellationToken);
        var token = await credential.GetTokenAsync(
            new TokenRequestContext([PlatformLandingZoneConstants.ManagementScope]), cancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    /// <summary>
    /// Makes an authenticated GET request.
    /// </summary>
    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        using var client = await GetAuthenticatedClientAsync(cancellationToken);
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Makes an authenticated POST request with JSON payload.
    /// </summary>
    public async Task<string> PostAsync<T>(string url, T payload, JsonSerializerContext context, CancellationToken cancellationToken = default)
    {
        using var client = await GetAuthenticatedClientAsync(cancellationToken);
        var json = JsonSerializer.Serialize(payload, typeof(T), context);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Makes an authenticated POST request without payload.
    /// </summary>
    public async Task<string> PostAsync(string url, CancellationToken cancellationToken = default)
    {
        using var client = await GetAuthenticatedClientAsync(cancellationToken);
        var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Downloads a file from a URL (no authentication).
    /// </summary>
    public async Task<byte[]> DownloadBytesAsync(string url, CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
