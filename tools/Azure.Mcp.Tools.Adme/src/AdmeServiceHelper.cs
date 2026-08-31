// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;
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
        string path,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPartition);
        var endpointUri = ValidateEndpoint(new Uri(endpoint));
        var credential = await credentialProvider.GetTokenCredentialAsync(tenantId: null, cancellationToken);
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
            throw new HttpRequestException(
                $"ADME schema request failed with {(int)response.StatusCode} {response.ReasonPhrase}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken)
            ?? throw new HttpRequestException(
                "ADME schema request returned an empty response body.",
                inner: null,
                statusCode: response.StatusCode);
    }

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
