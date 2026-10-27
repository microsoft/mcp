// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorInsightServiceTests
{
    private const string ServiceGroupRow = """
        {"id":"/providers/microsoft.management/servicegroups/commerce/providers/Microsoft.Advisor/insights/IdleResources",
         "name":"IdleResources","type":"microsoft.advisor/insights",
         "properties":{"insightName":"IdleResources","domain":"Cost","lastUpdatedTime":"2026-10-12T20:57:43.45Z",
           "insightDetail":[
             {"kpiValueType":"Integer","kpiValue":"30","kpiName":"IdleResources"},
             {"kpiValueType":"USD","kpiValue":"29677.128000000004","kpiName":"PotentialSavingsRetailAnnual_USD"},
             {"kpiValueType":"string","kpiValue":"n/a","kpiName":"Note"}]}}
        """;

    private const string SubscriptionRow = """
        {"id":"/subscriptions/11111111-1111-1111-1111-111111111111/providers/Microsoft.Advisor/insights/ServiceRetirement",
         "name":"ServiceRetirement",
         "properties":{"insightName":"ServiceRetirement","domain":"Recommenddata","lastUpdatedTime":"2026-10-13T09:28:33.124Z",
           "insightDetail":[{"kpiValueType":"","kpiValue":"141","kpiName":"CurrentImpactedResources"}]}}
        """;

    private static JsonElement Rows(params string[] rows) =>
        JsonDocument.Parse("[" + string.Join(",", rows) + "]").RootElement.Clone();

    [Fact]
    public void BuildInsightsQuery_ServiceGroup_ScopesByLowercasedIdPrefix()
    {
        var query = AdvisorInsightService.BuildInsightsQuery("Commerce", null, null);

        Assert.Contains("type =~ 'microsoft.advisor/insights'", query);
        Assert.Contains(
            "tolower(id) startswith '/providers/microsoft.management/servicegroups/commerce/providers/microsoft.advisor/insights/'",
            query);
        Assert.Contains("tobool(properties.isDeleted) != true", query);
        Assert.DoesNotContain("name =~", query);
        Assert.EndsWith("| project id, name, type, properties | order by id asc", query);
    }

    [Fact]
    public void FromJson_ArgRow_ReadsIdNameTypeAndProperties()
    {
        using var document = JsonDocument.Parse(ServiceGroupRow);

        var data = InsightData.FromJson(document.RootElement);

        Assert.NotNull(data);
        Assert.EndsWith("/insights/IdleResources", data.Id);
        Assert.Equal("IdleResources", data.Name);
        Assert.Equal("microsoft.advisor/insights", data.Type);
        Assert.NotNull(data.Properties);
    }

    [Fact]
    public void BuildInsightsQuery_SubscriptionWithName_FiltersByNameAndSubscriptionPrefix()
    {
        var query = AdvisorInsightService.BuildInsightsQuery(
            null, "11111111-1111-1111-1111-111111111111", "ServiceRetirement");

        Assert.Contains(
            "startswith '/subscriptions/11111111-1111-1111-1111-111111111111/providers/microsoft.advisor/insights/'",
            query);
        Assert.Contains("| where name =~ 'ServiceRetirement'", query);
    }

    [Fact]
    public void BuildInsightsQuery_EscapesUserInput()
    {
        var query = AdvisorInsightService.BuildInsightsQuery("sg'x", null, "a'b");

        Assert.DoesNotContain("sg'x", query);
        Assert.DoesNotContain("'a'b'", query);
    }

    [Fact]
    public void BuildInsightsQuery_NoScope_MatchesAllServiceGroupsAndSubscriptions()
    {
        var query = AdvisorInsightService.BuildInsightsQuery(null, null, null);

        Assert.Contains("startswith '/providers/microsoft.management/servicegroups/'", query);
        Assert.Contains("startswith '/subscriptions/'", query);
    }

    [Theory]
    [InlineData("sg", "sub")]
    public void BuildInsightsQuery_ConflictingScopes_Throws(string? serviceGroup, string? subscriptionId)
    {
        Assert.Throws<ArgumentException>(
            () => AdvisorInsightService.BuildInsightsQuery(serviceGroup, subscriptionId, null));
    }

    [Fact]
    public void ParseInsights_ServiceGroupRow_MapsScopeAndTypedKpis()
    {
        var insight = Assert.Single(AdvisorInsightService.ParseInsights(Rows(ServiceGroupRow)));

        Assert.Equal("IdleResources", insight.Name);
        Assert.Equal("Cost", insight.Domain);
        Assert.Equal("ServiceGroup", insight.ScopeType);
        Assert.Equal("/providers/microsoft.management/servicegroups/commerce", insight.ScopeId);
        Assert.NotNull(insight.LastUpdatedTime);

        Assert.Equal(30, insight.Kpis[0].Value.GetInt64());
        Assert.Null(insight.Kpis[0].Unit);
        Assert.Equal(29677.128000000004m, insight.Kpis[1].Value.GetDecimal());
        Assert.Equal("USD", insight.Kpis[1].Unit);
        Assert.Equal("n/a", insight.Kpis[2].Value.GetString());
    }

    [Fact]
    public void ParseInsights_SubscriptionRowWithEmptyValueType_InfersNumber()
    {
        var insight = Assert.Single(AdvisorInsightService.ParseInsights(Rows(SubscriptionRow)));

        Assert.Equal("Subscription", insight.ScopeType);
        Assert.Equal("/subscriptions/11111111-1111-1111-1111-111111111111", insight.ScopeId);
        Assert.Equal(JsonValueKind.Number, insight.Kpis[0].Value.ValueKind);
        Assert.Equal(141, insight.Kpis[0].Value.GetInt64());
    }

    [Fact]
    public void ParseInsights_NonArrayOrMalformedRows_AreSkipped()
    {
        Assert.Empty(AdvisorInsightService.ParseInsights(default));
        Assert.Empty(AdvisorInsightService.ParseInsights(Rows("{\"id\":\"\",\"name\":\"\"}")));
    }

    [Fact]
    public async Task CollectInsightsAsync_FollowsSkipTokensAndStopsWhenExhausted()
    {
        var tokens = new List<string?>();
        Task<AdvisorInsightService.InsightPage> Fetch(int size, string? token, CancellationToken ct)
        {
            tokens.Add(token);
            return Task.FromResult(token is null
                ? new AdvisorInsightService.InsightPage(Rows(ServiceGroupRow), "t1")
                : new AdvisorInsightService.InsightPage(Rows(SubscriptionRow), null));
        }

        var (insights, truncated) = await AdvisorInsightService.CollectInsightsAsync(Fetch, 10, CancellationToken.None);

        Assert.Equal(2, insights.Count);
        Assert.False(truncated);
        Assert.Equal([null, "t1"], tokens);
    }

    [Fact]
    public async Task CollectInsightsAsync_MoreThanTop_TruncatesAndFlags()
    {
        Task<AdvisorInsightService.InsightPage> Fetch(int size, string? token, CancellationToken ct) =>
            Task.FromResult(new AdvisorInsightService.InsightPage(Rows(ServiceGroupRow, SubscriptionRow), "more"));

        var (insights, truncated) = await AdvisorInsightService.CollectInsightsAsync(Fetch, 1, CancellationToken.None);

        Assert.Single(insights);
        Assert.True(truncated);
    }

    [Fact]
    public async Task CollectInsightsAsync_SourceTruncatedWithoutSkipToken_FlagsTruncated()
    {
        Task<AdvisorInsightService.InsightPage> Fetch(int size, string? token, CancellationToken ct) =>
            Task.FromResult(new AdvisorInsightService.InsightPage(Rows(ServiceGroupRow), null, IsTruncated: true));

        var (insights, truncated) = await AdvisorInsightService.CollectInsightsAsync(Fetch, 10, CancellationToken.None);

        Assert.Single(insights);
        Assert.True(truncated);
    }

    [Fact]
    public async Task CollectInsightsAsync_RepeatedSkipToken_Throws()
    {
        Task<AdvisorInsightService.InsightPage> Fetch(int size, string? token, CancellationToken ct) =>
            Task.FromResult(new AdvisorInsightService.InsightPage(Rows(ServiceGroupRow), "same"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => AdvisorInsightService.CollectInsightsAsync(Fetch, 100, CancellationToken.None));
    }

    [Fact]
    public async Task ListInsightsAsync_BothOrNeitherScope_Throws()
    {
        var service = new AdvisorInsightService(NSubstitute.Substitute.For<Azure.Mcp.Core.Services.Azure.IAzureService>());

        await Assert.ThrowsAsync<ArgumentException>(() => service.ListInsightsAsync("sg", "sub", null, cancellationToken: TestContext.Current.CancellationToken));
    }
}
