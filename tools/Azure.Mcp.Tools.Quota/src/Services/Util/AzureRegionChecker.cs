// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.Quota.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.CognitiveServices;
using Azure.ResourceManager.CognitiveServices.Models;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.PostgreSql.FlexibleServers.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util;

public interface IRegionChecker
{
    Task<List<string>> GetAvailableRegionsAsync(string resourceType);
}

public abstract class AzureRegionChecker : IRegionChecker
{
    protected readonly string SubscriptionId;
    protected readonly ArmClient ResourceClient;
    protected readonly ILogger Logger;

    protected AzureRegionChecker(ArmClient armClient, string subscriptionId, ILogger logger)
    {
        SubscriptionId = subscriptionId;
        ResourceClient = armClient;
        Logger = logger;
    }

    public abstract Task<List<string>> GetAvailableRegionsAsync(string resourceType);
}

public class DefaultRegionChecker(ArmClient armClient, string subscriptionId, ILogger<DefaultRegionChecker> logger) : AzureRegionChecker(armClient, subscriptionId, logger)
{
    public override async Task<List<string>> GetAvailableRegionsAsync(string resourceType)
    {
        try
        {
            var parts = resourceType.Split('/');
            var providerNamespace = parts[0];
            var resourceTypeName = parts[1];

            var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
            var provider = await subscription.GetResourceProviderAsync(providerNamespace);

            if (provider?.Value?.Data?.ResourceTypes == null)
            {
                return [];
            }

            var resourceTypeInfo = provider.Value.Data.ResourceTypes
                .FirstOrDefault(rt => rt.ResourceType.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase));

            if (resourceTypeInfo?.Locations == null)
            {
                return [];
            }

            return [.. resourceTypeInfo.Locations.Select(location => location.Replace(" ", "").ToLowerInvariant())];
        }
        catch (Exception error)
        {
            throw new InvalidOperationException($"Failed to fetch available regions for resource type '{resourceType}'. Please verify the resource type name and your subscription permissions.", error);
        }
    }
}

public class CognitiveServicesRegionChecker : AzureRegionChecker
{
    private readonly string? _skuName;
    private readonly string? _apiVersion;
    private readonly string? _modelName;

    public CognitiveServicesRegionChecker(ArmClient armClient, string subscriptionId, ILogger<CognitiveServicesRegionChecker> logger, string? skuName = null, string? apiVersion = null, string? modelName = null)
        : base(armClient, subscriptionId, logger)
    {
        _skuName = skuName;
        _apiVersion = apiVersion;
        _modelName = modelName;
    }

    public override async Task<List<string>> GetAvailableRegionsAsync(string resourceType)
    {
        var parts = resourceType.Split('/');
        var providerNamespace = parts[0];
        var resourceTypeName = parts[1];

        var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
        var provider = await subscription.GetResourceProviderAsync(providerNamespace);

        List<string> regions = provider?.Value?.Data?.ResourceTypes?
            .FirstOrDefault(rt => rt.ResourceType.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase))
            ?.Locations?
            .Select(location => location.Replace(" ", "").ToLowerInvariant())
            .ToList() ?? new List<string>();

        var tasks = regions.Select(async region =>
        {
            try
            {
                var quotas = subscription.GetModelsAsync(region);

                await foreach (CognitiveServicesModel modelElement in quotas)
                {
                    var nameMatch = string.IsNullOrEmpty(_modelName) ||
                        (modelElement.Model?.Name == _modelName);

                    var versionMatch = string.IsNullOrEmpty(_apiVersion) ||
                        (modelElement.Model?.Version == _apiVersion);

                    var skuMatch = string.IsNullOrEmpty(_skuName) ||
                        (modelElement.Model?.Skus?.Any(sku => sku.Name == _skuName) ?? false);

                    if (nameMatch && versionMatch && skuMatch)
                    {
                        return region;
                    }
                }
            }
            catch (Exception error)
            {
                Logger.LogWarning("Error checking cognitive services models for region {Region}: {Error}", region, error.Message);
            }
            return null;
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(region => region != null).ToList()!;
    }
}

public class PostgreSqlRegionChecker(ArmClient armClient, string subscriptionId, ILogger<PostgreSqlRegionChecker> logger) : AzureRegionChecker(armClient, subscriptionId, logger)
{
    public override async Task<List<string>> GetAvailableRegionsAsync(string resourceType)
    {
        var parts = resourceType.Split('/');
        var providerNamespace = parts[0];
        var resourceTypeName = parts[1];

        var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
        var provider = await subscription.GetResourceProviderAsync(providerNamespace);
        var regions = provider?.Value?.Data?.ResourceTypes?
            .FirstOrDefault(rt => rt.ResourceType.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase))
            ?.Locations?
            .Select(location => location.Replace(" ", "").ToLowerInvariant())
            .ToList() ?? new List<string>();

        var tasks = regions.Select(async region =>
        {
            try
            {
                AsyncPageable<PostgreSqlFlexibleServerCapabilityProperties> result = subscription.ExecuteLocationBasedCapabilitiesAsync(region);
                await foreach (var capability in result)
                {
                    if (capability.SupportedServerEditions?.Any() == true)
                    {
                        return region;
                    }
                }
            }
            catch (Exception error)
            {
                Logger.LogWarning("Error checking PostgreSQL capabilities for region {Region}: {Error}", region, error.Message);
            }
            return null;
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(region => region != null).ToList()!;
    }
}

public static class RegionCheckerFactory
{
    public static IRegionChecker CreateRegionChecker(
        ArmClient armClient,
        string subscriptionId,
        string resourceType,
        ILoggerFactory loggerFactory,
        CognitiveServiceProperties? properties = null)
    {
        var provider = resourceType.Split('/')[0].ToLowerInvariant();

        return provider switch
        {
            "microsoft.cognitiveservices" => new CognitiveServicesRegionChecker(
                armClient,
                subscriptionId,
                loggerFactory.CreateLogger<CognitiveServicesRegionChecker>(),
                properties?.DeploymentSkuName,
                properties?.ModelVersion,
                properties?.ModelName),
            "microsoft.dbforpostgresql" => new PostgreSqlRegionChecker(
                armClient,
                subscriptionId,
                loggerFactory.CreateLogger<PostgreSqlRegionChecker>()),
            _ => new DefaultRegionChecker(
                armClient,
                subscriptionId,
                loggerFactory.CreateLogger<DefaultRegionChecker>())
        };
    }
}

public static class AzureRegionService
{
    public static async Task<Dictionary<string, List<string>>> GetAvailableRegionsForResourceTypesAsync(
        ArmClient armClient,
        string[] resourceTypes,
        string subscriptionId,
        ILoggerFactory loggerFactory,
        CognitiveServiceProperties? cognitiveServiceProperties = null)
    {
        var tasks = resourceTypes.Select(async resourceType =>
        {
            var checker = RegionCheckerFactory.CreateRegionChecker(armClient, subscriptionId, resourceType, loggerFactory, cognitiveServiceProperties);
            var regions = await checker.GetAvailableRegionsAsync(resourceType);
            return new KeyValuePair<string, List<string>>(resourceType, regions);
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
