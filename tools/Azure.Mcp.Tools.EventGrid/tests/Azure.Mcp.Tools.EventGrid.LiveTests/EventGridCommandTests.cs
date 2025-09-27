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
                { "data", eventData }
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
                { "data", eventData }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(2, publishedEventCount);
    }

    [Fact]
    public async Task Should_publish_cloudevents_to_eventgrid_topic()
    {
        // Create CloudEvents format event data
        var eventData = JsonSerializer.Serialize(new
        {
            specversion = "1.0",
            type = "com.example.LiveTestEvent",
            source = "/live/test/cloudevents",
            id = Guid.NewGuid().ToString(),
            time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            data = new
            {
                message = "CloudEvents test from live integration test",
                testType = "live-test",
                timestamp = DateTime.UtcNow
            }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "data", eventData },
                { "schema", "CloudEvents" }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(1, publishedEventCount);
    }

    [Fact]
    public async Task Should_publish_custom_schema_to_eventgrid_topic()
    {
        // Create custom schema event data (business-oriented format)
        var eventData = JsonSerializer.Serialize(new
        {
            orderNumber = "LIVE-ORDER-" + DateTime.Now.Ticks,
            eventCategory = "OrderProcessed",
            resourcePath = "/orders/live-test",
            occurredAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            details = new
            {
                amount = 125.50m,
                currency = "USD",
                items = new[] {
                    new { sku = "LIVE-SKU-001", quantity = 2, price = 50.00m },
                    new { sku = "LIVE-SKU-002", quantity = 1, price = 25.50m }
                },
                customer = new
                {
                    id = "CUST-LIVE-001",
                    tier = "premium"
                }
            }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "data", eventData },
                { "schema", "Custom" }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(1, publishedEventCount);
    }

    [Fact]
    public async Task Should_publish_mixed_schemas_in_custom_format()
    {
        // Create array with mixed EventGrid and CloudEvents field styles
        var eventData = JsonSerializer.Serialize(new object[]
        {
            new // EventGrid-style fields
            {
                id = "live-eventgrid-" + Guid.NewGuid().ToString(),
                subject = "/live/test/eventgrid",
                eventType = "LiveTest.EventGrid",
                dataVersion = "2.0",
                eventTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                data = new { format = "EventGrid", test = "live" }
            },
            new // CloudEvents-style fields
            {
                id = "live-cloudevents-" + Guid.NewGuid().ToString(),
                source = "/live/test/cloudevents",
                type = "LiveTest.CloudEvents",
                specversion = "1.0",
                time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                data = new { format = "CloudEvents", test = "live" }
            }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "data", eventData },
                { "schema", "Custom" }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(2, publishedEventCount);
    }

    [Fact]
    public async Task Should_handle_eventgrid_schema_explicitly()
    {
        // Test explicit EventGrid schema specification
        var eventData = JsonSerializer.Serialize(new
        {
            id = "live-explicit-eventgrid-" + Guid.NewGuid().ToString(),
            subject = "/live/test/explicit",
            eventType = "LiveTest.ExplicitEventGrid",
            dataVersion = "1.5",
            eventTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            data = new
            {
                isExplicit = true,
                schema = "EventGrid",
                timestamp = DateTime.UtcNow
            }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "data", eventData },
                { "schema", "EventGrid" }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(1, publishedEventCount);
    }

    [Fact]
    public async Task Should_publish_with_default_schema_when_not_specified()
    {
        // Test that EventGrid schema is used by default when not specified
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/live/test/default",
            eventType = "LiveTest.DefaultSchema",
            dataVersion = "1.0",
            data = new
            {
                defaultTest = true,
                timestamp = DateTime.UtcNow
            }
        });

        var result = await CallToolAsync(
            "azmcp_eventgrid_events_publish",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "topic", Settings.ResourceBaseName },
                { "data", eventData }
            });

        var publishResult = result.AssertProperty("result");
        var status = publishResult.AssertProperty("status").GetString();
        var publishedEventCount = publishResult.AssertProperty("publishedEventCount").GetInt32();

        Assert.Equal("Success", status);
        Assert.Equal(1, publishedEventCount);
    }
}
