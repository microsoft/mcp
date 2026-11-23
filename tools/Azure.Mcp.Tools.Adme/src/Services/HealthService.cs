// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Adme.Models;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Calls the storage info endpoint for an ADME instance.
/// </summary>
public sealed class HealthService(
    IEndpointValidator endpointValidator,
    IAzureService azureService,
    IHttpClientFactory httpClientFactory) : IHealthService
{
    private readonly IAzureService _azureService = azureService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Calls the storage info endpoint for an ADME instance.
    /// </summary>
    public async Task<AdmeResponse<HealthCheckResult>> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        string? tenant,
        string? authAppId = null,
        CancellationToken cancellationToken = default)
    {
        return await AdmeServiceHelper.SendAsync(endpointValidator,
            _azureService,
            _httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            "/api/storage/v2/info",
            static statusCode => new HealthCheckResult((int)statusCode),
            authAppId,
            cancellationToken);
    }
}
