// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Options.Subscription;

public class SubscriptionListOptions : BaseEventGridOptions
{
    public string? TopicName { get; set; }
    public string? Location { get; set; }
}
