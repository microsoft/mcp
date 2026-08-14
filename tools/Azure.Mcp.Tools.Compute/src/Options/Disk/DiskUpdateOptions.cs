// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskUpdate command.
/// </summary>
public sealed class DiskUpdateOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskName, Aliases = ["name"])]
    public required string DiskName { get; set; }

    /// <summary>
    /// Gets or sets the new size of the disk in GB.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.SizeGb, Aliases = ["z"])]
    public int? SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the new storage SKU.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Sku)]
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the number of IOPS allowed for the disk (UltraSSD only).
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskIopsReadWrite)]
    public long? DiskIopsReadWrite { get; set; }

    /// <summary>
    /// Gets or sets the bandwidth in MBps allowed for the disk (UltraSSD only).
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskMbpsReadWrite)]
    public long? DiskMbpsReadWrite { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of VMs that can attach to the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.MaxShares)]
    public int? MaxShares { get; set; }

    /// <summary>
    /// Gets or sets the network access policy.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.NetworkAccessPolicy)]
    public string? NetworkAccessPolicy { get; set; }

    /// <summary>
    /// Gets or sets whether on-demand bursting is enabled ("true" or "false").
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.EnableBursting)]
    public bool? EnableBursting { get; set; }

    /// <summary>
    /// Gets or sets the tags for the disk in 'key=value' format.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.TagsUpdate, AllowEmptyOrWhiteSpaceString = true)]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk encryption set.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskEncryptionSet)]
    public string? DiskEncryptionSet { get; set; }

    /// <summary>
    /// Gets or sets the encryption type of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.EncryptionType)]
    public string? EncryptionType { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk access resource.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskAccess)]
    public string? DiskAccess { get; set; }

    /// <summary>
    /// Gets or sets the performance tier of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Tier)]
    public string? Tier { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
