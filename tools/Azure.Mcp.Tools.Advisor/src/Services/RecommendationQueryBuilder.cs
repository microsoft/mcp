// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;

using Azure.Mcp.Tools.Advisor.Models;
namespace Azure.Mcp.Tools.Advisor.Services;

internal static class RecommendationQueryBuilder
{
    internal const string CurrentRecommendationNameClause =
        "strlen(name) == 64";
    internal const string ServiceGroupExclusionClause =
        "isempty(properties.serviceGroupId)";
    internal const string ServiceGroupResourceIdPrefix =
        "/providers/Microsoft.Management/serviceGroups/";
    internal const string CurrentRecommendationEngineClause =
        CurrentRecommendationNameClause + " and " + ServiceGroupExclusionClause;
    internal const string ActiveRecommendationClause =
        "tostring(properties.recommendationStatus) =~ 'New'";

    internal static string BuildInstancePredicates(
        RecommendationFilters? filters,
        bool includeStatus,
        bool useRequestedStatus,
        bool includeCategoryAndImpact,
        bool resourceTypeUsesImpactedField,
        IEnumerable<string>? recommendationTypeIds = null,
        RecommendationQueryScope? scope = null)
    {
        ValidateCostOnlyFilters(filters);

        var clauses = new List<string>
        {
            CurrentRecommendationNameClause,
            BuildServiceGroupClause(scope),
        };
        if (includeStatus)
        {
            var status = useRequestedStatus
                ? filters?.Status ?? RecommendationStatus.New
                : RecommendationStatus.New;
            clauses.Add($"tostring(properties.recommendationStatus) =~ '{status}'");
        }

        var resolvedTypeIds = recommendationTypeIds?.ToList();
        var metadataWasResolved = resolvedTypeIds is not null;
        if (filters is not null)
        {
            if (filters.MinimumSavings is decimal minimumSavings)
            {
                var threshold = minimumSavings.ToString("G29", CultureInfo.InvariantCulture);
                clauses.Add(
                    $"isfinite(todouble(properties.savings.retail.dailyPotentialSavings)) and " +
                    $"todouble(properties.savings.retail.dailyPotentialSavings) * 30.4 * 12 >= {threshold}");
            }

            if (includeCategoryAndImpact &&
                !metadataWasResolved &&
                !string.IsNullOrWhiteSpace(filters.Category))
            {
                clauses.Add($"tostring(properties.category) =~ '{SanitizeForKql(filters.Category)}'");
            }

            if (includeCategoryAndImpact &&
                !metadataWasResolved &&
                !string.IsNullOrWhiteSpace(filters.Impact))
            {
                clauses.Add($"tostring(properties.impact) =~ '{SanitizeForKql(filters.Impact)}'");
            }

            if (filters.RecommendationTypeIds is { Count: > 0 })
            {
                clauses.Add(
                    $"tostring(properties.recommendationTypeId) in~ ({FormatKqlStringList(filters.RecommendationTypeIds)})");
            }
            else if (!string.IsNullOrWhiteSpace(filters.RecommendationTypeId))
            {
                clauses.Add(
                    $"tostring(properties.recommendationTypeId) =~ '{SanitizeForKql(filters.RecommendationTypeId)}'");
            }

            var resourceTypes = filters.ResourceTypes is { Count: > 0 }
                ? filters.ResourceTypes
                : string.IsNullOrWhiteSpace(filters.ResourceType)
                    ? []
                    : [filters.ResourceType];
            if (resourceTypes.Count > 0)
            {
                var resourceTypeProperty = resourceTypeUsesImpactedField
                    ? "properties.impactedField"
                    : "properties.resourceMetadata.resourceId";
                var comparisons = resourceTypes.Select(resourceType =>
                {
                    var comparison = resourceTypeUsesImpactedField ? "=~" : "contains";
                    return $"tostring({resourceTypeProperty}) {comparison} '{SanitizeForKql(resourceType)}'";
                });
                clauses.Add(resourceTypes.Count == 1
                    ? comparisons.Single()
                    : $"({string.Join(" or ", comparisons)})");
            }

            if (!string.IsNullOrWhiteSpace(filters.Resource))
            {
                clauses.Add(
                    $"tostring(properties.resourceMetadata.resourceId) contains '{SanitizeForKql(filters.Resource)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                clauses.Add(
                    $"tostring(properties.shortDescription.problem) contains '{SanitizeForKql(filters.Search)}'");
            }
        }

        if (resolvedTypeIds is { Count: > 0 })
        {
            clauses.Add(
                $"tostring(properties.recommendationTypeId) in~ ({FormatKqlStringList(resolvedTypeIds)})");
        }

        return string.Join(" and ", clauses);
    }

    internal static void ValidateCostOnlyFilters(RecommendationFilters? filters)
    {
        if (filters is null)
        {
            return;
        }

        var hasCostOnlyFilter =
            filters.MinimumSavings is not null ||
            filters.SavingsAsOf is not null ||
            filters.SavingsFrom is not null ||
            filters.SavingsTo is not null;
        if (hasCostOnlyFilter &&
            !string.Equals(filters.Category, "Cost", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "--minimum-savings, --as-of, --from, and --to require --category Cost.",
                nameof(filters));
        }
    }

    internal static string BuildServiceGroupClause(RecommendationQueryScope? scope)
    {
        var serviceGroupResourceIds = scope?.ScopedServiceGroupIds
            .Select(id => ServiceGroupResourceIdPrefix + id)
            .ToArray();
        return serviceGroupResourceIds is not { Length: > 0 }
            ? ServiceGroupExclusionClause
            : serviceGroupResourceIds.Length == 1
                ? $"isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '{SanitizeForKql(serviceGroupResourceIds[0])}'"
                : $"isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) in~ ({FormatKqlStringList(serviceGroupResourceIds)})";
    }

    internal static string BuildSubscriptionClause(RecommendationQueryScope scope)
    {
        var subscriptionIds = scope.ScopedSubscriptionIds;
        return subscriptionIds.Count == 1
            ? $"subscriptionId =~ '{SanitizeForKql(subscriptionIds[0])}'"
            : $"subscriptionId in~ ({FormatKqlStringList(subscriptionIds)})";
    }

    internal static string EscapeKqlString(string value) =>
        value.Replace("|", string.Empty)
            .Replace("\\", "\\\\")
            .Replace("'", "''");

    internal static string SanitizeForKql(string value) =>
        EscapeKqlString(value.Trim());

    private static string FormatKqlStringList(IEnumerable<string> values) =>
        string.Join(
            ", ",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => $"'{SanitizeForKql(value)}'"));
}
