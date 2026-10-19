// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

internal sealed class AdvisorResourceGraphQueryExecutor(IAzureService azureService)
{
    internal const int ResourceGraphPageSize = 1000;
    internal const int ResourceGraphScopeBatchSize = 50;
    internal const int ResourceGraphMaxPages = 100;
    internal const int MaxConcurrentScopeBatches = 4;
    internal static readonly TimeSpan ScopeBatchTimeout = TimeSpan.FromSeconds(45);

    internal sealed record PagedQueryResult(IReadOnlyList<JsonElement> Rows, bool IsTruncated);

    private readonly IAzureService _azureService = azureService;

    internal async Task<(RecommendationQueryScope Scope, TenantResource TenantResource)> ResolveSubscriptionScopeAsync(
        string subscription,
        string? resourceGroup,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var resolved = await ResolveSubscriptionsScopeAsync(
            [subscription],
            resourceGroup,
            tenant,
            cancellationToken);
        return (resolved.Scope, resolved.TenantResource);
    }

    internal async Task<(
        RecommendationQueryScope Scope,
        TenantResource TenantResource,
        SubscriptionResource SubscriptionResource)> ResolveSubscriptionsScopeAsync(
        IReadOnlyCollection<string> subscriptions,
        string? resourceGroup,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);
        var normalizedSubscriptions = subscriptions
            .Where(static subscription => !string.IsNullOrWhiteSpace(subscription))
            .Select(static subscription => subscription.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        ArgumentOutOfRangeException.ThrowIfZero(normalizedSubscriptions.Length);

        var subscriptionResources = new List<SubscriptionResource>(normalizedSubscriptions.Length);
        foreach (var subscription in normalizedSubscriptions)
        {
            subscriptionResources.Add(await _azureService.GetSubscription(
                subscription,
                tenant,
                cancellationToken: cancellationToken));
        }

        var subscriptionResource = subscriptionResources[0];
        var tenantIds = subscriptionResources
            .Select(resource => resource.Data.TenantId)
            .Distinct()
            .ToArray();
        if (tenantIds.Length != 1)
        {
            throw new InvalidOperationException(
                "All selected subscriptions must belong to the same Azure tenant.");
        }

        var subscriptionIds = subscriptionResources
            .Select(resource => resource.Data.SubscriptionId)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id => id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (subscriptionIds.Length != normalizedSubscriptions.Length)
        {
            throw new InvalidOperationException("One or more resolved Azure subscriptions did not have a subscription ID.");
        }

        var subscriptionId = subscriptionResource.Data.SubscriptionId
            ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");
        var normalizedResourceGroup = string.IsNullOrWhiteSpace(resourceGroup)
            ? null
            : resourceGroup.Trim();

        if (normalizedResourceGroup is not null)
        {
            foreach (var resource in subscriptionResources)
            {
                var exists = await resource
                    .GetResourceGroups()
                    .ExistsAsync(normalizedResourceGroup, cancellationToken);
                if (!exists.Value)
                {
                    throw new KeyNotFoundException(
                        $"Resource group '{normalizedResourceGroup}' does not exist in subscription '{resource.Data.SubscriptionId}'.");
                }
            }
        }

        var tenantResource = await ResolveTenantResourceAsync(
            subscriptionResource.Data.TenantId,
            cancellationToken);
        return (
            normalizedSubscriptions.Length == 1
                ? RecommendationQueryScope.ForSubscription(subscriptionId, normalizedResourceGroup)
                : RecommendationQueryScope.ForSubscriptions(subscriptionIds, normalizedResourceGroup),
            tenantResource,
            subscriptionResource);
    }

    internal async Task<(RecommendationQueryScope Scope, TenantResource TenantResource)> ResolveServiceGroupScopeAsync(
        string serviceGroup,
        string? tenant,
        CancellationToken cancellationToken)
        => await ResolveServiceGroupScopeAsync(
            [serviceGroup],
            tenant,
            cancellationToken);

    internal async Task<(RecommendationQueryScope Scope, TenantResource TenantResource)> ResolveServiceGroupScopeAsync(
        IReadOnlyCollection<string> serviceGroups,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var scope = RecommendationQueryScope.ForServiceGroups(serviceGroups);
        var tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);
        return (scope, tenantResource);
    }

    internal async Task<TenantResource> ResolveTenantResourceAsync(
        string? tenant,
        CancellationToken cancellationToken)
    {
        var tenants = await _azureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        if (string.IsNullOrWhiteSpace(tenant))
        {
            return tenants[0];
        }

        var resolvedTenantId = await _azureService.ResolveTenantIdAsync(tenant, cancellationToken)
            ?? throw new InvalidOperationException($"Could not resolve tenant '{tenant}'.");
        if (!Guid.TryParse(resolvedTenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Resolved tenant '{tenant}' does not have a valid tenant ID.");
        }

        return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
    }

    internal async Task<TenantResource> ResolveTenantResourceAsync(
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (tenantId is null)
        {
            throw new InvalidOperationException("The resolved Azure subscription does not have a tenant ID.");
        }

        var tenants = await _azureService.GetTenants(cancellationToken);
        return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
            ?? throw new InvalidOperationException(
                $"No accessible tenant found for tenant ID '{tenantId}'.");
    }

    /// <summary>Runs <paramref name="query"/> at tenant scope; the query's own predicates select the subscription or service group.</summary>
    internal static async Task<ResourceQueryResult> ExecuteAsync(
        TenantResource tenantResource,
        string query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantResource);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var response = await tenantResource.GetResourcesAsync(new ResourceQueryContent(query), cancellationToken);
        var limitHit = response.GetRawResponse().Headers.TryGetValue(TenantSubscriptionLimitHitHeader, out var value)
            ? value
            : null;
        EnsureFullTenantCoverage(limitHit);
        return response.Value;
    }

    internal static async Task<PagedQueryResult> ExecutePagedAsync(
        TenantResource tenantResource,
        string query,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(ScopeBatchTimeout);

        var rows = new List<JsonElement>();
        var isTruncated = false;
        var seenSkipTokens = new HashSet<string>(StringComparer.Ordinal);
        string? skipToken = null;
        for (var page = 1; ; page++)
        {
            var queryContent = new ResourceQueryContent(query)
            {
                Options = new ResourceQueryRequestOptions
                {
                    Top = ResourceGraphPageSize,
                    SkipToken = skipToken,
                },
            };
            try
            {
                var response = await tenantResource.GetResourcesAsync(queryContent, timeoutCancellation.Token);
                var limitHit = response.GetRawResponse().Headers.TryGetValue(TenantSubscriptionLimitHitHeader, out var value)
                    ? value
                    : null;
                EnsureFullTenantCoverage(limitHit);

                var result = response.Value;
                isTruncated |= result.ResultTruncated == ResultTruncated.True;
                if (result.Count > 0 && result.Data is not null)
                {
                    using var document = JsonDocument.Parse(result.Data);
                    if (document.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        rows.AddRange(document.RootElement.EnumerateArray().Select(static row => row.Clone()));
                    }
                }

                skipToken = result.SkipToken;
                if (string.IsNullOrWhiteSpace(skipToken))
                {
                    return new(rows, isTruncated);
                }
            }
            catch (OperationCanceledException) when (
                timeoutCancellation.IsCancellationRequested &&
                !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Azure Resource Graph Advisor query exceeded the {ScopeBatchTimeout.TotalSeconds:0}-second batch timeout.");
            }

            if (!seenSkipTokens.Add(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph returned a repeated skip token while paging Advisor recommendations.");
            }

            if (page >= ResourceGraphMaxPages)
            {
                throw new InvalidOperationException(
                    $"Azure Resource Graph pagination exceeded the {ResourceGraphMaxPages}-page limit; results are incomplete.");
            }
        }
    }

    internal static async Task<IReadOnlyList<PagedQueryResult>> ExecuteBatchedPagedAsync(
        RecommendationQueryScope scope,
        TenantResource tenantResource,
        Func<RecommendationQueryScope, string> queryFactory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(tenantResource);
        ArgumentNullException.ThrowIfNull(queryFactory);

        var batchScopes = scope.Batch(ResourceGraphScopeBatchSize).ToArray();
        var results = new PagedQueryResult[batchScopes.Length];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, batchScopes.Length),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxConcurrentScopeBatches,
                CancellationToken = cancellationToken,
            },
            async (index, batchCancellationToken) =>
            {
                results[index] = await ExecutePagedAsync(
                    tenantResource,
                    queryFactory(batchScopes[index]),
                    batchCancellationToken);
            });

        return results;
    }

    /// <summary>
    /// Response header Azure Resource Graph sets to <c>true</c> when a tenant-scope query covered only the first 10,000
    /// accessible subscriptions.
    /// </summary>
    internal const string TenantSubscriptionLimitHitHeader = "x-ms-tenant-subscription-limit-hit";

    /// <summary>
    /// Rejects a tenant-scope result that did not cover every accessible subscription. The KQL predicates cannot recover a
    /// subscription that was left out of the query scope, so a partial result would be reported as a complete total.
    /// </summary>
    internal static void EnsureFullTenantCoverage(string? tenantSubscriptionLimitHit)
    {
        if (bool.TryParse(tenantSubscriptionLimitHit, out var limitHit) && limitHit)
        {
            throw new InvalidOperationException(
                "Azure Resource Graph evaluated only the first 10,000 accessible subscriptions in the tenant, so the summary would be incomplete. Retry with an identity that has access to fewer subscriptions.");
        }
    }
}
