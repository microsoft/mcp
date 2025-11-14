// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.ResourceGroup;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.ResourceGroup;

public class ResourceGroupService(
    ICacheService cacheService,
    ISubscriptionService subscriptionService,
    ITenantService tenantService)
    : BaseAzureService(tenantService), IResourceGroupService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private const string CacheGroup = "resourcegroup";
    private const string CacheKey = "resourcegroups";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<ResourceGroupInfo>> GetResourceGroups(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;

        // Try to get from cache first
        var cacheKey = $"{CacheKey}_{subscriptionId}_{tenant ?? "default"}";
        var cachedResults = await _cacheService.GetAsync<List<ResourceGroupInfo>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        try
        {
            var resourceGroups = await subscriptionResource.GetResourceGroups()
                .GetAllAsync(cancellationToken: cancellationToken)
                .Select(rg => new ResourceGroupInfo(
                    rg.Data.Name,
                    rg.Data.Id.ToString(),
                    rg.Data.Location.ToString()))
                .ToListAsync(cancellationToken: cancellationToken);

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, resourceGroups, s_cacheDuration);

            return resourceGroups;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving resource groups: {ex.Message}", ex);
        }
    }

    public async Task<ResourceGroupInfo?> GetResourceGroup(string subscription, string resourceGroupName, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroupName), resourceGroupName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;

        // Try to get from cache first
        var cacheKey = $"{CacheKey}_{subscriptionId}_{tenant ?? "default"}";
        var cachedResults = await _cacheService.GetAsync<List<ResourceGroupInfo>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedResults != null)
        {
            return cachedResults.FirstOrDefault(rg => rg.Name.Equals(resourceGroupName, StringComparison.OrdinalIgnoreCase));
        }

        try
        {
            var rg = await GetResourceGroupResource(subscription, resourceGroupName, tenant, retryPolicy, cancellationToken);
            if (rg == null)
            {
                return null;
            }

            return new ResourceGroupInfo(
                rg.Data.Name,
                rg.Data.Id.ToString(),
                rg.Data.Location.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving resource group {resourceGroupName}: {ex.Message}", ex);
        }
    }

    public async Task<ResourceGroupResource?> GetResourceGroupResource(string subscription, string resourceGroupName, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroupName), resourceGroupName));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var resourceGroupResponse = await subscriptionResource.GetResourceGroups()
                .GetAsync(resourceGroupName, cancellationToken)
                .ConfigureAwait(false);

            return resourceGroupResponse?.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving resource group {resourceGroupName}: {ex.Message}", ex);
        }
    }
}
