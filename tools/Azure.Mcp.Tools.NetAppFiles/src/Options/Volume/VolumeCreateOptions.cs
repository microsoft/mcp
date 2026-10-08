// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Volume;

public class VolumeCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account that contains the capacity pool.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the capacity pool where the volume will be created.")]
    public required string Pool { get; set; }

    [Option(Description = "The name of the Azure NetApp Files volume to create.")]
    public required string Volume { get; set; }

    [Option(Description = "The Azure region where the volume will be created (for example, 'eastus' or 'westus2').")]
    public required string Location { get; set; }

    [Option(Description = "The full Azure resource ID of the delegated subnet used by the volume.")]
    public required string SubnetId { get; set; }

    [Option(Description = "The volume storage quota in GiB. Valid values are 50-102400.")]
    public required long QuotaGib { get; set; }

    [Option(Description = "The service level for the volume. Must match the capacity pool. Valid values are Standard, Premium, and Ultra.")]
    public required string ServiceLevel { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}