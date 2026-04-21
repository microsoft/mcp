// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppService;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public sealed class FunctionAppService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<FunctionAppService> logger) : BaseAzureService(tenantService), IFunctionAppService
{
    private const int MaxFunctionApps = 10_000;
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<FunctionAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "functionapp";
    private static readonly TimeSpan s_cacheDuration = CacheDurations.ServiceData;

    public async Task<List<FunctionAppInfo>?> GetFunctionApp(
        string subscription,
        string? functionAppName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
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
                ? CacheKeyBuilder.Build(subscription, resourceGroup!, functionAppName)
                : CacheKeyBuilder.Build(subscription, tenant, resourceGroup!, functionAppName);

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
        if (site?.Data != null && site?.Data.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true)
        {
            var data = site.Data;
            functionApps.Add(new(data.Name, data.Id.ResourceGroupName, data.Location.ToString(), data.AppServicePlanId.Name,
                data.State, data.DefaultHostName, data.Tags));
        }
    }
}
