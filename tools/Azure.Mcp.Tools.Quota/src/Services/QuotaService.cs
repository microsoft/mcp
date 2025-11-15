// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Quota.Models;
using Azure.Mcp.Tools.Quota.Services.Util;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services;

public class QuotaService(
    ITenantService tenantService,
    ILoggerFactory loggerFactory,
    IHttpClientService httpClientService)
    : BaseAzureService(tenantService), IQuotaService
{
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

    public async Task<Dictionary<string, List<UsageInfo>>> GetAzureQuotaAsync(
        List<string> resourceTypes,
        string subscriptionId,
        string location,
        CancellationToken cancellationToken)
    {
        TokenCredential credential = await GetCredential(cancellationToken);
        Dictionary<string, List<UsageInfo>> quotaByResourceTypes = await AzureQuotaService.GetAzureQuotaAsync(
            credential,
            resourceTypes,
            subscriptionId,
            location,
            loggerFactory,
            _httpClientService,
            cancellationToken);
        return quotaByResourceTypes;
    }

    public async Task<List<string>> GetAvailableRegionsForResourceTypesAsync(
        string[] resourceTypes,
        string subscriptionId,
        string? cognitiveServiceModelName = null,
        string? cognitiveServiceModelVersion = null,
        string? cognitiveServiceDeploymentSkuName = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(cancellationToken: cancellationToken);

        // Create cognitive service properties if any of the parameters are provided
        CognitiveServiceProperties? cognitiveServiceProperties = null;
        if (!string.IsNullOrWhiteSpace(cognitiveServiceModelName) ||
            !string.IsNullOrWhiteSpace(cognitiveServiceModelVersion) ||
            !string.IsNullOrWhiteSpace(cognitiveServiceDeploymentSkuName))
        {
            cognitiveServiceProperties = new CognitiveServiceProperties
            {
                ModelName = cognitiveServiceModelName,
                ModelVersion = cognitiveServiceModelVersion,
                DeploymentSkuName = cognitiveServiceDeploymentSkuName
            };
        }

        var availableRegions = await AzureRegionService.GetAvailableRegionsForResourceTypesAsync(
            armClient,
            resourceTypes,
            subscriptionId,
            loggerFactory,
            cognitiveServiceProperties,
            cancellationToken);

        var allRegions = availableRegions.Values
            .Where(regions => regions.Count > 0)
            .SelectMany(regions => regions)
            .Distinct()
            .ToList();

        List<string> commonValidRegions = availableRegions.Values
            .Aggregate((current, next) => [.. current.Intersect(next)]);

        return commonValidRegions;
    }
}
