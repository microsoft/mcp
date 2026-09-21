// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Models;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Checks authentication and connectivity for an ADME instance.
/// </summary>
public sealed class HealthService(
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientFactory httpClientFactory) : IHealthService
{
    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Checks authentication and connectivity for an ADME instance.
    /// </summary>
    public async Task<AdmeResponse<HealthCheckResult>> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        string? tenant,
        CancellationToken cancellationToken,
        string? authAppId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPartition);
        var endpointUri = AdmeServiceValidator.ValidateEndpoint(new Uri(endpoint));
        string token;

        try
        {
            var credential = await _credentialProvider.GetTokenCredentialAsync(tenant, cancellationToken);
            var accessToken = await credential.GetTokenAsync(
                new TokenRequestContext([AdmeServiceHelper.GetAuthScope(authAppId)]), cancellationToken);
            token = accessToken.Token;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new(new HealthCheckResult(
                false,
                "Microsoft Entra authentication failed. Verify your credentials and sign-in configuration.",
                false,
                "Connectivity check skipped because authentication failed.",
                null), null);
        }

        try
        {
            using var client = _httpClientFactory.CreateClient(AdmeServiceHelper.HttpClientName);
            client.BaseAddress = endpointUri;
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/storage/v2/info");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("data-partition-id", dataPartition);

            using var response = await client.SendAsync(request, cancellationToken);
            var statusCode = (int)response.StatusCode;
            var correlationId = response.Headers.TryGetValues(
                AdmeServiceHelper.CorrelationIdHeader, out var correlationIds)
                    ? correlationIds.FirstOrDefault()
                    : null;
            var responseContent = response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                ? await response.Content.ReadAsStringAsync(cancellationToken)
                : null;
            var authError = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => string.IsNullOrEmpty(responseContent)
                    ? "ADME authentication failed with HTTP status 401."
                    : responseContent,
                HttpStatusCode.Forbidden => string.IsNullOrEmpty(responseContent)
                    ? "ADME authorization failed with HTTP status 403."
                    : responseContent,
                _ => null
            };
            return new(new HealthCheckResult(
                authError is null,
                authError,
                response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? null : $"ADME returned HTTP status {statusCode}.",
                statusCode), correlationId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new(new HealthCheckResult(
                true,
                null,
                false,
                "Could not connect to the ADME endpoint. Verify the endpoint and network access.",
                null), null);
        }
    }
}
