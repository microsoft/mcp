// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.LiveTests;

[Trait("Area", "EventGrid")]
[Trait("Category", "Live")]
public class EventGridCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_eventgrid_topics_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_eventgrid_topic_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var topics = result.AssertProperty("topics");
        Assert.Equal(JsonValueKind.Array, topics.ValueKind);
        // Note: topics array might be empty if no Event Grid topics exist in the subscription
    }

    [Fact]
    public async Task Should_list_eventgrid_topics_by_subscription_and_resource_group()
    {
        var result = await CallToolAsync(
            "azmcp_eventgrid_topic_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var topics = result.AssertProperty("topics");
        Assert.Equal(JsonValueKind.Array, topics.ValueKind);
        // Note: topics array might be empty if no Event Grid topics exist in the resource group
    }
}
