// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options.Namespace;

public sealed class NamespaceUpdateOptions : BaseEventHubsOptions
{
    public string? Location { get; set; }
    public string? SkuName { get; set; }
    public string? SkuTier { get; set; }
    public int? SkuCapacity { get; set; }
    public bool? IsAutoInflateEnabled { get; set; }
    public int? MaximumThroughputUnits { get; set; }
    public bool? KafkaEnabled { get; set; }
    public bool? ZoneRedundant { get; set; }
    public string? Tags { get; set; }
}
