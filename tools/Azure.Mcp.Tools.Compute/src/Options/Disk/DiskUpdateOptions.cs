// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskUpdate command.
/// </summary>
public class DiskUpdateOptions : BaseComputeOptions
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    public string? Disk { get; set; }

    /// <summary>
    /// Gets or sets the new size of the disk in GB.
    /// </summary>
    public int? SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the new storage SKU.
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the number of IOPS allowed for the disk (UltraSSD only).
    /// </summary>
    public long? DiskIopsReadWrite { get; set; }

    /// <summary>
    /// Gets or sets the bandwidth in MBps allowed for the disk (UltraSSD only).
    /// </summary>
    public long? DiskMbpsReadWrite { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of VMs that can attach to the disk.
    /// </summary>
    public int? MaxShares { get; set; }

    /// <summary>
    /// Gets or sets the network access policy.
    /// </summary>
    public string? NetworkAccessPolicy { get; set; }

    /// <summary>
    /// Gets or sets whether on-demand bursting is enabled ("true" or "false").
    /// </summary>
    public string? EnableBursting { get; set; }
}
