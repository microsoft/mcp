// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Azure;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Models.Search;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme;

/// <summary>
/// Defines shared ADME service constants and endpoint validation.
/// </summary>
internal static class AdmeServiceHelper
{
    public const string HttpClientName = "adme";
    public const string NonRetryingHttpClientName = "adme-no-retry";
    public const string AuthScope = "https://energy.azure.com/.default";

    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    public static IReadOnlyList<string>? Normalize(string[]? values) =>
        values is { Length: > 0 } ? values : null;

    public static SearchSort? ParseSort(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : JsonSerializer.Deserialize(value, AdmeJsonContext.Default.SearchSort);

    public static JsonElement? ParseJsonObject(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : JsonSerializer.Deserialize(value, AdmeJsonContext.Default.JsonElement);

    public static Task<T> SendAsync<T>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken,
        bool sendJsonContentTypeHint = false) =>
        SendAsync(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            HttpMethod.Get,
            path,
            sendJsonContentTypeHint ? new StringContent(string.Empty, Encoding.UTF8, "application/json") : null,
            extraHeaders: null,
            typeInfo,
            cancellationToken);

    public static Task<TResponse> PostAsync<TRequest, TResponse>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        IReadOnlyCollection<KeyValuePair<string, string>>? extraHeaders,
        CancellationToken cancellationToken,
        bool disableRetries = false) =>
        SendAsync(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            HttpMethod.Post,
            path,
            JsonContent.Create(body, requestTypeInfo),
            extraHeaders,
            responseTypeInfo,
            cancellationToken,
            disableRetries);

    public static Task<TResponse> PutAsync<TRequest, TResponse>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken) =>
        SendAsync(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            HttpMethod.Put,
            path,
            JsonContent.Create(body, requestTypeInfo),
            extraHeaders: null,
            responseTypeInfo,
            cancellationToken);

    public static async Task DeleteAsync(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        CancellationToken cancellationToken)
    {
        await SendAsync<object?>(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            HttpMethod.Delete,
            path,
            content: null,
            extraHeaders: null,
            typeInfo: null,
            cancellationToken);
    }

    private static async Task<T> SendAsync<T>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        HttpMethod method,
        string path,
        HttpContent? content,
        IReadOnlyCollection<KeyValuePair<string, string>>? extraHeaders,
        JsonTypeInfo<T>? typeInfo,
        CancellationToken cancellationToken,
        bool disableRetries = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPartition);
        var endpointUri = AdmeServiceValidator.ValidateEndpoint(new Uri(endpoint));
        var credential = await credentialProvider.GetTokenCredentialAsync(tenant, cancellationToken);
        var accessToken = await credential.GetTokenAsync(
            new TokenRequestContext([AuthScope]), cancellationToken);

        using var client = httpClientFactory.CreateClient(
            disableRetries ? NonRetryingHttpClientName : HttpClientName);
        client.BaseAddress = endpointUri;

        using var request = new HttpRequestMessage(method, path);
        request.Content = content;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        request.Headers.Add("data-partition-id", dataPartition);
        if (extraHeaders is not null)
        {
            foreach (var header in extraHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new RequestFailedException(
                (int)response.StatusCode,
                GetRequestFailureMessage(response.StatusCode, response.ReasonPhrase));
        }

        if (typeInfo is null)
        {
            return default!;
        }

        return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken)
            ?? throw new RequestFailedException(
                (int)response.StatusCode,
                "ADME request returned an empty response body.");
    }

    private static string GetRequestFailureMessage(HttpStatusCode statusCode, string? reasonPhrase) => statusCode switch
    {
        HttpStatusCode.BadRequest => $"ADME rejected the client request with {(int)statusCode} {reasonPhrase}.",
        HttpStatusCode.Unauthorized => $"ADME authentication failed with {(int)statusCode} {reasonPhrase}.",
        HttpStatusCode.Forbidden => $"ADME authorization failed with {(int)statusCode} {reasonPhrase}.",
        _ => $"ADME request failed with {(int)statusCode} {reasonPhrase}."
    };

    public static string? Format(int? value) =>
        value?.ToString(CultureInfo.InvariantCulture);

    public static void Add(ICollection<KeyValuePair<string, string>> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add(new(name, value));
        }
    }

    public static string AppendQuery(string path, IReadOnlyCollection<KeyValuePair<string, string>> query)
    {
        if (query.Count == 0)
        {
            return path;
        }

        var builder = new StringBuilder(path).Append('?');
        builder.AppendJoin('&', query.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        return builder.ToString();
    }

}
