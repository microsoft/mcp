// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskCreate command.
/// </summary>
public class DiskCreateOptions : BaseComputeOptions
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    public string? Disk { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk access resource.
    /// </summary>
    public string? DiskAccessId { get; set; }

    /// <summary>
    /// Gets or sets the number of IOPS allowed for the disk (UltraSSD only).
    /// </summary>
    public long? DiskIopsReadWrite { get; set; }

    /// <summary>
    /// Gets or sets the bandwidth in MBps allowed for the disk (UltraSSD only).
    /// </summary>
    public long? DiskMbpsReadWrite { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk encryption set.
    /// </summary>
    public string? DiskEncryptionSet { get; set; }

    /// <summary>
    /// Gets or sets whether on-demand bursting is enabled ("true" or "false").
    /// </summary>
    public string? EnableBursting { get; set; }

    /// <summary>
    /// Gets or sets the encryption type of the disk.
    /// </summary>
    public string? EncryptionType { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of a Shared Image Gallery image version to use as the source.
    /// </summary>
    public string? GalleryImageReference { get; set; }

    /// <summary>
    /// Gets or sets the LUN of the data disk in the gallery image version.
    /// </summary>
    public int? GalleryImageReferenceLun { get; set; }

    /// <summary>
    /// Gets or sets the hypervisor generation (V1 or V2).
    /// </summary>
    public string? HyperVGeneration { get; set; }

    /// <summary>
    /// Gets or sets the Azure region/location for the disk.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of VMs that can attach to the disk.
    /// </summary>
    public int? MaxShares { get; set; }

    /// <summary>
    /// Gets or sets the network access policy.
    /// </summary>
    public string? NetworkAccessPolicy { get; set; }

    /// <summary>
    /// Gets or sets the Operating System type (Linux or Windows).
    /// </summary>
    public string? OsType { get; set; }

    /// <summary>
    /// Gets or sets the size of the disk in GB.
    /// </summary>
    public int? SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the storage SKU (e.g., Premium_LRS, Standard_LRS).
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the source to create the disk from (resource ID of a snapshot/disk, or a blob URI).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the tags for the disk in 'key=value' format.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the performance tier of the disk.
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Gets or sets the size in bytes of the content to be uploaded (including VHD footer).
    /// </summary>
    public long? UploadSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the security type of the managed disk (e.g., TrustedLaunch).
    /// </summary>
    public string? SecurityType { get; set; }

    /// <summary>
    /// Gets or sets the upload type (Upload or UploadWithSecurityData).
    /// </summary>
    public string? UploadType { get; set; }

    /// <summary>
    /// Gets or sets the availability zone.
    /// </summary>
    public string? Zone { get; set; }
}
