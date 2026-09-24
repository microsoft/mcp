// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Volume;

public sealed class VolumeUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account that contains the capacity pool.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the capacity pool that contains the volume.")]
    public required string Pool { get; set; }

    [Option(Description = "The name of the Azure NetApp Files volume to update.")]
    public required string Volume { get; set; }

    [Option(Description = "The updated volume storage quota in GiB. Valid values are 50-102400.")]
    public required long QuotaGib { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}