// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class ServiceGroupIntelligenceServiceTests
{
    [Fact]
    public void NormalizeServiceGroupIds_NameAndId_ReturnsLowercasedIds()
    {
        var ids = ServiceGroupIntelligenceService.NormalizeServiceGroupIds(
            ["SG1", "/providers/Microsoft.Management/serviceGroups/SG1"]);

        Assert.Equal(["/providers/microsoft.management/servicegroups/sg1"], ids);
    }

    [Fact]
    public void NormalizeServiceGroupIds_InvalidId_Throws() =>
        Assert.Throws<ArgumentException>(() => ServiceGroupIntelligenceService.NormalizeServiceGroupIds(["/subscriptions/x"]));

    [Fact]
    public void BuildQuery_QuotesInInsightName_AreEscaped()
    {
        var query = ServiceGroupIntelligenceService.BuildQuery([], ["a'b"]);

        Assert.Contains("@'a''b'", query);
        Assert.DoesNotContain('\r', query);
    }

    [Fact]
    public void Normalize_StatusAndInsights_ReturnsNewestStatusAndInsightKpis()
    {
        const string json = """
            {"serviceGroupScope":"/providers/microsoft.management/servicegroups/sg1","records":[
              {"id":"/x/Health","type":"microsoft.advisor/servicegroupintelligence","properties":{"intelligenceName":"Health","criticality":"1","lastUpdatedTime":"2024-01-01T00:00:00Z",
                "intelligenceDetail":[{"kpiName":"Status","kpiValue":"Critical"}]}},
              {"id":"/x/Health","type":"microsoft.advisor/servicegroupintelligence","properties":{"intelligenceName":"Health","criticality":"1","lastUpdatedTime":"2024-02-01T00:00:00Z",
                "intelligenceDetail":[{"kpiName":"Status","kpiValue":"At Risk"},{"kpiName":"StatusDescription","kpiValue":"d"},{"kpiName":"Description","kpiValue":"Cost Optimization has the largest impact."}]}},
              {"id":"/x/i","type":"microsoft.advisor/insights","properties":{"insightName":"IdleResources","insightDetail":[{"kpiName":"IdleResources","kpiValue":3}]}}
            ]}
            """;
        using var doc = JsonDocument.Parse(json);

        var result = ServiceGroupIntelligenceService.Normalize(doc.RootElement, true, true);

        Assert.NotNull(result);
        Assert.Equal("sg1", result.Name);
        Assert.Equal("At Risk", result.Status);
        Assert.Equal("Business-critical", result.CriticalityLabel);
        Assert.Equal("3", Assert.Single(result.Insights!).Kpis["IdleResources"]);
    }

    [Fact]
    public void FilterAndProject_StatusFilterWithInsightsOnly_FiltersBeforeRemovingStatus()
    {
        var item = new ServiceGroupStatusInsight(
            "sg1",
            "/providers/microsoft.management/servicegroups/sg1",
            "1",
            "Business-critical",
            "At Risk",
            "description",
            "impact",
            [new("IdleResources", new Dictionary<string, string?>())]);

        var result = ServiceGroupIntelligenceService.FilterAndProject(
            [item],
            null,
            ["At Risk"],
            includeStatus: false,
            includeInsights: true);

        var projected = Assert.Single(result);
        Assert.Null(projected.Status);
        Assert.Null(projected.StatusDescription);
        Assert.Null(projected.Description);
        Assert.NotNull(projected.Insights);
    }
}
