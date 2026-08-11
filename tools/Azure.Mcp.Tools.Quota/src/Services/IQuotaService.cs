// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Quota.Services.Util;

namespace Azure.Mcp.Tools.Quota.Services;

public interface IQuotaService
{
    Task<Dictionary<string, List<UsageInfo>>> GetAzureQuotaAsync(
        List<string> resourceTypes,
        string subscriptionId,
        string location,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<string>> GetAvailableRegionsForResourceTypesAsync(
        string[] resourceTypes,
        string subscriptionId,
        string? cognitiveServiceModelName = null,
        string? cognitiveServiceModelVersion = null,
        string? cognitiveServiceDeploymentSkuName = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
