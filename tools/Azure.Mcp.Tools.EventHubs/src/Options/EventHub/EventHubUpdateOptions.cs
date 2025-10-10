// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options.EventHub;

public class EventHubUpdateOptions : BaseEventHubsOptions
{
    public int? PartitionCount { get; set; }
    public long? MessageRetentionInHours { get; set; }
    public string? Status { get; set; }
}
