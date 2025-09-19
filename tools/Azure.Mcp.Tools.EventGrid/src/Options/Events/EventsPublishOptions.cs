// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Options.Events;

public class EventsPublishOptions : BaseEventGridOptions
{
    public string? TopicName { get; set; }
    public string? EventData { get; set; }
    public string? EventSchema { get; set; }
}
