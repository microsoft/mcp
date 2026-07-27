// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Insights.Options;
using Azure.Mcp.Tools.Insights.Services.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.Insights.Services;

/// <inheritdoc cref="IInsightsService"/>
public sealed class InsightsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<InsightsService> logger)
    : BaseAzureService(tenantService), IInsightsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private readonly ILogger<InsightsService> _logger = logger;

    private const int PageSize = 1000;

    private const int MaxPages = 100;

    private const string CacheGroup = "insights";

    // Cache ARG data for 1 hour
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    // Minimum interval between forced ARG scans for the same scope (--nocache)
    private static readonly TimeSpan NoCacheGuardWindow = TimeSpan.FromMinutes(5);

    // Filters out portal/test/managed/system resource groups
    private const string KqlQuery = """
        Resources
        | where type !startswith "microsoft.portal/"
        | where type !startswith "providers.test/"
        | where isempty(managedBy)
        | where not (tags contains "hidden-") and not (tags contains "link:")
        | where resourceGroup !startswith "mc_"
            and resourceGroup !startswith "databricks-rg-"
            and resourceGroup !startswith "azurebackuprg_"
            and resourceGroup !startswith "defaultresourcegroup-"
            and resourceGroup != "networkwatcherrg"
        | project id, name, type, kind, location, resourceGroup, subscriptionId, sku, identity, tags, properties
        """;

    public async Task<SubscriptionAggregation> AggregateSubscriptionAsync(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken,
        IProgress<string>? progress = null,
        bool noCache = false)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        // Cache ARG data by subscription ID
        var cacheKey = $"sub:{subscriptionResource.Data.SubscriptionId}";

        var cooldown = noCache ? await GetOrStartCooldown(cacheKey, cancellationToken) : null;
        var isRateLimited = cooldown is not null;

        // Use cache when --nocache is not set, or when it is set but rate limit is in effect.
        var shouldUseCache = !noCache || isRateLimited;

        if (shouldUseCache)
        {
            // Best effort to use cached data
            var cached = await _cacheService.GetAsync<SubscriptionAggregation>(CacheGroup, cacheKey, CacheTtl, cancellationToken);
            if (cached is not null)
            {
                progress?.Report(isRateLimited
                    ? $"Using cached Azure data due to rate limit. Try again in {cooldown.GetValueOrDefault().TotalSeconds:0}s."
                    : "Using cached Azure data.");
                return cached;
            }
        }

        progress?.Report(isRateLimited
            ? "No cache available; fetching fresh Azure data."
            : noCache
                ? $"--{InsightsOptionDefinitions.NoCacheName} set; fetching fresh Azure data."
                : "Fetching fresh Azure data.");

        var tenantResource = await GetTenantResourceAsync(subscriptionResource.Data.TenantId, cancellationToken);

        var aggregation = await RunQueryAsync(
            tenantResource,
            new[] { subscriptionResource.Data.SubscriptionId },
            subscriptionCount: 1,
            scopeLabel: subscription,
            progress,
            cancellationToken);

        await _cacheService.SetAsync(CacheGroup, cacheKey, aggregation, CacheTtl, cancellationToken);
        return aggregation;
    }

    public async Task<SubscriptionAggregation> AggregateTenantAsync(
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken,
        IProgress<string>? progress = null,
        bool noCache = false)
    {
        var subscriptions = await _subscriptionService.GetSubscriptions(tenant, retryPolicy, cancellationToken);
        if (subscriptions.Count == 0)
        {
            throw new InvalidOperationException("No accessible subscriptions were found in the tenant.");
        }

        var tenantId = subscriptions[0].TenantId
            ?? throw new InvalidOperationException("Could not determine tenant ID from accessible subscriptions.");

        // Cache ARG data by tenant ID
        var cacheKey = $"tenant:{tenantId}";

        var cooldown = noCache ? await GetOrStartCooldown(cacheKey, cancellationToken) : null;
        var isRateLimited = cooldown is not null;

        // Use cache when --nocache is not set, or when it is set but rate limit is in effect.
        var shouldUseCache = !noCache || isRateLimited;

        if (shouldUseCache)
        {
            // Best effort to use cached data
            var cached = await _cacheService.GetAsync<SubscriptionAggregation>(CacheGroup, cacheKey, CacheTtl, cancellationToken);
            if (cached is not null)
            {
                progress?.Report(isRateLimited
                    ? $"Using cached Azure data due to rate limit. Try again in {cooldown.GetValueOrDefault().TotalSeconds:0}s."
                    : "Using cached Azure data.");
                return cached;
            }
        }

        progress?.Report(isRateLimited
            ? "No cache available; fetching fresh Azure data."
            : noCache
                ? $"--{InsightsOptionDefinitions.NoCacheName} set; fetching fresh Azure data."
                : "Fetching fresh Azure data.");

        var tenantResource = await GetTenantResourceAsync(tenantId, cancellationToken);

        var subscriptionIds = subscriptions
            .Select(s => s.SubscriptionId)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToArray();

        var aggregation = await RunQueryAsync(
            tenantResource,
            subscriptionIds,
            subscriptionCount: subscriptionIds.Length,
            scopeLabel: $"tenant:{tenantId}",
            progress,
            cancellationToken);

        await _cacheService.SetAsync(CacheGroup, cacheKey, aggregation, CacheTtl, cancellationToken);
        return aggregation;
    }

    private async Task<SubscriptionAggregation> RunQueryAsync(
        TenantResource tenantResource,
        IReadOnlyList<string> subscriptionIds,
        int subscriptionCount,
        string scopeLabel,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var rows = new List<JsonElement>();
        string? skipToken = null;
        var pages = 0;
        var documents = new List<JsonDocument>();

        try
        {
            while (true)
            {
                pages++;
                progress?.Report($"Fetching ARG page {pages} for {scopeLabel}...");

                var queryContent = new ResourceQueryContent(KqlQuery)
                {
                    Options = new ResourceQueryRequestOptions
                    {
                        Top = PageSize,
                        SkipToken = skipToken,
                    },
                };
                foreach (var id in subscriptionIds)
                {
                    queryContent.Subscriptions.Add(id);
                }

                ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
                if (result is null)
                {
                    break;
                }

                if (result.Count > 0 && result.Data is not null)
                {
                    var doc = JsonDocument.Parse(result.Data);
                    documents.Add(doc);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        rows.AddRange(doc.RootElement.EnumerateArray());
                    }
                }

                skipToken = result.SkipToken;
                if (string.IsNullOrEmpty(skipToken))
                {
                    break;
                }

                if (pages >= MaxPages)
                {
                    _logger.LogWarning(
                        "Reached pagination cap of {MaxPages} pages for scope {Scope}; results may be truncated.",
                        MaxPages, scopeLabel);
                    break;
                }
            }

            var aggregation = PropertyAggregator.Aggregate(rows, subscriptionCount);
            progress?.Report($"Aggregating properties across {rows.Count} resources for {scopeLabel}.");
            return AggregationFilter.Filter(aggregation);
        }
        finally
        {
            foreach (var doc in documents)
            {
                doc.Dispose();
            }
        }
    }

    /// <summary>
    /// Get remaining cooldown duration for <c>--nocache</c>; or start a new rate-limit and return null.
    /// </summary>
    private async Task<TimeSpan?> GetOrStartCooldown(string cacheKey, CancellationToken cancellationToken)
    {
        var guardKey = $"nocache-guard:{cacheKey}";
        var cooldownEnds = await _cacheService.GetAsync<DateTimeOffset>(CacheGroup, guardKey, NoCacheGuardWindow, cancellationToken);
        if (cooldownEnds != default)
        {
            var remaining = cooldownEnds - DateTimeOffset.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        await _cacheService.SetAsync(CacheGroup, guardKey, DateTimeOffset.UtcNow + NoCacheGuardWindow, NoCacheGuardWindow, cancellationToken);
        return null;
    }

    private async Task<TenantResource> GetTenantResourceAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        if (tenantId is null)
        {
            throw new ArgumentException("Tenant ID cannot be null.", nameof(tenantId));
        }

        var allTenants = await TenantService.GetTenants(cancellationToken);
        var tenantResource = allTenants.FirstOrDefault(t => t.Data.TenantId == tenantId.Value)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant ID '{tenantId}'.");
        return tenantResource;
    }

}
