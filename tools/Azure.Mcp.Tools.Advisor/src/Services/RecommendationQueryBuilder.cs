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
    internal const string LifecycleSavingsSortExpression =
        "iff(recommendationStatus =~ 'Completed', realizedSavings, annualSavings)";
    internal const string RetailPricingBasis = "Retail";
    private const int MonthsPerYear = 12;
    private const decimal AverageDaysPerMonth = 30.4m;

    internal static bool IsIdleResourceCost(RecommendationFilters? filters) =>
        IsIdleResourceCost(filters?.Category, filters?.SubCategory);

    internal static bool IsIdleResourceCost(string? category, string? subCategory) =>
        string.Equals(category?.Trim(), "Cost", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(subCategory?.Trim(), "IdleResources", StringComparison.OrdinalIgnoreCase);

    internal static string AnnualizeSavings(string dailySavings) =>
        $"{dailySavings} * {MonthsPerYear.ToString(CultureInfo.InvariantCulture)} * {AverageDaysPerMonth.ToString(CultureInfo.InvariantCulture)}";

    internal static bool ShouldFilterSummaryStatus(RecommendationFilters? filters, bool groupByStatus = false) =>
        (IsIdleResourceCost(filters) && filters?.Status is not null) || !groupByStatus;

    internal static RecommendationFilters? ApplySummaryStatusDefault(RecommendationFilters? filters)
    {
        ValidateCostOnlyFilters(filters);
        if (filters?.Status is not null && !IsIdleResourceCost(filters))
        {
            throw new ArgumentException(
                "--status requires --category Cost and --sub-category IdleResources for summaries.",
                nameof(filters));
        }

        return IsIdleResourceCost(filters) && filters is { Status: null } &&
            (filters.SavingsAsOf is not null || filters.SavingsFrom is not null || filters.SavingsTo is not null)
            ? filters with { Status = RecommendationStatus.Completed }
            : filters;
    }

    internal static string BuildSavingsValues(RecommendationFilters? filters = null) =>
        " | extend dailySavings = todouble(properties.savings.retail.dailyPotentialSavings)," +
        " legacyAnnualSavings = todouble(properties.extendedProperties.annualSavingsAmount)," +
        " estimatedAccumulatedSavings = todouble(properties.savings.retail.estimatedAccumulatedSavings)," +
        " realizedAccumulatedSavings = todouble(properties.savings.retail.realizedAccumulatedSavings)," +
        " latestSavingsEffectiveOn = todatetime(properties.savings.retail.latestSavingsEffectiveOn)," +
        " resourceDeletedOn = todatetime(properties.savings.resourceDeletedOn)," +
        " recommendationStatus = tostring(properties.recommendationStatus)" +
        $" | extend annualSavings = iff(isfinite(dailySavings), {AnnualizeSavings("dailySavings")}," +
        (IsIdleResourceCost(filters)
            ? " real(null)),"
            : " iff(isfinite(legacyAnnualSavings), legacyAnnualSavings, real(null))),") +
        " isOpen = recommendationStatus !~ 'Completed'," +
        $" asOf = {(filters?.SavingsAsOf is DateTimeOffset asOf ? $"datetime({FormatUtcTimestamp(asOf)})" : "now()")}" +
        " | extend cost = iff(not(isOpen)," +
        " iff(isfinite(estimatedAccumulatedSavings), estimatedAccumulatedSavings, real(null))," +
        " iff(isfinite(dailySavings) and isfinite(estimatedAccumulatedSavings)" +
        " and isnotnull(latestSavingsEffectiveOn)," +
        " max_of(floor((asOf - latestSavingsEffectiveOn) / 1d, 1), 0.0) * dailySavings + estimatedAccumulatedSavings, real(null)))," +
        " realizedSavings = iff(not(isOpen) and isfinite(dailySavings) and isfinite(realizedAccumulatedSavings)" +
        " and isnotnull(resourceDeletedOn)," +
        " max_of(floor((asOf - resourceDeletedOn) / 1d, 1), 0.0) * dailySavings + realizedAccumulatedSavings, real(null))";

    internal static string BuildHistoricalCompletedFilter(RecommendationFilters? filters)
    {
        if (filters?.SavingsFrom is null && filters?.SavingsTo is null)
        {
            return string.Empty;
        }

        var clauses = new List<string>();
        if (filters.SavingsFrom is DateTimeOffset from)
        {
            clauses.Add($"todatetime(properties.savings.resourceDeletedOn) >= datetime({FormatUtcTimestamp(from)})");
        }
        if ((filters.SavingsTo ?? filters.SavingsAsOf) is DateTimeOffset to)
        {
            clauses.Add($"todatetime(properties.savings.resourceDeletedOn) <= datetime({FormatUtcTimestamp(to)})");
        }

        return $" | where (tostring(properties.recommendationStatus) !~ 'Completed' or ({string.Join(" and ", clauses)}))";
    }

    private static string FormatUtcTimestamp(DateTimeOffset value) =>
        value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);

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
                    $"{AnnualizeSavings("todouble(properties.savings.retail.dailyPotentialSavings)")} >= {threshold}");
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
            var resourceTypes = filters.ResourceTypes ?? [];
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

            if (filters.Resources is { Count: > 0 })
            {
                var resources = FormatKqlStringList(filters.Resources);
                clauses.Add($"tostring(properties.resourceMetadata.resourceId) in~ ({resources})");
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
        if (hasCostOnlyFilter && !IsIdleResourceCost(filters))
        {
            throw new ArgumentException(
                "--minimum-savings, --as-of, --from, and --to require --category Cost and --sub-category IdleResources.",
                nameof(filters));
        }
    }

    internal static string BuildServiceGroupClause(RecommendationQueryScope? scope) =>
        scope?.ServiceGroupResourceId is string serviceGroupResourceId
            ? $"isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '{SanitizeForKql(serviceGroupResourceId)}'"
            : ServiceGroupExclusionClause;

    internal static string BuildSubscriptionClause(RecommendationQueryScope scope)
    {
        var subscriptionIds = scope.ScopedSubscriptionIds;
        return subscriptionIds.Count == 1
            ? $"subscriptionId =~ '{SanitizeForKql(subscriptionIds[0])}'"
            : $"subscriptionId in~ ({FormatKqlStringList(subscriptionIds)})";
    }

    internal static string EscapeKqlString(string value) =>
        value.Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");

    internal static string SanitizeForKql(string value) =>
        EscapeKqlString(value.Trim());

    private static string FormatKqlStringList(IEnumerable<string> values) =>
        string.Join(
            ", ",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => $"'{SanitizeForKql(value)}'"));
}
