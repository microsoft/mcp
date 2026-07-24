// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskCreate command.
/// </summary>
public sealed class DiskCreateOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskName, Aliases = ["name"])]
    public required string DiskName { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk access resource.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskAccess)]
    public string? DiskAccess { get; set; }

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
    /// Gets or sets the resource ID of the disk encryption set.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskEncryptionSet)]
    public string? DiskEncryptionSet { get; set; }

    /// <summary>
    /// Gets or sets whether on-demand bursting is enabled ("true" or "false").
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.EnableBursting)]
    public bool? EnableBursting { get; set; }

    /// <summary>
    /// Gets or sets the encryption type of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.EncryptionType)]
    public string? EncryptionType { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of a Shared Image Gallery image version to use as the source.
    /// </summary>
    [Option(Description = "Resource ID of a Shared Image Gallery image version to use as the source for the disk. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/galleries/{gallery}/images/{image}/versions/{version}.")]
    public string? GalleryImageReference { get; set; }

    /// <summary>
    /// Gets or sets the LUN of the data disk in the gallery image version.
    /// </summary>
    [Option(Description = "LUN (Logical Unit Number) of the data disk in the gallery image version. If specified, the disk is created from the data disk at this LUN. If not specified, the disk is created from the OS disk of the image.")]
    public int? GalleryImageReferenceLun { get; set; }

    /// <summary>
    /// Gets or sets the hypervisor generation (V1 or V2).
    /// </summary>
    [Option(Description = "The hypervisor generation of the Virtual Machine. Applicable to OS disks only. Accepted values: V1, V2.", Name = "hyper-v-generation")]
    public string? HyperVGeneration { get; set; }

    /// <summary>
    /// Gets or sets the Azure region/location for the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Location, Aliases = ["l"])]
    public string? Location { get; set; }

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
    /// Gets or sets the Operating System type (Linux or Windows).
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.OsType)]
    public string? OsType { get; set; }

    /// <summary>
    /// Gets or sets the size of the disk in GB.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.SizeGb, Aliases = ["z"])]
    public int? SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the storage SKU (e.g., Premium_LRS, Standard_LRS).
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Sku)]
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the source to create the disk from (resource ID of a snapshot/disk, or a blob URI).
    /// </summary>
    [Option(Description = "Source to create the disk from, including a resource ID of a snapshot or disk, or a blob URI of a VHD. When a source is provided, --size-gb is optional and defaults to the source size.")]
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the tags for the disk in 'key=value' format.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Tags)]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the performance tier of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Tier)]
    public string? Tier { get; set; }

    /// <summary>
    /// Gets or sets the size in bytes of the content to be uploaded (including VHD footer).
    /// </summary>
    [Option(Description = "The size in bytes (including the VHD footer of 512 bytes) of the content to be uploaded. Required when --upload-type is specified.")]
    public long? UploadSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the security type of the managed disk (e.g., TrustedLaunch).
    /// </summary>
    [Option(Description = "Security type of the managed disk. Accepted values: ConfidentialVM_DiskEncryptedWithCustomerKey, ConfidentialVM_DiskEncryptedWithPlatformKey, ConfidentialVM_VMGuestStateOnlyEncryptedWithPlatformKey, Standard, TrustedLaunch. Required when --upload-type is UploadWithSecurityData.")]
    public string? SecurityType { get; set; }

    /// <summary>
    /// Gets or sets the upload type (Upload or UploadWithSecurityData).
    /// </summary>
    [Option(Description = "Type of upload for the disk. Accepted values: Upload, UploadWithSecurityData. When specified, the disk is created in a ReadyToUpload state.")]
    public string? UploadType { get; set; }

    /// <summary>
    /// Gets or sets the availability zone.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.Zone)]
    public string? Zone { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
