// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceSummarizeQueryTests
{
    [Theory]
    [InlineData(RecommendationGroupBy.Category)]
    [InlineData(RecommendationGroupBy.Impact)]
    [InlineData(RecommendationGroupBy.RecommendationType)]
    [InlineData(RecommendationGroupBy.ResourceType)]
    public void BuildSummarizeQuery_AllGroupByValues_ProduceValidQuery(RecommendationGroupBy groupBy)
    {
        var query = AdvisorService.BuildSummarizeQuery(groupBy, null, null);

        Assert.StartsWith("advisorresources | where type =~ 'Microsoft.Advisor/recommendations'", query);
        Assert.Contains("| summarize count() by key=", query);
        // 'Unknown' buckets are pushed to the end regardless of count.
        Assert.Contains("| order by iff(key == 'Unknown', 1, 0) asc, count_ desc, key asc", query);
    }

    [Fact]
    public void BuildSummarizeQuery_Category_UsesCorrectField()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.Category, null, null);
        Assert.Contains("properties.category", query);
    }

    [Fact]
    public void BuildSummarizeQuery_Impact_UsesCorrectField()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.Impact, null, null);
        Assert.Contains("properties.impact", query);
    }

    [Fact]
    public void BuildSummarizeQuery_RecommendationType_UsesCorrectField()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.RecommendationType, null, null);
        Assert.Contains("properties.shortDescription.problem", query);
    }

    [Fact]
    public void BuildSummarizeQuery_ResourceType_UsesExtractOnResourceId()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.ResourceType, null, null);
        Assert.Contains("extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId))", query);
    }

    [Fact]
    public void BuildSummarizeQuery_WithResourceGroup_AddsFilter()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.Category, "myRg", null);
        Assert.Contains("resourceGroup =~ 'myRg'", query);
    }

    [Fact]
    public void BuildSummarizeQuery_WithFilters_AddsFilterClauses()
    {
        var filters = new RecommendationFilters(Category: AdvisorCategory.Security, Impact: AdvisorImpact.High);
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.Category, null, filters);

        Assert.Contains("properties.category", query);
        Assert.Contains("'Security'", query);
        Assert.Contains("properties.impact", query);
        Assert.Contains("'High'", query);
    }

    [Fact]
    public void BuildSummarizeQuery_NoFilters_StillRestrictsToActiveRecommendations()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.Category, null, null);

        // Even with no user filters, the query always restricts to active ('New') recommendations.
        Assert.Contains("properties.recommendationStatus", query);
        Assert.Contains("'New'", query);
    }

    [Fact]
    public void BuildSummarizeQuery_UnsupportedGroupBy_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AdvisorService.BuildSummarizeQuery((RecommendationGroupBy)999, null, null));
    }

    [Theory]
    [InlineData(RecommendationGroupBy.Category)]
    [InlineData(RecommendationGroupBy.Impact)]
    [InlineData(RecommendationGroupBy.RecommendationType)]
    [InlineData(RecommendationGroupBy.ResourceType)]
    public void MapGroupByToKqlField_AllValues_HandleEmptyWithUnknown(RecommendationGroupBy groupBy)
    {
        var field = AdvisorService.MapGroupByToKqlField(groupBy);
        Assert.Contains("'Unknown'", field);
        Assert.Contains("isempty", field);
    }

    [Fact]
    public void MapGroupByToKqlField_UnsupportedValue_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => AdvisorService.MapGroupByToKqlField((RecommendationGroupBy)999));
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public void BuildSummarizeQuery_ResourceGroupWithSpecialChars_IsEscaped()
    {
        var query = AdvisorService.BuildSummarizeQuery(RecommendationGroupBy.Category, "rg'inject", null);
        Assert.Contains("rg''inject", query);
    }
}
