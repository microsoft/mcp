// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cSpell:ignore advisorresources

using System.Text.RegularExpressions;
using Azure.Mcp.Tools.Optimization.Services;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests.Services;

public class OptimizationKqlQueriesTests
{
    private const string ServiceGroupFilter = "| where isempty(properties.serviceGroupId)";
    private const string ResourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm";

    public static TheoryData<string, string> AdvisorQueries => new()
    {
        { nameof(OptimizationKqlQueries.TopCostSavingsQuery), OptimizationKqlQueries.TopCostSavingsQuery },
        { nameof(OptimizationKqlQueries.UnattachedDiskSummaryQuery), OptimizationKqlQueries.UnattachedDiskSummaryQuery },
        { nameof(OptimizationKqlQueries.BuildAlternativesQuery), OptimizationKqlQueries.BuildAlternativesQuery(ResourceId) },
        { nameof(OptimizationKqlQueries.BuildAdvisorRecommendationQuery), OptimizationKqlQueries.BuildAdvisorRecommendationQuery(ResourceId) },
    };

    [Theory]
    [MemberData(nameof(AdvisorQueries))]
    public void AdvisorQueries_FilterOutServiceGroupRecords_OnEveryAdvisorTable(string name, string query)
    {
        // Each advisorresources table read must be immediately narrowed by type and then by the service-group filter.
        var tableReads = Regex.Matches(query, @"advisorresources\s*\|\s*where type =~ '[^']+'\s*(?<next>\|[^\r\n|]*)");

        Assert.NotEmpty(tableReads);
        Assert.Equal(Regex.Matches(query, @"\badvisorresources\b").Count, tableReads.Count);
        Assert.All(tableReads, read => Assert.True(
            read.Groups["next"].Value.Trim() == ServiceGroupFilter,
            $"{name}: advisorresources read is missing '{ServiceGroupFilter}' right after its type filter: '{read.Value}'"));
    }
}
