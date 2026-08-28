// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>
/// Azure Resource Graph (KQL) queries used by the optimization tools. The curated cost-savings
/// query filters active, non-suppressed Advisor cost recommendations, normalizes savings across
/// currencies, and ranks by impact then annual savings.
/// </summary>
internal static class OptimizationKqlQueries
{
    /// <summary>Default recommendationTypeId for the underutilized-VM right-size recommendation.</summary>
    public const string DefaultRightSizeRecommendationTypeId = "e10b1381-5f0a-47ff-8c7b-37bd13d7c974";

    private static string EscapeKql(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    /// <summary>ARG query resolving a subscription id (and owning tenant) from a subscription name.</summary>
    public static string BuildSubscriptionIdByNameQuery(string subscriptionName) =>
        "resourcecontainers " +
        "| where type =~ 'microsoft.resources/subscriptions' " +
        $"| where name =~ '{EscapeKql(subscriptionName)}' " +
        "| project subscriptionId, tenantId, name";

    /// <summary>ARG query returning the alternative resize/SKU options for a compute resource.</summary>
    public static string BuildAlternativesQuery(string resourceId, string recommendationTypeId) =>
        "advisorresources " +
        "| where type =~ 'microsoft.advisor/recommendations' " +
        $"| where properties.resourceMetadata.resourceId =~ '{EscapeKql(resourceId)}' " +
        $"| where properties.recommendationTypeId == '{EscapeKql(recommendationTypeId)}' " +
        "| extend alternatives = parse_json(properties.extendedProperties.alternatives) " +
        "| project alternatives";

    /// <summary>ARG query returning a single Advisor recommendation for a resource and type.</summary>
    public static string BuildAdvisorRecommendationQuery(string resourceId, string recommendationTypeId) =>
        "advisorresources " +
        "| where type =~ 'microsoft.advisor/recommendations' " +
        $"| where properties.resourceMetadata.resourceId == '{EscapeKql(resourceId)}' " +
        $"| where properties.recommendationTypeId == '{EscapeKql(recommendationTypeId)}'";

    /// <summary>Curated top cost-savings ARG query. Subscription scoping is applied via the query content.</summary>
    public const string TopCostSavingsQuery = """
advisorresources
| where type =~ 'microsoft.advisor/recommendations'
| where isempty(properties.tracked) or properties.tracked == false
| extend termMatch = iff((isnotnull(properties.extendedProperties) and isnotempty(properties.extendedProperties.term) and properties.extendedProperties.term == 'P3Y'), true, false)
| extend lookbackPeriodMatch = iff((isnotnull(properties.extendedProperties) and isnotempty(properties.extendedProperties.lookbackPeriod) and properties.extendedProperties.lookbackPeriod == '30'), true, (properties.recommendationTypeId == '84b1a508-fc21-49da-979e-96894f1665df' and isempty(properties.extendedProperties.lookbackPeriod)))
| extend riFilterCondition = iff((termMatch == true and lookbackPeriodMatch == true), true, (isnull(properties.extendedProperties) or (not(bag_has_key(properties.extendedProperties, 'term')) and not(bag_has_key(properties.extendedProperties, 'lookbackPeriod')))))
| where iff(properties.recommendationTypeId in ('0169a2e1-c7bf-4c37-90b8-0714811c82d3', '06ad499a-0952-48d3-b061-ec81c9cabb8b', '0d524e8d-4cfd-4db5-9f91-8b4bb5235a8e', '0eb54047-acd9-4f26-8ffb-8cec713782d6', '10aedd06-621e-4b4f-a45c-5256573e0191', '148cdd60-97e8-426b-a7b9-141b7cb4bc2f', '171f87ad-4ead-42fc-8f32-a3b18d451837', '1b8c5187-32a6-4a2f-8ca1-b0b7d6ce9e86', '32755df6-aa2f-48d7-9ab7-92b8a80352ea', '3327646a-c325-417f-a3e3-36ae7119da69', '3f6c5689-6a05-4896-a6e0-c6f8a22a44c2', '407b6ad6-8e0b-40e7-9384-643520cae0ed', '5b8ddf04-be28-44ec-ab2c-a63a34d1de13', '680a5388-28aa-44e8-88af-32e3598dc869', '6dcd6657-7a07-404a-b462-db76946f6a97', '84b1a508-fc21-49da-979e-96894f1665df', '885cd4f5-dfa0-4d68-bbfd-00f89fc2b69c', '8ee30d6b-2c73-452a-b4ad-e4386cd6f7d0', 'a205074f-8049-48b3-903f-556f5e530ae3', 'a8fd63ce-4600-43eb-af33-a6d5481f5930', 'db621e98-4a20-4942-b174-c455dc71dbae', 'f0382960-6906-4b0d-add3-ed12690bff31', '89515250-1243-43d1-b4e7-f9437cedffd8'), riFilterCondition, true)
| project id, stableId = name, subscriptionId, resourceGroup, properties, tenantId
| join kind=leftouter (
    advisorresources
    | where type =~ 'microsoft.advisor/suppressions'
    | extend tokens = split(id, '/')
    | extend stableId = iff(array_length(tokens) > 3, tokens[array_length(tokens) - 3], '')
    | extend expirationTimeStamp = todatetime(iff(strcmp(tostring(properties.ttl), '-1') == 0, '9999-12-31', properties.expirationTimeStamp))
    | where expirationTimeStamp > now()
    | project suppressionId = tostring(properties.suppressionId), stableId, expirationTimeStamp
) on stableId
| project id, stableId, subscriptionId, resourceGroup, properties, expirationTimeStamp, suppressionId, tenantId
| join kind=leftouter (
    advisorresources
    | where type =~ 'microsoft.advisor/configurations'
    | where isempty(resourceGroup)
    | project subscriptionId, excludeRecomm = properties.exclude, lowCpuThreshold = properties.lowCpuThreshold
) on subscriptionId
| extend isActive1 = iff(isempty(excludeRecomm), true, tobool(excludeRecomm) == false)
| extend isActive2 = iff(
    properties.recommendationTypeId in ('e10b1381-5f0a-47ff-8c7b-37bd13d7c974', '94aea435-ef39-493f-a547-8408092c22a7'),
    iff(
        isnotempty(lowCpuThreshold) and isnotnull(properties.extendedProperties) and isnotempty(properties.extendedProperties.MaxCpuP95),
        todouble(properties.extendedProperties.MaxCpuP95) < todouble(lowCpuThreshold),
        iff(isnull(properties.extendedProperties) or isempty(properties.extendedProperties.MaxCpuP95) or todouble(properties.extendedProperties.MaxCpuP95) < 100, true, false)
    ),
    true
)
| where isActive1 and isActive2
| join kind=leftouter (
    advisorresources
    | where type =~ 'microsoft.advisor/configurations'
    | where isnotempty(resourceGroup)
    | project subscriptionId, resourceGroup, excludeProperty = properties.exclude
) on subscriptionId, resourceGroup
| extend isActive3 = iff(isempty(excludeProperty), true, tobool(excludeProperty) == false)
| where isActive3
| summarize expirationTimeStamp = max(expirationTimeStamp), suppressionIds = make_list(suppressionId)
    by id, stableId, subscriptionId, resourceGroup, tostring(properties), tenantId
| extend properties = parse_json(properties)
| extend
    recommendationTypeId = tostring(properties.recommendationTypeId),
    resourceType = tostring(properties.impactedField),
    category = tostring(properties.category),
    impact = tolower(tostring(properties.impact)),
    resourceId = tolower(substring(id, 0, strlen(id) - 81)),
    description = tostring(properties.shortDescription.solution),
    lastUpdate = tostring(properties.lastUpdated),
    isRecommendationActive = isnull(expirationTimeStamp) or isempty(expirationTimeStamp),
    extendedProperties = properties.extendedProperties
| extend
    recommendationSubcategory = tostring(extendedProperties.recommendationSubCategory),
    savingsAmount = toreal(extendedProperties.savingsAmount),
    impactedField = tolower(tostring(properties.impactedField)),
    impactedValue = tolower(tostring(properties.impactedValue)),
    recommendationMessage = tostring(properties.shortDescription.solution),
    recommendationMessageDetailed = tostring(properties.extendedProperties.recommendationMessage),
    recommendationTypeSubCategory = tostring(properties.extendedProperties.recommendationType),
    solution = tostring(properties.shortDescription.solution),
    annualSavingsAmount = toreal(extendedProperties.annualSavingsAmount),
    savingsCurrency = tostring(extendedProperties.savingsCurrency),
    PotentialMonthlyCarbonSavings = todouble(coalesce(extendedProperties.PotentialMonthlyCarbonSavings, extendedProperties.potentialMonthlyCarbonSavings)),
    descriptionOfChanges = tostring(extendedProperties.descriptionOfChanges),
    recommendationCostImplication = tostring(extendedProperties.recommendationCostImplication)
| where isRecommendationActive
| where category == 'Cost'
| extend
    savingsCurrencyForRanking = coalesce(savingsCurrency, 'USD'),
    annualSavingsAmountForRanking = coalesce(annualSavingsAmount, 0.0),
    savingsAmountForRanking = coalesce(savingsAmount, 0.0)
| extend impact_score = case(
    impact == 'high', 10,
    impact == 'medium', 5,
    impact == 'low', 0,
    0
)
| join kind=leftouter (
    datatable(savingsCurrencyForRanking: string, conversionRate: real) [
        'AUD', 0.650801,
        'BRL', 0.183611,
        'CAD', 0.725468,
        'CHF', 1.232450,
        'CNY', 0.139186,
        'DKK', 0.155726,
        'EUR', 1.162200,
        'GBP', 1.342975,
        'INR', 0.011395,
        'JPY', 0.006761,
        'KRW', 0.000718,
        'NOK', 0.097595,
        'NZD', 0.592695,
        'RUB', 0.012500,
        'SEK', 0.103939,
        'TWD', 0.033800,
        'USD', 1.000000
    ]
) on savingsCurrencyForRanking
| extend conversionRate = coalesce(conversionRate, 0.0)
| extend
    annualSavingsAmountForRanking = annualSavingsAmountForRanking * conversionRate,
    savingsAmountForRanking = savingsAmountForRanking * conversionRate
| order by impact_score desc, annualSavingsAmountForRanking desc
| project
    id = tolower(id),
    name = stableId,
    tenantId,
    resourceGroup,
    subscriptionId,
    recommendationTypeId,
    savingsCurrency,
    annualSavingsAmount = round(annualSavingsAmount),
    savingsAmount = round(savingsAmount),
    monthlyCarbonSavings = round(PotentialMonthlyCarbonSavings, 2),
    recommendationMessage,
    recommendationMessageDetailed,
    recommendationTypeSubCategory,
    solution,
    impactedField,
    impactedValue,
    impact,
    resourceId
""";
}
