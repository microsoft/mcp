// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.EventHubs.Options.EventHub;

public sealed class EventHubUpdateOptions : ISubscriptionOption
{
    [Option(Description = EventHubsOptionDescriptions.Namespace)]
    public required string Namespace { get; set; }

    [Option(Description = EventHubsOptionDescriptions.Eventhub)]
    public required string Eventhub { get; set; }

    [Option(Description = "The number of partitions for the event hub. Must be between 1 and 32 (or higher based on namespace tier).")]
    public int? PartitionCount { get; set; }

    [Option(Description = "The message retention time in hours. Minimum is 1 hour, maximum depends on the namespace tier.")]
    public long? MessageRetentionInHours { get; set; }

    [Option(Description = "The status of the event hub (Active, Disabled, etc.). Note: Status may be read-only in some operations.")]
    public string? Status { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
