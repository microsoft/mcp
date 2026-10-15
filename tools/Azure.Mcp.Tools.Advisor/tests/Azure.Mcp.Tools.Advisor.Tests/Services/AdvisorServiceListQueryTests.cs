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
            prioritized: false,
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

        // priorityScore is projected only as an internal metadata sort column, never merged into properties.
        Assert.Contains("metadataPriorityScore = todouble(properties.priorityScore)", query);
        Assert.DoesNotContain("pack('priorityScore'", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_OverridesTheSameFieldsTheClientMergeDid()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            prioritized: false,
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
            prioritized: false,
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
        Assert.DoesNotContain("retirementDate", query);
        Assert.DoesNotContain("retirementFeatureName", query);
        Assert.DoesNotContain("coalesce(properties.extendedProperties", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_AssessmentAndPersonalizedInstancesAreReturnedUnchanged()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            prioritized: false,
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
            prioritized: false,
            limit: 25,
            language: Language);

        Assert.DoesNotContain("criticality", query);
        Assert.DoesNotContain("order by", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_Prioritized_RanksByPriorityThenCriticalityBeforeProjectionAndLimit()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            prioritized: true,
            limit: 10,
            language: Language);

        // Prioritized still returns only criticality-scored recommendations.
        Assert.Contains("isnotempty(tostring(properties.criticality))", query);
        Assert.Contains("isnotnull(properties.criticalityScore)", query);

        // Ranked by the type's metadata priority score, then criticality, then the type's display name, then id.
        Assert.Contains(
            "| order by metadataPriorityScore desc, todouble(properties.criticalityScore) desc, metadataDisplayName asc, id asc",
            query);
        Assert.DoesNotContain("metadataImpactRank", query);

        var orderIndex = query.IndexOf("order by", StringComparison.Ordinal);
        var projectIndex = query.IndexOf("| project id, name, type, properties", StringComparison.Ordinal);
        var limitIndex = query.IndexOf("| limit", StringComparison.Ordinal);

        // Ordering runs before the projection (so join-only sort keys survive) and before the limit.
        Assert.True(orderIndex < projectIndex, "Ordering must precede the projection so join-only sort keys remain available.");
        Assert.True(projectIndex < limitIndex, "Projection must precede the limit.");
        Assert.EndsWith("| limit 10", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_SubscriptionScope_AddsSubscriptionFilter()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            prioritized: false,
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
            prioritized: false,
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
            prioritized: false,
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
