// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Options;

public static class EventGridOptionDefinitions
{
    public const string TopicNameParam = "topic";
    public const string LocationParam = "location";
    public const string EventDataParam = "event-data";
    public const string EventSchemaParam = "event-schema";

    public static readonly Option<string> TopicName = new(
        $"--{TopicNameParam}"
    )
    {
        Description = "The name of the Event Grid topic."
    };

    public static readonly Option<string> Location = new(
        $"--{LocationParam}"
    )
    {
        Description = "The Azure region to filter resources by (e.g., 'eastus', 'westus2')."
    };

    public static readonly Option<string> EventData = new(
        $"--{EventDataParam}"
    )
    {
        Description = "The event data as JSON string to publish to the Event Grid topic."
    };

    public static readonly Option<string> EventSchema = new(
        $"--{EventSchemaParam}"
    )
    {
        Description = "The event schema type (CloudEvents, EventGridEvent, or Custom). Defaults to EventGridEvent."
    };
}
