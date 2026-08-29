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
    /// Runs the requested health checks against an ADME instance.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        bool includeAuth,
        bool includeConnectivity,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPartition);
        var endpointUri = AdmeServiceHelper.ValidateEndpoint(new Uri(endpoint));
        var authOk = true;
        string? authError = null;
        string? token = null;

        if (includeAuth || includeConnectivity)
        {
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
                authOk = false;
                authError = "Microsoft Entra authentication failed. Verify your credentials and sign-in configuration.";
            }
        }

        var connectivityOk = true;
        string? connectivityError = null;
        int? statusCode = null;

        if (includeConnectivity && authOk)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient(AdmeServiceHelper.HttpClientName);
                client.BaseAddress = endpointUri;
                using var request = new HttpRequestMessage(HttpMethod.Get, "/api/storage/v2/info");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("data-partition-id", dataPartition);

                using var response = await client.SendAsync(request, cancellationToken);
                statusCode = (int)response.StatusCode;
                connectivityOk = response.IsSuccessStatusCode;
                if (!connectivityOk)
                {
                    connectivityError = $"ADME returned HTTP status {(int)response.StatusCode}.";
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                connectivityOk = false;
                connectivityError = "Could not connect to the ADME endpoint. Verify the endpoint and network access.";
            }
        }

        return new HealthCheckResult(
            authOk,
            authError,
            includeConnectivity ? connectivityOk : true,
            connectivityError,
            statusCode);
    }
}
