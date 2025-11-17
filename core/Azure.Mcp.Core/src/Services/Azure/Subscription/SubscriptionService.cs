// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

public class SubscriptionService(ICacheService cacheService, ITenantService tenantService)
    : BaseAzureService(tenantService), ISubscriptionService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CacheGroup = "subscription";
    private const string CacheKey = "subscriptions";
    private const string SubscriptionCacheKey = "subscription";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(12);

    public async Task<List<SubscriptionData>> GetSubscriptions(string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        // Try to get from cache first
        var cacheKey = string.IsNullOrEmpty(tenant) ? CacheKey : $"{CacheKey}_{tenant}";
        var cachedResults = await _cacheService.GetAsync<List<SubscriptionData>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var subscriptions = armClient.GetSubscriptions();
        var results = new List<SubscriptionData>();

        await foreach (var subscription in subscriptions.WithCancellation(cancellationToken))
        {
            results.Add(subscription.Data);
        }

        // Cache the results
        await _cacheService.SetAsync(CacheGroup, cacheKey, results, s_cacheDuration);

        return results;
    }

    public async Task<SubscriptionResource> GetSubscription(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        string subscriptionId;
        try
        {
            // Get the subscription ID first, whether the input is a name or ID
            subscriptionId = await GetSubscriptionId(subscription, tenant, retryPolicy, cancellationToken);
        }
        catch
        {
            // If the subscription lookup failed, try environment variable as fallback
            var envSubscription = Core.Helpers.EnvironmentHelpers.GetAzureSubscriptionId();
            if (string.IsNullOrEmpty(envSubscription))
            {
                throw; // Re-throw the original exception if no environment variable is available
            }

            subscriptionId = await GetSubscriptionId(envSubscription, tenant, retryPolicy, cancellationToken);
        }

        // Use subscription ID for cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{SubscriptionCacheKey}_{subscriptionId}"
            : $"{SubscriptionCacheKey}_{subscriptionId}_{tenant}";
        var cachedSubscription = await _cacheService.GetAsync<SubscriptionResource>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedSubscription != null)
        {
            return cachedSubscription;
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var response = await armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId)).GetAsync(cancellationToken);
        if (response?.Value == null)
        {
            throw new Exception($"Could not retrieve subscription {subscription}");
        }

        // Cache the result using subscription ID
        await _cacheService.SetAsync(CacheGroup, cacheKey, response.Value, s_cacheDuration);

        return response.Value;
    }

    public bool IsSubscriptionId(string subscription, string? tenant = null)
    {
        return Guid.TryParse(subscription, out _);
    }

    public async Task<string> GetSubscriptionIdByName(string subscriptionName, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptions = await GetSubscriptions(tenant, retryPolicy, cancellationToken);
        var subscription = subscriptions.FirstOrDefault(s => s.DisplayName.Equals(subscriptionName, StringComparison.OrdinalIgnoreCase)) ??
            throw new Exception($"Could not find subscription with name {subscriptionName}");

        return subscription.SubscriptionId;
    }

    public async Task<string> GetSubscriptionNameById(string subscriptionId, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptions = await GetSubscriptions(tenant, retryPolicy, cancellationToken);
        var subscription = subscriptions.FirstOrDefault(s => s.SubscriptionId.Equals(subscriptionId, StringComparison.OrdinalIgnoreCase)) ??
            throw new Exception($"Could not find subscription with ID {subscriptionId}");

        return subscription.DisplayName;
    }

    private async Task<string> GetSubscriptionId(string subscription, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        if (IsSubscriptionId(subscription))
        {
            return subscription;
        }

        return await GetSubscriptionIdByName(subscription, tenant, retryPolicy, cancellationToken);
    }
}
