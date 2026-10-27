// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class RecommendationQueryBuilderTests
{
    [Fact]
    public void BuildInstancePredicates_ExactSelectionsReuseEscapingInTheSharedFilter()
    {
        var predicate = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Resources: ["Owner's-disk", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/disks/disk"]),
            includeStatus: true, useRequestedStatus: true, includeCategoryAndImpact: true, resourceTypeUsesImpactedField: false);

        Assert.Contains("tostring(properties.resourceMetadata.resourceId) in~", predicate);
        Assert.Contains("tostring(properties.impactedValue) in~", predicate);
        Assert.Contains(RecommendationQueryBuilder.EscapeKqlString("Owner's-disk"), predicate);
        Assert.DoesNotContain("top ", predicate);
    }

    [Theory]
    [InlineData("Cost", "IdleResources", true)]
    [InlineData(" cost ", " idleresources ", true)]
    [InlineData("Cost", null, false)]
    [InlineData(null, "IdleResources", false)]
    [InlineData("Cost", "ReservedInstances", false)]
    [InlineData("Security", "IdleResources", false)]
    public void IsIdleResourceCost_RequiresBothWorkflowFilters(string? category, string? subCategory, bool expected)
    {
        Assert.Equal(expected, RecommendationQueryBuilder.IsIdleResourceCost(
            new RecommendationFilters(Category: category, SubCategory: subCategory)));
        Assert.False(RecommendationQueryBuilder.IsIdleResourceCost(null));
    }

    [Fact]
    public void BuildSavingsValues_UsesRetailOnlyWithoutProviderOrBillingCurrency()
    {
        var query = RecommendationQueryBuilder.BuildSavingsValues();

        Assert.Contains("properties.savings.retail.dailyPotentialSavings", query);
        Assert.Contains("properties.savings.retail.estimatedAccumulatedSavings", query);
        Assert.Contains("properties.savings.retail.realizedAccumulatedSavings", query);
        Assert.Contains("legacyAnnualSavings", query);
        Assert.DoesNotContain("providerCurrency", query);
        Assert.DoesNotContain("billingCurrency", query);
        Assert.DoesNotContain("savings.contracted", query);
    }

    [Fact]
    public void BuildSavingsValues_PreservesCompletedCostWithoutAccruingOrRequiringDailyPricing()
    {
        var query = RecommendationQueryBuilder.BuildSavingsValues();

        Assert.Contains(
            "cost = iff(not(isOpen)," +
            " iff(isfinite(estimatedAccumulatedSavings), estimatedAccumulatedSavings, real(null))," +
            " iff(isfinite(dailySavings) and isfinite(estimatedAccumulatedSavings)" +
            " and isnotnull(latestSavingsEffectiveOn)," +
            " max_of(floor((asOf - latestSavingsEffectiveOn) / 1d, 1), 0.0) * dailySavings + estimatedAccumulatedSavings, real(null)))",
            query);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(5.5, false)]
    [InlineData(-7, false)]
    [InlineData(0, true)]
    [InlineData(5.5, true)]
    [InlineData(-7, true)]
    public void BuildSavingsQueries_PreserveTimestampPrecisionInUtc(double offsetHours, bool explicitTo)
    {
        var timestamp = new DateTimeOffset(2020, 9, 30, 23, 59, 59, TimeSpan.Zero)
            .AddTicks(1234567)
            .ToOffset(TimeSpan.FromHours(offsetHours));
        var filters = new RecommendationFilters(
            Category: "Cost",
            SavingsAsOf: timestamp,
            SavingsFrom: timestamp,
            SavingsTo: explicitTo ? timestamp : null);

        var values = RecommendationQueryBuilder.BuildSavingsValues(filters);
        var window = RecommendationQueryBuilder.BuildHistoricalCompletedFilter(filters);

        Assert.Contains("asOf = datetime(2020-09-30T23:59:59.1234567Z)", values);
        Assert.Contains("resourceDeletedOn) >= datetime(2020-09-30T23:59:59.1234567Z)", window);
        Assert.Contains("resourceDeletedOn) <= datetime(2020-09-30T23:59:59.1234567Z)", window);
    }

    [Fact]
    public void BuildInstancePredicates_SubscriptionRetainsPivotAndNameLengthRestriction()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null, includeStatus: true, useRequestedStatus: true,
            includeCategoryAndImpact: true, resourceTypeUsesImpactedField: false,
            scope: RecommendationQueryScope.ForSubscription("sub1", null));

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationNameClause} and " +
            $"{RecommendationQueryBuilder.ServiceGroupExclusionClause} and {RecommendationQueryBuilder.ActiveRecommendationClause}",
            result);
        Assert.Contains("strlen(name) == 64", result);
    }

    [Fact]
    public void BuildInstancePredicates_ServiceGroupRetainsCanonicalPivotAndNameLengthRestriction()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null, includeStatus: true, useRequestedStatus: true,
            includeCategoryAndImpact: true, resourceTypeUsesImpactedField: false,
            scope: RecommendationQueryScope.ForServiceGroup("sg1"));

        Assert.Contains(
            "isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/sg1'",
            result);
        Assert.Contains("strlen(name) == 64", result);
        Assert.DoesNotContain(RecommendationQueryBuilder.ServiceGroupExclusionClause, result);
    }

    [Fact]
    public void BuildInstancePredicates_NullFilters_ReturnsEngineAndActiveFilters()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            RecommendationQueryBuilder.ActiveRecommendationClause,
            result);
    }

    [Fact]
    public void BuildInstancePredicates_StatusDisabled_RetainsEngineFilters()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: false,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(RecommendationQueryBuilder.CurrentRecommendationEngineClause, result);
    }

    [Fact]
    public void BuildServiceGroupClause_NullScope_ExcludesServiceGroupProjections()
    {
        Assert.Equal(
            RecommendationQueryBuilder.ServiceGroupExclusionClause,
            RecommendationQueryBuilder.BuildServiceGroupClause(null));
    }

    [Fact]
    public void BuildServiceGroupClause_SubscriptionScope_ExcludesServiceGroupProjections()
    {
        var scope = RecommendationQueryScope.ForSubscription("sub1", null);

        Assert.Equal(
            RecommendationQueryBuilder.ServiceGroupExclusionClause,
            RecommendationQueryBuilder.BuildServiceGroupClause(scope));
    }

    [Fact]
    public void BuildSubscriptionClause_MultipleSubscriptionsUsesInPredicate()
    {
        var scope = RecommendationQueryScope.ForSubscriptions(
            ["sub1", "sub2", "SUB1"],
            null);

        Assert.Equal(
            "subscriptionId in~ ('sub1', 'sub2')",
            RecommendationQueryBuilder.BuildSubscriptionClause(scope));
    }

    [Fact]
    public void BuildInstancePredicates_ServiceGroupScope_MatchesCanonicalServiceGroupId()
    {
        var scope = RecommendationQueryScope.ForServiceGroup("sg1");

        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            scope: scope);

        Assert.Contains(
            $"isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '{RecommendationQueryBuilder.ServiceGroupResourceIdPrefix}sg1'",
            result);
        Assert.DoesNotContain(RecommendationQueryBuilder.ServiceGroupExclusionClause, result);
    }

    [Fact]
    public void BuildInstancePredicates_SummaryIgnoresRequestedStatus()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Status: RecommendationStatus.Completed),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            RecommendationQueryBuilder.ActiveRecommendationClause,
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ListUsesRequestedStatus()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Status: RecommendationStatus.Completed),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            "tostring(properties.recommendationStatus) =~ 'Completed'",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_CostOnlyFiltersRequireCostCategory()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RecommendationQueryBuilder.BuildInstancePredicates(
                new RecommendationFilters(
                    MinimumSavings: 100,
                    SavingsAsOf: DateTimeOffset.UtcNow),
                includeStatus: true,
                useRequestedStatus: false,
                includeCategoryAndImpact: false,
                resourceTypeUsesImpactedField: true));

        Assert.Contains("require --category Cost", exception.Message);
    }

    [Fact]
    public void BuildInstancePredicates_CostOnlyFiltersAllowCostCategory()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                Category: "Cost",
                MinimumSavings: 100,
                SavingsAsOf: DateTimeOffset.UtcNow),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true);

        Assert.Contains("dailyPotentialSavings", result);
    }

    [Fact]
    public void BuildInstancePredicates_ListUsesMultipleRecommendationTypeIds()
    {
        var firstTypeId = "1d70919c-1a4a-4f79-8300-bb576c291e9d";
        var secondTypeId = "2d70919c-1a4a-4f79-8300-bb576c291e9d";

        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(RecommendationTypeIds: [firstTypeId, secondTypeId]),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Contains(
            $"tostring(properties.recommendationTypeId) in~ ('{firstTypeId}', '{secondTypeId}')",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_UsesMultipleImpactedResourceTypes()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                ResourceTypes: ["Microsoft.Storage/storageAccounts", "Microsoft.Compute/virtualMachines"]),
            includeStatus: false,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true);

        Assert.Contains(
            "(tostring(properties.impactedField) =~ 'Microsoft.Storage/storageAccounts' or " +
            "tostring(properties.impactedField) =~ 'Microsoft.Compute/virtualMachines')",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_UsesMultipleResourceTypesInResourceIds()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                ResourceTypes: ["Microsoft.Storage/storageAccounts", "Microsoft.Compute/virtualMachines"]),
            includeStatus: false,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: false);

        Assert.Contains(
            "(tostring(properties.resourceMetadata.resourceId) contains 'Microsoft.Storage/storageAccounts' or " +
            "tostring(properties.resourceMetadata.resourceId) contains 'Microsoft.Compute/virtualMachines')",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ListUsesSingleRecommendationTypeIdCollection()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                RecommendationTypeIds: ["1d70919c-1a4a-4f79-8300-bb576c291e9d"]),
            includeStatus: false,
            useRequestedStatus: false,
            includeCategoryAndImpact: false,
            resourceTypeUsesImpactedField: true);

        Assert.Contains(
            "tostring(properties.recommendationTypeId) in~ ('1d70919c-1a4a-4f79-8300-bb576c291e9d')",
            result);
    }

    [Theory]
    [InlineData("properties.impactedField", "properties.resourceMetadata.resourceId")]
    [InlineData("impactedField", "resourceId")]
    public void BuildProductFamilyPredicate_ComputeUsesMetadataThenAppServiceFallback(
        string resourceTypeExpression,
        string resourceIdExpression)
    {
        var result = ProductFamilyFilter.BuildPredicate("Compute", resourceTypeExpression);

        Assert.Contains(
            "tolower(tostring(metadataServiceFamily)) =~ 'compute'",
            result);
        Assert.Contains(
            $"strcat(tolower(tostring({resourceTypeExpression})), '/') startswith 'microsoft.compute/'",
            result);
        Assert.Contains(
            $"strcat(tolower(tostring({resourceTypeExpression})), '/') startswith 'microsoft.web/serverfarms/'",
            result);
        Assert.Contains(
            $"tolower(tostring({resourceIdExpression})) contains '/providers/microsoft.web/serverfarms/'",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ListExcludesLegacyAndServiceGroupRecommendations()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.StartsWith(
            $"{RecommendationQueryBuilder.CurrentRecommendationNameClause} and " +
            $"{RecommendationQueryBuilder.ServiceGroupExclusionClause} and ",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ServiceGroup_UsesExactCanonicalScope()
    {
        var scope = RecommendationQueryScope.ForServiceGroup("service-group-test");

        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            scope: scope);

        Assert.StartsWith(
            $"{RecommendationQueryBuilder.CurrentRecommendationNameClause} and " +
            "isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ " +
            $"'{RecommendationQueryBuilder.ServiceGroupResourceIdPrefix}service-group-test' and ",
            result);
        Assert.DoesNotContain(RecommendationQueryBuilder.ServiceGroupExclusionClause, result);
    }

    [Theory]
    [InlineData(RecommendationStatus.New, "New")]
    [InlineData(RecommendationStatus.Postponed, "Postponed")]
    [InlineData(RecommendationStatus.Dismissed, "Dismissed")]
    [InlineData(RecommendationStatus.Completed, "Completed")]
    public void BuildInstancePredicates_ListUsesEverySupportedStatus(
        RecommendationStatus status,
        string expectedStatus)
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Status: status),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            $"tostring(properties.recommendationStatus) =~ '{expectedStatus}'",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_AllInstanceFilters_UsesExpectedSemantics()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                Category: "Security",
                Impact: "High",
                RecommendationTypeId: "1d70919c-1a4a-4f79-8300-bb576c291e9d",
                ResourceType: "Microsoft.Web/sites",
                Resource: "webapp",
                Search: "encrypt"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: true);

        Assert.Contains("properties.recommendationStatus", result);
        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, result);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, result);
        Assert.Contains("tostring(properties.category) =~ 'Security'", result);
        Assert.Contains("tostring(properties.impact) =~ 'High'", result);
        Assert.Contains("tostring(properties.recommendationTypeId) =~ '1d70919c-1a4a-4f79-8300-bb576c291e9d'", result);
        Assert.Contains("tostring(properties.impactedField) =~ 'Microsoft.Web/sites'", result);
        Assert.Contains("tostring(properties.resourceMetadata.resourceId) contains 'webapp'", result);
        Assert.Contains("tostring(properties.shortDescription.problem) contains 'encrypt'", result);
    }

    [Fact]
    public void BuildInstancePredicates_ListResourceType_PreservesResourceIdContains()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(ResourceType: "Microsoft.Web/sites"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Contains(
            "tostring(properties.resourceMetadata.resourceId) contains 'Microsoft.Web/sites'",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ResolvedMetadataIds_SkipsCategoryAndImpact()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Category: "Security", Impact: "High"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: ["Type-A", "Type-B"]);

        Assert.DoesNotContain("properties.category", result);
        Assert.DoesNotContain("properties.impact", result);
        Assert.Contains("tostring(properties.recommendationTypeId) in~ ('Type-A', 'Type-B')", result);
    }

    [Fact]
    public void BuildInstancePredicates_MetadataOnlyFilters_AddNoInstanceClauses()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                SubCategory: "ServiceUpgradeAndRetirement",
                TrackingIds: ["QNY1-HB8", "9G0V-_G8"],
                RetirementDateOperator: "ge",
                RetirementDate: new DateOnly(2026, 3, 31)),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            RecommendationQueryBuilder.ActiveRecommendationClause,
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ResolvedMetadataIds_IntersectsTypeId()
    {
        var typeId = "1d70919c-1a4a-4f79-8300-bb576c291e9d";
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(RecommendationTypeId: typeId),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: ["Type-A", typeId]);

        Assert.Contains(
            $"tostring(properties.recommendationTypeId) =~ '{typeId}'",
            result);
        Assert.Contains(
            $"tostring(properties.recommendationTypeId) in~ ('Type-A', '{typeId}')",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_EscapesKqlValuesWithoutChangingTheirMeaning()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Search: @"it's\unsafe|"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Contains(@"'it\'s\\unsafe|'", result);
    }

    [Theory]
    [InlineData("Owner's Group", @"Owner\'s Group")]
    [InlineData("Owner''s Group", @"Owner\'\'s Group")]
    [InlineData("Finance | Operations", "Finance | Operations")]
    [InlineData(@"a\'|b", @"a\\\'|b")]
    [InlineData("line\r\n\tend", @"line\r\n\tend")]
    [InlineData("", "")]
    public void EscapeKqlString_PreservesTheLiteralValue(string value, string escaped)
    {
        var result = RecommendationQueryBuilder.EscapeKqlString(value);

        Assert.Equal(escaped, result);
        Assert.Equal(value, System.Text.RegularExpressions.Regex.Unescape(result));
    }
}
