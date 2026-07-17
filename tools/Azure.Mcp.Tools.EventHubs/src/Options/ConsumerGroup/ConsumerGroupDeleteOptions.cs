// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;

public sealed class ConsumerGroupDeleteOptions : ISubscriptionOption
{
    [Option(Description = EventHubsOptionDescriptions.Namespace)]
    public required string Namespace { get; set; }

    [Option(Description = EventHubsOptionDescriptions.Eventhub)]
    public required string Eventhub { get; set; }

    [Option(Description = EventHubsOptionDescriptions.ConsumerGroup)]
    public required string ConsumerGroup { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
