// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public sealed class FunctionAppService(IAzureService azureService, ICacheService cacheService, ILogger<FunctionAppService> logger)
    : BaseAzureService(azureService), IFunctionAppService
{
    private const int MaxFunctionApps = 10_000;
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<FunctionAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "functionapp";
    private static readonly TimeSpan s_cacheDuration = CacheDurations.ServiceData;

    public async Task<List<FunctionAppInfo>?> GetFunctionApp(
        string subscription,
        string? functionAppName,
        string? resourceGroup,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        var functionApps = new List<FunctionAppInfo>();
        if (string.IsNullOrEmpty(functionAppName))
        {
            var cacheKey = (string.IsNullOrEmpty(tenant), string.IsNullOrEmpty(resourceGroup)) switch
            {
                (true, true) => subscription,
                (false, true) => CacheKeyBuilder.Build(subscription, tenant),
                (true, false) => CacheKeyBuilder.Build(subscription, resourceGroup),
                (false, false) => CacheKeyBuilder.Build(subscription, tenant, resourceGroup)
            };

            var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            if (string.IsNullOrEmpty(resourceGroup))
            {
                await RetrieveAndAddFunctionApp(subscriptionResource.GetWebSitesAsync(cancellationToken), functionApps, _logger, cancellationToken);
            }
            else
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                if (!resourceGroupResource.HasValue)
                {
                    throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                }

                await RetrieveAndAddFunctionApp(resourceGroupResource.Value.GetWebSites().GetAllAsync(cancellationToken: cancellationToken), functionApps, _logger, cancellationToken);
            }

            await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration, cancellationToken);
        }
        else
        {
            ValidateRequiredParameters(
                (nameof(functionAppName), functionAppName),
                (nameof(resourceGroup), resourceGroup));

            var cacheKey = string.IsNullOrEmpty(tenant)
                ? CacheKeyBuilder.Build(subscription, resourceGroup, functionAppName)
                : CacheKeyBuilder.Build(subscription, tenant, resourceGroup, functionAppName);

            var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (!resourceGroupResource.HasValue)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
            }
            var site = await resourceGroupResource.Value.GetWebSites().GetAsync(functionAppName, cancellationToken);

            TryAddFunctionApp(site.Value, functionApps);
            await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration, cancellationToken);
        }

        return functionApps;
    }

    public async Task<FunctionAppInfo> CreateFunctionApp(
        string subscription,
        string resourceGroup,
        string functionApp,
        string location,
        string? appServicePlan = null,
        string? planType = null,
        string? planSku = null,
        string? runtime = null,
        string? runtimeVersion = null,
        string? operatingSystem = null,
        string? storageAccount = null,
        string? storageAuthMode = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var useManagedIdentity = FunctionAppValidation.ParseStorageAuthMode(storageAuthMode) ?? true;
        var inputs = FunctionAppValidation.ValidateAndNormalizeInputs(
            subscription, resourceGroup, functionApp, location,
            runtime, runtimeVersion, planType, planSku, operatingSystem,
            storageAccount, containerAppsEnvironmentName: null);

        if (FunctionAppValidation.ParseHostingKind(inputs.PlanType) == HostingKind.ContainerApp)
        {
            throw new ArgumentException("Use the 'functionapp containerapp create' command to host a Function App in Azure Container Apps.");
        }

        var options = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentity);
        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken);
        var resourceGroupResource = await EnsureResourceGroup(subscriptionResource, resourceGroup, location, cancellationToken);

        var site = await FunctionAppAppServiceStrategy.CreateFunctionAppAsync(
            resourceGroupResource, functionApp, location, appServicePlan, options, inputs.StorageAccountName, GetStorageEndpointSuffix(), cancellationToken);

        return ToFunctionAppInfo(site);
    }

    public async Task<FunctionAppInfo> CreateContainerAppFunctionApp(
        string subscription,
        string resourceGroup,
        string functionApp,
        string location,
        string? runtime = null,
        string? runtimeVersion = null,
        string? storageAccount = null,
        string? storageAuthMode = null,
        string? containerAppsEnvironment = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var useManagedIdentity = FunctionAppValidation.ParseStorageAuthMode(storageAuthMode) ?? true;
        var inputs = FunctionAppValidation.ValidateAndNormalizeInputs(
            subscription, resourceGroup, functionApp, location,
            runtime, runtimeVersion, planType: "containerapp", planSku: null, operatingSystem: null,
            storageAccount, containerAppsEnvironment);

        var options = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentity);
        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken);
        var resourceGroupResource = await EnsureResourceGroup(subscriptionResource, resourceGroup, location, cancellationToken);

        var site = await FunctionAppContainerAppStrategy.CreateFunctionAppAsync(
            resourceGroupResource, functionApp, location, options, inputs.StorageAccountName, inputs.ContainerAppsEnvironmentName, GetStorageEndpointSuffix(), cancellationToken);

        return ToFunctionAppInfo(site);
    }

    internal static async Task<ResourceGroupResource> EnsureResourceGroup(
        SubscriptionResource subscription,
        string resourceGroup,
        string location,
        CancellationToken cancellationToken)
    {
        var resourceGroups = subscription.GetResourceGroups();
        if (await resourceGroups.ExistsAsync(resourceGroup, cancellationToken))
        {
            return (await resourceGroups.GetAsync(resourceGroup, cancellationToken)).Value;
        }

        var operation = await resourceGroups.CreateOrUpdateAsync(WaitUntil.Completed, resourceGroup, new ResourceGroupData(location), cancellationToken);
        return operation.Value;
    }

    internal static FunctionAppInfo ToFunctionAppInfo(WebSiteResource site)
    {
        var data = site.Data;
        return new FunctionAppInfo(
            data.Name,
            data.Id.ResourceGroupName,
            data.Location.ToString(),
            data.AppServicePlanId?.Name,
            data.State,
            data.DefaultHostName,
            FunctionAppValidation.GetOperatingSystem(data.Kind),
            data.Tags);
    }

    private string GetStorageEndpointSuffix() => AzureService.CloudConfiguration.CloudType switch
    {
        AzureCloudConfiguration.AzureCloud.AzureChinaCloud => "core.chinacloudapi.cn",
        AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => "core.usgovcloudapi.net",
        _ => FunctionAppStorageProvisioner.DefaultStorageEndpointSuffix
    };

    private static async Task RetrieveAndAddFunctionApp(
        AsyncPageable<WebSiteResource> sites,
        List<FunctionAppInfo> functionApps,
        ILogger<FunctionAppService> logger,
        CancellationToken cancellationToken)
    {
        await foreach (var site in sites.WithCancellation(cancellationToken))
        {
            TryAddFunctionApp(site, functionApps);
            if (functionApps.Count >= MaxFunctionApps)
            {
                logger.LogWarning("Warning: Reached maximum function app limit of {MaxFunctionApps}. Some function apps may not be included in the results.", MaxFunctionApps);
                break;
            }
        }
    }

    private static void TryAddFunctionApp(WebSiteResource site, List<FunctionAppInfo> functionApps)
    {
        if (site?.Data != null && FunctionAppValidation.IsFunctionApp(site.Data))
        {
            functionApps.Add(ToFunctionAppInfo(site));
        }
    }
}
