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

        // Properties are rewritten in place with the metadata overrides.
        Assert.Contains("extend properties = bag_merge(metadataOverrides, properties)", query);
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
        Assert.Contains("pack('label', mergedLabel)", query);
        Assert.Contains("pack('shortDescription', pack('problem', metadataDisplayName, 'solution', metadataDisplayName))", query);
        Assert.Contains("pack('extendedProperties', mergedExtendedProperties)", query);
        Assert.Contains("pack('recommendationSubCategory', metadataSubCategory)", query);
        Assert.Contains("pack('retirementDate', metadataRetirementDate)", query);
        Assert.Contains("pack('retirementFeatureName', metadataRetirementFeatureName)", query);
        Assert.Contains(
            "pack('retirementFeatureName', metadataRetirementFeatureName), dynamic({})), coalesce(properties.extendedProperties, dynamic({})))",
            query);
        Assert.Contains("bag_merge(metadataOverrides, properties)", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_LabelPrefersInstanceWhenSourceSystemPresent()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            SubscriptionScope(),
            predicates: "strlen(name) == 64",
            prioritized: false,
            limit: 50,
            language: Language);

        Assert.Contains("iff(isempty(instanceSourceSystem)", query);
        Assert.Contains("iff(isnotempty(metadataLabel), metadataLabel, instanceLabel)", query);
        Assert.Contains("iff(isnotempty(instanceLabel), instanceLabel, metadataLabel)", query);
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

        // Ranked by the type's metadata priority score, then business impact, kept contiguous per type, then criticality, then id.
        Assert.Contains(
            "metadataImpactRank = case(tolower(metadataImpact) == 'high', 0, tolower(metadataImpact) == 'medium', 1, tolower(metadataImpact) == 'low', 2, 3)",
            query);
        Assert.Contains(
            "| order by metadataPriorityScore desc, metadataImpactRank asc, joinTypeId asc, todouble(properties.criticalityScore) desc, id asc",
            query);

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
            isTruncated: false,
            skipToken: null);

        Assert.False(result.AreResultsTruncated);
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
