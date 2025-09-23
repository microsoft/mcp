// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Options;

public static class EventGridOptionDefinitions
{
    public const string TopicNameParam = "topic";
    public const string LocationParam = "location";

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
}
