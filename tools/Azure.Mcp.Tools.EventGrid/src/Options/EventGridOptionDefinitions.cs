// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Options;

public static class EventGridOptionDefinitions
{
    public const string TopicNameParam = "topic";
    public const string LocationParam = "location";
    public const string EventDataParam = "data";
    public const string EventSchemaParam = "schema";

    public static readonly Option<string> TopicName = new(
        $"--{TopicNameParam}"
    )
    {
        Description = "The name of the Event Grid topic.",
        Required = true
    };

    public static readonly Option<string> Location = new(
        $"--{LocationParam}"
    )
    {
        Description = "The Azure region to filter resources by (e.g., 'eastus', 'westus2').",
        Required = false
    };

    public static readonly Option<string> EventData = new(
        $"--{EventDataParam}"
    )
    {
        Description = "The event data as JSON string to publish to the Event Grid topic.",
        Required = true
    };

    public static readonly Option<string> EventSchema = new(
        $"--{EventSchemaParam}"
    )
    {
        Description = "The event schema type (CloudEvents, EventGrid, or Custom). Defaults to EventGrid.",
        Required = false
    };
}
