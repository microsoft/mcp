// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventGrid.Options.Events;

public sealed class EventsPublishOptions : ISubscriptionOption
{
    [Option(Description = EventGridOptionDescriptions.Topic)]
    public required string Topic { get; set; }

    [Option(Description = "The event data as JSON string to publish to the Event Grid topic.")]
    public required string Data { get; set; }

    [Option(Description = "The event schema type (CloudEvents, EventGrid, or Custom). Defaults to EventGrid.")]
    public string? Schema { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
