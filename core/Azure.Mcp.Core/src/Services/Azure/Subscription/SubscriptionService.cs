// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Helpers;
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
        var cachedResults = await _cacheService.GetAsync<List<SubscriptionData>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
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
        await _cacheService.SetAsync(CacheGroup, cacheKey, results, s_cacheDuration, cancellationToken);

        return results;
    }

    public async Task<SubscriptionResource> GetSubscription(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        // Get the subscription ID first, whether the input is a name or ID
        var subscriptionId = await GetSubscriptionId(subscription, tenant, retryPolicy, cancellationToken);

        // Use subscription ID for cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{SubscriptionCacheKey}_{subscriptionId}"
            : $"{SubscriptionCacheKey}_{subscriptionId}_{tenant}";
        var cachedSubscription = await _cacheService.GetAsync<SubscriptionResource>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
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
        await _cacheService.SetAsync(CacheGroup, cacheKey, response.Value, s_cacheDuration, cancellationToken);

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

    /// <inheritdoc/>
    public string? GetDefaultSubscriptionId()
    {
        // Primary: read from Azure CLI profile (~/.azure/azureProfile.json)
        var profileDefault = ReadDefaultSubscriptionFromAzureProfile();
        if (!string.IsNullOrEmpty(profileDefault))
        {
            return profileDefault;
        }

        // Fallback: AZURE_SUBSCRIPTION_ID environment variable
        return EnvironmentHelpers.GetAzureSubscriptionId();
    }

    internal static string? ReadDefaultSubscriptionFromAzureProfile()
    {
        try
        {
            var profilePath = GetAzureProfilePath();
            if (!File.Exists(profilePath))
            {
                return null;
            }

            var json = File.ReadAllText(profilePath);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("subscriptions", out var subscriptions) ||
                subscriptions.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var sub in subscriptions.EnumerateArray())
            {
                if (sub.TryGetProperty("isDefault", out var isDefault) &&
                    isDefault.ValueKind == JsonValueKind.True &&
                    sub.TryGetProperty("id", out var id) &&
                    id.ValueKind == JsonValueKind.String)
                {
                    return id.GetString();
                }
            }
        }
        catch
        {
            // Silently ignore errors reading the profile - this is best-effort
        }

        return null;
    }

    internal static string GetAzureProfilePath()
    {
        var azureDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azure");
        return Path.Combine(azureDir, "azureProfile.json");
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
