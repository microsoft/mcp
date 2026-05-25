// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

using Recommendation = Azure.Mcp.Tools.Advisor.Models.Recommendation;

public class RecommendationAggregatorTests
{
    private static readonly List<Recommendation> SampleRecommendations =
    [
        new("/sub/a/rg/r/Microsoft.Storage/storageAccounts/s1", "Enable soft delete",       "Security",        "High",   "Microsoft.Storage/storageAccounts"),
        new("/sub/a/rg/r/Microsoft.Storage/storageAccounts/s1", "Enable soft delete",       "Security",        "High",   "Microsoft.Storage/storageAccounts"),
        new("/sub/a/rg/r/Microsoft.Storage/storageAccounts/s2", "Use private endpoints",    "Security",        "Medium", "Microsoft.Storage/storageAccounts"),
        new("/sub/a/rg/r/Microsoft.Sql/servers/sql1",            "Right-size database",     "Cost",            "Low",    "Microsoft.Sql/servers"),
        new("/sub/a/rg/r/Microsoft.Sql/servers/sql1",            "Enable backup",           "HighAvailability","High",   "Microsoft.Sql/servers"),
    ];

    [Fact]
    public void Aggregate_NullRecommendations_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RecommendationAggregator.Aggregate(null!, "category", 5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Aggregate_NullOrWhitespaceGroupBy_Throws(string? groupBy)
    {
        Assert.ThrowsAny<ArgumentException>(() => RecommendationAggregator.Aggregate(SampleRecommendations, groupBy!, 5));
    }

    [Fact]
    public void Aggregate_UnknownGroupBy_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => RecommendationAggregator.Aggregate(SampleRecommendations, "nonsense", 5));
        Assert.Contains("nonsense", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Aggregate_NonPositiveTop_ReturnsEmpty(int top)
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "category", top);
        Assert.Empty(result);
    }

    [Fact]
    public void Aggregate_EmptySource_ReturnsEmpty()
    {
        var result = RecommendationAggregator.Aggregate([], "category", 5);
        Assert.Empty(result);
    }

    [Fact]
    public void Aggregate_ByCategory_CountsCorrectly()
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "category", 10);

        Assert.Equal(3, result.Count);
        Assert.Equal("Security", result[0].Key);
        Assert.Equal(3, result[0].Count);
        Assert.Contains(result, g => g.Key == "Cost" && g.Count == 1);
        Assert.Contains(result, g => g.Key == "HighAvailability" && g.Count == 1);
    }

    [Fact]
    public void Aggregate_ByImpact_CountsCorrectly()
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "impact", 10);

        Assert.Equal(3, result.Count);
        Assert.Equal("High", result[0].Key);
        Assert.Equal(3, result[0].Count);
    }

    [Fact]
    public void Aggregate_ByResourceType_CountsCorrectly()
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "resource-type", 10);

        Assert.Equal(2, result.Count);
        Assert.Equal("Microsoft.Storage/storageAccounts", result[0].Key);
        Assert.Equal(3, result[0].Count);
        Assert.Equal("Microsoft.Sql/servers", result[1].Key);
        Assert.Equal(2, result[1].Count);
    }

    [Fact]
    public void Aggregate_ByResource_CountsCorrectly()
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "resource", 10);

        // s1 -> 2, s2 -> 1, sql1 -> 2. Ties broken alphabetically by key.
        Assert.Equal(3, result.Count);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(1, result[2].Count);
    }

    [Fact]
    public void Aggregate_ByRecommendation_CountsCorrectly()
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "recommendation", 10);

        Assert.Equal(4, result.Count);
        Assert.Equal("Enable soft delete", result[0].Key);
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public void Aggregate_TopN_TruncatesToHighestCounts()
    {
        var result = RecommendationAggregator.Aggregate(SampleRecommendations, "category", 1);

        Assert.Single(result);
        Assert.Equal("Security", result[0].Key);
        Assert.Equal(3, result[0].Count);
    }

    [Fact]
    public void Aggregate_GroupByIsCaseInsensitive()
    {
        var lower = RecommendationAggregator.Aggregate(SampleRecommendations, "category", 10);
        var upper = RecommendationAggregator.Aggregate(SampleRecommendations, "CATEGORY", 10);
        var mixed = RecommendationAggregator.Aggregate(SampleRecommendations, "Category", 10);

        Assert.Equal(lower.Count, upper.Count);
        Assert.Equal(lower.Count, mixed.Count);
        Assert.Equal(lower[0].Key, upper[0].Key);
    }

    [Fact]
    public void Aggregate_KeyComparisonIsCaseInsensitive()
    {
        List<Recommendation> recs =
        [
            new("/id/1", "rec a", "security", "High",   "Microsoft.Storage/storageAccounts"),
            new("/id/2", "rec b", "Security", "Medium", "Microsoft.Storage/storageAccounts"),
        ];

        var result = RecommendationAggregator.Aggregate(recs, "category", 10);

        Assert.Single(result);
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public void Aggregate_NullImpact_BucketsAsUnknown()
    {
        List<Recommendation> recs =
        [
            new("/id/1", "rec a", "Security", null, "Microsoft.Storage/storageAccounts"),
            new("/id/2", "rec b", "Security", null, "Microsoft.Sql/servers"),
        ];

        var result = RecommendationAggregator.Aggregate(recs, "impact", 10);

        Assert.Single(result);
        Assert.Equal("Unknown", result[0].Key);
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public void Aggregate_NullImpactedResourceType_BucketsAsUnknown()
    {
        List<Recommendation> recs =
        [
            new("/sub/x", "rec a", "Security", "High", null),
        ];

        var result = RecommendationAggregator.Aggregate(recs, "resource-type", 10);

        Assert.Single(result);
        Assert.Equal("Unknown", result[0].Key);
    }

    [Fact]
    public void Aggregate_TiedCounts_OrderedByKeyAscending()
    {
        List<Recommendation> recs =
        [
            new("/id/1", "rec", "Cost",     "High", "Microsoft.Sql/servers"),
            new("/id/2", "rec", "Security", "High", "Microsoft.Sql/servers"),
        ];

        var result = RecommendationAggregator.Aggregate(recs, "category", 10);

        Assert.Equal(2, result.Count);
        Assert.Equal("Cost", result[0].Key);
        Assert.Equal("Security", result[1].Key);
    }

    [Fact]
    public void AllowedGroupBy_ExposesAllSupportedAxes()
    {
        Assert.Contains(RecommendationAggregator.GroupByRecommendation, RecommendationAggregator.AllowedGroupBy);
        Assert.Contains(RecommendationAggregator.GroupByCategory, RecommendationAggregator.AllowedGroupBy);
        Assert.Contains(RecommendationAggregator.GroupByImpact, RecommendationAggregator.AllowedGroupBy);
        Assert.Contains(RecommendationAggregator.GroupByResourceType, RecommendationAggregator.AllowedGroupBy);
        Assert.Contains(RecommendationAggregator.GroupByResource, RecommendationAggregator.AllowedGroupBy);
        Assert.Equal(5, RecommendationAggregator.AllowedGroupBy.Count);
    }
}
