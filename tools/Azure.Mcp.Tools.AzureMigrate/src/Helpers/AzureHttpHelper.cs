// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure;

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Helper for making authenticated HTTP requests to Azure APIs.
/// </summary>
public sealed class AzureHttpHelper(IAzureService azureService)
{
    /// <summary>
    /// Gets an HTTP client with Azure authentication headers.
    /// </summary>
    /// <param name="tenant">Optional Microsoft Entra ID tenant ID or display name used to authenticate the request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task<HttpClient> GetAuthenticatedClientAsync(string? tenant, CancellationToken cancellationToken = default)
    {
        var client = azureService.GetClient();
        var tenantId = string.IsNullOrEmpty(tenant) ? null : await azureService.ResolveTenantIdAsync(tenant, cancellationToken);
        var credential = await azureService.GetTokenCredentialAsync(tenantId, cancellationToken);
        var token = await credential.GetTokenAsync(
            new([azureService.CloudConfiguration.ArmEnvironment.DefaultScope]), cancellationToken);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);
        return client;
    }

    /// <summary>
    /// Makes an authenticated GET request.
    /// </summary>
    public async Task<string> GetAsync(string url, string? tenant, CancellationToken cancellationToken = default)
    {
        using var client = await GetAuthenticatedClientAsync(tenant, cancellationToken);
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Makes an authenticated POST request with JSON payload.
    /// </summary>
    public async Task<string> PostAsync<T>(string url, T payload, JsonSerializerContext context, string? tenant, CancellationToken cancellationToken = default)
    {
        using var client = await GetAuthenticatedClientAsync(tenant, cancellationToken);
        var json = JsonSerializer.Serialize(payload, typeof(T), context);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Makes an authenticated POST request without payload.
    /// </summary>
    public async Task<string> PostAsync(string url, string? tenant, CancellationToken cancellationToken = default)
    {
        using var client = await GetAuthenticatedClientAsync(tenant, cancellationToken);
        var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Downloads a file from a URL (no authentication).
    /// </summary>
    public async Task<byte[]> DownloadBytesAsync(string url, CancellationToken cancellationToken = default)
    {
        using var client = azureService.GetClient();
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
