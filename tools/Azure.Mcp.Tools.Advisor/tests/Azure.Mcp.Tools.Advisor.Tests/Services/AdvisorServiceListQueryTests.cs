// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

// Focused unit tests for the list-specific query wrapper, its server-side metadata join, and prioritized ordering.
public class AdvisorServiceListQueryTests
{
    private const string Language = "en";

    private static RecommendationQueryScope SubscriptionScope(string? resourceGroup = null) =>
        RecommendationQueryScope.ForSubscription("sub-123", resourceGroup);

    private static RecommendationQueryScope ServiceGroupScope() =>
        RecommendationQueryScope.ForServiceGroup("commerce");

    [Theory]
    [InlineData("category", "tostring(properties.recommendationCategory) =~ 'HighAvailability'")]
    [InlineData("impact", "tostring(properties.recommendationImpact) =~ 'High'")]
    [InlineData("resourceType", "tostring(properties.supportedResourceType) =~ 'Microsoft.Storage/storageAccounts'")]
    [InlineData("subCategory", "tostring(properties.recommendationSubCategory) =~ 'ZoneResiliency'")]
    [InlineData("trackingIds", "tostring(trackingId) in~ ('QNY1-''HB8', '9G0V-_G8')")]
    [InlineData("retirementDate", "startofday(todatetime(properties.sourceProperties.serviceRetirement.retirementDate)) <= datetime(2026-03-31)")]
    public void BuildListQueries_MetadataFiltersUseInnerJoin(string filter, string expectedPredicate)
    {
        var filters = filter switch
        {
            "category" => new RecommendationFilters(Category: " HighAvailability "),
            "impact" => new RecommendationFilters(Impact: " High "),
            "resourceType" => new RecommendationFilters(ResourceType: " Microsoft.Storage/storageAccounts "),
            "subCategory" => new RecommendationFilters(SubCategory: " ZoneResiliency "),
            "trackingIds" => new RecommendationFilters(TrackingIds: [" QNY1-'HB8 ", "qny1-'hb8", "", "9G0V-_G8"]),
            "retirementDate" => new RecommendationFilters(RetirementDateOperator: "le", RetirementDate: new DateOnly(2026, 3, 31)),
            _ => throw new ArgumentException("Unknown filter.", nameof(filter)),
        };

        foreach (var scope in new[] { SubscriptionScope(), ServiceGroupScope() })
        {
            foreach (var prioritized in new[] { false, true })
            {
                var query = prioritized
                    ? AdvisorService.BuildPrioritizedRecommendationListQuery(scope, null, 51, filters)
                    : AdvisorService.BuildRecommendationListQuery(scope, null, 51, Language, filters);
                var metadataBranch = query[query.IndexOf("| join kind=inner (", StringComparison.Ordinal)..];
                metadataBranch = metadataBranch[..metadataBranch.IndexOf(") on joinTypeId", StringComparison.Ordinal)];

                Assert.Contains(expectedPredicate, metadataBranch);
                Assert.Contains("tostring(properties.language) =~ 'en'", metadataBranch);
                Assert.DoesNotContain("arg_max", metadataBranch);
                Assert.DoesNotContain("lastRefreshed", metadataBranch, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("tostring(properties.recommendationTypeId) in~", query);
                Assert.Equal(filter == "trackingIds", metadataBranch.Contains("| distinct *", StringComparison.Ordinal));
                if (filter is "trackingIds" or "retirementDate")
                {
                    Assert.Contains("tostring(properties.recommendationSubCategory) =~ 'ServiceUpgradeAndRetirement'", metadataBranch);
                }
                if (filter == "trackingIds")
                {
                    Assert.Contains("| mv-expand trackingId = properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds", metadataBranch);
                    Assert.True(metadataBranch.IndexOf("| distinct *", StringComparison.Ordinal) >
                        metadataBranch.IndexOf("| project joinTypeId,", StringComparison.Ordinal));
                }
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BuildListQueries_InstanceOnlyFiltersKeepLeftJoin(bool prioritized)
    {
        var filters = new RecommendationFilters(Resource: "storage", Search: "encrypt", Status: RecommendationStatus.Completed);
        var query = prioritized
            ? AdvisorService.BuildPrioritizedRecommendationListQuery(ServiceGroupScope(), null, 51, filters)
            : AdvisorService.BuildRecommendationListQuery(SubscriptionScope(), null, 51, Language, filters);

        Assert.Contains("| join kind=leftouter (", query);
        Assert.DoesNotContain("| distinct", query);
        Assert.DoesNotContain("| mv-expand", query);
    }

    [Fact]
    public void IsPrioritized_ReflectsFilterFlag()
    {
        Assert.True(AdvisorService.IsPrioritized(new RecommendationFilters(Prioritized: true)));
        Assert.False(AdvisorService.IsPrioritized(new RecommendationFilters(Prioritized: false)));
        Assert.False(AdvisorService.IsPrioritized(new RecommendationFilters()));
        Assert.False(AdvisorService.IsPrioritized(null));
    }

    [Fact]
    public void BuildRecommendationListQuery_JoinsMetadataAndRewritesPropertiesServerSide()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            limit: 25,
            language: Language);

        Assert.StartsWith("advisorresources | where type =~ 'Microsoft.Advisor/recommendations'", query);
        Assert.Contains("and strlen(name) == 64", query);

        // The metadata catalog is joined server-side rather than merged on the client.
        Assert.Contains("join kind=leftouter", query);
        Assert.Contains("where type =~ 'microsoft.advisor/metadata'", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        Assert.Contains("on joinTypeId", query);

        // Properties are rewritten in place with the metadata overrides, except for Assessment and personalized instances.
        Assert.Contains(
            "extend properties = iff(keepInstanceProperties, properties, bag_merge(metadataOverrides, properties))",
            query);
        Assert.Contains("| project id, name, type, properties", query);
        Assert.EndsWith("| limit 25", query);

        Assert.DoesNotContain("metadataPriorityScore", query);
        Assert.DoesNotContain("pack('priorityScore'", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_OverridesTheSameFieldsTheClientMergeDid()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            limit: 50,
            language: Language);

        Assert.Contains("pack('category', metadataCategory)", query);
        Assert.Contains("pack('impact', metadataImpact)", query);
        Assert.Contains("pack('description', metadataDescription)", query);
        Assert.Contains("pack('learnMoreLink', metadataLearnMoreLink)", query);
        Assert.Contains("pack('potentialBenefits', metadataPotentialBenefits)", query);
        Assert.Contains("pack('label', metadataLabel)", query);
        Assert.Contains("pack('shortDescription', pack('problem', metadataDisplayName, 'solution', metadataDisplayName))", query);
        Assert.Contains("bag_merge(metadataOverrides, properties)", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_OnlyReplacesAnExistingSubCategoryInExtendedProperties()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            limit: 50,
            language: Language);

        // extendedProperties is only rewritten when the instance already has recommendationSubCategory,
        // so no new keys are ever added to it.
        Assert.Contains(
            "| extend updateSubCategory = isnotempty(metadataSubCategory) and isnotnull(properties.extendedProperties.recommendationSubCategory)",
            query);
        Assert.Contains(
            "iff(updateSubCategory, pack('extendedProperties', bag_merge(pack('recommendationSubCategory', metadataSubCategory), properties.extendedProperties)), dynamic({}))",
            query);
        Assert.DoesNotContain("pack('retirementDate'", query);
        Assert.DoesNotContain("retirementFeatureName", query);
        Assert.DoesNotContain("coalesce(properties.extendedProperties", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_AssessmentAndPersonalizedInstancesAreReturnedUnchanged()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            limit: 50,
            language: Language);

        // Assessment instances and personalized instances (the Personalized recommendation type) keep their original
        // properties with no metadata applied. Review instances of automated types are still enriched from metadata,
        // so a non-empty sourceSystem alone does not skip enrichment.
        Assert.Contains(
            "| extend keepInstanceProperties = tostring(properties.sourceSystem) =~ 'Assessment' or joinTypeId == '6d732ac5-82e0-4a66-887e-eccee79a2063'",
            query);
        Assert.Contains(
            "| extend properties = iff(keepInstanceProperties, properties, bag_merge(metadataOverrides, properties))",
            query);
        Assert.DoesNotContain("isnotempty(tostring(properties.sourceSystem))", query);
        Assert.DoesNotContain("preferInstanceValues", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_NotPrioritized_HasNoCriticalityClausesOrOrder()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            limit: 25,
            language: Language);

        Assert.DoesNotContain("criticality", query);
        Assert.DoesNotContain("order by", query);
        Assert.DoesNotContain("suppressionIds", query);
        Assert.DoesNotContain("properties.tracked", query);
        Assert.DoesNotContain("!~ 'Security'", query);
        Assert.DoesNotContain("arg_max", query);
        Assert.DoesNotContain("metadataRetirementDate", query);
    }

    [Fact]
    public void BuildPrioritizedRecommendationListQuery_FiltersThenSortsThenLimits()
    {
        var query = AdvisorService.BuildPrioritizedRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64 and isempty(properties.serviceGroupId) and tostring(properties.recommendationStatus) =~ 'Completed'",
            limit: 51);

        Assert.Contains("tostring(properties.recommendationStatus) =~ 'Completed'", query);
        Assert.DoesNotContain("'New'", query);
        Assert.DoesNotContain("suppressionIds", query);
        Assert.DoesNotContain("properties.tracked", query);
        Assert.Contains("isempty(properties.serviceGroupId)", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        Assert.DoesNotContain("isempty(properties.language)", query);
        Assert.DoesNotContain("startswith 'en-'", query);
        Assert.DoesNotContain("arg_max", query);
        Assert.DoesNotContain("lastRefreshed", query, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("| take 51 | project id, name, type, properties", query);
        Assert.DoesNotContain("metadataPriorityScore", query);
        Assert.Contains("join kind=leftouter", query);
        Assert.Contains(
            "metadataRetirementDate = todatetime(properties.sourceProperties.serviceRetirement.retirementDate)",
            query);
        Assert.DoesNotContain("!~ 'Security'", query);
        Assert.DoesNotContain("isnotempty(tostring(properties.criticality))", query);
        Assert.DoesNotContain("isnotnull(properties.criticalityScore)", query);
        Assert.Contains("strlen(name) == 64", query);
        Assert.Contains("instanceScore = coalesce(todouble(properties.criticalityScore), 0.0)", query);
        Assert.Contains("retirementDate = coalesce(todatetime(properties.extendedProperties.retirementDate), metadataRetirementDate)", query);
        Assert.Contains("resourceNameSort = tolower(tostring(properties.impactedValue))", query);
        Assert.Contains("| order by instanceScore desc nulls last, retirementDate asc nulls last, resourceNameSort asc | take", query);
        Assert.DoesNotContain(", id asc", query);
        Assert.DoesNotContain("summarize", query);
        Assert.True(query.IndexOf("bag_merge(metadataOverrides, properties)", StringComparison.Ordinal) <
            query.IndexOf("| order by", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Cost")]
    [InlineData("HighAvailability")]
    [InlineData("Performance")]
    [InlineData("OperationalExcellence")]
    public void BuildPrioritizedRecommendationListQuery_DoesNotFilterOnScoringInputs(string category)
    {
        var query = AdvisorService.BuildPrioritizedRecommendationListQuery(
            SubscriptionScope(),
            predicates: $"tostring(properties.category) =~ '{category}'",
            limit: 11);

        Assert.DoesNotContain("isnotempty(tostring(properties.criticality))", query);
        Assert.DoesNotContain("isnotnull(properties.criticalityScore)", query);
        Assert.DoesNotContain("!~ 'Security'", query);
        Assert.DoesNotContain("Savings", query);
        Assert.Contains($"tostring(properties.category) =~ '{category}'", query);
    }

    [Fact]
    public void BuildPrioritizedRecommendationListQuery_Security_IsRetained()
    {
        var query = AdvisorService.BuildPrioritizedRecommendationListQuery(
            SubscriptionScope(),
            predicates: "tostring(properties.category) =~ 'Security'",
            limit: 11);

        Assert.DoesNotContain("!~ 'Security'", query);
        Assert.Contains("tostring(properties.category) =~ 'Security'", query);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("HighAvailability", false)]
    [InlineData("Security", false)]
    [InlineData("Cost", true)]
    [InlineData(" cost ", true)]
    [InlineData("COST", true)]
    public void BuildPrioritizedRecommendationListQuery_OnlyExplicitCostSelectsFiniteSavings(string? category, bool useSavings)
    {
        foreach (var scope in new[] { SubscriptionScope(), ServiceGroupScope() })
        {
            var query = AdvisorService.BuildPrioritizedRecommendationListQuery(
                scope, null, 101, new RecommendationFilters(Category: category, RecommendationTypeId: "cost-type"));

            Assert.Equal(useSavings, query.Contains("dailySavings =", StringComparison.Ordinal));
            Assert.Equal(!useSavings, query.Contains("coalesce(todouble(properties.criticalityScore), 0.0)", StringComparison.Ordinal));
            if (useSavings)
            {
                Assert.Contains("dailySavings = todouble(tostring(properties.savings.retail.dailyPotentialSavings))", query);
                Assert.Contains("annualSavings = todouble(tostring(properties.extendedProperties.annualSavingsAmount))", query);
                Assert.Contains("coalesce(iff(isfinite(dailySavings), dailySavings, real(null)), iff(isfinite(annualSavings), annualSavings, real(null)))", query);
                Assert.DoesNotContain("dailySavings > 0", query);
            }
            Assert.EndsWith("| take 101 | project id, name, type, properties", query);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(51)]
    [InlineData(101)]
    public void BuildPrioritizedRecommendationListQuery_OutputOptionsDoNotChangeQuery(int limit)
    {
        var filters = new RecommendationFilters(Prioritized: true, ShowPrioritizationSignals: false);
        var query = AdvisorService.BuildPrioritizedRecommendationListQuery(SubscriptionScope(), null, limit, filters);

        Assert.Equal(query, AdvisorService.BuildPrioritizedRecommendationListQuery(
            SubscriptionScope(), null, limit, filters with { ShowPrioritizationSignals = true }));
        Assert.EndsWith($"| take {limit} | project id, name, type, properties", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_SubscriptionScope_AddsSubscriptionFilter()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            limit: 50,
            language: Language);

        Assert.Contains("subscriptionId =~ 'sub-123'", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_WithResourceGroup_AddsEscapedFilter()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope("rg'inject"),
            predicates: "strlen(name) == 64",
            limit: 50,
            language: Language);

        Assert.Contains("resourceGroup =~ 'rg''inject'", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_ServiceGroupScope_OmitsSubscriptionAndResourceGroupFilters()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            ServiceGroupScope(),
            predicates: "strlen(name) == 64",
            limit: 50,
            language: Language);

        Assert.DoesNotContain("subscriptionId =~", query);
        Assert.DoesNotContain("resourceGroup =~", query);
        Assert.Contains("join kind=leftouter", query);
    }

    [Fact]
    public void ParseRecommendationListResult_NoTruncationSignals_IsNotTruncated()
    {
        var result = AdvisorService.ParseRecommendationListResult(
            BinaryData.FromString(CreateRecommendationPayload("first")),
            limit: 1,
            isTruncated: false,
            skipToken: null);

        Assert.False(result.AreResultsTruncated);
        Assert.Equal("first", Assert.Single(result.Results).Name);
    }

    [Fact]
    public void ParseRecommendationListResult_FewerRowsThanLimit_IsNotTruncated()
    {
        var result = AdvisorService.ParseRecommendationListResult(
            BinaryData.FromString(CreateRecommendationPayload("first")),
            limit: 2,
            isTruncated: false,
            skipToken: null);

        Assert.False(result.AreResultsTruncated);
        Assert.Equal("first", Assert.Single(result.Results).Name);
    }

    [Fact]
    public void ParseRecommendationListResult_ExtraRowBeyondLimit_IsTrimmedAndTruncated()
    {
        var result = AdvisorService.ParseRecommendationListResult(
            BinaryData.FromString($"[{CreateRecommendation("first")},{CreateRecommendation("second")}]"),
            limit: 1,
            isTruncated: false,
            skipToken: null);

        Assert.True(result.AreResultsTruncated);
        Assert.Equal("first", Assert.Single(result.Results).Name);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "next-page")]
    public void ParseRecommendationListResult_TruncationSignal_IsTruncated(
        bool isTruncated,
        string? skipToken)
    {
        var result = AdvisorService.ParseRecommendationListResult(
            BinaryData.FromString(CreateRecommendationPayload("first")),
            limit: 1,
            isTruncated,
            skipToken);

        Assert.True(result.AreResultsTruncated);
        Assert.Equal("first", Assert.Single(result.Results).Name);
    }

    private static string CreateRecommendationPayload(string name) => $"[{CreateRecommendation(name)}]";

    private static string CreateRecommendation(string name) =>
        $$"""
        {
          "id": "/subscriptions/sub-123/providers/Microsoft.Advisor/recommendations/{{name}}",
          "name": "{{name}}",
          "type": "Microsoft.Advisor/recommendations",
          "properties": {}
        }
        """;
}
