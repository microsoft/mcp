// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Calls the storage info endpoint for an ADME instance.
/// </summary>
public sealed class HealthService(
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientFactory httpClientFactory) : IHealthService
{
    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Calls the storage info endpoint for an ADME instance.
    /// </summary>
    public async Task<AdmeResponse<HealthCheckResult>> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        string? tenant,
        CancellationToken cancellationToken)
    {
        return await AdmeServiceHelper.SendAsync(
            _credentialProvider,
            _httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            "/api/storage/v2/info",
            static statusCode => new HealthCheckResult((int)statusCode),
            cancellationToken);
    }
}
