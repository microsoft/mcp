// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Quota.Models;
using Azure.Mcp.Tools.Quota.Services.Util;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services;

public class QuotaService: BaseAzureService, IQuotaService
{
    private readonly ILogger<QuotaService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public QuotaService(IAzureService azureService, ILoggerFactory loggerFactory)
        : base(azureService)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<QuotaService>();
    }

    public async Task<Dictionary<string, List<UsageInfo>>> GetAzureQuotaAsync(
        List<string> resourceTypes,
        string subscriptionId,
        string location,
        CancellationToken cancellationToken)
    {
        Dictionary<string, List<string>> providerToResourceTypes = resourceTypes
            .GroupBy(resourceType => resourceType.Split('/')[0])
            .ToDictionary(group => group.Key, group => group.ToList());

        TokenCredential credential = await GetCredential(null, cancellationToken);
        ArmClient resourceClient = await CreateArmClientAsync(cancellationToken: cancellationToken);

        IEnumerable<Task<IEnumerable<KeyValuePair<string, List<UsageInfo>>>>> quotaTasks =
            providerToResourceTypes.Select(async providerResourceTypes =>
            {
                string provider = providerResourceTypes.Key;
                List<string> resourceTypesForProvider = providerResourceTypes.Value;

                try
                {
                    IUsageChecker usageChecker = UsageCheckerFactory.CreateUsageChecker(
                        resourceClient,
                        credential,
                        provider,
                        subscriptionId,
                        _loggerFactory,
                        AzureService);
                    List<UsageInfo> quotaInfo = await usageChecker.GetUsageForLocationAsync(location, cancellationToken);
                    _logger.LogDebug(
                        "Retrieved quota info for provider {Provider}: {ItemCount} items",
                        provider,
                        quotaInfo.Count);

                    return resourceTypesForProvider.Select(resourceType =>
                        new KeyValuePair<string, List<UsageInfo>>(resourceType, quotaInfo));
                }
                catch (ArgumentException ex) when (ex.Message.Contains(
                    "Unsupported resource provider",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return resourceTypesForProvider.Select(resourceType =>
                        new KeyValuePair<string, List<UsageInfo>>(
                            resourceType,
                            [new(resourceType, 0, 0, Description: "No Limit")]));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Told to cancel. Don't return anything.
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        "Error fetching quota for provider {Provider}: {Error}",
                        provider,
                        exception.Message);
                    return resourceTypesForProvider.Select(resourceType =>
                        new KeyValuePair<string, List<UsageInfo>>(
                            resourceType,
                            [new(resourceType, 0, 0, Description: exception.Message)]));
                }
            });

        IEnumerable<KeyValuePair<string, List<UsageInfo>>>[] results = await Task.WhenAll(quotaTasks);

        return results
            .SelectMany(result => result)
            .ToDictionary(result => result.Key, result => result.Value);
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
            _loggerFactory,
            cognitiveServiceProperties,
            cancellationToken);

        List<string> commonValidRegions = availableRegions.Values
            .Aggregate((current, next) => [.. current.Intersect(next)]);

        return commonValidRegions;
    }
}
