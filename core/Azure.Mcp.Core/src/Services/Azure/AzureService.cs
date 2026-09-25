// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Helpers;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Resource;
using Microsoft.Mcp.Core.Models.ResourceGroup;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Core.Services.Azure;

public sealed class AzureService(
    ICacheService cacheService,
    ILogger<AzureService> logger,
    ISubscriptionResolver subscriptionResolver,
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientFactory httpClientFactory,
    IAzureCloudConfiguration cloudConfiguration) : IAzureService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<AzureService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISubscriptionResolver _subscriptionResolver = subscriptionResolver ?? throw new ArgumentNullException(nameof(subscriptionResolver));
    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private const int MaxTenants = 10_000;
    private const int MaxSubscriptions = 10_000;

    internal const string TenantCacheGroup = "tenant";
    internal const string TenantCacheKey = "tenants";
    internal const string SubscriptionCacheGroup = "subscription";
    internal const string GetSubscriptionCacheKey = "subscription";
    internal const string ListSubscriptionCacheKey = "subscriptions";
    internal const string ResourceGroupCacheGroup = "resourcegroup";
    internal const string ListResourceGroupCacheKey = "resourcegroups";

    private static readonly TimeSpan s_tenantCacheDuration = CacheDurations.Tenant;
    private static readonly TimeSpan s_subscriptionCacheDuration = CacheDurations.Subscription;
    private static readonly TimeSpan s_resourceGroupCacheDuration = CacheDurations.ServiceData;

    #region General

    /// <inheritdoc/>
    public IAzureCloudConfiguration CloudConfiguration { get; } = cloudConfiguration ?? throw new ArgumentNullException(nameof(cloudConfiguration));

    /// <inheritdoc/>
    public async Task<TokenCredential> GetTokenCredentialAsync(string? tenantId, CancellationToken cancellationToken) =>
        await _credentialProvider.GetTokenCredentialAsync(tenantId, cancellationToken);

    /// <inheritdoc/>
    public HttpClient GetClient(string? name = null) =>
        _httpClientFactory.CreateClient(name ?? Microsoft.Extensions.Options.Options.DefaultName);

    #endregion General

    #region Tenant

    /// <inheritdoc/>
    public async Task<List<TenantResource>> GetTenants(CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cachedResults = await _cacheService.GetAsync<List<TenantResource>>(TenantCacheGroup, TenantCacheKey, s_tenantCacheDuration, cancellationToken);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var results = new List<TenantResource>();

        var client = await AzureHelper.CreateArmClientAsync(this, cancellationToken: cancellationToken);

        await foreach (var tenant in client.GetTenants().WithCancellation(cancellationToken))
        {
            results.Add(tenant);
            if (results.Count >= MaxTenants)
            {
                _logger.LogWarning("Reached maximum tenant limit of {MaxTenants}. Some tenants may not be included in the results.", MaxTenants);
                break;
            }
        }

        // Cache the results
        await _cacheService.SetAsync(TenantCacheGroup, TenantCacheKey, results, s_tenantCacheDuration, cancellationToken);
        return results;
    }

    /// <inheritdoc/>
    public async Task<string?> ResolveTenantIdAsync(string? tenant, CancellationToken cancellationToken)
    {
        if (tenant == null)
            return tenant;
        return await GetTenantId(tenant, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> GetTenantId(string tenantIdOrName, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(tenantIdOrName, out _))
        {
            return tenantIdOrName;
        }

        return await GetTenantIdByName(tenantIdOrName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> GetTenantIdByName(string tenantName, CancellationToken cancellationToken)
    {
        var tenants = await GetTenants(cancellationToken);
        var tenant = tenants.FirstOrDefault(t => tenantName.Equals(t.Data.DisplayName, StringComparison.OrdinalIgnoreCase)) ??
            throw new KeyNotFoundException($"Could not find tenant with name {tenantName}");

        string? tenantId = tenant.Data.TenantId?.ToString() ??
            throw new InvalidOperationException($"Tenant {tenantName} has a null TenantId");

        return tenantId.ToString();
    }

    #endregion Tenant

    #region Subscription

    /// <inheritdoc/>
    public async Task<List<SubscriptionData>> GetSubscriptions(
        string? tenant,
        CancellationToken cancellationToken = default)
    {
        // Try to get from cache first
        var cacheKey = string.IsNullOrEmpty(tenant) ? ListSubscriptionCacheKey : CacheKeyBuilder.Build(ListSubscriptionCacheKey, tenant);
        var cachedResults = await _cacheService.GetAsync<List<SubscriptionData>>(SubscriptionCacheGroup, cacheKey, s_subscriptionCacheDuration, cancellationToken);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var armClient = await AzureHelper.CreateArmClientAsync(this, tenant, cancellationToken: cancellationToken);
        var subscriptions = armClient.GetSubscriptions();
        var results = new List<SubscriptionData>();

        await foreach (var subscription in subscriptions.WithCancellation(cancellationToken))
        {
            results.Add(subscription.Data);
            if (results.Count >= MaxSubscriptions)
            {
                _logger.LogWarning("Reached maximum subscription limit of {MaxSubscriptions}. Some subscriptions may not be included in the results.", MaxSubscriptions);
                break;
            }
        }

        // Cache the results
        await _cacheService.SetAsync(SubscriptionCacheGroup, cacheKey, results, s_subscriptionCacheDuration, cancellationToken);

        return results;
    }

    /// <inheritdoc/>
    public async Task<SubscriptionResource> GetSubscription(
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        AzureHelper.ValidateRequiredParameters((nameof(subscription), subscription));

        // Get the subscription ID first, whether the input is a name or ID
        var subscriptionId = await GetSubscriptionId(subscription, tenant, cancellationToken);

        // Use subscription ID for cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? CacheKeyBuilder.Build(GetSubscriptionCacheKey, subscriptionId)
            : CacheKeyBuilder.Build(GetSubscriptionCacheKey, subscriptionId, tenant);
        var cachedSubscription = await _cacheService.GetAsync<SubscriptionResource>(SubscriptionCacheGroup, cacheKey, s_subscriptionCacheDuration, cancellationToken);
        if (cachedSubscription != null)
        {
            return cachedSubscription;
        }

        var armClient = await AzureHelper.CreateArmClientAsync(this, tenant, cancellationToken: cancellationToken);
        var response = await armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId)).GetAsync(cancellationToken);
        if (response?.Value == null)
        {
            throw new KeyNotFoundException($"Could not retrieve subscription {subscription}");
        }

        // Cache the result using subscription ID
        await _cacheService.SetAsync(SubscriptionCacheGroup, cacheKey, response.Value, s_subscriptionCacheDuration, cancellationToken);

        return response.Value;
    }

    /// <inheritdoc/>
    public bool IsSubscriptionId(string subscription) => Guid.TryParse(subscription, out _);

    /// <inheritdoc/>
    public async Task<string> GetSubscriptionIdByName(
        string subscriptionName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await GetSubscriptions(tenant, cancellationToken);
        var matchingSubscriptions = subscriptions.Where(s => s.DisplayName.Equals(subscriptionName, StringComparisons.SubscriptionDisplayName)).ToList();
        if (matchingSubscriptions.Count == 0)
        {
            throw new KeyNotFoundException($"Could not find subscription with name {subscriptionName}");
        }
        else if (matchingSubscriptions.Count > 1)
        {
            throw new InvalidOperationException($"Multiple subscriptions found with name {subscriptionName}.");
        }

        return matchingSubscriptions[0].SubscriptionId;
    }

    /// <inheritdoc/>
    public string? GetDefaultSubscriptionId() => _subscriptionResolver.GetDefaultSubscriptionId();

    private async Task<string> GetSubscriptionId(
        string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        if (IsSubscriptionId(subscription))
        {
            return subscription;
        }

        return await GetSubscriptionIdByName(subscription, tenant, cancellationToken);
    }

    #endregion Subscription

    #region Resource Group

    public async Task<List<ResourceGroupInfo>> GetResourceGroups(string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        AzureHelper.ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await GetSubscription(subscription, tenant, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;

        // Try to get from cache first
        var cacheKey = CacheKeyBuilder.Build(ListResourceGroupCacheKey, subscriptionId, tenant ?? "default");
        var cachedResults = await _cacheService.GetAsync<List<ResourceGroupInfo>>(ResourceGroupCacheGroup, cacheKey, s_resourceGroupCacheDuration, cancellationToken);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var resourceGroups = await subscriptionResource.GetResourceGroups()
            .GetAllAsync(cancellationToken: cancellationToken)
            .Select(rg => new ResourceGroupInfo(
                rg.Data.Name,
                rg.Data.Id.ToString(),
                rg.Data.Location.ToString()))
            .ToListAsync(cancellationToken: cancellationToken);

        // Cache the results
        await _cacheService.SetAsync(ResourceGroupCacheGroup, cacheKey, resourceGroups, s_resourceGroupCacheDuration, cancellationToken);

        return resourceGroups;
    }

    public async Task<ResourceGroupInfo?> GetResourceGroup(
        string subscription,
        string resourceGroupName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        AzureHelper.ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroupName), resourceGroupName));

        var subscriptionResource = await GetSubscription(subscription, tenant, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;

        // Try to get from cache first
        var cacheKey = CacheKeyBuilder.Build(ListResourceGroupCacheKey, subscriptionId, tenant ?? "default");
        var cachedResults = await _cacheService.GetAsync<List<ResourceGroupInfo>>(ResourceGroupCacheGroup, cacheKey, s_resourceGroupCacheDuration, cancellationToken);
        if (cachedResults != null)
        {
            return cachedResults.FirstOrDefault(rg => rg.Name.Equals(resourceGroupName, StringComparisons.ResourceGroup));
        }

        var rg = await GetResourceGroupResource(subscription, resourceGroupName, tenant, cancellationToken);
        if (rg == null)
        {
            return null;
        }

        return new(rg.Data.Name, rg.Data.Id.ToString(), rg.Data.Location.ToString());
    }

    #endregion Resource Group

    #region Generic Resource

    public async Task<ResourceGroupResource?> GetResourceGroupResource(
        string subscription,
        string resourceGroupName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        AzureHelper.ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroupName), resourceGroupName));

        var subscriptionResource = await GetSubscription(subscription, tenant, cancellationToken);
        var resourceGroupResponse = await subscriptionResource.GetResourceGroups()
            .GetAsync(resourceGroupName, cancellationToken)
            .ConfigureAwait(false);

        return resourceGroupResponse?.Value;
    }

    public async IAsyncEnumerable<GenericResourceInfo> GetGenericResources(
        string subscription,
        string resourceGroupName,
        string? tenant = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        AzureHelper.ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroupName), resourceGroupName));

        var resourceGroupResource = await GetResourceGroupResource(subscription, resourceGroupName, tenant, cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroupName}' not found");

        await foreach (var r in resourceGroupResource.GetGenericResourcesAsync(cancellationToken: cancellationToken))
        {
            yield return new GenericResourceInfo(
                r.Data.Name,
                r.Data.Id.ToString(),
                r.Data.ResourceType.ToString(),
                r.Data.Location.ToString());
        }
    }

    #endregion Generic Resource
}
