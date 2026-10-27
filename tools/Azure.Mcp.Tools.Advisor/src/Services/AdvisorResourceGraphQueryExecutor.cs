// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Validation;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

internal sealed class AdvisorResourceGraphQueryExecutor(IAzureService azureService)
{
    internal const int ResourceGraphPageSize = 1000;
    internal const int ResourceGraphMaxPages = 100;
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
        IReadOnlyList<SubscriptionResource> Subscriptions)> ResolveSubscriptionsScopeAsync(
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

        if (subscriptionResources.Any(static resource => string.IsNullOrWhiteSpace(resource.Data.SubscriptionId)))
        {
            throw new InvalidOperationException("One or more resolved Azure subscriptions did not have a subscription ID.");
        }
        var subscriptionIds = subscriptionResources
            .Select(static resource => resource.Data.SubscriptionId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

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
            subscriptionIds.Length == 1
                ? RecommendationQueryScope.ForSubscription(subscriptionId, normalizedResourceGroup)
                : RecommendationQueryScope.ForSubscriptions(subscriptionIds, normalizedResourceGroup),
            tenantResource,
            subscriptionResources);
    }

    internal async Task<(RecommendationQueryScope Scope, TenantResource TenantResource)> ResolveServiceGroupScopeAsync(
        string serviceGroup,
        string? tenant,
        CancellationToken cancellationToken)
    {
        RecommendationScopeValidator.ThrowIfInvalidServiceGroupInput(serviceGroup);
        var tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);
        var requestedScope = serviceGroup.Trim();
        var escapedScope = RecommendationQueryBuilder.EscapeKqlString(requestedScope);
        var query = $"""
            resourcecontainers
            | where type =~ 'microsoft.management/servicegroups'
            | extend
                serviceGroupId = tostring(id),
                serviceGroupName = coalesce(tostring(properties.displayName), tostring(name)),
                parentServiceGroupId = tostring(properties.parent.resourceId),
                tenantRootServiceGroupId =
                    strcat('/providers/Microsoft.Management/serviceGroups/', tostring(tenantId))
            | where isnotempty(serviceGroupId)
            | where parentServiceGroupId =~ tenantRootServiceGroupId
            | where serviceGroupId =~ '{escapedScope}' or name =~ '{escapedScope}' or serviceGroupName =~ '{escapedScope}'
            | project id = serviceGroupId, name = serviceGroupName
            | order by name asc
            """;
        var result = await ExecutePagedAsync(tenantResource, query, cancellationToken);
        if (result.IsTruncated)
        {
            throw new InvalidOperationException(
                "Service Group lookup was truncated. A unique scope cannot be determined from incomplete results.");
        }

        var matches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in result.Rows)
        {
            if (row.ValueKind != JsonValueKind.Object ||
                !row.TryGetProperty("id", out var idProperty) ||
                idProperty.ValueKind != JsonValueKind.String ||
                !row.TryGetProperty("name", out var nameProperty) ||
                nameProperty.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("Azure Resource Graph returned a Service Group without a valid ID or name.");
            }

            var id = idProperty.GetString()!;
            // The projected name is a display label; the ARM ID supplies the canonical scope identifier.
            var identifier = id.StartsWith(RecommendationQueryBuilder.ServiceGroupResourceIdPrefix, StringComparison.OrdinalIgnoreCase)
                ? id[RecommendationQueryBuilder.ServiceGroupResourceIdPrefix.Length..]
                : string.Empty;
            if (!RecommendationScopeValidator.IsValidServiceGroupId(identifier))
            {
                throw new JsonException("Azure Resource Graph returned an invalid Service Group resource ID.");
            }
            matches.TryAdd(id, identifier);
        }

        // An explicit identifier takes precedence over another group's matching display name.
        var exactMatch = matches.FirstOrDefault(match =>
            match.Key.Equals(requestedScope, StringComparison.OrdinalIgnoreCase) ||
            match.Value.Equals(requestedScope, StringComparison.OrdinalIgnoreCase));
        if (exactMatch.Key is not null)
        {
            return (RecommendationQueryScope.ForServiceGroup(exactMatch.Value), tenantResource);
        }
        if (matches.Count == 1)
        {
            return (RecommendationQueryScope.ForServiceGroup(matches.Values.Single()), tenantResource);
        }
        if (matches.Count == 0)
        {
            throw new KeyNotFoundException(
                $"No accessible Service Group matched '{requestedScope}' in the selected tenant. Verify the name or ID, tenant, and access.");
        }

        throw new ArgumentException(
            $"Service Group name '{requestedScope}' is ambiguous in the selected tenant. Specify one full ARM resource ID: {string.Join(", ", matches.Keys.Order(StringComparer.OrdinalIgnoreCase))}.",
            nameof(serviceGroup));
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
                isTruncated = result.ResultTruncated == ResultTruncated.True;
                if (result.Count > 0)
                {
                    if (result.Data is null)
                    {
                        throw new JsonException("Azure Resource Graph returned non-empty results without data.");
                    }
                    using var document = JsonDocument.Parse(result.Data);
                    if (document.RootElement.ValueKind != JsonValueKind.Array ||
                        document.RootElement.GetArrayLength() != result.Count)
                    {
                        throw new JsonException("Azure Resource Graph returned invalid or incomplete row data.");
                    }
                    rows.AddRange(document.RootElement.EnumerateArray().Select(static row => row.Clone()));
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
