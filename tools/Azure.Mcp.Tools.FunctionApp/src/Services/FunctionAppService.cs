// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppService;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public sealed class FunctionAppService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), IFunctionAppService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private const string CacheGroup = "functionapp";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<FunctionAppInfo>?> GetFunctionApp(
        string subscription,
        string? functionAppName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var functionApps = new List<FunctionAppInfo>();
        if (string.IsNullOrEmpty(functionAppName))
        {
            var cacheKey = string.IsNullOrEmpty(tenant) ? subscription : $"{subscription}_{tenant}";
            cacheKey = string.IsNullOrEmpty(resourceGroup) ? cacheKey : $"{cacheKey}_{resourceGroup}";

            var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                if (string.IsNullOrEmpty(resourceGroup))
                {
                    await RetrieveAndAddFunctionApp(subscriptionResource.GetWebSitesAsync(), functionApps);
                }
                else
                {
                    var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                    if (!resourceGroupResource.HasValue)
                    {
                        throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                    }

                    await RetrieveAndAddFunctionApp(resourceGroupResource.Value.GetWebSites().GetAllAsync(), functionApps);
                }

                await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing Function Apps: {ex.Message}", ex);
            }
        }
        else
        {
            ValidateRequiredParameters(
                (nameof(functionAppName), functionAppName),
                (nameof(resourceGroup), resourceGroup));

            var cacheKey = string.IsNullOrEmpty(tenant)
                ? $"{subscription}_{resourceGroup}_{functionAppName}"
                : $"{subscription}_{tenant}_{resourceGroup}_{functionAppName}";

            var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                if (!resourceGroupResource.HasValue)
                {
                    throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                }
                var site = await resourceGroupResource.Value.GetWebSites().GetAsync(functionAppName);

                TryAddFunctionApp(site.Value, functionApps);
                await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Function App '{functionAppName}' in resource group '{resourceGroup}': {ex.Message}", ex);
            }
        }

        return functionApps;
    }

    private static async Task RetrieveAndAddFunctionApp(AsyncPageable<WebSiteResource> sites, List<FunctionAppInfo> functionApps)
    {
        await foreach (var site in sites)
        {
            TryAddFunctionApp(site, functionApps);
        }
    }

    private static void TryAddFunctionApp(WebSiteResource site, List<FunctionAppInfo> functionApps)
    {
        if (site?.Data != null && site?.Data.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true)
        {
            var data = site.Data;
            functionApps.Add(new(data.Name, data.Id.ResourceGroupName, data.Location.ToString(), data.AppServicePlanId.Name,
                data.State, data.DefaultHostName, data.Tags));
        }
    }
}
