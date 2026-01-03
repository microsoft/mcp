// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.LiveTests;

public class EventGridCommandTests(ITestOutputHelper output, TestProxyFixture fixture)
    : RecordedCommandTestsBase(output, fixture)
{
    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        .. base.UriRegexSanitizers,
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = @"resource[gG]roups\/([^?\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        // Sanitize topic endpoints in URIs
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = @"https:\/\/([^.]+)\.([^.]+)-1\.eventgrid\.azure\.net",
            Value = "https://Sanitized.Sanitized-1.eventgrid.azure.net"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        // Sanitize topic names in endpoint URLs
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..endpoint")
        {
            Value = "https://Sanitized.Sanitized-1.eventgrid.azure.net/api/events"
        }),
        // Sanitize subscription display name
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..displayName")
        {
            Value = "Sanitized"
        }),
        // Sanitize owner tags (lowercase)
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..tags.owners")
        {
            Value = "Sanitized"
        }),
        // Sanitize owner tags (capitalized)
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..tags.Owners")
        {
            Value = "Sanitized"
        })
    ];

    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        .. base.BodyRegexSanitizers,
        // Sanitize resource group names with usernames in resource IDs
        new BodyRegexSanitizer(new BodyRegexSanitizerBody
        {
            Regex = @"SSS3PT_[^-/\""]+",
            Value = "Sanitized"
        })
    ];

    [Fact]
    public async Task Should_list_eventgrid_topics_by_subscription()
    {
        var result = await CallToolAsync(
            "eventgrid_topic_list",
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
            "eventgrid_topic_list",
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
            "eventgrid_subscription_list",
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
            "eventgrid_subscription_list",
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
        // Create test event data with deterministic ID and time
        var eventId = RegisterOrRetrieveVariable("Should_publish_events_id", Guid.NewGuid().ToString());
        var eventTime = RegisterOrRetrieveVariable("Should_publish_events_time", DateTimeOffset.UtcNow.ToString("O"));
        var timestamp = RegisterOrRetrieveVariable("Should_publish_events_timestamp", DateTime.UtcNow.ToString("O"));

        var eventData = JsonSerializer.Serialize(new
        {
            id = eventId,
            eventTime = eventTime,
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0",
            data = new { message = "Test event from integration test", timestamp }
        });

        var result = await CallToolAsync(
            "eventgrid_events_publish",
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
        // Create test event data array with deterministic IDs and times
        var eventId1 = RegisterOrRetrieveVariable("multiple_event_id1", Guid.NewGuid().ToString());
        var eventTime1 = RegisterOrRetrieveVariable("multiple_event_time1", DateTimeOffset.UtcNow.ToString("O"));
        var timestamp1 = RegisterOrRetrieveVariable("timestamp1", DateTime.UtcNow.ToString("O"));

        var eventId2 = RegisterOrRetrieveVariable("multiple_event_id2", Guid.NewGuid().ToString());
        var eventTime2 = RegisterOrRetrieveVariable("multiple_event_time2", DateTimeOffset.UtcNow.ToString("O"));
        var timestamp2 = RegisterOrRetrieveVariable("timestamp2", DateTime.UtcNow.ToString("O"));

        var eventData = JsonSerializer.Serialize(new[]
        {
            new
            {
                id = eventId1,
                eventTime = eventTime1,
                subject = "/test/subject1",
                eventType = "TestEvent",
                dataVersion = "1.0",
                data = new { message = "Test event 1", timestamp = timestamp1 }
            },
            new
            {
                id = eventId2,
                eventTime = eventTime2,
                subject = "/test/subject2",
                eventType = "TestEvent",
                dataVersion = "1.0",
                data = new { message = "Test event 2", timestamp = timestamp2 }
            }
        });

        var result = await CallToolAsync(
            "eventgrid_events_publish",
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
        var eventId = RegisterOrRetrieveVariable("cloudEvent_id", Guid.NewGuid().ToString());
        var eventTime = RegisterOrRetrieveVariable("cloudEvent_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        var dataTimestamp = RegisterOrRetrieveVariable("cloudEvent_data_timestamp", DateTime.UtcNow.ToString("O"));

        var eventData = JsonSerializer.Serialize(new
        {
            specversion = "1.0",
            type = "com.example.LiveTestEvent",
            source = "/live/test/cloudevents",
            id = eventId,
            time = eventTime,
            data = new
            {
                message = "CloudEvents test from live integration test",
                testType = "live-test",
                timestamp = dataTimestamp
            }
        });

        var result = await CallToolAsync(
            "eventgrid_events_publish",
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
        // Create custom schema event data (business-oriented format) with deterministic ID and time
        var eventId = RegisterOrRetrieveVariable("custom_event_id", Guid.NewGuid().ToString());
        var eventTime = RegisterOrRetrieveVariable("custom_event_time", DateTimeOffset.UtcNow.ToString("O"));
        var orderTime = RegisterOrRetrieveVariable("custom_order_time", DateTime.UtcNow.Ticks.ToString());
        var occurredTime = RegisterOrRetrieveVariable("custom_occurred_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

        var eventData = JsonSerializer.Serialize(new
        {
            id = eventId,
            eventTime = eventTime,
            orderNumber = "LIVE-ORDER-" + orderTime,
            eventCategory = "OrderProcessed",
            resourcePath = "/orders/live-test",
            occurredAt = occurredTime,
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
            "eventgrid_events_publish",
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
        var eventgridId = RegisterOrRetrieveVariable("mixed_eventgrid_id", Guid.NewGuid().ToString());
        var eventgridTime = RegisterOrRetrieveVariable("mixed_eventgrid_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        var cloudEventsId = RegisterOrRetrieveVariable("mixed_cloudevents_id", Guid.NewGuid().ToString());
        var cloudEventsTime = RegisterOrRetrieveVariable("mixed_cloudevents_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

        var eventData = JsonSerializer.Serialize(new object[]
        {
            new // EventGrid-style fields
            {
                id = "live-eventgrid-" + eventgridId,
                subject = "/live/test/eventgrid",
                eventType = "TestEvent.LiveTest.EventGrid",
                dataVersion = "1.0",
                eventTime = eventgridTime,
                data = new { format = "EventGrid", test = "live" }
            },
            new // CloudEvents-style fields
            {
                id = "live-cloudevents-" + cloudEventsId,
                source = "/live/test/cloudevents",
                type = "LiveTest.CloudEvents",
                specversion = "1.0",
                time = cloudEventsTime,
                data = new { format = "CloudEvents", test = "live" }
            }
        });

        var result = await CallToolAsync(
            "eventgrid_events_publish",
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
        var eventId = RegisterOrRetrieveVariable("explicit_event_id", Guid.NewGuid().ToString());
        var eventTime = RegisterOrRetrieveVariable("explicit_event_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        var dataTimestamp = RegisterOrRetrieveVariable("explicit_data_timestamp", DateTime.UtcNow.ToString("O"));

        var eventData = JsonSerializer.Serialize(new
        {
            id = "live-explicit-eventgrid-" + eventId,
            subject = "/live/test/explicit",
            eventType = "LiveTest.ExplicitEventGrid",
            dataVersion = "1.5",
            eventTime,
            data = new
            {
                isExplicit = true,
                schema = "EventGrid",
                timestamp = dataTimestamp
            }
        });

        var result = await CallToolAsync(
            "eventgrid_events_publish",
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
        var eventId = RegisterOrRetrieveVariable("default_event_id", Guid.NewGuid().ToString());
        var eventTime = RegisterOrRetrieveVariable("default_event_time", DateTimeOffset.UtcNow.ToString("O"));
        var dataTimestamp = RegisterOrRetrieveVariable("default_data_timestamp", DateTime.UtcNow.ToString("O"));

        var eventData = JsonSerializer.Serialize(new
        {
            id = eventId,
            eventTime = eventTime,
            subject = "/live/test/default",
            eventType = "LiveTest.DefaultSchema",
            dataVersion = "1.0",
            data = new
            {
                defaultTest = true,
                timestamp = dataTimestamp
            }
        });

        var result = await CallToolAsync(
            "eventgrid_events_publish",
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
