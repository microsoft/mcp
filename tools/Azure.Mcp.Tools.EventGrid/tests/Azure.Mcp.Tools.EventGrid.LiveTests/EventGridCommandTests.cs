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

    [Fact]
    public async Task Should_list_eventgrid_subscriptions_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_eventgrid_subscription_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var subscriptions = result.AssertProperty("subscriptions");
        Assert.Equal(JsonValueKind.Array, subscriptions.ValueKind);
        // Note: subscriptions array might be empty if no Event Grid subscriptions exist in the subscription
    }

    [Fact]
    public async Task Should_list_eventgrid_subscriptions_by_subscription_and_resource_group()
    {
        var result = await CallToolAsync(
            "azmcp_eventgrid_subscription_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var subscriptions = result.AssertProperty("subscriptions");
        Assert.Equal(JsonValueKind.Array, subscriptions.ValueKind);
        // Note: subscriptions array might be empty if no Event Grid subscriptions exist in the resource group
    }

    [Fact]
    public async Task Should_publish_events_to_eventgrid_topic()
    {
        // Create test event data
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0",
            data = new { message = "Test event from integration test", timestamp = DateTime.UtcNow }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "event-data", eventData }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(1, publishedEventCount);
    }

    [Fact]
    public async Task Should_publish_multiple_events_to_eventgrid_topic()
    {
        // Create test event data array
        var eventData = JsonSerializer.Serialize(new[]
        {
            new
            {
                subject = "/test/subject1",
                eventType = "TestEvent",
                dataVersion = "1.0",
                data = new { message = "Test event 1", timestamp = DateTime.UtcNow }
            },
            new
            {
                subject = "/test/subject2",
                eventType = "TestEvent",
                dataVersion = "1.0",
                data = new { message = "Test event 2", timestamp = DateTime.UtcNow }
            }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "event-data", eventData }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(2, publishedEventCount);
    }
}
