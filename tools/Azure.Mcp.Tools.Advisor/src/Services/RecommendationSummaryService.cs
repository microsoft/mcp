// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Validation;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public class RecommendationSummaryService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IRecommendationSummaryService
{
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
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var normalizedGroupBy = groupBy.Trim().ToLowerInvariant();
        if (!AllowedGroupBy.Contains(normalizedGroupBy, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported group-by value '{groupBy}'. Allowed values: {string.Join(", ", AllowedGroupBy)}.",
                nameof(groupBy));
        }

        var subscriptionResource = await AzureService.GetSubscription(
            subscription,
            tenant,
            cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId
            ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");

        if (!string.IsNullOrWhiteSpace(resourceGroup))
        {
            var exists = await subscriptionResource
                .GetResourceGroups()
                .ExistsAsync(resourceGroup.Trim(), cancellationToken);
            if (!exists.Value)
            {
                throw new KeyNotFoundException(
                    $"Resource group '{resourceGroup}' does not exist in subscription '{subscriptionId}'.");
            }
        }

        var tenants = await AzureService.GetTenants(cancellationToken);
        var tenantResource = tenants.FirstOrDefault(
            candidate => candidate.Data.TenantId == subscriptionResource.Data.TenantId)
            ?? throw new InvalidOperationException(
                $"No accessible tenant was found for subscription '{subscription}'.");

        var usesMetadata = RequiresMetadata(normalizedGroupBy, filters);
        var query = BuildSummaryQuery(
            subscriptionId,
            resourceGroup,
            normalizedGroupBy,
            filters,
            usesMetadata);
        var queryContent = new ResourceQueryContent(query);
        if (!usesMetadata)
        {
            queryContent.Subscriptions.Add(subscriptionId);
        }

        var response = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        var result = response.Value;
        EnsureCompleteResult(
            result.ResultTruncated == ResultTruncated.True,
            result.SkipToken);

        if (result.Count == 0)
        {
            return new(normalizedGroupBy, 0, []);
        }

        using var document = JsonDocument.Parse(result.Data);
        return ParseSummary(normalizedGroupBy, document.RootElement);
    }

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
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var normalizedGroupBy = groupBy.Trim().ToLowerInvariant();
        if (!AllowedGroupBy.Contains(normalizedGroupBy, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported group-by value '{groupBy}'. Allowed values: {string.Join(", ", AllowedGroupBy)}.",
                nameof(groupBy));
        }

        ValidateFilters(filters, normalizedGroupBy);

        return useMetadata ?? RequiresMetadata(normalizedGroupBy, filters)
            ? BuildMetadataSummaryQuery(subscriptionId, resourceGroup, normalizedGroupBy, filters)
            : BuildInstanceSummaryQuery(subscriptionId, resourceGroup, normalizedGroupBy, filters);
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
            if (!item.TryGetProperty("count_", out var countProperty) ||
                countProperty.ValueKind != JsonValueKind.Number ||
                !countProperty.TryGetInt64(out var count64) ||
                count64 < 0)
            {
                throw new JsonException("Recommendation summary row has an invalid count.");
            }

            var count = checked((int)count64);
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

    private static string BuildInstanceSummaryQuery(
        string subscriptionId,
        string? resourceGroup,
        string groupBy,
        RecommendationFilters? filters)
    {
        var query = BuildRecommendationScope(subscriptionId, resourceGroup);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: groupBy != GroupByStatus,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: true);
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
        string subscriptionId,
        string? resourceGroup,
        string groupBy,
        RecommendationFilters? filters)
    {
        var query = BuildRecommendationScope(subscriptionId, resourceGroup);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: groupBy != GroupByStatus,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true);
        if (predicates.Length > 0)
        {
            query += $" | where {predicates}";
        }

        query +=
            " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))" +
            " | project recommendationTypeId," +
            " instanceCategory=tostring(properties.category)," +
            " instanceImpact=tostring(properties.impact)," +
            " instanceSubCategory=tostring(properties.extendedProperties.recommendationSubCategory)," +
            " instanceRetirementDate=tostring(properties.extendedProperties.retirementDate)," +
            " impactedField=tostring(properties.impactedField)," +
            " resourceId=tostring(properties.resourceMetadata.resourceId)," +
            " recommendationStatus=tostring(properties.recommendationStatus)" +
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
            " metadataLabel=tostring(properties.label)" +
            " ) on recommendationTypeId" +
            " | extend category = iff(isnotempty(metadataCategory), metadataCategory, instanceCategory)," +
            " impact = iff(isnotempty(metadataImpact), metadataImpact, instanceImpact)," +
            " subCategory = iff(isnotempty(metadataSubCategory), metadataSubCategory, instanceSubCategory)," +
            " retirementDateRaw = iff(isnotempty(metadataRetirementDate), metadataRetirementDate, instanceRetirementDate)" +
            " | extend retirementDate = format_datetime(startofday(todatetime(retirementDateRaw)), 'yyyy-MM-dd')" +
            " | extend impactedResourceType = tolower(impactedField)" +
            " | extend resourceIdType = tolower(extract(@'/providers/([^/]+/[^/]+)', 1, resourceId))" +
            " | extend resourceType = iff(isnotempty(impactedResourceType), impactedResourceType, resourceIdType)" +
            " | extend typeLabel = iff(isnotempty(metadataDisplayName), metadataDisplayName," +
            " iff(isnotempty(metadataLabel), metadataLabel, recommendationTypeId))";

        query = AddMetadataFilters(query, groupBy, filters);
        query += BuildMetadataGrouping(groupBy);
        return query + " | summarize count() by key, label";
    }

    private static string BuildRecommendationScope(string subscriptionId, string? resourceGroup)
    {
        var query =
            "advisorresources" +
            " | where type =~ 'microsoft.advisor/recommendations'" +
            $" | where subscriptionId =~ '{RecommendationQueryBuilder.EscapeKqlString(subscriptionId.Trim())}'";
        if (!string.IsNullOrWhiteSpace(resourceGroup))
        {
            query +=
                $" | where resourceGroup =~ '{RecommendationQueryBuilder.EscapeKqlString(resourceGroup.Trim())}'";
        }

        return query;
    }

    private static string AddMetadataFilters(
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
