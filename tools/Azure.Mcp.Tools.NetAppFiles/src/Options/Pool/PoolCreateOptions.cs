// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Pool;

public sealed class PoolCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account that will contain the capacity pool.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the capacity pool to create. Must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string Pool { get; set; }

    [Option(Description = "The provisioned pool size in tebibytes. Must be a positive multiple of 4.")]
    public required long Size { get; set; }

    [Option(Description = "The service level for the capacity pool. Accepted values: Flexible, Premium, Standard, StandardZRS, Ultra.")]
    public required string ServiceLevel { get; set; }

    [Option(Description = "The Azure region for the capacity pool. Defaults to the parent account region.")]
    public string? Location { get; set; }

    [Option(Description = "The quality-of-service type for the capacity pool. Accepted values: Auto, Manual.")]
    public string? QosType { get; set; }

    [Option(Description = "Whether the capacity pool can contain cool-access-enabled volumes.")]
    public bool? CoolAccess { get; set; }

    [Option(Description = "The data-at-rest encryption type for the capacity pool. Accepted values: Single, Double.")]
    public string? EncryptionType { get; set; }

    [Option(Description = "The maximum throughput in MiB/s. Supported only when --service-level is Flexible and --qos-type is Manual.")]
    public int? CustomThroughputMibps { get; set; }

    [Option(Description = "Resource tags as a JSON key-value object.")]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
