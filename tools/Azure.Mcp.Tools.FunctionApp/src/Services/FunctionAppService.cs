// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.FunctionApp.Models;

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
