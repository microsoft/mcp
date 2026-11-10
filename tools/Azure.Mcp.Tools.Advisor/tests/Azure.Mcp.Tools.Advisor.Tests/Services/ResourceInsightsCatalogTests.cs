// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class ResourceInsightsCatalogTests
{
    [Fact]
    public void Catalog_ContainsOnlyTheOnboardedResourceTypes()
    {
        Assert.Equal(16, ResourceInsightsCatalog.All.Count);
        Assert.Equal(150, ResourceInsightsCatalog.All.Values.Sum(static insight => insight.Checks.Count));
        Assert.Equal(16, ResourceInsightsCatalog.All.Values.Select(static insight => insight.RecommendationTypeId).Distinct().Count());
        foreach (var (resourceType, insight) in ResourceInsightsCatalog.All)
        {
            Assert.Equal(resourceType, insight.ResourceType);
            Assert.True(Guid.TryParse(insight.RecommendationTypeId, out _));
            Assert.Equal(insight, ResourceInsightsCatalog.Load(resourceType.ToUpperInvariant(), insight.RecommendationTypeId.ToUpperInvariant()));
            Assert.Null(ResourceInsightsCatalog.Load(resourceType, Guid.Empty.ToString()));
            Assert.Single(insight.Checks, static check => check.Id == "recheck-detection");
            Assert.Single(insight.Checks, static check => check.Kind == "unusedAge");
            Assert.Single(insight.Checks, static check => check.Kind == "locks");
            Assert.Equal(insight.Checks.Count, insight.Checks.Select(static check => check.Id).Distinct().Count());
        }
        Assert.Null(ResourceInsightsCatalog.Load("microsoft.compute/virtualmachines", Guid.Empty.ToString()));
        Assert.Null(ResourceInsightsCatalog.Load("microsoft.compute/availabilitysets", Guid.Empty.ToString()));
    }

    [Theory]
    [InlineData("microsoft.compute/disks", "48eda464-1485-4dcf-a674-d0905df5054a")]
    [InlineData("  Microsoft.Web/serverfarms  ", "39a8510f-5bbf-4304-9bcd-4106c996473b")]
    public void Load_UsesResourceTypeFirstAndRecommendationIdAsGuard(string resourceType, string typeId)
    {
        var insight = ResourceInsightsCatalog.Load(resourceType, typeId);

        Assert.NotNull(insight);
        Assert.Equal(resourceType.Trim().ToLowerInvariant(), insight.ResourceType);
        Assert.Null(ResourceInsightsCatalog.Load(resourceType, "00000000-0000-0000-0000-000000000001"));
        Assert.Null(ResourceInsightsCatalog.Load(typeId, typeId));
    }

    [Fact]
    public void Catalog_ArgChecksAndActivitySignalsHaveBoundedResourceKeys()
    {
        foreach (var insight in ResourceInsightsCatalog.All.Values)
        {
            foreach (var check in insight.Checks.Where(static check => check.Kind == "arg"))
            {
                Assert.NotNull(check.Query);
                Assert.Contains("<RESOURCE_IDS>", check.Query);
                Assert.DoesNotContain("<RESOURCE_ID>", check.Query);
                Assert.Contains("key", check.Query);
            }
            var age = Assert.Single(insight.Checks, static check => check.Kind == "unusedAge");
            Assert.NotNull(age.Signals);
            Assert.NotEmpty(age.Signals);
            foreach (var signal in age.Signals)
            {
                Assert.Contains(signal.Source, new[] { "armChangedTime", "arg", "metrics" });
                if (signal.Source == "metrics")
                {
                    Assert.NotNull(signal.MetricNames);
                    Assert.NotEmpty(signal.MetricNames);
                }
            }
        }
    }
}
