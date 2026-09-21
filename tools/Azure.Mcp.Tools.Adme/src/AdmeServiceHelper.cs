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
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Search;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme;

/// <summary>
/// Defines shared ADME service constants and endpoint validation.
/// </summary>
internal static class AdmeServiceHelper
{
    private const int MaxErrorResponseLength = 1024;

    public const string CorrelationIdHeader = "correlation-id";
    public const string HttpClientName = "adme";
    public const string NonRetryingHttpClientName = "adme-no-retry";
    public const string AuthScope = "https://energy.azure.com/.default";

    public static string GetAuthScope(string? authAppId)
    {
        if (string.IsNullOrWhiteSpace(authAppId))
        {
            return AuthScope;
        }

        var value = authAppId.Trim();
        var resource = value.EndsWith("/.default", StringComparison.OrdinalIgnoreCase)
            ? value[..^"/.default".Length]
            : value;
        if (Guid.TryParse(resource, out _))
        {
            return $"{resource}/.default";
        }

        var isValidAppIdUri = Uri.TryCreate(resource, UriKind.Absolute, out var uri) &&
            (uri.Scheme.Equals("api", StringComparison.OrdinalIgnoreCase) ||
             uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) &&
            !string.IsNullOrWhiteSpace(uri.Host) &&
            string.IsNullOrEmpty(uri.Query) &&
            string.IsNullOrEmpty(uri.Fragment) &&
            string.IsNullOrEmpty(uri.UserInfo);

        if (!isValidAppIdUri)
        {
            throw new ArgumentException(
                "The ADME authentication application ID must be a GUID or an absolute api:// or https:// App ID URI.",
                nameof(authAppId));
        }

        return $"{resource.TrimEnd('/')}/.default";
    }

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

    public static Task<AdmeResponse<T>> SendAsync<T>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken,
        string? authAppId = null,
        bool sendJsonContentTypeHint = false) =>
        SendAsync(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            authAppId,
            HttpMethod.Get,
            path,
            sendJsonContentTypeHint ? new StringContent(string.Empty, Encoding.UTF8, "application/json") : null,
            extraHeaders: null,
            typeInfo,
            cancellationToken);

    public static Task<AdmeResponse<TResponse>> PostAsync<TRequest, TResponse>(
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
        string? authAppId = null,
        bool disableRetries = false) =>
        SendAsync(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            authAppId,
            HttpMethod.Post,
            path,
            JsonContent.Create(body, requestTypeInfo),
            extraHeaders,
            responseTypeInfo,
            cancellationToken,
            disableRetries);

    public static Task<AdmeResponse<TResponse>> PutAsync<TRequest, TResponse>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken,
        string? authAppId = null) =>
        SendAsync(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            authAppId,
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
        CancellationToken cancellationToken,
        string? authAppId = null)
    {
        await SendAsync<object?>(
            credentialProvider,
            httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            authAppId,
            HttpMethod.Delete,
            path,
            content: null,
            extraHeaders: null,
            typeInfo: null,
            cancellationToken);
    }

    private static async Task<AdmeResponse<T>> SendAsync<T>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string? authAppId,
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
            new TokenRequestContext([GetAuthScope(authAppId)]), cancellationToken);

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
        var correlationId = response.Headers.TryGetValues(CorrelationIdHeader, out var correlationIds)
            ? correlationIds.FirstOrDefault()
            : null;
        // Carried in failure messages so ADME support can trace the request that failed.
        var correlationSuffix = string.IsNullOrWhiteSpace(correlationId)
            ? string.Empty
            : $" ({CorrelationIdHeader}: {correlationId})";
        if (!response.IsSuccessStatusCode)
        {
            // ADME APIs ensure client-facing error responses do not expose sensitive information.
            var responseContent = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            var message = string.IsNullOrWhiteSpace(responseContent)
                ? GetRequestFailureMessage(response.StatusCode, response.ReasonPhrase)
                : responseContent[..Math.Min(responseContent.Length, MaxErrorResponseLength)];

            throw new RequestFailedException(
                (int)response.StatusCode,
                message + correlationSuffix);
        }

        if (typeInfo is null)
        {
            return new(default!, correlationId);
        }

        var result = await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken)
            ?? throw new RequestFailedException(
                (int)response.StatusCode,
                "ADME request returned an empty response body." + correlationSuffix);
        return new(result, correlationId);
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
