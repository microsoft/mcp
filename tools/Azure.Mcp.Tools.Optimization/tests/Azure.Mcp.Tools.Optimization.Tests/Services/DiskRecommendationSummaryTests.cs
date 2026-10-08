// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Optimization.Services;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests.Services;

public class DiskRecommendationSummaryTests
{
    private const string SubscriptionId = "11111111-1111-1111-1111-111111111111";

    [Fact]
    public void BuildDiskRecommendationSummary_NoRows_ReturnsNull()
    {
        Assert.Null(OptimizationService.BuildDiskRecommendationSummary([], SubscriptionId));
    }

    [Fact]
    public void BuildDiskRecommendationSummary_ZeroCount_ReturnsNull()
    {
        var rows = ParseRows("""[{"unattachedDiskCount":0,"subscriptionIds":[]}]""");

        Assert.Null(OptimizationService.BuildDiskRecommendationSummary(rows, SubscriptionId));
    }

    [Fact]
    public void BuildDiskRecommendationSummary_WithDisks_ReturnsSummaryAndActionUrl()
    {
        var rows = ParseRows("""[{"unattachedDiskCount":3,"subscriptionIds":["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"]}]""");

        var summary = OptimizationService.BuildDiskRecommendationSummary(rows, SubscriptionId);

        Assert.NotNull(summary);
        Assert.Equal(3, summary.UnattachedDiskCount);
        Assert.Contains("3 found", summary.Message);
        Assert.Equal(2, summary.SubscriptionIds.Count);
        Assert.Equal(
            "https://ms.portal.azure.com/#view/Microsoft_Azure_Expert/RecommendationList.ReactView/recommendationTypeId/" +
            "48eda464-1485-4dcf-a674-d0905df5054a/recommendationStatus~/0/subscriptionIds~/" +
            "%5B%22aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa%22%2C%22bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb%22%5D",
            summary.ActionUrl);
    }

    [Fact]
    public void BuildDiskRecommendationSummary_MissingSubscriptionIds_FallsBackToQueriedSubscription()
    {
        var rows = ParseRows("""[{"unattachedDiskCount":1}]""");

        var summary = OptimizationService.BuildDiskRecommendationSummary(rows, SubscriptionId);

        Assert.NotNull(summary);
        Assert.Equal([SubscriptionId], summary.SubscriptionIds);
        Assert.Contains($"%22{SubscriptionId}%22", summary.ActionUrl);
    }

    [Fact]
    public void BuildDiskRecommendationSummary_AllSubscriptionsScope_UsesSubscriptionsFromQuery()
    {
        var rows = ParseRows("""[{"unattachedDiskCount":4,"subscriptionIds":["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"]}]""");

        var summary = OptimizationService.BuildDiskRecommendationSummary(rows, subscriptionId: null);

        Assert.NotNull(summary);
        Assert.Equal(4, summary.UnattachedDiskCount);
        Assert.Equal(["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"], summary.SubscriptionIds);
    }

    [Fact]
    public void TopCostSavingsQuery_ExcludesUnattachedDiskRecommendations()
    {
        Assert.Contains(
            $"!= '{OptimizationKqlQueries.UnattachedDiskRecommendationTypeId}'",
            OptimizationKqlQueries.TopCostSavingsQuery);
        Assert.Contains(
            $"== '{OptimizationKqlQueries.UnattachedDiskRecommendationTypeId}'",
            OptimizationKqlQueries.UnattachedDiskSummaryQuery);
    }

    private static List<JsonElement> ParseRows(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
    }
}
