// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    public async Task<HealthCheckResult> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPartition);
        var endpointUri = AdmeServiceHelper.ValidateEndpoint(new Uri(endpoint));
        string token;

        try
        {
            var credential = await _credentialProvider.GetTokenCredentialAsync(tenantId: null, cancellationToken);
            var accessToken = await credential.GetTokenAsync(
                new TokenRequestContext([AdmeServiceHelper.AuthScope]), cancellationToken);
            token = accessToken.Token;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new HealthCheckResult(
                false,
                "Microsoft Entra authentication failed. Verify your credentials and sign-in configuration.",
                false,
                "Connectivity check skipped because authentication failed.",
                null);
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
            return new HealthCheckResult(
                true,
                null,
                response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? null : $"ADME returned HTTP status {statusCode}.",
                statusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new HealthCheckResult(
                true,
                null,
                false,
                "Could not connect to the ADME endpoint. Verify the endpoint and network access.",
                null);
        }
    }
}
