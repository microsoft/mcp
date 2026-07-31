// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IAdvisorService
{
    private static readonly Dictionary<string, int> ImpactRank = new(StringComparer.OrdinalIgnoreCase)
    {
        ["High"] = 0,
        ["Medium"] = 1,
        ["Low"] = 2,
    };

    internal const string GroupByRecommendationType = "recommendation-type";
    internal const string GroupByCategory = "category";
    internal const string GroupByImpact = "impact";
    internal const string GroupByResourceType = "resource-type";

    internal static readonly IReadOnlyList<string> AllowedGroupBy =
    [
        GroupByRecommendationType,
        GroupByCategory,
        GroupByImpact,
        GroupByResourceType,
    ];

    private readonly ISubscriptionService _advisorSubscriptionService = subscriptionService;

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        RecommendationFilters? filters = null,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var additionalFilter = BuildAdditionalFilter(filters);

        return await ExecuteResourceQueryAsync(
            "Microsoft.Advisor/recommendations",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToAdvisorRecommendationModel,
            tableName: "advisorresources",
            additionalFilter: additionalFilter,
            limit: top,
            tenant: tenant,
            cancellationToken: cancellationToken);
    }

    public async Task<ResourceQueryResults<RecommendationMetadata>> ListRecommendationMetadataAsync(
        string language,
        string? resourceType,
        string? impact,
        string? category,
        string? tenant,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var tenantResource = await GetTenantResourceAsync(tenant, cancellationToken);
        var queryContent = new ResourceQueryContent(
            BuildMetadataListQuery(language, resourceType, impact, category));

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        if (result == null || result.Count == 0)
        {
            return new([], false);
        }

        var results = new List<RecommendationMetadata>();
        using var jsonDocument = JsonDocument.Parse(result.Data);
        if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid recommendation metadata payload.");
        }

        foreach (var item in jsonDocument.RootElement.EnumerateArray())
        {
            results.Add(ConvertToRecommendationMetadataModel(item));
        }

        return new(
            [.. results
            .OrderBy(r => ImpactRank.TryGetValue(r.Impact ?? string.Empty, out var rank) ? rank : int.MaxValue)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)],
            result.ResultTruncated == ResultTruncated.True);
    }

    internal static string BuildMetadataListQuery(
        string language,
        string? resourceType,
        string? impact,
        string? category)
    {
        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/metadata' " +
            $"| where tostring(properties.language) =~ '{EscapeKqlString(language.Trim())}'";

        if (!string.IsNullOrWhiteSpace(resourceType))
        {
            query += $" | where tostring(properties.supportedResourceType) =~ '{EscapeKqlString(resourceType.Trim())}'";
        }

        if (!string.IsNullOrWhiteSpace(impact))
        {
            query += $" | where tostring(properties.recommendationImpact) =~ '{EscapeKqlString(impact.Trim())}'";
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query += $" | where tostring(properties.recommendationCategory) =~ '{EscapeKqlString(category.Trim())}'";
        }

        return query + " | project properties";
    }

    internal static RecommendationMetadata ConvertToRecommendationMetadataModel(JsonElement item)
    {
        var data = Models.RecommendationMetadataData.FromJson(item)
            ?? throw new JsonException("Failed to parse Advisor recommendation metadata data.");

        var properties = data.Properties
            ?? throw new JsonException("Recommendation metadata record is missing its properties payload.");
        if (string.IsNullOrWhiteSpace(properties.RecommendationTypeId))
        {
            throw new JsonException("Recommendation metadata record is missing recommendationTypeId.");
        }

        RecommendationServiceRetirement? serviceRetirement = null;
        var retirement = properties.SourceProperties?.ServiceRetirement;
        if (retirement is not null)
        {
            serviceRetirement = new RecommendationServiceRetirement(
                RetirementDate: retirement.RetirementDate,
                RetirementFeatureName: retirement.RetirementFeatureName,
                TrackingIds: retirement.ServiceHealth?.TrackingIds,
                AshUrls: retirement.ServiceHealth?.AshUrls);
        }

        IReadOnlyList<RecommendationMetadataAction>? actions = null;
        if (properties.Actions is { Count: > 0 } actionList)
        {
            actions = actionList
                .Select(ConvertMetadataAction)
                .ToList();
        }

        return new RecommendationMetadata(
            RecommendationTypeId: properties.RecommendationTypeId,
            DisplayName: properties.DisplayName,
            Label: properties.Label,
            Category: properties.RecommendationCategory,
            SubCategory: properties.RecommendationSubCategory,
            Impact: properties.RecommendationImpact,
            PriorityScore: properties.PriorityScore,
            PotentialBenefits: properties.PotentialBenefits,
            DetailedDescription: properties.DetailedDescription,
            LearnMoreLink: properties.LearnMoreLink,
            SupportedResourceType: properties.SupportedResourceType,
            Scope: properties.RecommendationScope,
            DataSourceQuery: properties.RecommendationDataSourceQuery,
            ResourceSingularName: properties.ResourceMetadata?.Singular,
            ResourcePluralName: properties.ResourceMetadata?.Plural,
            Actions: actions,
            Language: properties.Language,
            LastRefreshed: properties.LastRefreshed,
            ServiceRetirement: serviceRetirement);
    }

    private static RecommendationMetadataAction ConvertMetadataAction(
        Models.RecommendationMetadataActionData action) =>
        new(
            ActionType: action.ActionType,
            Caption: action.Caption,
            DocumentLink: action.DocumentLink,
            BladeName: action.BladeName);

    private async Task<TenantResource> GetTenantResourceAsync(
        string? tenant,
        CancellationToken cancellationToken)
    {
        var tenants = await TenantService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        if (string.IsNullOrWhiteSpace(tenant))
        {
            return tenants[0];
        }

        var tenantId = await ResolveTenantIdAsync(tenant, cancellationToken);
        return tenants.FirstOrDefault(candidate =>
            candidate.Data.TenantId?.ToString().Equals(tenantId, StringComparison.OrdinalIgnoreCase) == true)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
    }

    public async Task<RecommendationSummary> SummarizeRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var subscriptionResource = await _advisorSubscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var allTenants = await TenantService.GetTenants(cancellationToken);
        var tenantResource = allTenants.FirstOrDefault(t => t.Data.TenantId == subscriptionResource.Data.TenantId)
            ?? throw new InvalidOperationException($"No accessible tenant found for subscription '{subscription}'");

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgExists = await subscriptionResource!.GetResourceGroups().ExistsAsync(resourceGroup, cancellationToken);
            if (!rgExists.Value)
            {
                throw new KeyNotFoundException(
                    $"Resource group '{resourceGroup}' does not exist in subscription '{subscriptionResource.Data.SubscriptionId}'");
            }
        }

        var query = BuildSummarizeQuery(groupBy, resourceGroup, filters);
        var queryContent = new ResourceQueryContent(query)
        {
            Subscriptions = { subscriptionResource!.Data.SubscriptionId }
        };

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);

        var allGroups = new List<RecommendationGroup>();
        if (result?.Count > 0)
        {
            using var jsonDocument = JsonDocument.Parse(result.Data);
            var dataArray = jsonDocument.RootElement;
            if (dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    var key = item.TryGetProperty("key", out var keyProp) && keyProp.ValueKind == JsonValueKind.String
                        ? keyProp.GetString() ?? "Unknown"
                        : "Unknown";
                    var count = item.TryGetProperty("count_", out var countProp) ? countProp.GetInt64() : 0;
                    allGroups.Add(new RecommendationGroup(key, (int)count));
                }
            }
        }

        var totalRecommendations = allGroups.Sum(g => g.Count);

        return new(groupBy, totalRecommendations, allGroups);
    }

    internal static string BuildSummarizeQuery(string groupBy, string? resourceGroup, RecommendationFilters? filters)
    {
        var query = "advisorresources | where type =~ 'Microsoft.Advisor/recommendations'";

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            query += $" and resourceGroup =~ '{EscapeKqlString(resourceGroup)}'";
        }

        var additionalFilter = BuildAdditionalFilter(filters);
        if (!string.IsNullOrEmpty(additionalFilter))
        {
            query += $" and {additionalFilter}";
        }

        var summarizeField = MapGroupByToKqlField(groupBy);
        query += $" | summarize count() by key={summarizeField}";
        // Push 'Unknown' to the end regardless of count so real categories are surfaced first.
        query += " | order by iff(key == 'Unknown', 1, 0) asc, count_ desc, key asc";

        return query;
    }

    internal static string MapGroupByToKqlField(string groupBy) => groupBy.ToLowerInvariant() switch
    {
        GroupByCategory =>
            "iff(isempty(tostring(properties.category)), 'Unknown', tostring(properties.category))",
        GroupByImpact =>
            "iff(isempty(tostring(properties.impact)), 'Unknown', tostring(properties.impact))",
        GroupByRecommendationType =>
            "iff(isempty(tostring(properties.shortDescription.problem)), 'Unknown', tostring(properties.shortDescription.problem))",
        GroupByResourceType =>
            "iff(isempty(extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId))), 'Unknown', " +
            "extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId)))",
        _ => throw new ArgumentException(
            $"Unsupported group-by value '{groupBy}'. Allowed values: {string.Join(", ", AllowedGroupBy)}.",
            nameof(groupBy)),
    };

    internal static string? BuildAdditionalFilter(RecommendationFilters? filters)
    {
        // Advisor surfaces recommendations in several lifecycle states (e.g. 'New', 'Dismissed', 'Postponed').
        // Only 'New' recommendations are active and actionable, so we always constrain results to these and
        // never expose dismissed or postponed noise in lists or summaries.
        var clauses = new List<string> { ActiveRecommendationClause };

        if (filters is not null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Category))
            {
                clauses.Add($"tostring(properties.category) =~ '{SanitizeForKql(filters.Category)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Impact))
            {
                clauses.Add($"tostring(properties.impact) =~ '{SanitizeForKql(filters.Impact)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.ResourceType))
            {
                clauses.Add($"tostring(properties.resourceMetadata.resourceId) contains '{SanitizeForKql(filters.ResourceType)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Resource))
            {
                clauses.Add($"tostring(properties.resourceMetadata.resourceId) contains '{SanitizeForKql(filters.Resource)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                clauses.Add($"tostring(properties.shortDescription.problem) contains '{SanitizeForKql(filters.Search)}'");
            }
        }

        return string.Join(" and ", clauses);
    }

    // KQL clause that restricts results to active ('New') recommendations only.
    internal const string ActiveRecommendationClause = "tostring(properties.recommendationStatus) =~ 'New'";

    private static string SanitizeForKql(string value) => EscapeKqlString(value.Replace("|", string.Empty));

    internal static Recommendation ConvertToAdvisorRecommendationModel(JsonElement item)
    {
        var advisorRecommendation = Models.RecommendationData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation data");

        var resourceId = advisorRecommendation.Properties?.ResourceMetadata?.ResourceId ?? "Unknown";

        return new(
            ResourceId: resourceId,
            RecommendationText: advisorRecommendation.Properties?.ShortDescription?.Problem ?? "Unknown",
            Category: advisorRecommendation.Properties?.Category ?? "Unknown",
            Impact: advisorRecommendation.Properties?.Impact,
            ImpactedResourceType: ParseImpactedResourceType(resourceId));
    }

    internal static string? ParseImpactedResourceType(string? resourceId)
    {
        if (string.IsNullOrEmpty(resourceId))
        {
            return null;
        }

        var segments = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? ns = null;
        var typeParts = new List<string>();

        for (var i = 0; i < segments.Length; i++)
        {
            if (!string.Equals(segments[i], "providers", StringComparison.OrdinalIgnoreCase) || i + 2 >= segments.Length)
            {
                continue;
            }

            ns = segments[i + 1];
            typeParts.Clear();
            typeParts.Add(segments[i + 2]);

            for (var j = i + 4; j < segments.Length; j += 2)
            {
                typeParts.Add(segments[j]);
            }

            break;
        }

        return ns is null ? null : $"{ns}/{string.Join('/', typeParts)}";
    }
}
