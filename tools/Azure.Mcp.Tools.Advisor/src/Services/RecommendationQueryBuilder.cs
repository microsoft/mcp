// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

internal static class RecommendationQueryBuilder
{
    internal const string CurrentRecommendationNameClause =
        "strlen(name) == 64";
    internal const string ServiceGroupExclusionClause =
        "isempty(properties.serviceGroupId)";
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
        IEnumerable<string>? recommendationTypeIds = null)
    {
        var clauses = new List<string>
        {
            CurrentRecommendationNameClause,
            ServiceGroupExclusionClause,
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

            if (!string.IsNullOrWhiteSpace(filters.RecommendationTypeId))
            {
                clauses.Add(
                    $"tostring(properties.recommendationTypeId) =~ '{SanitizeForKql(filters.RecommendationTypeId)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.ResourceType))
            {
                var resourceTypeProperty = resourceTypeUsesImpactedField
                    ? "properties.impactedField"
                    : "properties.resourceMetadata.resourceId";
                var comparison = resourceTypeUsesImpactedField ? "=~" : "contains";
                clauses.Add(
                    $"tostring({resourceTypeProperty}) {comparison} '{SanitizeForKql(filters.ResourceType)}'");
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
