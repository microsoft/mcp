// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAdvisorService
{
    private const string RetirementDateProperty =
        "properties.sourceProperties.serviceRetirement.retirementDate";
    private const string TrackingIdsProperty =
        "properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds";
    private const int MetadataPageSize = 1000;

    // Recommendation instances are not localized per request, so the metadata join always uses the
    // invariant English metadata to keep the enriched fields deterministic.
    internal const string MetadataJoinLanguage = "en";

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

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        RecommendationFilters? filters = null,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, RecommendationMetadata>? metadataByTypeId =
            await ResolveMetadataFilterMatchesAsync(filters, cancellationToken);

        if (metadataByTypeId is { Count: 0 })
        {
            return new([], false);
        }

        var additionalFilter = BuildAdditionalFilter(filters, metadataByTypeId?.Keys);

        var recommendations = await ExecuteResourceQueryAsync(
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

        if (recommendations.Results.Count == 0 || IsDirectSecurityQuery(filters))
        {
            return recommendations;
        }

        // Enrich non-Security recommendations with matching type-level metadata before returning them.
        metadataByTypeId ??= BuildMetadataLookup(
            await GetRecommendationMetadataByTypeIdsAsync(
                recommendations.Results.Select(r => r.Properties.RecommendationTypeId),
                MetadataJoinLanguage,
                cancellationToken));

        return new(
            JoinWithMetadata(recommendations.Results, metadataByTypeId),
            recommendations.AreResultsTruncated);
    }

    internal static bool HasMetadataOnlyFilters(RecommendationFilters? filters) =>
        !string.IsNullOrWhiteSpace(filters?.SubCategory) ||
        filters?.TrackingIds?.Any(id => !string.IsNullOrWhiteSpace(id)) == true ||
        filters?.RetirementDate is not null ||
        !string.IsNullOrWhiteSpace(filters?.RetirementDateOperator);

    internal static bool HasMetadataFilters(RecommendationFilters? filters) =>
        !IsSecurityCategory(filters?.Category) &&
        (HasMetadataOnlyFilters(filters) ||
            (!string.IsNullOrWhiteSpace(filters?.Category) ||
            !string.IsNullOrWhiteSpace(filters?.Impact) ||
            !string.IsNullOrWhiteSpace(filters?.ResourceType)));

    internal static bool IsDirectSecurityQuery(RecommendationFilters? filters) =>
        IsSecurityCategory(filters?.Category);

    private static bool IsSecurityCategory(string? category) =>
        string.Equals(category?.Trim(), "Security", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves metadata-backed filters against metadata first and returns matching recommendation type IDs.
    /// Resource and search filters remain predicates on recommendation instances.
    /// </summary>
    private async Task<Dictionary<string, RecommendationMetadata>?> ResolveMetadataFilterMatchesAsync(
        RecommendationFilters? filters,
        CancellationToken cancellationToken)
    {
        if (!HasMetadataFilters(filters))
        {
            return null;
        }

        var matchingMetadata = await ListAllRecommendationMetadataAsync(
            MetadataJoinLanguage,
            new RecommendationMetadataFilters(
                ResourceType: filters!.ResourceType,
                Impact: filters.Impact,
                Category: filters.Category,
                SubCategory: filters!.SubCategory,
                TrackingIds: filters.TrackingIds,
                RetirementDateOperator: filters.RetirementDateOperator,
                RetirementDate: filters.RetirementDate),
            cancellationToken);

        return BuildMetadataLookup(matchingMetadata);
    }

    internal static Dictionary<string, RecommendationMetadata> BuildMetadataLookup(
        IEnumerable<RecommendationMetadata> metadata)
    {
        var lookup = new Dictionary<string, RecommendationMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in metadata)
        {
            if (!string.IsNullOrWhiteSpace(entry.RecommendationTypeId))
            {
                lookup[entry.RecommendationTypeId] = entry;
            }
        }

        return lookup;
    }

    internal static List<Recommendation> JoinWithMetadata(
        IEnumerable<Recommendation> recommendations,
        IReadOnlyDictionary<string, RecommendationMetadata> metadataByTypeId)
    {
        var joined = new List<Recommendation>();

        foreach (var recommendation in recommendations)
        {
            if (string.IsNullOrWhiteSpace(recommendation.Properties.RecommendationTypeId) ||
                !metadataByTypeId.TryGetValue(recommendation.Properties.RecommendationTypeId, out var metadata))
            {
                joined.Add(recommendation);
                continue;
            }

            joined.Add(recommendation with
            {
                Properties = recommendation.Properties with
                {
                    Category = metadata.Category,
                    Impact = metadata.Impact,
                    ShortDescription = new RecommendationShortDescription(
                        metadata.DisplayName ?? recommendation.Properties.ShortDescription?.Problem,
                        metadata.DisplayName ?? recommendation.Properties.ShortDescription?.Solution),
                    Description = metadata.DetailedDescription ?? recommendation.Properties.Description,
                    Label = metadata.Label ?? recommendation.Properties.Label,
                    LearnMoreLink = metadata.LearnMoreLink ?? recommendation.Properties.LearnMoreLink,
                    PotentialBenefits = metadata.PotentialBenefits ?? recommendation.Properties.PotentialBenefits,
                    ExtendedProperties = AddMetadataSubCategory(
                        AddMetadataRetirementProperties(
                            recommendation.Properties.ExtendedProperties,
                            metadata.ServiceRetirement),
                        metadata.SubCategory),
                    ResourceMetadata = recommendation.Properties.ResourceMetadata,
                },
            });
        }

        return joined;
    }

    private async Task<List<RecommendationMetadata>> GetRecommendationMetadataByTypeIdsAsync(
        IEnumerable<string?> recommendationTypeIds,
        string language,
        CancellationToken cancellationToken)
    {
        var distinctIds = recommendationTypeIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctIds.Count == 0)
        {
            return [];
        }

        return await ExecuteMetadataQueryAsync(
            BuildMetadataByTypeIdsQuery(distinctIds, language),
            cancellationToken);
    }

    internal static string BuildMetadataByTypeIdsQuery(
        IReadOnlyCollection<string> recommendationTypeIds,
        string language) =>
        "advisorresources " +
        "| where type =~ 'microsoft.advisor/metadata' " +
        $"| where tostring(properties.language) =~ '{EscapeKqlString(language.Trim())}' " +
        $"| where tostring(properties.recommendationTypeId) in~ ({FormatKqlStringList(recommendationTypeIds)}) " +
        "| project properties";

    private static string FormatKqlStringList(IEnumerable<string> values) =>
        string.Join(", ", values.Select(value => $"'{SanitizeForKql(value)}'"));

    private static List<string> NormalizeFilterValues(IEnumerable<string>? values) =>
        values is null
            ? []
            : [.. values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)];

    public async Task<ResourceQueryResults<RecommendationMetadata>> ListRecommendationMetadataAsync(
        string language,
        RecommendationMetadataFilters? filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var query = BuildMetadataListQuery(language, filters);
        var tenantResource = await GetTenantResourceAsync(cancellationToken);
        var result = await ExecuteMetadataPageAsync(tenantResource, query, null, cancellationToken);

        return new(
            SortMetadata(result.Metadata),
            result.IsTruncated || !string.IsNullOrEmpty(result.SkipToken));
    }

    private async Task<List<RecommendationMetadata>> ListAllRecommendationMetadataAsync(
        string language,
        RecommendationMetadataFilters? filters,
        CancellationToken cancellationToken)
    {
        var query = BuildMetadataListQuery(language, filters);
        var tenantResource = await GetTenantResourceAsync(cancellationToken);
        var results = new List<RecommendationMetadata>();
        results.AddRange(await CollectMetadataPagesAsync(
            (skipToken, token) => ExecuteMetadataPageAsync(
                tenantResource,
                query,
                skipToken,
                token),
            cancellationToken));

        return SortMetadata(BuildMetadataLookup(results).Values);
    }

    internal static async Task<List<RecommendationMetadata>> CollectMetadataPagesAsync(
        Func<string?, CancellationToken, Task<(List<RecommendationMetadata> Metadata, string? SkipToken, bool IsTruncated)>> getPage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(getPage);

        var results = new List<RecommendationMetadata>();
        var seenSkipTokens = new HashSet<string>(StringComparer.Ordinal);
        string? skipToken = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await getPage(skipToken, cancellationToken);
            results.AddRange(page.Metadata);
            skipToken = page.SkipToken;

            if (page.IsTruncated && string.IsNullOrEmpty(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph truncated Advisor metadata results without returning a continuation token.");
            }

            if (!string.IsNullOrEmpty(skipToken) && !seenSkipTokens.Add(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph returned a repeated continuation token while paging Advisor metadata.");
            }
        }
        while (!string.IsNullOrEmpty(skipToken));

        return results;
    }

    private static async Task<(List<RecommendationMetadata> Metadata, string? SkipToken, bool IsTruncated)> ExecuteMetadataPageAsync(
        TenantResource tenantResource,
        string query,
        string? skipToken,
        CancellationToken cancellationToken)
    {
        var queryContent = new ResourceQueryContent(query)
        {
            Options = new ResourceQueryRequestOptions
            {
                Top = MetadataPageSize,
                SkipToken = skipToken,
            },
        };

        var response = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        var result = response.Value;
        if (result == null || result.Count == 0)
        {
            return new([], result?.SkipToken, result?.ResultTruncated == ResultTruncated.True);
        }

        return new(
            ParseMetadata(result.Data),
            result.SkipToken,
            result.ResultTruncated == ResultTruncated.True);
    }

    private static List<RecommendationMetadata> ParseMetadata(BinaryData data)
    {
        var results = new List<RecommendationMetadata>();
        using var jsonDocument = JsonDocument.Parse(data);
        if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid recommendation metadata payload.");
        }

        foreach (var item in jsonDocument.RootElement.EnumerateArray())
        {
            results.Add(ConvertToRecommendationMetadataModel(item));
        }

        return results;
    }

    private static List<RecommendationMetadata> SortMetadata(IEnumerable<RecommendationMetadata> metadata) =>
        [.. metadata
            .OrderBy(r => ImpactRank.TryGetValue(r.Impact ?? string.Empty, out var rank) ? rank : int.MaxValue)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)];

    private async Task<List<RecommendationMetadata>> ExecuteMetadataQueryAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var tenantResource = await GetTenantResourceAsync(cancellationToken);

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(
            new ResourceQueryContent(query),
            cancellationToken);

        if (result == null || result.Count == 0)
        {
            return [];
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

        return results;
    }

    internal static string BuildMetadataListQuery(
        string language,
        RecommendationMetadataFilters? filters)
    {
        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/metadata' " +
            $"| where tostring(properties.language) =~ '{EscapeKqlString(language.Trim())}'";

        if (!string.IsNullOrWhiteSpace(filters?.ResourceType))
        {
            query += $" | where tostring(properties.supportedResourceType) =~ '{EscapeKqlString(filters.ResourceType.Trim())}'";
        }

        if (!string.IsNullOrWhiteSpace(filters?.Impact))
        {
            query += $" | where tostring(properties.recommendationImpact) =~ '{EscapeKqlString(filters.Impact.Trim())}'";
        }

        var category = string.IsNullOrWhiteSpace(filters?.Category)
            ? null
            : filters.Category.Trim();
        var subCategory = string.IsNullOrWhiteSpace(filters?.SubCategory)
            ? null
            : filters.SubCategory.Trim();
        var trackingIds = NormalizeFilterValues(filters?.TrackingIds);
        var retirementDate = filters?.RetirementDate;
        var retirementDateOperator = string.IsNullOrWhiteSpace(filters?.RetirementDateOperator)
            ? null
            : filters.RetirementDateOperator.Trim();

        if ((retirementDate is null) != (retirementDateOperator is null))
        {
            throw new ArgumentException(
                "RetirementDate and RetirementDateOperator must be provided together.",
                nameof(filters));
        }

        var hasTrackingIdFilter = trackingIds.Count > 0;
        var hasRetirementDateFilter = retirementDate is not null && retirementDateOperator is not null;
        var hasServiceRetirementFilter = hasTrackingIdFilter || hasRetirementDateFilter;
        subCategory = ResolveServiceRetirementSubCategory(
            subCategory,
            hasServiceRetirementFilter);

        if (category is not null)
        {
            query += $" | where tostring(properties.recommendationCategory) =~ '{EscapeKqlString(category)}'";
        }

        if (subCategory is not null)
        {
            query += $" | where tostring(properties.recommendationSubCategory) =~ '{EscapeKqlString(subCategory)}'";
        }

        if (hasTrackingIdFilter)
        {
            query += $" | mv-expand trackingId = {TrackingIdsProperty}";
            query += $" | where tostring(trackingId) in~ ({FormatKqlStringList(trackingIds)})";
        }

        if (retirementDate is { } date && retirementDateOperator is not null)
        {
            query += $" | where isnotempty(tostring({RetirementDateProperty}))";
            query += $" | where startofday(todatetime({RetirementDateProperty})) " +
                $"{GetKqlComparisonOperator(retirementDateOperator)} " +
                $"datetime({date:yyyy-MM-dd})";
        }

        return query +
            " | project id, recommendationTypeId = tostring(properties.recommendationTypeId), properties" +
            " | order by id asc, recommendationTypeId asc";
    }

    private static string? ResolveServiceRetirementSubCategory(
        string? subCategory,
        bool hasServiceRetirementFilter)
    {
        var isServiceUpgradeAndRetirement = subCategory?.Equals(
            RecommendationMetadataFilters.ServiceRetirementSubCategory,
            StringComparison.OrdinalIgnoreCase) == true;

        if (hasServiceRetirementFilter && subCategory is not null && !isServiceUpgradeAndRetirement)
        {
            throw new ArgumentException(
                "When a subcategory is specified with tracking ID or retirement-date filters, it must be " +
                $"{RecommendationMetadataFilters.ServiceRetirementSubCategory}.",
                nameof(subCategory));
        }

        if (!hasServiceRetirementFilter && !isServiceUpgradeAndRetirement)
        {
            return subCategory;
        }

        return subCategory ?? RecommendationMetadataFilters.ServiceRetirementSubCategory;
    }

    private static string GetKqlComparisonOperator(string comparisonOperator) =>
        comparisonOperator.ToLowerInvariant() switch
        {
            "eq" => "==",
            "lt" => "<",
            "le" => "<=",
            "gt" => ">",
            "ge" => ">=",
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparisonOperator),
                comparisonOperator,
                "Unsupported retirement-date comparison operator.")
        };

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

    private async Task<TenantResource> GetTenantResourceAsync(CancellationToken cancellationToken)
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        return tenants[0];
    }

    public async Task<RecommendationMetadata?> GetRecommendationMetadataAsync(
        string recommendationTypeId,
        string language,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recommendationTypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var tenantResource = await GetTenantResourceAsync(cancellationToken);

        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/metadata' " +
            $"and tostring(properties.recommendationTypeId) =~ '{EscapeKqlString(recommendationTypeId)}' " +
            $"and tostring(properties.language) =~ '{EscapeKqlString(language)}' " +
            "| limit 1";

        var queryContent = new ResourceQueryContent(query);

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        if (result == null || result.Count == 0)
        {
            return null;
        }

        using var jsonDocument = JsonDocument.Parse(result.Data);
        var dataArray = jsonDocument.RootElement;
        if (dataArray.ValueKind != JsonValueKind.Array || dataArray.GetArrayLength() == 0)
        {
            return null;
        }

        return ConvertToRecommendationMetadataModel(dataArray[0]);
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

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var allTenants = await AzureService.GetTenants(cancellationToken);
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

    internal static string? BuildAdditionalFilter(
        RecommendationFilters? filters,
        IEnumerable<string>? recommendationTypeIds = null)
    {
        // Advisor surfaces recommendations in several lifecycle states (e.g. 'New', 'Dismissed', 'Postponed').
        // Only 'New' recommendations are active and actionable, so we always constrain results to these and
        // never expose dismissed or postponed noise in lists or summaries.
        var clauses = new List<string> { ActiveRecommendationClause };

        if (filters is not null)
        {
            var resolvedTypeIds = recommendationTypeIds?.ToList();
            var metadataWasResolved = resolvedTypeIds is not null;

            if (!metadataWasResolved && !string.IsNullOrWhiteSpace(filters.Category))
            {
                clauses.Add($"tostring(properties.category) =~ '{SanitizeForKql(filters.Category)}'");
            }

            if (!metadataWasResolved && !string.IsNullOrWhiteSpace(filters.Impact))
            {
                clauses.Add($"tostring(properties.impact) =~ '{SanitizeForKql(filters.Impact)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.RecommendationTypeId))
            {
                clauses.Add($"tostring(properties.recommendationTypeId) =~ '{SanitizeForKql(filters.RecommendationTypeId)}'");
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

        // Metadata-backed filters arrive here as recommendation type IDs. Resource and search filters remain
        // predicates on recommendation-instance properties.
        var typeIds = recommendationTypeIds?.ToList();
        if (typeIds is { Count: > 0 })
        {
            clauses.Add($"tostring(properties.recommendationTypeId) in~ ({FormatKqlStringList(typeIds)})");
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

        return new(
            Properties: new RecommendationProperties(
                Category: advisorRecommendation.Properties?.Category,
                Impact: advisorRecommendation.Properties?.Impact,
                ImpactedField: advisorRecommendation.Properties?.ImpactedField,
                ImpactedValue: advisorRecommendation.Properties?.ImpactedValue,
                RecommendationStatus: advisorRecommendation.Properties?.RecommendationStatus,
                LastRefreshed: advisorRecommendation.Properties?.LastRefreshed,
                LastUpdated: advisorRecommendation.Properties?.LastUpdated,
                CreatedTime: advisorRecommendation.Properties?.CreatedTime,
                RecommendationTypeId: advisorRecommendation.Properties?.RecommendationTypeId,
                ShortDescription: advisorRecommendation.Properties?.ShortDescription is { } shortDescription
                    ? new RecommendationShortDescription(shortDescription.Problem, shortDescription.Solution)
                    : null,
                Metadata: advisorRecommendation.Properties?.Metadata,
                ExtendedProperties: advisorRecommendation.Properties?.ExtendedProperties,
                ResourceMetadata: advisorRecommendation.Properties?.ResourceMetadata is { } resourceMetadata
                    ? new RecommendationResourceMetadata(resourceMetadata.ResourceId)
                    : null,
                Risk: advisorRecommendation.Properties?.Risk,
                Description: advisorRecommendation.Properties?.Description,
                Label: advisorRecommendation.Properties?.Label,
                LearnMoreLink: advisorRecommendation.Properties?.LearnMoreLink,
                PotentialBenefits: advisorRecommendation.Properties?.PotentialBenefits,
                Actions: advisorRecommendation.Properties?.Actions,
                Remediation: advisorRecommendation.Properties?.Remediation,
                ExposedMetadataProperties: advisorRecommendation.Properties?.ExposedMetadataProperties,
                TrackedProperties: advisorRecommendation.Properties?.TrackedProperties,
                Review: advisorRecommendation.Properties?.Review,
                ResourceWorkload: advisorRecommendation.Properties?.ResourceWorkload,
                SourceSystem: advisorRecommendation.Properties?.SourceSystem,
                Notes: advisorRecommendation.Properties?.Notes),
            Id: advisorRecommendation.ResourceId,
            Type: advisorRecommendation.ResourceType,
            Name: advisorRecommendation.ResourceName,
            HardwareDetails: advisorRecommendation.HardwareDetails);
    }

    private static IReadOnlyDictionary<string, JsonElement>? AddMetadataSubCategory(
        IReadOnlyDictionary<string, JsonElement>? properties,
        string? subCategory)
    {
        if (string.IsNullOrWhiteSpace(subCategory))
        {
            return properties;
        }

        var result = properties is null
            ? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, JsonElement>(properties, StringComparer.OrdinalIgnoreCase);
        result["recommendationSubCategory"] = JsonSerializer.SerializeToElement(
            subCategory,
            Commands.AdvisorJsonContext.Default.String);
        return result;
    }

    private static IReadOnlyDictionary<string, JsonElement>? AddMetadataRetirementProperties(
        IReadOnlyDictionary<string, JsonElement>? properties,
        RecommendationServiceRetirement? serviceRetirement)
    {
        if (serviceRetirement is null)
        {
            return properties;
        }

        var result = properties is null
            ? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, JsonElement>(properties, StringComparer.OrdinalIgnoreCase);

        AddStringProperty(result, "retirementDate", serviceRetirement.RetirementDate);
        AddStringProperty(result, "retirementFeatureName", serviceRetirement.RetirementFeatureName);
        return result;
    }

    private static void AddStringProperty(
        IDictionary<string, JsonElement> properties,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            properties[name] = JsonSerializer.SerializeToElement(
                value,
                Commands.AdvisorJsonContext.Default.String);
        }
    }

    private static string? GetExtendedPropertyString(
        IReadOnlyDictionary<string, JsonElement>? extendedProperties,
        string propertyName) =>
        extendedProperties is not null &&
        extendedProperties.TryGetValue(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

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
