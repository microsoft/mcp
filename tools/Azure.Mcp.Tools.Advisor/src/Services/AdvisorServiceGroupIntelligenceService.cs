// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorServiceGroupIntelligenceService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAdvisorServiceGroupIntelligenceService
{
    private const string ServiceGroupPrefix = "/providers/microsoft.management/servicegroups/";
    private const string IntelligenceSegment = "/providers/microsoft.advisor/servicegroupintelligence/";
    private const int MaxArgPageSize = 1000;

    internal const string NotApplicableLabel = "NotApplicable";

    // ARG stores criticality as a numeric string (Tier 0-3); labels follow the Service Group criticality tiers.
    // NOTE: "Not applicable" is assumed to be stored as a missing or empty criticality; this has not been
    // observed in live data, so verify it against a Service Group set to "Not applicable".
    private static readonly (string Label, string Level)[] CriticalityLevels =
    [
        ("MissionCritical", "0"),
        ("BusinessCritical", "1"),
        ("BusinessOperational", "2"),
        ("Administrative", "3"),
        (NotApplicableLabel, string.Empty),
    ];

    private static readonly string[] StatusValues =
    [
        "On Track",
        "Attention Needed",
        "At Risk",
        "Critical",
        "Unavailable",
    ];

    internal delegate Task<StatusPage> FetchStatusPageAsync(
        int pageSize,
        string? skipToken,
        CancellationToken cancellationToken);

    internal sealed record StatusPage(JsonElement Rows, string? SkipToken, bool IsTruncated = false);

    public async Task<ResourceQueryResults<AdvisorServiceGroupStatus>> GetStatusAsync(
        string? serviceGroup,
        string? criticality,
        string? status,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(top, 1);

        var query = BuildStatusQuery(serviceGroup, criticality, status);
        var tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);

        async Task<StatusPage> FetchAsync(int pageSize, string? skipToken, CancellationToken ct)
        {
            var queryContent = new ResourceQueryContent(query)
            {
                Options = new ResourceQueryRequestOptions { Top = pageSize, SkipToken = skipToken },
            };

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

        var statuses = await CollectStatusAsync(FetchAsync, top, cancellationToken);
        return new(statuses.Statuses, statuses.IsTruncated);
    }

    /// <summary>
    /// Maps a user supplied criticality label to its canonical label. Matching ignores case and spaces.
    /// </summary>
    internal static bool TryNormalizeCriticality(string? value, out string label)
    {
        var key = Compact(value);
        foreach (var (candidate, level) in CriticalityLevels)
        {
            // Accept the label, and for tiers 0-3 also "Tier0"/"Tier 0" and the bare number.
            var isTier = level.Length > 0;
            if (candidate.Equals(key, StringComparison.OrdinalIgnoreCase)
                || (isTier && (key == level || key.Equals($"Tier{level}", StringComparison.OrdinalIgnoreCase))))
            {
                label = candidate;
                return true;
            }
        }

        label = string.Empty;
        return false;
    }

    /// <summary>
    /// Maps a user supplied status to its canonical value. Matching ignores case and spaces.
    /// </summary>
    internal static bool TryNormalizeStatus(string? value, out string status)
    {
        var key = Compact(value);
        foreach (var candidate in StatusValues)
        {
            if (Compact(candidate).Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                status = candidate;
                return true;
            }
        }

        status = string.Empty;
        return false;
    }

    internal static string BuildStatusQuery(string? serviceGroup, string? criticality, string? status)
    {
        var scopePrefix = string.IsNullOrWhiteSpace(serviceGroup)
            ? ServiceGroupPrefix
            : $"{ServiceGroupPrefix}{serviceGroup.Trim().ToLowerInvariant()}{IntelligenceSegment}";

        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/servicegroupintelligence' " +
            $"| where tolower(id) startswith '{RecommendationQueryBuilder.SanitizeForKql(scopePrefix)}' " +
            "| where tostring(properties.intelligenceName) =~ 'Health' " +
            "| where tobool(properties.isDeleted) != true ";

        if (!string.IsNullOrWhiteSpace(criticality))
        {
            if (!TryNormalizeCriticality(criticality, out var label))
            {
                throw new ArgumentException($"Unsupported criticality '{criticality}'.", nameof(criticality));
            }

            var level = CriticalityLevels.First(item => item.Label == label).Level;
            query += level.Length == 0
                ? "| where isempty(tostring(properties.criticality)) "
                : $"| where tostring(properties.criticality) == '{level}' ";
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!TryNormalizeStatus(status, out var canonicalStatus))
            {
                throw new ArgumentException($"Unsupported status '{status}'.", nameof(status));
            }

            // ARG does not support mv-apply; expanding is safe because each record has exactly one Status KPI,
            // so 'id' stays unique for $skipToken paging.
            query +=
                "| mv-expand kpi = properties.intelligenceDetail " +
                $"| where tostring(kpi.kpiName) =~ 'Status' and tostring(kpi.kpiValue) =~ '{canonicalStatus}' ";
        }

        // 'id' must stay projected and sorted: Azure Resource Graph needs it to issue a valid $skipToken.
        return query + "| project id, name, type, properties | order by id asc";
    }

    internal static async Task<(List<AdvisorServiceGroupStatus> Statuses, bool IsTruncated)> CollectStatusAsync(
        FetchStatusPageAsync fetchPage,
        int top,
        CancellationToken cancellationToken)
    {
        var statuses = new List<AdvisorServiceGroupStatus>();
        var seenTokens = new HashSet<string>(StringComparer.Ordinal);
        string? skipToken = null;
        var sourceTruncated = false;

        // Request one row beyond 'top' so truncation can be reported without a second call.
        var pageSize = (int)Math.Min((long)top + 1, MaxArgPageSize);

        do
        {
            var page = await fetchPage(pageSize, skipToken, cancellationToken);
            statuses.AddRange(ParseStatuses(page.Rows));

            skipToken = string.IsNullOrWhiteSpace(page.SkipToken) ? null : page.SkipToken;

            // ARG can flag a truncated result without a continuation token; surface it instead of reporting a complete list.
            sourceTruncated |= page.IsTruncated && skipToken is null;
            if (skipToken is not null && !seenTokens.Add(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph returned a repeated skip token while getting Advisor Service Group status.");
            }
        }
        while (skipToken is not null && statuses.Count <= top);

        var exceedsTop = statuses.Count > top;
        return (exceedsTop ? statuses.GetRange(0, top) : statuses, exceedsTop || sourceTruncated);
    }

    internal static List<AdvisorServiceGroupStatus> ParseStatuses(JsonElement rows)
    {
        var statuses = new List<AdvisorServiceGroupStatus>();
        if (rows.ValueKind != JsonValueKind.Array)
        {
            return statuses;
        }

        foreach (var row in rows.EnumerateArray())
        {
            if (ServiceGroupIntelligenceData.FromJson(row) is { } data && ToStatus(data) is { } status)
            {
                statuses.Add(status);
            }
        }

        return statuses;
    }

    private static AdvisorServiceGroupStatus? ToStatus(ServiceGroupIntelligenceData data)
    {
        if (string.IsNullOrWhiteSpace(data.Id))
        {
            return null;
        }

        var segmentIndex = data.Id.IndexOf(IntelligenceSegment, StringComparison.OrdinalIgnoreCase);
        var serviceGroupId = data.Properties?.ServiceGroupId
            ?? (segmentIndex > 0 ? data.Id[..segmentIndex] : data.Id);
        // The name comes from the ARM ID (not properties.serviceGroupName, which can be a display name)
        // so it can be passed straight back to --service-group.
        var serviceGroupName = serviceGroupId[(serviceGroupId.LastIndexOf('/') + 1)..];

        var kpis = data.Properties?.IntelligenceDetail ?? [];
        string? Kpi(string name) => kpis
            .FirstOrDefault(kpi => string.Equals(kpi.KpiName, name, StringComparison.OrdinalIgnoreCase))?.KpiValue;

        DateTimeOffset? lastUpdated = DateTimeOffset.TryParse(
            data.Properties?.LastUpdatedTime,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed
            : null;

        return new(
            serviceGroupName,
            serviceGroupId,
            GetCriticalityLabel(data.Properties?.Criticality),
            Kpi("Status"),
            Kpi("StatusDescription"),
            Kpi("Description"),
            GetGoalText(data.Properties?.Goal),
            lastUpdated);
    }

    internal static string? GetCriticalityLabel(string? level)
    {
        var trimmed = level?.Trim() ?? string.Empty;
        return CriticalityLevels.FirstOrDefault(item => item.Level == trimmed).Label;
    }

    // The goal is null in observed data and its shape is not documented, so a string is returned as-is
    // and any other JSON value as raw JSON text.
    internal static string? GetGoalText(JsonElement? goal) => goal switch
    {
        null => null,
        { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => null,
        { ValueKind: JsonValueKind.String } value => value.GetString(),
        { } value => value.GetRawText(),
    };

    private static string Compact(string? value) =>
        string.Concat((value ?? string.Empty).Where(character => !char.IsWhiteSpace(character)));

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
