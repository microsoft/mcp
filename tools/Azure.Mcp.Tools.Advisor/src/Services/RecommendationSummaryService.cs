// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Validation;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

public class RecommendationSummaryService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IRecommendationSummaryService
{
    private readonly AdvisorResourceGraphQueryExecutor _queryExecutor = new(azureService);

    internal const string GroupByRecommendationType = "recommendation-type";
    internal const string GroupByCategory = "category";
    internal const string GroupByImpact = "impact";
    internal const string GroupByResourceType = "resource-type";
    internal const string GroupByStatus = "status";
    internal const string GroupBySubCategory = "sub-category";
    internal const string GroupByRetirementDate = "retirement-date";

    internal static readonly IReadOnlyList<string> AllowedGroupBy =
    [
        GroupByRecommendationType,
        GroupByCategory,
        GroupByImpact,
        GroupByResourceType,
        GroupByStatus,
        GroupBySubCategory,
        GroupByRetirementDate,
    ];

    public async Task<RecommendationSummary> SummarizeRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        var normalizedGroupBy = NormalizeGroupBy(groupBy);
        var (scope, tenantResource) = await _queryExecutor.ResolveSubscriptionScopeAsync(
            subscription,
            resourceGroup,
            tenant,
            cancellationToken);

        return await SummarizeRecommendationsCoreAsync(
            scope,
            tenantResource,
            normalizedGroupBy,
            filters,
            cancellationToken);
    }

    public async Task<RecommendationSummary> SummarizeServiceGroupRecommendationsAsync(
        string serviceGroup,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedGroupBy = NormalizeGroupBy(groupBy);
        var (scope, tenantResource) = await _queryExecutor.ResolveServiceGroupScopeAsync(
            serviceGroup,
            tenant,
            cancellationToken);

        return await SummarizeRecommendationsCoreAsync(
            scope,
            tenantResource,
            normalizedGroupBy,
            filters,
            cancellationToken);
    }

    private static string NormalizeGroupBy(string groupBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);
        var normalizedGroupBy = groupBy.Trim().ToLowerInvariant();
        if (!AllowedGroupBy.Contains(normalizedGroupBy, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported group-by value '{groupBy}'. Allowed values: {string.Join(", ", AllowedGroupBy)}.",
                nameof(groupBy));
        }

        return normalizedGroupBy;
    }

    private static async Task<RecommendationSummary> SummarizeRecommendationsCoreAsync(
        RecommendationQueryScope scope,
        TenantResource tenantResource,
        string normalizedGroupBy,
        RecommendationFilters? filters,
        CancellationToken cancellationToken)
    {
        var prioritized = IsPrioritized(filters);
        var usesMetadata = prioritized || RequiresMetadata(normalizedGroupBy, filters);
        var query = BuildSummaryQuery(
            scope,
            normalizedGroupBy,
            filters,
            usesMetadata);

        var result = await AdvisorResourceGraphQueryExecutor.ExecuteAsync(
            tenantResource,
            scope,
            query,
            useSubscriptionRequestScope:
                !usesMetadata && scope.Kind == RecommendationQueryScopeKind.Subscription,
            cancellationToken);
        EnsureCompleteResult(
            result.ResultTruncated == ResultTruncated.True,
            result.SkipToken);

        if (result.Count == 0)
        {
            return new(normalizedGroupBy, 0, []);
        }

        using var document = JsonDocument.Parse(result.Data);
        return prioritized
            ? ParsePrioritizedSummary(document.RootElement)
            : ParseSummary(normalizedGroupBy, document.RootElement);
    }

    internal static bool IsPrioritized(RecommendationFilters? filters) =>
        filters?.Prioritized == true;

    internal static bool RequiresMetadata(string groupBy, RecommendationFilters? filters) =>
        groupBy is GroupByRecommendationType
            or GroupByCategory
            or GroupByImpact
            or GroupBySubCategory
            or GroupByRetirementDate ||
        !string.IsNullOrWhiteSpace(filters?.Category) ||
        !string.IsNullOrWhiteSpace(filters?.Impact) ||
        !string.IsNullOrWhiteSpace(filters?.SubCategory) ||
        filters?.RetirementDate is not null;

    internal static string BuildSummaryQuery(
        string subscriptionId,
        string? resourceGroup,
        string groupBy,
        RecommendationFilters? filters,
        bool? useMetadata = null)
        => BuildSummaryQuery(
            RecommendationQueryScope.ForSubscription(subscriptionId, resourceGroup),
            groupBy,
            filters,
            useMetadata);

    internal static string BuildServiceGroupSummaryQuery(
        string serviceGroup,
        string groupBy,
        RecommendationFilters? filters,
        bool? useMetadata = null)
        => BuildSummaryQuery(
            RecommendationQueryScope.ForServiceGroup(serviceGroup),
            groupBy,
            filters,
            useMetadata);

    internal static string BuildSummaryQuery(
        RecommendationQueryScope scope,
        string groupBy,
        RecommendationFilters? filters,
        bool? useMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var normalizedGroupBy = NormalizeGroupBy(groupBy);

        ValidateFilters(filters, normalizedGroupBy);

        if (IsPrioritized(filters))
        {
            if (normalizedGroupBy != GroupByRecommendationType)
            {
                throw new ArgumentException(
                    $"Prioritized summaries are grouped by '{GroupByRecommendationType}' only.",
                    nameof(groupBy));
            }

            return PrioritizedRecommendationSummaryQuery.Build(scope, filters);
        }

        return useMetadata ?? RequiresMetadata(normalizedGroupBy, filters)
            ? BuildMetadataSummaryQuery(scope, normalizedGroupBy, filters)
            : BuildInstanceSummaryQuery(scope, normalizedGroupBy, filters);
    }

    internal static void EnsureCompleteResult(bool isTruncated, string? skipToken)
    {
        if (isTruncated || !string.IsNullOrWhiteSpace(skipToken))
        {
            throw new InvalidOperationException(
                "Azure Resource Graph returned more than 1,000 summary buckets. Narrow the filters and retry.");
        }
    }

    internal static RecommendationSummary ParseSummary(string groupBy, JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid recommendation summary payload.");
        }

        var groupsByKey = new Dictionary<string, RecommendationGroup>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.EnumerateArray())
        {
            var key = ReadRequiredString(item, "key");
            var label = ReadRequiredString(item, "label");
            var count = ReadCount(item);
            if (key.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                key = "Unknown";
                label = "Unknown";
            }

            if (groupsByKey.TryGetValue(key, out var existing))
            {
                var preferredLabel = existing.Label.Equals(existing.Key, StringComparison.OrdinalIgnoreCase) &&
                    !label.Equals(key, StringComparison.OrdinalIgnoreCase)
                        ? label
                        : existing.Label;
                groupsByKey[key] = new(
                    existing.Key,
                    preferredLabel,
                    checked(existing.Count + count));
            }
            else
            {
                groupsByKey.Add(key, new(key, label, count));
            }
        }

        var groups = SortGroups(groupBy, groupsByKey.Values);
        var total = groups.Aggregate(
            0,
            static (current, group) => checked(current + group.Count));
        return new(groupBy, total, groups);
    }

    /// <summary>
    /// Reads prioritized summary rows. The rows arrive already ranked by
    /// <see cref="PrioritizedRecommendationSummaryQuery"/>, and that order is kept unchanged.
    /// </summary>
    /// <remarks>
    /// Each recommendation type occurs once because the query aggregates per type and the English metadata has one row
    /// per type. If a key were ever repeated, the first, higher-ranked row would be kept and later rows ignored, so a
    /// type is never counted twice.
    /// </remarks>
    internal static RecommendationSummary ParsePrioritizedSummary(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid recommendation summary payload.");
        }

        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var groups = new List<RecommendationGroup>();
        var total = 0;
        foreach (var item in data.EnumerateArray())
        {
            var key = ReadRequiredString(item, "key");
            if (!seenKeys.Add(key))
            {
                continue;
            }

            var count = ReadCount(item);
            total = checked(total + count);
            groups.Add(new(
                key,
                ReadRequiredString(item, "label"),
                count,
                ReadPrioritizationSignals(item)));
        }

        return new(GroupByRecommendationType, total, groups);
    }

    /// <summary>
    /// Combines the type's metadata fields, annual savings, and filtered published signals into one object. Absent
    /// values are left unset so they are omitted from the response.
    /// </summary>
    private static RecommendationPrioritizationSignals ReadPrioritizationSignals(JsonElement item)
    {
        var signals = item.TryGetProperty("signalValues", out var signalValues)
            ? PrioritizationSignalFilter.Filter(signalValues)
            : new();
        var annualSavings = ReadOptionalFiniteNumber(item, "annualSavings");

        return signals with
        {
            Category = ReadOptionalString(item, "category"),
            Impact = ReadOptionalString(item, "impact"),
            SubCategory = ReadOptionalString(item, "subCategory"),
            RetirementDate = ReadOptionalString(item, "retirementDate"),
            AnnualSavings = annualSavings,
            AnnualSavingsCurrency = annualSavings is null ? null : PrioritizedRecommendationSummaryQuery.SavingsCurrency,
        };
    }

    private static string? ReadOptionalString(JsonElement item, string propertyName) =>
        item.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.String &&
        !PrioritizationSignalFilter.IsAbsent(property.GetString())
            ? property.GetString()
            : null;

    private static double? ReadOptionalFiniteNumber(JsonElement item, string propertyName) =>
        item.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.Number &&
        property.TryGetDouble(out var value) &&
        double.IsFinite(value)
            ? value
            : null;

    private static int ReadCount(JsonElement item)
    {
        if (!item.TryGetProperty("count_", out var countProperty) ||
            countProperty.ValueKind != JsonValueKind.Number ||
            !countProperty.TryGetInt64(out var count64) ||
            count64 < 0)
        {
            throw new JsonException("Recommendation summary row has an invalid count.");
        }

        return checked((int)count64);
    }

    private static string BuildInstanceSummaryQuery(
        RecommendationQueryScope scope,
        string groupBy,
        RecommendationFilters? filters)
    {
        var query = BuildRecommendationScope(scope);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: groupBy != GroupByStatus,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: true,
            scope: scope);
        if (predicates.Length > 0)
        {
            query += $" | where {predicates}";
        }

        query += groupBy switch
        {
            GroupByStatus =>
                " | extend groupValue = tostring(properties.recommendationStatus)",
            GroupByResourceType =>
                " | extend impactedResourceType = tolower(tostring(properties.impactedField))" +
                " | extend resourceIdType = tolower(extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId)))" +
                " | extend groupValue = iff(isnotempty(impactedResourceType), impactedResourceType, resourceIdType)",
            _ => throw new ArgumentException(
                $"Group-by value '{groupBy}' requires recommendation metadata.",
                nameof(groupBy)),
        };

        return query +
            " | extend key = iff(isempty(groupValue), 'Unknown', groupValue)" +
            " | extend label = key" +
            " | summarize count() by key, label";
    }

    private static string BuildMetadataSummaryQuery(
        RecommendationQueryScope scope,
        string groupBy,
        RecommendationFilters? filters)
    {
        var query = BuildRecommendationScope(scope);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: groupBy != GroupByStatus,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true,
            scope: scope);
        if (predicates.Length > 0)
        {
            query += $" | where {predicates}";
        }

        query +=
            " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))" +
            " | project recommendationTypeId," +
            " impactedField=tostring(properties.impactedField)," +
            " resourceId=tostring(properties.resourceMetadata.resourceId)," +
            " recommendationStatus=tostring(properties.recommendationStatus)," +
            " problem=tostring(properties.shortDescription.problem)" +
            JoinRecommendationTypeMetadata +
            " | extend category = metadataCategory," +
            " impact = metadataImpact," +
            " subCategory = metadataSubCategory" +
            " | extend retirementDate = format_datetime(startofday(todatetime(metadataRetirementDate)), 'yyyy-MM-dd')" +
            " | extend impactedResourceType = tolower(impactedField)" +
            " | extend resourceIdType = tolower(extract(@'/providers/([^/]+/[^/]+)', 1, resourceId))" +
            " | extend resourceType = iff(isnotempty(impactedResourceType), impactedResourceType, resourceIdType)" +
            " | extend typeLabel = iff(isnotempty(metadataDisplayName), metadataDisplayName," +
            " iff(isnotempty(metadataLabel), metadataLabel, problem))";

        query = AddMetadataFilters(query, groupBy, filters);
        query += BuildMetadataGrouping(groupBy);

        // min(label) keeps one deterministic label per key when a type without metadata has differing problem texts.
        return query + " | summarize count_ = count(), label = min(label) by key";
    }

    /// <summary>
    /// Left outer join to English recommendation-type metadata, shared by normal and prioritized summaries.
    /// </summary>
    /// <remarks>
    /// Category, impact, subcategory, retirement date, priority score, and published signals are taken only from this
    /// metadata. Recommendation records are not used as a fallback: metadata is the catalog of record for type-level
    /// properties, and where the two disagree the metadata is authoritative. A type without a metadata row is kept and
    /// reports these properties as missing (normal summaries group it under <c>Unknown</c>).
    /// </remarks>
    internal const string JoinRecommendationTypeMetadata =
        " | join kind=leftouter (" +
        " advisorresources" +
        " | where type =~ 'microsoft.advisor/metadata'" +
        " | where tostring(properties.language) =~ 'en'" +
        " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))" +
        " | project recommendationTypeId," +
        " metadataCategory=tostring(properties.recommendationCategory)," +
        " metadataImpact=tostring(properties.recommendationImpact)," +
        " metadataSubCategory=tostring(properties.recommendationSubCategory)," +
        " metadataRetirementDate=tostring(properties.sourceProperties.serviceRetirement.retirementDate)," +
        " metadataDisplayName=tostring(properties.displayName)," +
        " metadataLabel=tostring(properties.label)," +
        " metadataPriorityScore=todouble(properties.priorityScore)," +
        " metadataSignalValues=properties.signalValues" +
        " ) on recommendationTypeId";

    internal static string BuildRecommendationScope(RecommendationQueryScope scope)
    {
        var query = "advisorresources | where type =~ 'microsoft.advisor/recommendations'";
        if (scope.SubscriptionId is { } subscriptionId)
        {
            query +=
                $" | where subscriptionId =~ '{RecommendationQueryBuilder.EscapeKqlString(subscriptionId)}'";
        }

        if (scope.ResourceGroup is { } resourceGroup)
        {
            query +=
                $" | where resourceGroup =~ '{RecommendationQueryBuilder.EscapeKqlString(resourceGroup)}'";
        }

        return query;
    }

    internal static string AddMetadataFilters(
        string query,
        string groupBy,
        RecommendationFilters? filters)
    {
        if (!string.IsNullOrWhiteSpace(filters?.Category))
        {
            query +=
                $" | where category =~ '{RecommendationQueryBuilder.EscapeKqlString(filters.Category.Trim())}'";
        }

        if (!string.IsNullOrWhiteSpace(filters?.Impact))
        {
            query +=
                $" | where impact =~ '{RecommendationQueryBuilder.EscapeKqlString(filters.Impact.Trim())}'";
        }

        var serviceRetirementOnly =
            groupBy == GroupByRetirementDate ||
            filters?.RetirementDate is not null;
        var subCategory = ServiceRetirementFilterValidator.ResolveSubCategory(
            filters?.SubCategory,
            serviceRetirementOnly);
        if (subCategory is not null)
        {
            query +=
                $" | where subCategory =~ '{RecommendationQueryBuilder.EscapeKqlString(subCategory)}'";
        }

        if (filters?.RetirementDate is { } retirementDate &&
            !string.IsNullOrWhiteSpace(filters.RetirementDateOperator))
        {
            query +=
                " | where isnotempty(retirementDate)" +
                $" | where todatetime(retirementDate) {ServiceRetirementFilterValidator.GetKqlComparisonOperator(filters.RetirementDateOperator)} datetime({retirementDate:yyyy-MM-dd})";
        }

        return query;
    }

    private static string BuildMetadataGrouping(string groupBy)
    {
        var projection = groupBy switch
        {
            GroupByRecommendationType =>
                " | extend key = iff(isempty(recommendationTypeId), 'Unknown', recommendationTypeId)" +
                " | extend label = iff(key == 'Unknown', 'Unknown', iff(isempty(typeLabel), key, typeLabel))",
            GroupByCategory => BuildKeyAndLabel("category"),
            GroupByImpact => BuildKeyAndLabel("impact"),
            GroupByResourceType => BuildKeyAndLabel("resourceType"),
            GroupByStatus => BuildKeyAndLabel("recommendationStatus"),
            GroupBySubCategory => BuildKeyAndLabel("subCategory"),
            GroupByRetirementDate => BuildKeyAndLabel("retirementDate"),
            _ => throw new ArgumentException(
                $"Unsupported group-by value '{groupBy}'.",
                nameof(groupBy)),
        };

        return projection;
    }

    private static string BuildKeyAndLabel(string property) =>
        $" | extend key = iff(isempty({property}), 'Unknown', {property})" +
        " | extend label = key";

    private static List<RecommendationGroup> SortGroups(
        string groupBy,
        IEnumerable<RecommendationGroup> groups)
    {
        var known = groups.Where(group => !IsUnknown(group));
        known = groupBy == GroupByRetirementDate
            ? known.OrderBy(group => group.Key, StringComparer.Ordinal)
            : known
                .OrderByDescending(group => group.Count)
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        return
        [
            .. known,
            .. groups.Where(IsUnknown),
        ];
    }

    private static bool IsUnknown(RecommendationGroup group) =>
        group.Key.Equals("Unknown", StringComparison.OrdinalIgnoreCase);

    private static void ValidateFilters(RecommendationFilters? filters, string groupBy)
    {
        if (filters is null)
        {
            return;
        }

        if ((filters.RetirementDate is null) ==
            string.IsNullOrWhiteSpace(filters.RetirementDateOperator))
        {
            ServiceRetirementFilterValidator.ResolveSubCategory(
                filters.SubCategory,
                groupBy == GroupByRetirementDate || filters.RetirementDate is not null);
            return;
        }

        throw new ArgumentException(
            "RetirementDate and RetirementDateOperator must be provided together.",
            nameof(filters));
    }

    private static string ReadRequiredString(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrEmpty(property.GetString()))
        {
            throw new JsonException(
                $"Recommendation summary row is missing a valid {propertyName} value.");
        }

        return property.GetString()!;
    }
}
