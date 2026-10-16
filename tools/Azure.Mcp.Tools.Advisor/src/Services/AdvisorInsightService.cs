// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorInsightService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAdvisorInsightService
{
    internal const string ScopeTypeServiceGroup = "ServiceGroup";
    internal const string ScopeTypeSubscription = "Subscription";

    private const string ServiceGroupPrefix = "/providers/microsoft.management/servicegroups/";
    private const string SubscriptionPrefix = "/subscriptions/";
    private const string InsightsSegment = "/providers/microsoft.advisor/insights/";
    private const int MaxArgPageSize = 1000;

    internal delegate Task<InsightPage> FetchInsightPageAsync(
        int pageSize,
        string? skipToken,
        CancellationToken cancellationToken);

    internal sealed record InsightPage(JsonElement Rows, string? SkipToken, bool IsTruncated = false);

    public async Task<ResourceQueryResults<AdvisorInsight>> ListInsightsAsync(
        string? serviceGroup,
        string? subscription,
        string? insightName,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var hasServiceGroup = !string.IsNullOrWhiteSpace(serviceGroup);
        var hasSubscription = !string.IsNullOrWhiteSpace(subscription);
        if (hasServiceGroup && hasSubscription)
        {
            throw new ArgumentException("Specify at most one of serviceGroup or subscription.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(top, 1);

        string? subscriptionId = null;
        TenantResource tenantResource;
        if (hasSubscription)
        {
            var subscriptionResource = await AzureService.GetSubscription(subscription!, tenant, cancellationToken);
            subscriptionId = subscriptionResource.Data.SubscriptionId
                ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");
            var tenantId = subscriptionResource.Data.TenantId;
            var tenants = await AzureService.GetTenants(cancellationToken);
            tenantResource = tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
                ?? throw new InvalidOperationException(
                    $"No accessible tenant was found for subscription '{subscriptionId}'.");
        }
        else
        {
            tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);
        }

        var query = BuildInsightsQuery(serviceGroup, subscriptionId, insightName);

        async Task<InsightPage> FetchAsync(int pageSize, string? skipToken, CancellationToken ct)
        {
            var queryContent = new ResourceQueryContent(query)
            {
                Options = new ResourceQueryRequestOptions { Top = pageSize, SkipToken = skipToken },
            };
            if (subscriptionId is not null)
            {
                queryContent.Subscriptions.Add(subscriptionId);
            }

            var response = await tenantResource.GetResourcesAsync(queryContent, ct);
            var result = response.Value;
            var isTruncated = result.ResultTruncated == ResultTruncated.True;
            if (result.Count == 0)
            {
                return new(default, result.SkipToken, isTruncated);
            }

            using var document = JsonDocument.Parse(result.Data);
            return new(document.RootElement.Clone(), result.SkipToken, isTruncated);
        }

        var insights = await CollectInsightsAsync(FetchAsync, top, cancellationToken);
        return new(insights.Insights, insights.IsTruncated);
    }

    internal static string BuildInsightsQuery(string? serviceGroup, string? subscriptionId, string? insightName)
    {
        var hasServiceGroup = !string.IsNullOrWhiteSpace(serviceGroup);
        var hasSubscription = !string.IsNullOrWhiteSpace(subscriptionId);
        if (hasServiceGroup && hasSubscription)
        {
            throw new ArgumentException("Specify at most one of serviceGroup or subscriptionId.");
        }

        // The ARM id is the only scope identifier with a uniform format: for subscription records
        // 'properties.insightResourceId' is a bare GUID while for Service Groups it is a full ARM id.
        // With no scope, insights for every accessible Service Group and subscription in the tenant are returned.
        var scopeFilter = hasServiceGroup
            ? StartsWith($"{ServiceGroupPrefix}{serviceGroup!.Trim().ToLowerInvariant()}{InsightsSegment}")
            : hasSubscription
                ? StartsWith($"{SubscriptionPrefix}{subscriptionId!.Trim().ToLowerInvariant()}{InsightsSegment}")
                : $"({StartsWith(ServiceGroupPrefix)} or {StartsWith(SubscriptionPrefix)})";

        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/insights' " +
            $"| where {scopeFilter} " +
            "| where tobool(properties.isDeleted) != true ";

        if (!string.IsNullOrWhiteSpace(insightName))
        {
            query += $"| where name =~ '{RecommendationQueryBuilder.SanitizeForKql(insightName)}' ";
        }

        // 'id' must stay projected and sorted: Azure Resource Graph needs it to issue a valid $skipToken.
        return query + "| project id, name, type, properties | order by id asc";
    }

    private static string StartsWith(string prefix) =>
        $"tolower(id) startswith '{RecommendationQueryBuilder.SanitizeForKql(prefix)}'";

    internal static async Task<(List<AdvisorInsight> Insights, bool IsTruncated)> CollectInsightsAsync(
        FetchInsightPageAsync fetchPage,
        int top,
        CancellationToken cancellationToken)
    {
        var insights = new List<AdvisorInsight>();
        var seenTokens = new HashSet<string>(StringComparer.Ordinal);
        string? skipToken = null;
        var sourceTruncated = false;

        // Request one row beyond 'top' so truncation can be reported without a second call.
        var pageSize = (int)Math.Min((long)top + 1, MaxArgPageSize);

        do
        {
            var page = await fetchPage(pageSize, skipToken, cancellationToken);
            insights.AddRange(ParseInsights(page.Rows));

            skipToken = string.IsNullOrWhiteSpace(page.SkipToken) ? null : page.SkipToken;

            // ARG can flag a truncated result without a continuation token; surface it instead of reporting a complete list.
            sourceTruncated |= page.IsTruncated && skipToken is null;
            if (skipToken is not null && !seenTokens.Add(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph returned a repeated skip token while listing Advisor insights.");
            }
        }
        while (skipToken is not null && insights.Count <= top);

        var exceedsTop = insights.Count > top;
        return (exceedsTop ? insights.GetRange(0, top) : insights, exceedsTop || sourceTruncated);
    }

    internal static List<AdvisorInsight> ParseInsights(JsonElement rows)
    {
        var insights = new List<AdvisorInsight>();
        if (rows.ValueKind != JsonValueKind.Array)
        {
            return insights;
        }

        foreach (var row in rows.EnumerateArray())
        {
            if (InsightData.FromJson(row) is { } data && ToInsight(data) is { } insight)
            {
                insights.Add(insight);
            }
        }

        return insights;
    }

    private static AdvisorInsight? ToInsight(InsightData data)
    {
        var name = data.Properties?.InsightName ?? data.Name;
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(data.Id))
        {
            return null;
        }

        var segmentIndex = data.Id.IndexOf(InsightsSegment, StringComparison.OrdinalIgnoreCase);
        var scopeId = segmentIndex > 0 ? data.Id[..segmentIndex] : string.Empty;
        var scopeType = GetScopeType(scopeId);

        DateTimeOffset? lastUpdated = DateTimeOffset.TryParse(
            data.Properties?.LastUpdatedTime,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed
            : null;

        var kpis = (data.Properties?.InsightDetail ?? [])
            .Where(kpi => !string.IsNullOrWhiteSpace(kpi.KpiName))
            .Select(ToKpi)
            .ToList();

        return new(name, data.Properties?.Domain, scopeType, scopeId, lastUpdated, kpis);
    }

    internal static string GetScopeType(string scopeId) =>
        scopeId.StartsWith(ServiceGroupPrefix, StringComparison.OrdinalIgnoreCase) ? ScopeTypeServiceGroup :
        scopeId.StartsWith(SubscriptionPrefix, StringComparison.OrdinalIgnoreCase) ? ScopeTypeSubscription :
        "Unknown";

    internal static AdvisorInsightKpi ToKpi(InsightKpiData kpi)
    {
        var name = kpi.KpiName!;
        var raw = kpi.KpiValue?.Trim() ?? string.Empty;
        var valueType = kpi.KpiValueType?.Trim() ?? string.Empty;

        // Some KPIs (for example ServiceRetirement counts) carry no value type, so numeric strings are inferred.
        var isCurrency = valueType.Equals("USD", StringComparison.OrdinalIgnoreCase);
        if ((isCurrency || valueType.Length == 0 || valueType.Equals("Integer", StringComparison.OrdinalIgnoreCase))
            && decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            var element = JsonDocument.Parse(number.ToString(CultureInfo.InvariantCulture)).RootElement.Clone();
            return new(name, element, isCurrency ? "USD" : null);
        }

        return new(
            name,
            JsonSerializer.SerializeToElement(kpi.KpiValue ?? string.Empty, AdvisorJsonContext.Default.String),
            null);
    }

    // Mirrors the base tenant-ambiguity behavior: never guess among several accessible tenants.
    private async Task<TenantResource> ResolveTenantResourceAsync(string? tenant, CancellationToken cancellationToken)
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found for the current credential.");
        }

        if (!string.IsNullOrWhiteSpace(tenant))
        {
            var resolvedTenantId = await AzureService.ResolveTenantIdAsync(tenant, cancellationToken)
                ?? throw new InvalidOperationException($"Could not resolve tenant '{tenant}'.");
            var tenantId = Guid.Parse(resolvedTenantId);
            return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
                ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
        }

        if (tenants.Count == 1)
        {
            return tenants[0];
        }

        throw new ArgumentException(
            "Multiple tenants are accessible, so the tenant to query cannot be inferred. Specify the tenant explicitly.",
            nameof(tenant));
    }
}
