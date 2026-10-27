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
        return await SummarizeRecommendationsAsync(
            [subscription],
            resourceGroup,
            groupBy,
            filters,
            tenant,
            cancellationToken);
    }

    public async Task<RecommendationSummary> SummarizeRecommendationsAsync(
        IReadOnlyCollection<string> subscriptions,
        string? resourceGroup,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);
        var normalizedGroupBy = NormalizeGroupBy(groupBy);
        var (scope, tenantResource, _) = await _queryExecutor.ResolveSubscriptionsScopeAsync(
            subscriptions,
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
        filters = RecommendationQueryBuilder.ApplySummaryStatusDefault(filters);
        var prioritized = IsPrioritized(filters);
        var rows = await ExecuteSummaryRowsAsync(
            tenantResource,
            BuildSummaryQuery(scope, normalizedGroupBy, filters),
            cancellationToken);

        var summary = prioritized
            ? ParsePrioritizedSummaryRows(rows)
            : ParseSummaryRows(normalizedGroupBy, rows);

        if (RecommendationQueryBuilder.IsIdleResourceCost(filters))
        {
            var sortByRealizedSavings = filters!.Status == RecommendationStatus.Completed;
            var costSummary = ParseCostSummaryRows(rows, sortByRealizedSavings) with
            {
                AsOfUtc = filters.SavingsAsOf,
                FromUtc = filters.SavingsFrom,
                ToUtc = filters.SavingsTo,
            };
            if (costSummary.RecommendationCount != summary.TotalRecommendations)
            {
                throw new JsonException("Azure Resource Graph returned inconsistent recommendation and cost summary counts.");
            }
            summary = summary with { Cost = costSummary };
        }

        return summary;
    }

    private static async Task<IReadOnlyList<JsonElement>> ExecuteSummaryRowsAsync(
        TenantResource tenantResource,
        string query,
        CancellationToken cancellationToken)
    {
        var result = await AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(tenantResource, query, cancellationToken);
        if (result.IsTruncated)
        {
            throw new InvalidOperationException(
                "Azure Resource Graph returned a truncated summary result; the complete summary cannot be calculated.");
        }
        return result.Rows;
    }

    internal static string BuildFilteredRecommendationQuery(
        RecommendationQueryScope scope,
        RecommendationFilters? filters,
        string groupBy)
    {
        filters = RecommendationQueryBuilder.ApplySummaryStatusDefault(filters);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: RecommendationQueryBuilder.ShouldFilterSummaryStatus(filters, groupBy == GroupByStatus),
            useRequestedStatus: true,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true,
            scope: scope);
        return BuildRecommendationScope(scope) +
            $" | where {predicates}" +
            RecommendationQueryBuilder.BuildHistoricalCompletedFilter(filters) +
            " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))";
    }

    internal const string CostReportAggregation =
        " potentialSavings = sum(potential), potentialCount = countif(isfinite(potential))," +
        " billReducingOpportunityCount = countif(isfinite(potential) and potential > 0)," +
        " zeroSavingsCount = countif(isfinite(potential) and potential <= 0)," +
        " unavailablePricingCount = countif(not(isfinite(potential) or isfinite(cost) or isfinite(realizedSavings)))";

    internal const string RestoreCostReportValues =
        " | extend potentialSavings = iff(potentialCount > 0, potentialSavings, real(null))," +
        " accumulatedWaste = iff(costCount > 0, cost, real(null))," +
        " realizedSavings = iff(realizedSavingsCount > 0, realizedSavings, real(null))," +
        " realizedCount = realizedSavingsCount";

    internal const string CostReportRow =
        "bag_pack('recommendationStatus', recommendationStatus, 'recommendationCount', resourceCount," +
        " 'potentialSavings', potentialSavings, 'potentialCount', potentialCount," +
        " 'accumulatedWaste', accumulatedWaste, 'costCount', costCount," +
        " 'realizedSavings', realizedSavings, 'realizedCount', realizedCount," +
        " 'billReducingOpportunityCount', billReducingOpportunityCount," +
        " 'zeroSavingsCount', zeroSavingsCount, 'unavailablePricingCount', unavailablePricingCount)";

    internal static RecommendationCostSummary ParseCostSummary(JsonElement data, bool sortByRealizedSavings = false)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid cost summary payload.");
        }

        return ParseCostSummaryRows(data.EnumerateArray(), sortByRealizedSavings);
    }

    private static RecommendationCostSummary ParseCostSummaryRows(
        IEnumerable<JsonElement> rows, bool sortByRealizedSavings)
    {
        RecommendationCostCurrencySummary? totals = null;
        var totalsByType = new Dictionary<string, (string Name, RecommendationCostCurrencySummary Totals)>(
            StringComparer.OrdinalIgnoreCase);
        var statusCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var recommendationCount = 0;
        var billReducingCount = 0;
        var zeroSavingsCount = 0;
        var unavailableCount = 0;
        foreach (var (item, typeId, name) in EnumerateCostRows(rows))
        {
            var potential = ReadOptionalDecimal(item, "potentialSavings");
            var cost = ReadOptionalDecimal(item, "accumulatedWaste");
            var realized = ReadOptionalDecimal(item, "realizedSavings");
            var status = ReadOptionalString(item, "recommendationStatus") ?? "Unknown";
            var count = ReadCount(item, "recommendationCount");
            recommendationCount = checked(recommendationCount + count);
            billReducingCount = checked(billReducingCount + ReadCount(item, "billReducingOpportunityCount"));
            zeroSavingsCount = checked(zeroSavingsCount + ReadCount(item, "zeroSavingsCount"));
            unavailableCount = checked(unavailableCount + ReadCount(item, "unavailablePricingCount"));
            statusCounts[status] = checked(statusCounts.GetValueOrDefault(status) + count);
            var rowTotals = new RecommendationCostCurrencySummary(
                PrioritizedRecommendationSummaryQuery.SavingsCurrency, potential, cost, realized, count,
                ReadCount(item, "potentialCount"), ReadCount(item, "costCount"), ReadCount(item, "realizedCount"));
            totals = totals is null ? rowTotals : MergeCostTotals(totals, rowTotals);

            if (typeId is not null)
            {
                var key = typeId.ToLowerInvariant();
                totalsByType[key] = totalsByType.TryGetValue(key, out var existingType)
                    ? (existingType.Name, MergeCostTotals(existingType.Totals, rowTotals))
                    : (name ?? typeId, rowTotals);
            }
        }

        return new(
            RecommendationQueryBuilder.RetailPricingBasis, recommendationCount, billReducingCount, zeroSavingsCount, unavailableCount,
            totals is null ? [] : [totals],
            StatusTotals: statusCounts
                .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(static pair => new RecommendationCostStatusSummary(pair.Key, pair.Value))
                .ToList(),
            TypeTotals: totalsByType
                .Select(static entry => new RecommendationCostTypeSummary(
                    entry.Key, entry.Value.Name, PrioritizedRecommendationSummaryQuery.SavingsCurrency, RecommendationQueryBuilder.RetailPricingBasis,
                    entry.Value.Totals.PotentialSavings, entry.Value.Totals.AccumulatedWaste, entry.Value.Totals.RealizedSavings,
                    entry.Value.Totals.RecommendationCount, entry.Value.Totals.PotentialValueCount,
                    entry.Value.Totals.AccumulatedWasteCount, entry.Value.Totals.RealizedSavingsCount))
                .OrderByDescending(item => GetSavingsSortValue(item, sortByRealizedSavings))
                .ThenBy(static item => item.RecommendationName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static item => item.RecommendationTypeId, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private static IEnumerable<(JsonElement Row, string? TypeId, string? Name)> EnumerateCostRows(
        IEnumerable<JsonElement> rows)
    {
        foreach (var row in rows)
        {
            if (row.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Azure Resource Graph returned an invalid cost summary row.");
            }
            if (row.TryGetProperty("costSummaryRows", out var costRows))
            {
                if (costRows.ValueKind != JsonValueKind.Array)
                {
                    throw new JsonException("Azure Resource Graph returned invalid cost summary rows.");
                }
                var typeId = ReadRequiredString(row, "key");
                var name = ReadRequiredString(row, "label");
                foreach (var item in costRows.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        throw new JsonException("Azure Resource Graph returned an invalid cost summary row.");
                    }
                    yield return (item, typeId, name);
                }
            }
            else
            {
                yield return (row, ReadOptionalString(row, "recommendationTypeId"),
                    ReadOptionalString(row, "recommendationName"));
            }
        }
    }

    private static decimal? GetSavingsSortValue(RecommendationCostTypeSummary item, bool sortByRealizedSavings) =>
        sortByRealizedSavings ? item.RealizedSavings : item.PotentialSavings;

    private static RecommendationCostCurrencySummary MergeCostTotals(
        RecommendationCostCurrencySummary left, RecommendationCostCurrencySummary right) =>
        left with
        {
            PotentialSavings = AddOptional(left.PotentialSavings, right.PotentialSavings),
            AccumulatedWaste = AddOptional(left.AccumulatedWaste, right.AccumulatedWaste),
            RealizedSavings = AddOptional(left.RealizedSavings, right.RealizedSavings),
            RecommendationCount = checked(left.RecommendationCount + right.RecommendationCount),
            PotentialValueCount = checked(left.PotentialValueCount + right.PotentialValueCount),
            AccumulatedWasteCount = checked(left.AccumulatedWasteCount + right.AccumulatedWasteCount),
            RealizedSavingsCount = checked(left.RealizedSavingsCount + right.RealizedSavingsCount),
        };

    private static decimal? AddOptional(decimal? left, decimal? right) =>
        left is decimal leftValue && right is decimal rightValue ? leftValue + rightValue : left ?? right;

    private static decimal? ReadOptionalDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        if (property.ValueKind != JsonValueKind.Number || !property.TryGetDecimal(out var value))
        {
            throw new JsonException($"Recommendation cost summary row has an invalid {propertyName} value.");
        }
        return value;
    }

    internal static bool IsPrioritized(RecommendationFilters? filters) =>
        filters?.Prioritized == true;

    internal static string BuildSummaryQuery(
        string subscriptionId,
        string? resourceGroup,
        string groupBy,
        RecommendationFilters? filters)
        => BuildSummaryQuery(
            RecommendationQueryScope.ForSubscription(subscriptionId, resourceGroup),
            groupBy,
            filters);

    internal static string BuildServiceGroupSummaryQuery(
        string serviceGroup,
        string groupBy,
        RecommendationFilters? filters)
        => BuildSummaryQuery(
            RecommendationQueryScope.ForServiceGroup(serviceGroup),
            groupBy,
            filters);

    internal static string BuildSummaryQuery(
        RecommendationQueryScope scope,
        string groupBy,
        RecommendationFilters? filters)
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

        return BuildMetadataSummaryQuery(scope, normalizedGroupBy, filters);
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

        return ParseSummaryRows(groupBy, data.EnumerateArray());
    }

    private static RecommendationSummary ParseSummaryRows(string groupBy, IEnumerable<JsonElement> rows)
    {
        var groupsByKey = new Dictionary<string, RecommendationGroup>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in rows)
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
        return new(groupBy, total, groups, TotalRecommendationGroupCount: groups.Count);
    }

    /// <summary>
    /// Reads globally aggregated rows, preserving the query's priority ordering.
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

        return ParsePrioritizedSummaryRows(data.EnumerateArray());
    }

    private static RecommendationSummary ParsePrioritizedSummaryRows(IEnumerable<JsonElement> rows)
    {
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var groups = new List<RecommendationGroup>();
        var total = 0;
        foreach (var item in rows)
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
            AnnualSavingsCurrency = annualSavings is null
                ? null
                : PrioritizedRecommendationSummaryQuery.SavingsCurrency,
            YearlyPotentialSavings = ReadOptionalFiniteNumber(item, "yearlyPotentialSavings"),
            Cost = ReadOptionalFiniteNumber(item, "cost"),
            RealizedSavings = ReadOptionalFiniteNumber(item, "realizedSavings"),
            Currency = ReadOptionalString(item, "currency"),
            PricingBasis = ReadOptionalString(item, "pricingBasis"),
            Status = ReadOptionalString(item, "recommendationStatus"),
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

    private static int ReadCount(JsonElement item) => ReadCount(item, "count_");

    private static int ReadCount(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var countProperty) ||
            countProperty.ValueKind != JsonValueKind.Number ||
            !countProperty.TryGetInt64(out var count64) ||
            count64 < 0)
        {
            throw new JsonException("Recommendation summary row has an invalid count.");
        }

        return checked((int)count64);
    }

    private static string BuildMetadataSummaryQuery(
        RecommendationQueryScope scope,
        string groupBy,
        RecommendationFilters? filters)
    {
        var includeCost = RecommendationQueryBuilder.IsIdleResourceCost(filters);
        var query = BuildFilteredRecommendationQuery(scope, filters, groupBy);

        query +=
            " | extend resourceId = tostring(properties.resourceMetadata.resourceId)" +
            " | project recommendationTypeId," +
            " impactedField=tostring(properties.impactedField)," +
            " resourceId," +
            " recommendationStatus=tostring(properties.recommendationStatus)" +
            (includeCost ? ", properties" : string.Empty) +
            JoinRecommendationTypeMetadata +
            BuildProductFamilyFilterClause(filters, "impactedField") +
            " | extend category = metadataCategory," +
            " impact = metadataImpact," +
            " subCategory = metadataSubCategory" +
            " | extend retirementDate = format_datetime(startofday(todatetime(metadataRetirementDate)), 'yyyy-MM-dd')" +
            " | extend impactedResourceType = tolower(impactedField)" +
            " | extend resourceIdType = tolower(extract(@'/providers/([^/]+/[^/]+)', 1, resourceId))" +
            " | extend resourceType = iff(isnotempty(impactedResourceType), impactedResourceType, resourceIdType)" +
            " | extend typeLabel = iff(isnotempty(metadataDisplayName), metadataDisplayName," +
            " iff(isnotempty(metadataLabel), metadataLabel, recommendationTypeId))";

        query = AddMetadataFilters(query, groupBy, filters);
        query += BuildMetadataGrouping(groupBy);
        if (includeCost)
        {
            return query +
                RecommendationQueryBuilder.BuildSavingsValues(filters) +
                " | extend potential = iff(isOpen, annualSavings, real(null)), recommendationName = typeLabel" +
                " | summarize count_ = count(), cost = sum(cost), costCount = countif(isfinite(cost))," +
                " realizedSavings = sum(realizedSavings), realizedSavingsCount = countif(isfinite(realizedSavings))," +
                CostReportAggregation +
                " by key, label, recommendationStatus, recommendationTypeId, recommendationName" +
                RestoreCostReportValues +
                " | extend recommendationCount = count_";
        }
        return query + " | summarize count() by key, label";
    }

    /// <summary>
    /// Inner join to English recommendation-type metadata, shared by every summary query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Category, impact, subcategory, retirement date, priority score, and published signals are taken only from this
    /// metadata. Recommendation records are not used as a fallback: metadata is the catalog of record for type-level
    /// properties, and where the two disagree the metadata is authoritative.
    /// </para>
    /// <para>
    /// The join is inner, as in the Azure portal Advisor experience: recommendations whose type has no English
    /// metadata row are excluded from every summary, so all groupings and prioritized summaries share one population
    /// and their totals agree. A type can still have an empty metadata field, which normal summaries group under
    /// <c>Unknown</c>.
    /// </para>
    /// </remarks>
    internal const string JoinRecommendationTypeMetadata =
        " | join kind=inner (" +
        " advisorresources" +
        " | where type =~ 'microsoft.advisor/metadata'" +
        " | where tostring(properties.language) =~ 'en'" +
        " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))" +
        " | project recommendationTypeId," +
        " metadataCategory=tostring(properties.recommendationCategory)," +
        " metadataImpact=tostring(properties.recommendationImpact)," +
        " metadataSubCategory=tostring(properties.recommendationSubCategory)," +
        " metadataServiceFamily=tostring(properties.serviceFamily)," +
        " metadataRetirementDate=tostring(properties.sourceProperties.serviceRetirement.retirementDate)," +
        " metadataDisplayName=tostring(properties.displayName)," +
        " metadataLabel=tostring(properties.label)," +
        " metadataPriorityScore=todouble(properties.priorityScore)," +
        " metadataSignalValues=properties.signalValues" +
        " ) on recommendationTypeId";

    internal static string BuildProductFamilyFilterClause(
        RecommendationFilters? filters,
        string resourceTypeExpression = "properties.impactedField") =>
        string.IsNullOrWhiteSpace(filters?.ProductFamily)
            ? string.Empty
            : $" | where {ProductFamilyFilter.BuildPredicate(filters.ProductFamily, resourceTypeExpression)}";

    internal static string BuildRecommendationScope(RecommendationQueryScope scope)
    {
        var query = "advisorresources | where type =~ 'Microsoft.Advisor/recommendations'";
        if (scope.Kind == RecommendationQueryScopeKind.Subscription)
        {
            query += $" | where {RecommendationQueryBuilder.BuildSubscriptionClause(scope)}";
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
