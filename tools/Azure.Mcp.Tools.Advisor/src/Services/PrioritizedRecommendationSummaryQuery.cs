// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

/// <summary>
/// Builds the Azure Resource Graph query behind <c>advisor recommendation summary --prioritized</c>: active
/// recommendations grouped by recommendation type and ranked by priority.
/// </summary>
/// <remarks>
/// <para>
/// The query is assembled from named stages, applied in this order:
/// </para>
/// <list type="number">
/// <item><see cref="FilterRecords"/>: which recommendation records participate.</item>
/// <item><see cref="KeepOneReservedInstanceScenario"/>: one purchase scenario per reserved-instance recommendation.</item>
/// <item><see cref="ComputePerRecordValues"/>: resource key, criticality score, and annual savings per record.</item>
/// <item><see cref="CollapseToOneRowPerResource"/>: one row per (recommendation type, resource).</item>
/// <item><see cref="AggregatePerType"/>: one row per recommendation type.</item>
/// <item><see cref="JoinTypeMetadata"/>: type-level properties, taken only from recommendation-type metadata.</item>
/// <item>Metadata filters (category, impact, subcategory, retirement date), shared with normal summaries.</item>
/// <item><see cref="OrderByPriority"/>: the single ordering that defines the ranking.</item>
/// <item><see cref="ProjectResultRows"/>: the columns returned to the service.</item>
/// </list>
/// <para>
/// The ranking follows the Azure portal Advisor prioritized view, with the deliberate differences that are
/// called out on each stage. Ordering happens only in KQL; the service keeps the returned order and applies
/// <c>--top</c> afterwards, so <c>totalRecommendations</c> always covers every matching type.
/// </para>
/// </remarks>
internal static class PrioritizedRecommendationSummaryQuery
{
    /// <summary>Category whose types rank by resource count ascending instead of descending; see <see cref="OrderByPriority"/>.</summary>
    internal const string CostCategory = "Cost";

    /// <summary>
    /// Daily savings are annualized as daily × 12 months × 30.4 days per month, the same factor the Azure portal
    /// Advisor experience uses.
    /// </summary>
    internal const double MonthsPerYear = 12;
    internal const double DaysPerMonth = 30.4;

    /// <summary>
    /// Savings are reported in US dollars. A missing currency on a record also means US dollars; no currency conversion is
    /// performed.
    /// </summary>
    internal const string SavingsCurrency = "USD";

    /// <summary>Returned annual savings are rounded to cents.</summary>
    internal const int SavingsDecimals = 2;

    /// <summary>Reserved-instance purchase scenario kept in prioritized mode: a three-year term...</summary>
    internal const string ReservedInstanceTerm = "P3Y";

    /// <summary>...sized from the last seven days of usage, as in the Azure portal Advisor prioritized view.</summary>
    internal const string ReservedInstanceLookbackPeriod = "7";

    /// <summary>
    /// Recommendation types that publish one record per reserved-instance purchase scenario (term × lookback period)
    /// for the same resource. This is the same set of type IDs the Azure portal Advisor experience treats as reserved
    /// instances.
    /// </summary>
    internal static readonly IReadOnlyList<string> ReservedInstanceRecommendationTypeIds =
    [
        "0169a2e1-c7bf-4c37-90b8-0714811c82d3",
        "06ad499a-0952-48d3-b061-ec81c9cabb8b",
        "0d524e8d-4cfd-4db5-9f91-8b4bb5235a8e",
        "0eb54047-acd9-4f26-8ffb-8cec713782d6",
        "10aedd06-621e-4b4f-a45c-5256573e0191",
        "148cdd60-97e8-426b-a7b9-141b7cb4bc2f",
        "171f87ad-4ead-42fc-8f32-a3b18d451837",
        "1b8c5187-32a6-4a2f-8ca1-b0b7d6ce9e86",
        "32755df6-aa2f-48d7-9ab7-92b8a80352ea",
        "3327646a-c325-417f-a3e3-36ae7119da69",
        "3f6c5689-6a05-4896-a6e0-c6f8a22a44c2",
        "407b6ad6-8e0b-40e7-9384-643520cae0ed",
        "5b8ddf04-be28-44ec-ab2c-a63a34d1de13",
        "680a5388-28aa-44e8-88af-32e3598dc869",
        "6dcd6657-7a07-404a-b462-db76946f6a97",
        "7ac18a52-fb6c-4931-8d9d-ef5efff6acf0",
        "84b1a508-fc21-49da-979e-96894f1665df",
        "885cd4f5-dfa0-4d68-bbfd-00f89fc2b69c",
        "89515250-1243-43d1-b4e7-f9437cedffd8",
        "8ee30d6b-2c73-452a-b4ad-e4386cd6f7d0",
        "9ed827e8-2a1c-45f3-93f0-df6962034a33",
        "a205074f-8049-48b3-903f-556f5e530ae3",
        "a8fd63ce-4600-43eb-af33-a6d5481f5930",
        "dadc1876-61e3-42fc-a70e-e863bbc460b6",
        "db621e98-4a20-4942-b174-c455dc71dbae",
        "f0382960-6906-4b0d-add3-ed12690bff31",
    ];

    internal static string Build(RecommendationQueryScope scope, RecommendationFilters? filters)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = FilterRecords(scope, filters);
        query += KeepOneReservedInstanceScenario();
        query += ComputePerRecordValues();
        query += CollapseToOneRowPerResource();
        query += AggregatePerType();
        query += JoinTypeMetadata();
        query = RecommendationSummaryService.AddMetadataFilters(
            query,
            RecommendationSummaryService.GroupByRecommendationType,
            filters);
        query += OrderByPriority();
        return query + ProjectResultRows();
    }

    /// <summary>
    /// Stage 1: active recommendation records in the requested scope that match the instance-level filters.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Scope, current-engine (<c>strlen(name) == 64</c>), and service-group predicates are the same as in every
    /// other summary.</item>
    /// <item>Only <c>New</c> (active) records are ranked. Suppressed records are never <c>New</c>. Lifecycle questions
    /// such as "top dismissed" are answered by <c>recommendation list --status</c>.</item>
    /// <item>Tracked records are included. The Azure portal prioritized view excludes them, but they are still active
    /// recommendations that need action.</item>
    /// <item>Advisor configuration exclusions (excluded subscriptions or resource groups, low-CPU threshold) are not
    /// applied, consistent with every other Advisor tool in this server. The Azure portal applies them.</item>
    /// <item>Records without a criticality score or savings are kept; those values stay missing, never zero.</item>
    /// <item>Category, impact, subcategory, and retirement-date filters are applied after the metadata join because
    /// those properties come only from metadata.</item>
    /// </list>
    /// </remarks>
    private static string FilterRecords(RecommendationQueryScope scope, RecommendationFilters? filters)
    {
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true,
            scope: scope);

        return RecommendationSummaryService.BuildRecommendationScope(scope) +
            $" | where {predicates}" +
            " | extend recommendationTypeId = tolower(tostring(properties.recommendationTypeId))";
    }

    /// <summary>
    /// Stage 2: for reserved-instance recommendation types, keep only the three-year-term, seven-day-lookback scenario.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reserved-instance types publish one record per purchase scenario (term P1Y or P3Y × lookback period 7, 30, or
    /// 60 days) for the same resource. The scenarios are alternatives, so keeping all of them would count and sum the
    /// same purchase several times. The Azure portal Advisor prioritized view keeps P3Y with a seven-day lookback; this
    /// query does the same.
    /// </para>
    /// <para>
    /// Records that carry no scenario at all (no extended properties, or neither a term nor a lookback period) are kept,
    /// as in the portal. The portal also treats a legacy virtual-machine reserved-instance record without a lookback
    /// period as a 30-day lookback; with a seven-day lookback that rule never matches, so it is intentionally omitted.
    /// </para>
    /// <para>
    /// This stage applies only to prioritized summaries. Normal summaries count every record, as they always have.
    /// </para>
    /// </remarks>
    private static string KeepOneReservedInstanceScenario()
    {
        var typeIds = string.Join(", ", ReservedInstanceRecommendationTypeIds.Select(id => $"'{id}'"));
        return
            " | extend extendedProperties = properties.extendedProperties" +
            $" | where recommendationTypeId !in ({typeIds})" +
            $" or (tostring(extendedProperties.term) == '{ReservedInstanceTerm}'" +
            $" and tostring(extendedProperties.lookbackPeriod) == '{ReservedInstanceLookbackPeriod}')" +
            " or isnull(extendedProperties)" +
            " or (not(bag_has_key(extendedProperties, 'term')) and not(bag_has_key(extendedProperties, 'lookbackPeriod')))";
    }

    /// <summary>
    /// Stage 3: per-record resource key, criticality score, and annual savings.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>resourceKey</c> identifies the affected resource: the lowercased resource ID, or the record name when
    /// the resource ID is empty.</item>
    /// <item><c>criticalityScore</c> is the contextual score some recommendations carry; it is missing on others.</item>
    /// <item><c>annualSavings</c> is <c>savings.retail.dailyPotentialSavings × 12 × 30.4</c> when that value is a finite
    /// number. Otherwise it is the legacy <c>extendedProperties.annualSavingsAmount</c> when that is a finite number.
    /// Otherwise it is missing (null). Missing is never turned into 0, so "no data" cannot be read as "no savings".</item>
    /// </list>
    /// </remarks>
    private static string ComputePerRecordValues()
    {
        var monthsPerYear = MonthsPerYear.ToString(CultureInfo.InvariantCulture);
        var daysPerMonth = DaysPerMonth.ToString(CultureInfo.InvariantCulture);
        return
            " | extend resourceKey = tolower(tostring(properties.resourceMetadata.resourceId))" +
            " | extend resourceKey = iff(isempty(resourceKey), tolower(name), resourceKey)" +
            " | extend criticalityScore = todouble(properties.criticalityScore)" +
            " | extend dailySavings = todouble(properties.savings.retail.dailyPotentialSavings)" +
            " | extend legacyAnnualSavings = todouble(properties.extendedProperties.annualSavingsAmount)" +
            $" | extend annualSavings = iff(isfinite(dailySavings), dailySavings * {monthsPerYear} * {daysPerMonth}," +
            " iff(isfinite(legacyAnnualSavings), legacyAnnualSavings, real(null)))";
    }

    /// <summary>
    /// Stage 4: one row per (recommendation type, resource), so a resource is counted once per type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some types publish several records for the same resource, for example additional reserved-instance SKU or
    /// region records. Per resource, the highest criticality score and the highest annual savings are kept.
    /// </para>
    /// <para>
    /// Difference from the Azure portal: the portal takes an arbitrary record per resource (<c>any()</c>), which can
    /// change between runs. Taking the maximum makes the result reproducible.
    /// </para>
    /// <para>
    /// KQL aggregates ignore nulls. The <c>...Count</c> columns record whether any value existed, so a resource with no
    /// value stays missing instead of becoming 0.
    /// </para>
    /// </remarks>
    private static string CollapseToOneRowPerResource() =>
        " | summarize criticalityScore = max(criticalityScore)," +
        " criticalityScoreCount = countif(isnotnull(criticalityScore))," +
        " annualSavings = max(annualSavings)," +
        " annualSavingsCount = countif(isnotnull(annualSavings))" +
        " by recommendationTypeId, resourceKey" +
        " | extend criticalityScore = iff(criticalityScoreCount > 0, criticalityScore, real(null))," +
        " annualSavings = iff(annualSavingsCount > 0, annualSavings, real(null))";

    /// <summary>
    /// Stage 5: one row per recommendation type.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>resourceCount</c>: distinct affected resources. This is the group <c>count</c>. Normal summaries count
    /// records instead, so a list drill-down can return more records than this count.</item>
    /// <item><c>summedCriticality</c>: sum of per-resource criticality scores. Used only for ordering and never
    /// returned. Missing when no resource has a score.</item>
    /// <item><c>annualSavings</c>: sum of per-resource annual savings. Missing when no resource reports savings.</item>
    /// </list>
    /// </remarks>
    private static string AggregatePerType() =>
        " | summarize resourceCount = count()," +
        " summedCriticality = sum(criticalityScore)," +
        " criticalityScoreCount = countif(isnotnull(criticalityScore))," +
        " annualSavings = sum(annualSavings)," +
        " annualSavingsCount = countif(isnotnull(annualSavings))" +
        " by recommendationTypeId" +
        " | extend summedCriticality = iff(criticalityScoreCount > 0, summedCriticality, real(null))," +
        " annualSavings = iff(annualSavingsCount > 0, annualSavings, real(null))";

    /// <summary>
    /// Stage 6: type-level properties from English recommendation-type metadata.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Category, impact, subcategory, retirement date, priority score, and published signals come only from metadata;
    /// recommendation records are not used as a fallback. The join is inner, as in the Azure portal Advisor experience, so
    /// recommendation types without an English metadata row are excluded.
    /// </para>
    /// <para>
    /// The label is the metadata display name, then the metadata label, then the type ID.
    /// </para>
    /// </remarks>
    private static string JoinTypeMetadata() =>
        RecommendationSummaryService.JoinRecommendationTypeMetadata +
        " | extend category = metadataCategory," +
        " impact = metadataImpact," +
        " subCategory = metadataSubCategory," +
        " retirementDate = format_datetime(startofday(todatetime(metadataRetirementDate)), 'yyyy-MM-dd')" +
        " | extend label = iff(isnotempty(metadataDisplayName), metadataDisplayName," +
        " iff(isnotempty(metadataLabel), metadataLabel, recommendationTypeId))";

    /// <summary>
    /// Stage 8: the ranking. This single <c>order by</c> is the only place the order is defined.
    /// </summary>
    /// <remarks>
    /// Keys, in order of precedence:
    /// <list type="number">
    /// <item>Metadata priority score, highest first. This is the primary signal and already combines impact,
    /// subcategory or retirement urgency, resource-type criticality, critical service, effort, actionability, and cost
    /// implication.</item>
    /// <item>Annual savings, highest first. Only cost recommendations report savings, so this orders cost types among
    /// themselves. Every cost type has priority score 0, the same as security types; within that band, cost types with
    /// savings rank above security types. This matches the Azure portal ordering.</item>
    /// <item>Summed criticality score, highest first. This orders non-cost types that share a priority score.</item>
    /// <item>Retirement date, earliest first, so overdue retirements come before upcoming ones.</item>
    /// <item>Resource count: cost types with fewer affected resources first, all other types with more affected
    /// resources first. This follows the Azure portal ordering.</item>
    /// <item>Label, case-insensitive, then type ID, so that ties are broken deterministically.</item>
    /// </list>
    /// Missing values sort after present values for every key.
    /// </remarks>
    private static string OrderByPriority() =>
        $" | extend resourceCountSortKey = iff(category =~ '{CostCategory}', resourceCount, -resourceCount)," +
        " retirementSortKey = todatetime(retirementDate)," +
        " labelSortKey = tolower(label)" +
        " | order by metadataPriorityScore desc nulls last," +
        " annualSavings desc nulls last," +
        " summedCriticality desc nulls last," +
        " retirementSortKey asc nulls last," +
        " resourceCountSortKey asc," +
        " labelSortKey asc," +
        " recommendationTypeId asc";

    /// <summary>
    /// Stage 9: result columns. Priority score, summed criticality, and sort keys are deliberately not returned; the
    /// order of the rows carries the ranking.
    /// </summary>
    /// <remarks>
    /// Annual savings are rounded to cents here, after ordering, so the ranking uses the full-precision value while the
    /// response does not show floating-point artifacts such as 1598.5535999999997.
    /// </remarks>
    private static string ProjectResultRows() =>
        " | project key = recommendationTypeId," +
        " label," +
        " count_ = resourceCount," +
        " category," +
        " impact," +
        " subCategory," +
        " retirementDate," +
        $" annualSavings = round(annualSavings, {SavingsDecimals})," +
        " signalValues = metadataSignalValues";
}
