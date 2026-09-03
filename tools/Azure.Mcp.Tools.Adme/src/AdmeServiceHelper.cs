// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme;

/// <summary>
/// Defines shared ADME service constants and endpoint validation.
/// </summary>
internal static class AdmeServiceHelper
{
    public const string HttpClientName = "adme";
    public const string AuthScope = "https://energy.azure.com/.default";

    public static void ValidateTarget(
        string endpoint,
        string dataPartition,
        ValidationResult validationResult)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            validationResult.Errors.Add(
                "--endpoint must be an absolute HTTPS Azure Data Manager for Energy endpoint.");
        }
        else
        {
            try
            {
                ValidateEndpoint(endpointUri);
            }
            catch (Exception)
            {
                validationResult.Errors.Add(
                    "--endpoint must be an HTTPS Azure Data Manager for Energy endpoint hosted on an allowed domain.");
            }
        }

        if (string.IsNullOrWhiteSpace(dataPartition))
        {
            validationResult.Errors.Add("--data-partition must not be empty.");
        }
    }

    public static void ValidateKind(string kind, ValidationResult validationResult)
    {
        var components = kind.Split(':');
        var hasValidComponents = components.Length == 4
            && components.All(component => !string.IsNullOrWhiteSpace(component))
            && components.All(component => !component.Any(char.IsWhiteSpace))
            && components.All(component => !component.Contains('*', StringComparison.Ordinal));
        var versionComponents = components.Length == 4
            ? components[^1].Split('.')
            : [];
        var hasValidVersion = versionComponents.Length == 3
            && versionComponents.All(component => int.TryParse(
                component,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out _));

        if (!hasValidComponents || !hasValidVersion)
        {
            validationResult.Errors.Add(
                "--kind must be a fully-qualified kind in the format 'authority:source:type:major.minor.patch'.");
        }
    }

    /// <summary>
    /// Validates an ADME service endpoint URI.
    /// </summary>
    public static Uri ValidateEndpoint(Uri endpoint)
    {
        EndpointValidator.ValidateAzureServiceEndpoint(endpoint.AbsoluteUri, "adme", ArmEnvironment.AzurePublicCloud);
        return endpoint;
    }

    public static async Task<T> SendAsync<T>(
        IAzureTokenCredentialProvider credentialProvider,
        IHttpClientFactory httpClientFactory,
        string endpoint,
        string dataPartition,
        string? tenant,
        string path,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPartition);
        var endpointUri = ValidateEndpoint(new Uri(endpoint));
        var credential = await credentialProvider.GetTokenCredentialAsync(tenant, cancellationToken);
        var accessToken = await credential.GetTokenAsync(
            new TokenRequestContext([AuthScope]), cancellationToken);

        using var client = httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress = endpointUri;

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        request.Headers.Add("data-partition-id", dataPartition);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new RequestFailedException(
                (int)response.StatusCode,
                GetRequestFailureMessage(response.StatusCode, response.ReasonPhrase));
        }

        return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken)
            ?? throw new HttpRequestException(
                "ADME schema request returned an empty response body.",
                inner: null,
                statusCode: response.StatusCode);
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
