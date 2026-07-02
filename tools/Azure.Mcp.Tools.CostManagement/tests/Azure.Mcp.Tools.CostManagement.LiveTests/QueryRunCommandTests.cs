// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.CostManagement.LiveTests;

public sealed class QueryRunCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string ResultKey = "result";

    [Fact]
    public async Task Should_query_subscription_costs_month_to_date()
    {
        var response = await CallToolAsync(
            "costmanagement_query_run",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "timeframe", "MonthToDate" }
            });

        var result = response.AssertProperty(ResultKey);
        Assert.Equal(JsonValueKind.Object, result.ValueKind);

        var totalCost = result.AssertProperty("totalCost");
        Assert.Equal(JsonValueKind.Number, totalCost.ValueKind);
        Assert.True(totalCost.GetDecimal() >= 0m);
    }

    [Fact]
    public async Task Should_query_subscription_costs_grouped_by_service()
    {
        var response = await CallToolAsync(
            "costmanagement_query_run",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "timeframe", "MonthToDate" },
                { "group-by", "ServiceName" }
            });

        var result = response.AssertProperty(ResultKey);
        var groupBy = result.AssertProperty("groupBy");
        Assert.Equal("ServiceName", groupBy.GetString());

        var rows = result.AssertProperty("rows");
        Assert.Equal(JsonValueKind.Array, rows.ValueKind);
        Assert.True(rows.GetArrayLength() > 0,
            "Expected at least one cost row when grouping by ServiceName MTD against a subscription with known spend.");
    }

    [Fact]
    public async Task Should_query_custom_timeframe_with_daily_granularity()
    {
        var response = await CallToolAsync(
            "costmanagement_query_run",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "timeframe", "Custom" },
                { "from", "2026-04-01" },
                { "to", "2026-04-07" },
                { "granularity", "Daily" }
            });

        var result = response.AssertProperty(ResultKey);
        var granularity = result.AssertProperty("granularity");
        Assert.Equal("Daily", granularity.GetString());

        var rows = result.AssertProperty("rows");
        Assert.Equal(JsonValueKind.Array, rows.ValueKind);
    }
}
