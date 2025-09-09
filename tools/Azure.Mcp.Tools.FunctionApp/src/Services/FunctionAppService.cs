// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppService;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public sealed class FunctionAppService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IResourceGroupService resourceGroupService) : BaseAzureService(tenantService), IFunctionAppService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));

    private const string CacheGroup = "functionapp";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<FunctionAppInfo>?> ListFunctionApps(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var cacheKey = string.IsNullOrEmpty(tenant)
            ? subscription
            : $"{subscription}_{tenant}";

        var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var functionApps = new List<FunctionAppInfo>();

        try
        {
            await foreach (var site in subscriptionResource.GetWebSitesAsync())
            {
                if (site?.Data != null && IsFunctionApp(site.Data))
                {
                    functionApps.Add(ConvertToFunctionAppModel(site));
                }
            }

            await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Function Apps: {ex.Message}", ex);
        }

        return functionApps;
    }

    public async Task<FunctionAppInfo?> GetFunctionApp(
        string subscription,
        string functionAppName,
        string resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, functionAppName, resourceGroup);

        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{subscription}_{resourceGroup}_{functionAppName}"
            : $"{subscription}_{tenant}_{resourceGroup}_{functionAppName}";

        var cachedResults = await _cacheService.GetAsync<FunctionAppInfo>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        try
        {
            var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy);
            if (rg is null)
            {
                return null;
            }
            var site = await rg.GetWebSites().GetAsync(functionAppName);

            if (site?.Value?.Data is null || !IsFunctionApp(site.Value.Data))
            {
                return null;
            }

            var info = ConvertToFunctionAppModel(site.Value);
            await _cacheService.SetAsync(CacheGroup, cacheKey, info, s_cacheDuration);
            return info;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Function App '{functionAppName}' in resource group '{resourceGroup}': {ex.Message}", ex);
        }
    }

    private static bool IsFunctionApp(WebSiteData siteData)
    {
        return siteData.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static FunctionAppInfo ConvertToFunctionAppModel(WebSiteResource siteResource)
    {
        var data = siteResource.Data;

        return new FunctionAppInfo(
            data.Name,
            siteResource.Id.ResourceGroupName,
            data.Location.ToString(),
            data.AppServicePlanId.Name,
            data.State,
            data.DefaultHostName,
            data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
}
