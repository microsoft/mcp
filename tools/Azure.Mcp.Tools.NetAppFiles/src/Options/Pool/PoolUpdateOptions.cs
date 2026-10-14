// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Pool;

public sealed class PoolUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account that contains the capacity pool.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the capacity pool to update.")]
    public required string Pool { get; set; }

    [Option(Description = "The new provisioned pool size in tebibytes. Must be a positive multiple of 4.")]
    public long? Size { get; set; }

    [Option(Description = "The new quality-of-service type for the capacity pool. Accepted values: Auto, Manual.")]
    public string? QosType { get; set; }

    [Option(Description = "Whether the capacity pool can contain cool-access-enabled volumes.")]
    public bool? CoolAccess { get; set; }

    [Option(Description = "The new maximum throughput in MiB/s. Supported only for Flexible service-level pools using Manual QoS.")]
    public int? CustomThroughputMibps { get; set; }

    [Option(Description = "Resource tags as a JSON key-value object. Providing an empty object removes all tags.")]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
