// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

// Focused unit tests for the list-specific query wrapper and prioritized ordering.
public class AdvisorServiceListQueryTests
{
    [Fact]
    public void IsPrioritized_ReflectsFilterFlag()
    {
        Assert.True(AdvisorService.IsPrioritized(new RecommendationFilters(Prioritized: true)));
        Assert.False(AdvisorService.IsPrioritized(new RecommendationFilters(Prioritized: false)));
        Assert.False(AdvisorService.IsPrioritized(new RecommendationFilters()));
        Assert.False(AdvisorService.IsPrioritized(null));
    }

    [Fact]
    public void BuildRecommendationListQuery_NotPrioritized_HasNoCriticalityClausesOrOrder()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            resourceGroup: null,
            predicates: "strlen(name) == 64",
            prioritized: false,
            limit: 25);

        Assert.StartsWith("advisorresources | where type =~ 'Microsoft.Advisor/recommendations'", query);
        Assert.Contains("and strlen(name) == 64", query);
        Assert.DoesNotContain("criticality", query);
        Assert.DoesNotContain("order by", query);
        Assert.EndsWith("| limit 25", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_Prioritized_AddsCriticalityFilterAndOrdersBeforeLimit()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            resourceGroup: null,
            predicates: "strlen(name) == 64",
            prioritized: true,
            limit: 10);

        Assert.Contains("isnotempty(tostring(properties.criticality))", query);
        Assert.Contains("isnotnull(properties.criticalityScore)", query);
        Assert.Contains("| order by todouble(properties.criticalityScore) desc", query);

        var orderIndex = query.IndexOf("order by", System.StringComparison.Ordinal);
        var limitIndex = query.IndexOf("| limit", System.StringComparison.Ordinal);
        Assert.True(orderIndex < limitIndex, "Ordering must be applied before the result set is capped.");
    }

    [Fact]
    public void BuildRecommendationListQuery_WithResourceGroup_AddsEscapedFilter()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            resourceGroup: "rg'inject",
            predicates: "strlen(name) == 64",
            prioritized: false,
            limit: 50);

        Assert.Contains("resourceGroup =~ 'rg''inject'", query);
    }

    [Fact]
    public void BuildRecommendationListQuery_NullResourceGroup_OmitsResourceGroupFilter()
    {
        var query = AdvisorService.BuildRecommendationListQuery(
            resourceGroup: null,
            predicates: "strlen(name) == 64",
            prioritized: false,
            limit: 50);

        Assert.DoesNotContain("resourceGroup", query);
    }
}
