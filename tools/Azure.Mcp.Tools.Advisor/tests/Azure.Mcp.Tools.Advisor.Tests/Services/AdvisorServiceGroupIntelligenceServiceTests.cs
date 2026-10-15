// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceGroupIntelligenceServiceTests
{
    private const string Row = """
        {"id":"/providers/microsoft.management/servicegroups/commerce/providers/Microsoft.Advisor/serviceGroupIntelligence/Health",
         "name":"Health","type":"microsoft.advisor/servicegroupintelligence",
         "properties":{"intelligenceName":"Health","serviceGroupName":"commerce",
           "serviceGroupId":"/providers/microsoft.management/servicegroups/commerce",
           "lastUpdatedTime":"2026-10-12T03:14:06.625414Z","criticality":"2","isDeleted":false,
           "intelligenceDetail":[
             {"kpiValueType":"string","kpiValue":"Attention Needed","kpiName":"Status"},
             {"kpiValueType":"string","kpiValue":"Some issues exist.","kpiName":"StatusDescription"},
             {"kpiValueType":"string","kpiValue":"Cost Optimization has the largest impact.","kpiName":"Description"}]}}
        """;

    private static JsonElement Rows(params string[] rows) =>
        JsonDocument.Parse("[" + string.Join(",", rows) + "]").RootElement.Clone();

    [Fact]
    public void BuildStatusQuery_NoFilters_MatchesAllServiceGroupStatusRecords()
    {
        var query = AdvisorServiceGroupIntelligenceService.BuildStatusQuery(null, null, null);

        Assert.Contains("type =~ 'microsoft.advisor/servicegroupintelligence'", query);
        Assert.Contains("startswith '/providers/microsoft.management/servicegroups/'", query);
        Assert.Contains("properties.intelligenceName) =~ 'Health'", query);
        Assert.Contains("tobool(properties.isDeleted) != true", query);
        Assert.DoesNotContain("properties.criticality", query);
        Assert.DoesNotContain("mv-expand", query);
        Assert.EndsWith("| project id, name, type, properties | order by id asc", query);
    }

    [Fact]
    public void BuildStatusQuery_ServiceGroup_ScopesByLowercasedIdPrefix()
    {
        var query = AdvisorServiceGroupIntelligenceService.BuildStatusQuery("Commerce", null, null);

        Assert.Contains(
            "startswith '/providers/microsoft.management/servicegroups/commerce/providers/microsoft.advisor/servicegroupintelligence/'",
            query);
    }

    [Fact]
    public void BuildStatusQuery_CriticalityAndStatus_AddsCanonicalFilters()
    {
        var query = AdvisorServiceGroupIntelligenceService.BuildStatusQuery(null, "missioncritical", "attentionneeded");

        Assert.Contains("tostring(properties.criticality) == '0'", query);
        Assert.Contains("tostring(kpi.kpiValue) =~ 'Attention Needed'", query);
    }

    [Theory]
    [InlineData("High", null)]
    [InlineData(null, "Healthy")]
    public void BuildStatusQuery_UnknownFilter_Throws(string? criticality, string? status) =>
        Assert.Throws<ArgumentException>(
            () => AdvisorServiceGroupIntelligenceService.BuildStatusQuery(null, criticality, status));

    [Theory]
    [InlineData("MissionCritical", "MissionCritical")]
    [InlineData("business critical", "BusinessCritical")]
    [InlineData("ADMINISTRATIVE", "Administrative")]
    [InlineData("tier 0", "MissionCritical")]
    [InlineData("Tier1", "BusinessCritical")]
    [InlineData("2", "BusinessOperational")]
    [InlineData("3", "Administrative")]
    [InlineData("not applicable", "NotApplicable")]
    public void TryNormalizeCriticality_AcceptsLabelsAndTierAliases(string input, string expected)
    {
        Assert.True(AdvisorServiceGroupIntelligenceService.TryNormalizeCriticality(input, out var label));
        Assert.Equal(expected, label);
    }

    [Theory]
    [InlineData("4")]
    [InlineData("Tier 4")]
    [InlineData("")]
    [InlineData(null)]
    public void TryNormalizeCriticality_RejectsOthers(string? input) =>
        Assert.False(AdvisorServiceGroupIntelligenceService.TryNormalizeCriticality(input, out _));

    [Theory]
    [InlineData("on track", "On Track")]
    [InlineData("AttentionNeeded", "Attention Needed")]
    [InlineData("at risk", "At Risk")]
    [InlineData("CRITICAL", "Critical")]
    [InlineData("unavailable", "Unavailable")]
    public void TryNormalizeStatus_AcceptsKnownStatuses(string input, string expected)
    {
        Assert.True(AdvisorServiceGroupIntelligenceService.TryNormalizeStatus(input, out var status));
        Assert.Equal(expected, status);
    }

    [Fact]
    public void ParseStatuses_MapsKpisAndCriticalityLabel()
    {
        var statuses = AdvisorServiceGroupIntelligenceService.ParseStatuses(Rows(Row));

        var status = Assert.Single(statuses);
        Assert.Equal("commerce", status.ServiceGroupName);
        Assert.Equal("/providers/microsoft.management/servicegroups/commerce", status.ServiceGroupId);
        Assert.Equal("BusinessOperational", status.Criticality);
        Assert.Equal("Attention Needed", status.Status);
        Assert.Equal("Some issues exist.", status.StatusDescription);
        Assert.Equal("Cost Optimization has the largest impact.", status.Description);
        Assert.Null(status.Goal);
        Assert.Equal(DateTimeOffset.Parse("2026-10-12T03:14:06.625414Z"), status.LastUpdatedTime);
    }

    [Fact]
    public void FromJson_ArgRow_ReadsIdNameTypeAndProperties()
    {
        using var document = JsonDocument.Parse(Row);

        var data = ServiceGroupIntelligenceData.FromJson(document.RootElement);

        Assert.NotNull(data);
        Assert.EndsWith("/serviceGroupIntelligence/Health", data.Id);
        Assert.Equal("Health", data.Name);
        Assert.Equal("microsoft.advisor/servicegroupintelligence", data.Type);
        Assert.NotNull(data.Properties);
    }

    [Fact]
    public void BuildStatusQuery_NotApplicable_FiltersMissingCriticality()
    {
        var query = AdvisorServiceGroupIntelligenceService.BuildStatusQuery(null, "NotApplicable", null);

        Assert.Contains("isempty(tostring(properties.criticality))", query);
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData("\"Reduce cost\"", "Reduce cost")]
    [InlineData("{\"target\":99}", "{\"target\":99}")]
    public void GetGoalText_HandlesStringObjectAndNull(string json, string? expected)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);

        Assert.Equal(expected, AdvisorServiceGroupIntelligenceService.GetGoalText(document.RootElement.Clone()));
    }

    [Fact]
    public void ParseStatuses_MissingCriticality_ReturnsNotApplicableAndGoal()
    {
        var statuses = AdvisorServiceGroupIntelligenceService.ParseStatuses(Rows("""
            {"id":"/providers/microsoft.management/servicegroups/payments/providers/Microsoft.Advisor/serviceGroupIntelligence/Health",
             "properties":{"goal":"Reduce cost"}}
            """));

        var status = Assert.Single(statuses);
        Assert.Equal("NotApplicable", status.Criticality);
        Assert.Equal("Reduce cost", status.Goal);
    }

    [Fact]
    public void ParseStatuses_MissingNameAndKpis_FallsBackToIdAndNulls()
    {
        var statuses = AdvisorServiceGroupIntelligenceService.ParseStatuses(Rows("""
            {"id":"/providers/microsoft.management/servicegroups/payments/providers/Microsoft.Advisor/serviceGroupIntelligence/Health",
             "properties":{"criticality":"9"}}
            """));

        var status = Assert.Single(statuses);
        Assert.Equal("payments", status.ServiceGroupName);
        Assert.Null(status.Criticality);
        Assert.Null(status.Status);
    }

    [Fact]
    public void ParseStatuses_NonArray_ReturnsEmpty() =>
        Assert.Empty(AdvisorServiceGroupIntelligenceService.ParseStatuses(default));

    [Fact]
    public async Task CollectStatusAsync_MoreRowsThanTop_TruncatesAndFlags()
    {
        var (statuses, truncated) = await AdvisorServiceGroupIntelligenceService.CollectStatusAsync(
            (_, _, _) => Task.FromResult(new AdvisorServiceGroupIntelligenceService.StatusPage(Rows(Row, Row), null)),
            1,
            TestContext.Current.CancellationToken);

        Assert.Single(statuses);
        Assert.True(truncated);
    }

    [Fact]
    public async Task CollectStatusAsync_FollowsSkipTokensUntilExhausted()
    {
        var calls = 0;
        var (statuses, truncated) = await AdvisorServiceGroupIntelligenceService.CollectStatusAsync(
            (_, token, _) =>
            {
                calls++;
                return Task.FromResult(new AdvisorServiceGroupIntelligenceService.StatusPage(
                    Rows(Row), token is null ? "t1" : null));
            },
            10,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, calls);
        Assert.Equal(2, statuses.Count);
        Assert.False(truncated);
    }

    [Fact]
    public async Task CollectStatusAsync_SourceTruncatedWithoutSkipToken_FlagsTruncated()
    {
        var (statuses, truncated) = await AdvisorServiceGroupIntelligenceService.CollectStatusAsync(
            (_, _, _) => Task.FromResult(new AdvisorServiceGroupIntelligenceService.StatusPage(Rows(Row), null, IsTruncated: true)),
            10,
            TestContext.Current.CancellationToken);

        Assert.Single(statuses);
        Assert.True(truncated);
    }

    [Fact]
    public async Task CollectStatusAsync_RepeatedSkipToken_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdvisorServiceGroupIntelligenceService.CollectStatusAsync(
                (_, _, _) => Task.FromResult(new AdvisorServiceGroupIntelligenceService.StatusPage(Rows(Row), "same")),
                10,
                TestContext.Current.CancellationToken));
}
