// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
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

    public async Task<RecommendationSummary> SummarizeServiceGroupRecommendationsAsync(
        IReadOnlyCollection<string> serviceGroups,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceGroups);
        var normalizedGroupBy = NormalizeGroupBy(groupBy);
        var (scope, tenantResource) = await _queryExecutor.ResolveServiceGroupScopeAsync(
            serviceGroups,
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
        var rows = await ExecuteBatchedRowsAsync(
            scope,
            tenantResource,
            batchScope => BuildSummaryQuery(batchScope, normalizedGroupBy, filters),
            cancellationToken);
        if (rows.Count == 0)
        {
            return new(normalizedGroupBy, 0, []);
        }

        using var document = JsonDocument.Parse(ToJsonArray(rows));
        var summary = prioritized
            ? ParsePrioritizedSummary(document.RootElement)
            : ParseSummary(normalizedGroupBy, document.RootElement);

        if (string.Equals(filters?.Category, "Cost", StringComparison.OrdinalIgnoreCase))
        {
            summary = summary with
            {
                Cost = await ReadCostSummaryAsync(
                    scope,
                    tenantResource,
                    normalizedGroupBy,
                    filters ?? new RecommendationFilters(Category: "Cost"),
                    cancellationToken),
            };
        }

        return summary;
    }

    private static async Task<RecommendationCostSummary> ReadCostSummaryAsync(
        RecommendationQueryScope scope,
        TenantResource tenantResource,
        string groupBy,
        RecommendationFilters filters,
        CancellationToken cancellationToken)
    {
        const string pricingBasis = "Retail";
        var rows = await ExecuteBatchedRowsAsync(
            scope,
            tenantResource,
            batchScope => BuildCostSummaryQuery(batchScope, filters, groupBy),
            cancellationToken);
        if (rows.Count == 0)
        {
            return new(
                pricingBasis,
                0,
                0,
                0,
                0,
                [],
                filters.SavingsAsOf,
                filters.SavingsFrom,
                filters.SavingsTo,
                [],
                []);
        }

        using var document = JsonDocument.Parse(ToJsonArray(rows));
        return ParseCostSummary(document.RootElement, pricingBasis) with
        {
            AsOfUtc = filters.SavingsAsOf,
            FromUtc = filters.SavingsFrom,
            ToUtc = filters.SavingsTo,
        };
    }

    private static string ToJsonArray(IEnumerable<JsonElement> rows) =>
        $"[{string.Join(',', rows.Select(static row => row.GetRawText()))}]";

    private static async Task<List<JsonElement>> ExecuteBatchedRowsAsync(
        RecommendationQueryScope scope,
        TenantResource tenantResource,
        Func<RecommendationQueryScope, string> queryFactory,
        CancellationToken cancellationToken)
    {
        var rows = new List<JsonElement>();
        var batchResults = await AdvisorResourceGraphQueryExecutor.ExecuteBatchedPagedAsync(
            scope,
            tenantResource,
            queryFactory,
            cancellationToken);
        foreach (var batchResult in batchResults)
        {
            if (batchResult.IsTruncated)
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph returned a truncated summary result; the complete summary cannot be calculated.");
            }

            rows.AddRange(batchResult.Rows);
        }

        return rows;
    }

    internal static string BuildCostSummaryQuery(
        RecommendationQueryScope scope,
        RecommendationFilters? filters,
        string? groupBy = null)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = BuildRecommendationScope(scope);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: !HasHistoricalCostWindow(filters) &&
                !string.Equals(groupBy, GroupByStatus, StringComparison.OrdinalIgnoreCase),
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
            JoinRecommendationTypeMetadata +
            BuildProductFamilyFilterClause(filters) +
            " | extend category = metadataCategory, impact = metadataImpact, subCategory = metadataSubCategory" +
            " | extend recommendationName = iff(isnotempty(metadataDisplayName), metadataDisplayName, " +
            "iff(isnotempty(metadataLabel), metadataLabel, recommendationTypeId))" +
            " | extend retirementDate = format_datetime(startofday(todatetime(metadataRetirementDate)), 'yyyy-MM-dd')";
        query = AddMetadataFilters(query, GroupByRecommendationType, filters);
        query +=
            " | extend _daily = todouble(properties.savings.retail.dailyPotentialSavings)" +
            " | extend _estimatedAccumulated = todouble(properties.savings.retail.estimatedAccumulatedSavings)" +
            " | extend _realizedAccumulated = todouble(properties.savings.retail.realizedAccumulatedSavings)" +
            " | extend _effectiveOn = todatetime(properties.savings.retail.latestSavingsEffectiveOn)" +
            " | extend _deletedOn = todatetime(properties.savings.resourceDeletedOn)" +
            " | extend _isOpen = tostring(properties.recommendationStatus) !~ 'Completed'" +
            " | extend recommendationStatus = tostring(properties.recommendationStatus)" +
            $" | extend _asOf = {BuildAsOfExpression(filters)}" +
            " | extend _idleDays = max_of(floor((_asOf - _effectiveOn) / 1d, 1), 0.0)" +
            " | extend _deletedDays = max_of(floor((_asOf - _deletedOn) / 1d, 1), 0.0)" +
            " | extend potential = iif(_isOpen and isfinite(_daily), _daily * 364.8, double(null))" +
            " | extend cost = iif(_isOpen and isfinite(_daily) and isnotnull(_effectiveOn), " +
            "_idleDays * _daily + _estimatedAccumulated, double(null))" +
            " | extend realized = iif(not(_isOpen) and isnotnull(_deletedOn) and isfinite(_daily), " +
            "_deletedDays * _daily + _realizedAccumulated, double(null))" +
            " | extend currency = iff(isfinite(potential) or isfinite(cost) or isfinite(realized), " +
            "iff(isempty(tostring(properties.savings.providerCurrency)), 'USD', " +
            "tostring(properties.savings.providerCurrency)), 'Unavailable')" +
            BuildHistoricalCompletedFilter(filters) +
            " | summarize arg_max(_effectiveOn, *) by recommendationTypeId, " +
            "tostring(properties.resourceMetadata.resourceId), recommendationStatus" +
            " | summarize potentialTotal=sum(potential), potentialCount=countif(isfinite(potential)), " +
            "costTotal=sum(cost), costCount=countif(isfinite(cost)), " +
            "realizedTotal=sum(realized), realizedCount=countif(isfinite(realized)), " +
            "recommendationCount=count(), " +
            "billReducingOpportunityCount=countif(isfinite(potential) and potential > 0), " +
            "zeroSavingsCount=countif(isfinite(potential) and potential <= 0), " +
            " unavailablePricingCount=countif(not(isfinite(potential) or isfinite(cost) or isfinite(realized))) by currency, recommendationStatus, recommendationTypeId, recommendationName" +
            " | extend potentialSavings = iif(potentialCount == 0, double(null), potentialTotal)," +
            " accumulatedWaste = iif(costCount == 0, double(null), costTotal)," +
            " realizedSavings = iif(realizedCount == 0, double(null), realizedTotal)";
        return query;
    }

    private static bool HasHistoricalCostWindow(RecommendationFilters? filters) =>
        filters?.SavingsAsOf is not null ||
        filters?.SavingsFrom is not null ||
        filters?.SavingsTo is not null;

    private static string BuildAsOfExpression(RecommendationFilters? filters) =>
        filters?.SavingsAsOf is DateTimeOffset asOf
            ? $"datetime({FormatUtcTimestamp(asOf)})"
            : "now()";

    private static string BuildHistoricalCompletedFilter(RecommendationFilters? filters)
    {
        if (filters?.SavingsFrom is null && filters?.SavingsTo is null)
        {
            return string.Empty;
        }

        var dateClauses = new List<string>();
        if (filters.SavingsFrom is DateTimeOffset from)
        {
            dateClauses.Add($"(_deletedOn >= datetime({FormatUtcTimestamp(from)}))");
        }

        if (filters.SavingsTo is DateTimeOffset to)
        {
            dateClauses.Add($"(_deletedOn <= datetime({FormatUtcTimestamp(to)}))");
        }
        else if (filters.SavingsAsOf is DateTimeOffset asOf)
        {
            dateClauses.Add($"(_deletedOn <= datetime({FormatUtcTimestamp(asOf)}))");
        }

        return $" | where (_isOpen or ({string.Join(" and ", dateClauses)}))";
    }

    private static string FormatUtcTimestamp(DateTimeOffset value) =>
        value.UtcDateTime.ToString("yyyy-MM-dd'T'HH':'mm':'ss.fff'Z'", CultureInfo.InvariantCulture);

    internal static RecommendationCostSummary ParseCostSummary(JsonElement data, string pricingBasis = "Retail")
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid cost summary payload.");
        }

        var totalsByCurrency = new Dictionary<string, RecommendationCostCurrencySummary>(StringComparer.OrdinalIgnoreCase);
        var recommendationCount = 0;
        var billReducingCount = 0;
        var zeroSavingsCount = 0;
        var unavailableCount = 0;
        var statusCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var typeTotals = new Dictionary<string, RecommendationCostTypeSummary>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.EnumerateArray())
        {
            var potentialSavings = ReadOptionalDecimal(item, "potentialSavings");
            var accumulatedWaste = ReadOptionalDecimal(item, "accumulatedWaste");
            var realizedSavings = ReadOptionalDecimal(item, "realizedSavings");
            // Resource Graph can omit the projected currency for an aggregate row. Match the query's Retail default
            // when the row still contains a savings measure; otherwise preserve it under the unavailable bucket.
            var currency = ReadOptionalString(item, "currency") ??
                (potentialSavings is not null || accumulatedWaste is not null || realizedSavings is not null
                    ? "USD"
                    : "Unavailable");
            var status = item.TryGetProperty("recommendationStatus", out var statusProperty)
                ? statusProperty.GetString() ?? "Unknown"
                : "Unknown";
            var count = ReadCount(item, "recommendationCount");
            recommendationCount = checked(recommendationCount + count);
            billReducingCount = checked(billReducingCount + ReadCount(item, "billReducingOpportunityCount"));
            zeroSavingsCount = checked(zeroSavingsCount + ReadCount(item, "zeroSavingsCount"));
            unavailableCount = checked(unavailableCount + ReadCount(item, "unavailablePricingCount"));
            statusCounts[status] = checked(statusCounts.GetValueOrDefault(status) + count);
            var currencySummary = new RecommendationCostCurrencySummary(
                currency,
                potentialSavings,
                accumulatedWaste,
                realizedSavings,
                count,
                ReadCount(item, "potentialCount"),
                ReadCount(item, "costCount"),
                ReadCount(item, "realizedCount"));
            if (totalsByCurrency.TryGetValue(currency, out var existing))
            {
                totalsByCurrency[currency] = MergeCurrencyTotals(existing, currencySummary);
            }
            else
            {
                totalsByCurrency[currency] = currencySummary;
            }

            if (item.TryGetProperty("recommendationTypeId", out var typeIdProperty) &&
                typeIdProperty.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(typeIdProperty.GetString()))
            {
                var recommendationTypeId = typeIdProperty.GetString()!;
                var recommendationName = item.TryGetProperty("recommendationName", out var nameProperty) &&
                    nameProperty.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(nameProperty.GetString())
                        ? nameProperty.GetString()!
                        : recommendationTypeId;
                var typeSummary = new RecommendationCostTypeSummary(
                    recommendationTypeId,
                    recommendationName,
                    currency,
                    pricingBasis,
                    currencySummary.PotentialSavings,
                    currencySummary.AccumulatedWaste,
                    currencySummary.RealizedSavings,
                    currencySummary.RecommendationCount,
                    currencySummary.PotentialValueCount,
                    currencySummary.AccumulatedWasteCount,
                    currencySummary.RealizedSavingsCount);
                var typeKey = $"{recommendationTypeId}|{currency}";
                typeTotals[typeKey] = typeTotals.TryGetValue(typeKey, out var existingType)
                    ? MergeTypeTotals(existingType, typeSummary)
                    : typeSummary;
            }
        }

        return new(
            pricingBasis,
            recommendationCount,
            billReducingCount,
            zeroSavingsCount,
            unavailableCount,
            totalsByCurrency.Values
                .OrderBy(static item => item.Currency, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            StatusTotals: statusCounts
                .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(static pair => new RecommendationCostStatusSummary(pair.Key, pair.Value))
                .ToList(),
            TypeTotals: typeTotals.Values
                .OrderByDescending(static item => item.PotentialSavings.HasValue)
                .ThenByDescending(static item => item.PotentialSavings ?? decimal.MinValue)
                .ThenBy(static item => item.RecommendationName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static item => item.Currency, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private static RecommendationCostTypeSummary MergeTypeTotals(
        RecommendationCostTypeSummary left,
        RecommendationCostTypeSummary right) =>
        new(
            left.RecommendationTypeId,
            left.RecommendationName,
            left.Currency,
            left.PricingBasis,
            AddOptional(left.PotentialSavings, right.PotentialSavings),
            AddOptional(left.AccumulatedWaste, right.AccumulatedWaste),
            AddOptional(left.RealizedSavings, right.RealizedSavings),
            checked(left.RecommendationCount + right.RecommendationCount),
            checked(left.PotentialValueCount + right.PotentialValueCount),
            checked(left.AccumulatedWasteCount + right.AccumulatedWasteCount),
            checked(left.RealizedSavingsCount + right.RealizedSavingsCount));

    private static RecommendationCostCurrencySummary MergeCurrencyTotals(
        RecommendationCostCurrencySummary left,
        RecommendationCostCurrencySummary right) =>
        new(
            left.Currency,
            AddOptional(left.PotentialSavings, right.PotentialSavings),
            AddOptional(left.AccumulatedWaste, right.AccumulatedWaste),
            AddOptional(left.RealizedSavings, right.RealizedSavings),
            checked(left.RecommendationCount + right.RecommendationCount),
            checked(left.PotentialValueCount + right.PotentialValueCount),
            checked(left.AccumulatedWasteCount + right.AccumulatedWasteCount),
            checked(left.RealizedSavingsCount + right.RealizedSavingsCount));

    private static decimal? AddOptional(decimal? left, decimal? right) =>
        left is decimal leftValue && right is decimal rightValue
            ? leftValue + rightValue
            : left ?? right;

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
        return new(groupBy, total, groups, TotalGroups: groups.Count);
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

    private static decimal? ReadOptionalDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.Number ||
            !property.TryGetDecimal(out var value))
        {
            throw new JsonException(
                $"Recommendation cost summary row has an invalid {propertyName} value.");
        }

        return value;
    }

    private static string BuildMetadataSummaryQuery(
        RecommendationQueryScope scope,
        string groupBy,
        RecommendationFilters? filters)
    {
        var query = BuildRecommendationScope(scope);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: groupBy != GroupByStatus && !HasHistoricalCostWindow(filters),
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true,
            scope: scope);
        if (predicates.Length > 0)
        {
            query += $" | where {predicates}";
        }
        if (HasHistoricalCostWindow(filters))
        {
            query += BuildHistoricalCompletedFilterForRawProperties(filters);
        }

        query +=
            " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))" +
            " | extend resourceId = tostring(properties.resourceMetadata.resourceId)" +
            " | project recommendationTypeId," +
            " impactedField=tostring(properties.impactedField)," +
            " resourceId," +
            " recommendationStatus=tostring(properties.recommendationStatus)" +
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
        return query + " | summarize count() by key, label";
    }

    private static string BuildHistoricalCompletedFilterForRawProperties(RecommendationFilters? filters)
    {
        if (filters?.SavingsFrom is null && filters?.SavingsTo is null)
        {
            return string.Empty;
        }

        var clauses = new List<string> { "tostring(properties.recommendationStatus) !~ 'Completed'" };
        if (filters.SavingsFrom is DateTimeOffset from)
        {
            clauses.Add(
                $"todatetime(properties.savings.resourceDeletedOn) >= datetime({FormatUtcTimestamp(from)})");
        }

        if (filters.SavingsTo is DateTimeOffset to)
        {
            clauses.Add(
                $"todatetime(properties.savings.resourceDeletedOn) <= datetime({FormatUtcTimestamp(to)})");
        }
        else if (filters.SavingsAsOf is DateTimeOffset asOf)
        {
            clauses.Add(
                $"todatetime(properties.savings.resourceDeletedOn) <= datetime({FormatUtcTimestamp(asOf)})");
        }

        var completedDateClauses = clauses.Skip(1).ToArray();
        return $" | where ({clauses[0]} or ({string.Join(" and ", completedDateClauses)}))";
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

    private static string BuildProductFamilyFilterClause(
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
