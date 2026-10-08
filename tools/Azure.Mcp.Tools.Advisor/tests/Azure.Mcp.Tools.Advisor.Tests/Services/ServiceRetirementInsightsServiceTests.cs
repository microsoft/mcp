// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class ServiceRetirementInsightsServiceTests
{
    [Fact]
    public void BuildQuery_FiltersAndEscapesScopeValues()
    {
        var query = AdvisorService.BuildServiceRetirementInsightsQuery("scope'1", 25);

        Assert.Contains("type =~ 'microsoft.advisor/insights'", query);
        Assert.Contains("name =~ 'ServiceRetirement'", query);
        Assert.Contains("tobool(properties.isDeleted) == false", query);
        Assert.Contains("tostring(properties.insightResourceId) =~ 'scope''1'", query);
        Assert.Contains("tostring(properties.insightResourceName) =~ 'scope''1'", query);
        Assert.Contains("take 25", query);
        Assert.EndsWith("project properties", query);
    }

    [Fact]
    public void BuildQuery_WithoutScope_OmitsScopeFilter()
    {
        var query = AdvisorService.BuildServiceRetirementInsightsQuery(null, 100);

        Assert.DoesNotContain("insightResourceId) =~", query);
        Assert.DoesNotContain("insightResourceName) =~", query);
        Assert.Contains("take 100", query);
    }

    [Fact]
    public void ConvertToInsightsData_MapsNestedPropertiesAndKpisByName()
    {
        using var document = CreateDocument(
            """
            [
              { "kpiValueType": "", "kpiValue": "103", "kpiName": "CurrentImpactedResources" },
              { "kpiValueType": "", "kpiValue": "999", "kpiName": "UnrelatedKpi" },
              { "kpiValueType": "", "kpiValue": "178", "kpiName": "day1impactedresources" }
            ]
            """);

        var insight = AdvisorService.ConvertToInsightsData(document.RootElement);

        Assert.Equal("Azure CXP Reliability Data Domain - Test", insight.InsightResourceName);
        Assert.Equal("11dd81b2-631a-45f2-9c30-35d30a49e952", insight.InsightResourceId);
        Assert.Equal(DateTimeOffset.Parse("2026-10-07T21:00:54.171Z"), insight.LastUpdatedTime);
        Assert.Equal(178, insight.Day1ImpactedResources);
        Assert.Equal(103, insight.CurrentImpactedResources);
        Assert.Equal("ServiceRetirement", insight.InsightName);
        Assert.False(insight.IsDeleted);
        Assert.Equal("Recommenddata", insight.Domain);
    }

    [Theory]
    [InlineData("""[{ "kpiValue": "103", "kpiName": "CurrentImpactedResources" }]""")]
    [InlineData("""[{ "kpiValue": "178", "kpiName": "Day1ImpactedResources" }]""")]
    [InlineData("""
        [
          { "kpiValue": "not-a-number", "kpiName": "Day1ImpactedResources" },
          { "kpiValue": "103", "kpiName": "CurrentImpactedResources" }
        ]
        """)]
    [InlineData("""
        [
          { "kpiValue": "178", "kpiName": "Day1ImpactedResources" },
          { "kpiValue": "-1", "kpiName": "CurrentImpactedResources" }
        ]
        """)]
    public void ConvertToInsightsData_InvalidKpiValues_ThrowsJsonException(string insightDetail)
    {
        using var document = CreateDocument(insightDetail);

        Assert.Throws<JsonException>(
            () => AdvisorService.ConvertToInsightsData(document.RootElement));
    }

    [Fact]
    public void ConvertToInsightsData_DuplicateKpi_ThrowsJsonException()
    {
        using var document = CreateDocument(
            """
            [
              { "kpiValue": "178", "kpiName": "Day1ImpactedResources" },
              { "kpiValue": "179", "kpiName": "day1impactedresources" },
              { "kpiValue": "103", "kpiName": "CurrentImpactedResources" }
            ]
            """);

        var exception = Assert.Throws<JsonException>(
            () => AdvisorService.ConvertToInsightsData(document.RootElement));

        Assert.Contains("duplicate", exception.Message);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "properties": [] }""")]
    public void ConvertToInsightsData_InvalidProperties_ThrowsJsonException(string payload)
    {
        using var document = JsonDocument.Parse(payload);

        Assert.Throws<JsonException>(
            () => AdvisorService.ConvertToInsightsData(document.RootElement));
    }

    [Fact]
    public void ConvertToInsightsData_NonArrayInsightDetail_ThrowsJsonException()
    {
        using var document = CreateDocument("{}");

        Assert.Throws<JsonException>(
            () => AdvisorService.ConvertToInsightsData(document.RootElement));
    }

    private static JsonDocument CreateDocument(string insightDetailJson) =>
        JsonDocument.Parse(
            $$"""
            {
              "properties": {
                "insightResourceName": "Azure CXP Reliability Data Domain - Test",
                "insightResourceId": "11dd81b2-631a-45f2-9c30-35d30a49e952",
                "lastUpdatedTime": "2026-10-07T21:00:54.171Z",
                "insightDetail": {{insightDetailJson}},
                "insightName": "ServiceRetirement",
                "isDeleted": false,
                "domain": "Recommenddata"
              }
            }
            """);
}
