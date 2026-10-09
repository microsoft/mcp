// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed class ServiceGroupIntelligenceService(IAzureService azureService)
    : BaseAzureService(azureService), IServiceGroupIntelligenceService
{
    internal const string ServiceGroupPrefix = "/providers/microsoft.management/servicegroups/";
    private const int PageSize = 10;
    private const int MaxPages = 5;

    private static readonly Dictionary<string, string> TierLabels = new()
    {
        ["0"] = "Mission-critical",
        ["1"] = "Business-critical",
        ["2"] = "Business-operational",
        ["3"] = "Administrative",
    };

    private static readonly string[] StatusOrder = ["critical", "atrisk", "attentionneeded", "ontrack", "unavailable"];

    public async Task<ServiceGroupStatusInsightsPage> GetStatusInsightsAsync(
        string[]? serviceGroups,
        string[]? criticalityTiers,
        string[]? statuses,
        string[]? insightNames,
        bool includeStatus,
        bool includeInsights,
        string? continuationToken,
        string? tenant,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(NormalizeServiceGroupIds(serviceGroups), insightNames);
        var tenantResource = await GetTenantResourceAsync(tenant, cancellationToken);

        var matched = new List<ServiceGroupStatusInsight>();
        var token = continuationToken;
        var truncatedWithoutToken = false;

        // Bounded paging keeps one call useful without an unbounded Resource Graph scan.
        for (var page = 0; page < MaxPages; page++)
        {
            var response = await tenantResource.GetResourcesAsync(
                new ResourceQueryContent(query)
                {
                    Options = new ResourceQueryRequestOptions { Top = PageSize, SkipToken = token }
                },
                cancellationToken);

            using var doc = JsonDocument.Parse(response.Value.Data);
            foreach (var row in doc.RootElement.EnumerateArray())
            {
                var item = Normalize(row, true, includeInsights);
                if (item is not null)
                {
                    matched.Add(item);
                }
            }

            var next = response.Value.SkipToken;
            if (string.IsNullOrEmpty(next) || next == token)
            {
                // A truncated page without a new token cannot be continued; report it as partial, not complete.
                truncatedWithoutToken = response.Value.ResultTruncated == ResultTruncated.True;
                token = null;
                break;
            }

            token = next;
        }

        var results = FilterAndProject(matched, criticalityTiers, statuses, includeStatus, includeInsights);

        return new(results, token is not null || truncatedWithoutToken, token);
    }

    internal static List<ServiceGroupStatusInsight> FilterAndProject(
        IEnumerable<ServiceGroupStatusInsight> items,
        string[]? criticalityTiers,
        string[]? statuses,
        bool includeStatus,
        bool includeInsights)
    {
        var tiers = (criticalityTiers ?? []).Select(t => t.Trim()).ToHashSet();
        var wantedStatuses = (statuses ?? []).Select(NormalizeStatus).ToHashSet();

        return items
            .Where(item => (tiers.Count == 0 || tiers.Contains(item.Criticality ?? string.Empty))
                && (wantedStatuses.Count == 0 || wantedStatuses.Contains(NormalizeStatus(item.Status))))
            .OrderBy(g => g.Criticality ?? "9", StringComparer.Ordinal)
            .ThenBy(g => Array.IndexOf(StatusOrder, NormalizeStatus(g.Status)) is var i and >= 0 ? i : int.MaxValue)
            .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .Select(item => item with
            {
                Status = includeStatus ? item.Status : null,
                StatusDescription = includeStatus ? item.StatusDescription : null,
                Description = includeStatus ? item.Description : null,
                Insights = includeInsights ? item.Insights : null,
            })
            .ToList();
    }

    // Mirrors BaseAzureResourceService: an explicit tenant is resolved; otherwise the tenant must be unambiguous.
    private async Task<TenantResource> GetTenantResourceAsync(string? tenant, CancellationToken cancellationToken)
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found for the current credential.");
        }

        if (string.IsNullOrWhiteSpace(tenant))
        {
            return tenants.Count == 1
                ? tenants[0]
                : throw new ArgumentException(
                    "Multiple tenants are accessible, so the tenant to query cannot be inferred. Specify the tenant explicitly.",
                    nameof(tenant));
        }

        var resolvedTenantId = await AzureService.ResolveTenantIdAsync(tenant, cancellationToken)
            ?? throw new InvalidOperationException($"Could not resolve tenant '{tenant}'.");
        var tenantId = Guid.Parse(resolvedTenantId);
        return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
    }

    internal static string[] NormalizeServiceGroupIds(string[]? serviceGroups) =>
        (serviceGroups ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s =>
            {
                var value = s.Trim().TrimEnd('/');
                var name = value.StartsWith('/')
                    ? value.StartsWith(ServiceGroupPrefix, StringComparison.OrdinalIgnoreCase)
                        ? value[ServiceGroupPrefix.Length..]
                        : throw new ArgumentException($"Service Group ID must match '/providers/Microsoft.Management/serviceGroups/{{name}}': {s}")
                    : value;
                if (name.Length == 0 || name.Contains('/'))
                {
                    throw new ArgumentException($"Invalid Service Group name or ID: {s}");
                }

                return ServiceGroupPrefix + name.ToLowerInvariant();
            })
            .Distinct()
            .ToArray();

    internal static string BuildQuery(string[] serviceGroupIds, string[]? insightNames)
    {
        var scopeFilter = serviceGroupIds.Length == 0
            ? string.Empty
            : $"\n| where serviceGroupScope in~ ({string.Join(",", serviceGroupIds.Select(KqlLiteral))})";
        var names = (insightNames ?? []).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToArray();
        // Status records are always kept; the name filter applies only to Insights.
        var insightFilter = names.Length == 0
            ? string.Empty
            : $"\n| where type =~ 'microsoft.advisor/serviceGroupIntelligence' or tostring(properties.insightName) in~ ({string.Join(",", names.Select(KqlLiteral))})";

        return $"""
            advisorresources
            | where type in~ ('microsoft.advisor/insights', 'microsoft.advisor/serviceGroupIntelligence')
            | where tolower(id) startswith '{ServiceGroupPrefix}'
            | extend serviceGroupScope = tolower(case(
                isnotempty(tostring(properties.serviceGroupId)), tostring(properties.serviceGroupId),
                isnotempty(tostring(properties.insightResourceId)), tostring(properties.insightResourceId),
                tostring(properties.resourceId)))
            | where serviceGroupScope startswith '{ServiceGroupPrefix}'{scopeFilter}
            | where isempty(tostring(properties.isDeleted)) or tobool(properties.isDeleted) == false{insightFilter}
            | project id, name, type, properties, serviceGroupScope
            | summarize records = make_list(pack_all()) by id = serviceGroupScope
            | order by id asc
            """.ReplaceLineEndings("\n");
    }

    internal static ServiceGroupStatusInsight? Normalize(JsonElement row, bool includeStatus, bool includeInsights)
    {
        var scope = Str(row, "id");
        if (scope is null || !row.TryGetProperty("records", out var records) || records.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var list = records.EnumerateArray().Where(r => Prop(r, "properties") is { ValueKind: JsonValueKind.Object }).ToList();

        // The newest authoritative Status record wins when duplicates exist.
        var status = Newest(list.Where(r =>
            string.Equals(Str(r, "type"), "microsoft.advisor/servicegroupintelligence", StringComparison.OrdinalIgnoreCase)
            && string.Equals(Str(Prop(r, "properties"), "intelligenceName"), "Health", StringComparison.OrdinalIgnoreCase)));

        var insights = list
            .Where(r => string.Equals(Str(r, "type"), "microsoft.advisor/insights", StringComparison.OrdinalIgnoreCase))
            .Select(r => (Record: r, Name: Str(Prop(r, "properties"), "insightName")))
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !x.Name!.Equals("Health", StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.Name!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ServiceGroupInsight(g.Key, Kpis(Newest(g.Select(x => x.Record))!.Value, "insightDetail")))
            .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var meta = status ?? list.FirstOrDefault();
        var props = meta is { } m ? Prop(m, "properties") : null;
        var name = Str(props, "serviceGroupName") ?? scope[(scope.LastIndexOf('/') + 1)..];
        var criticality = Str(props, "criticality");
        var statusKpis = status is { } s ? Kpis(s, "intelligenceDetail") : [];
        statusKpis.TryGetValue("Status", out var statusValue);
        statusKpis.TryGetValue("StatusDescription", out var statusDescription);
        statusKpis.TryGetValue("Description", out var description);

        return new ServiceGroupStatusInsight(
            name,
            scope,
            criticality,
            criticality is not null && TierLabels.TryGetValue(criticality, out var label) ? label : null,
            includeStatus ? statusValue : null,
            includeStatus ? statusDescription : null,
            includeStatus ? description : null,
            includeInsights ? insights : null);
    }

    private static JsonElement? Newest(IEnumerable<JsonElement> records) =>
        records
            .Select((r, i) => (Record: r, Index: i, Time: Str(Prop(r, "properties"), "lastUpdatedTime")))
            .OrderByDescending(x => DateTimeOffset.TryParse(x.Time, out var t) ? t : DateTimeOffset.MinValue)
            .ThenByDescending(x => x.Index)
            .Select(x => (JsonElement?)x.Record)
            .FirstOrDefault();

    private static Dictionary<string, string?> Kpis(JsonElement record, string detailName)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (Prop(Prop(record, "properties"), detailName) is { ValueKind: JsonValueKind.Array } detail)
        {
            foreach (var kpi in detail.EnumerateArray())
            {
                var kpiName = Str(kpi, "kpiName");
                if (!string.IsNullOrWhiteSpace(kpiName) && kpi.TryGetProperty("kpiValue", out var value))
                {
                    result[kpiName] = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
                }
            }
        }

        return result;
    }

    private static JsonElement? Prop(JsonElement? element, string name)
    {
        if (element is { ValueKind: JsonValueKind.Object } e)
        {
            foreach (var p in e.EnumerateObject())
            {
                if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return p.Value;
                }
            }
        }

        return null;
    }

    private static string? Str(JsonElement? element, string name) =>
        Prop(element, name) is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;

    private static string NormalizeStatus(string? value) =>
        new((value ?? string.Empty).Where(char.IsLetter).Select(char.ToLowerInvariant).ToArray());

    private static string KqlLiteral(string value) => $"@'{value.Replace("'", "''")}'";

}
