// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ApplicationInsights.LiveTests;

[Trait("Area", "ApplicationInsights")]
[Trait("Category", "Live")]
public class ApplicationInsightsCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_applicationinsights_recommendations_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_applicationinsights_recommendation_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var recommendations = result.AssertProperty("recommendations");
        Assert.Equal(JsonValueKind.Array, recommendations.ValueKind);
        // Note: recommendations array can be empty if no profiler-based recommendations are available.
    }

    [Fact]
    public async Task Should_list_applicationinsights_recommendations_by_subscription_and_resource_group()
    {
        var result = await CallToolAsync(
            "azmcp_applicationinsights_recommendation_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var recommendations = result.AssertProperty("recommendations");
        Assert.Equal(JsonValueKind.Array, recommendations.ValueKind);
        // Note: recommendations array can be empty if no profiler-based recommendations are available in the RG.
    }
}
