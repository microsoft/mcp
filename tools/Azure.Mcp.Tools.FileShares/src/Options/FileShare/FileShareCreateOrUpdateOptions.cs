// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareCreateOrUpdateCommand.
/// </summary>
public class FileShareCreateOrUpdateOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share to create or update.
    /// </summary>
    public string? FileShareName { get; set; }

    /// <summary>
    /// Gets or sets the location for the file share.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the mount name of the file share as seen by the end user.
    /// </summary>
    public string? MountName { get; set; }

    /// <summary>
    /// Gets or sets the storage media tier (e.g., "SSD").
    /// </summary>
    public string? MediaTier { get; set; }

    /// <summary>
    /// Gets or sets the redundancy level (e.g., "Local", "Zone").
    /// </summary>
    public string? Redundancy { get; set; }

    /// <summary>
    /// Gets or sets the file sharing protocol (e.g., "NFS").
    /// </summary>
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the provisioned storage size in GiB.
    /// </summary>
    public int? ProvisionedStorageInGiB { get; set; }

    /// <summary>
    /// Gets or sets the provisioned IOPS.
    /// </summary>
    public int? ProvisionedIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the provisioned throughput in MiB/sec.
    /// </summary>
    public int? ProvisionedThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the public network access setting (e.g., "Enabled", "Disabled").
    /// </summary>
    public string? PublicNetworkAccess { get; set; }

    /// <summary>
    /// Gets or sets the NFS root squash setting (e.g., "NoRootSquash", "RootSquash", "AllSquash").
    /// </summary>
    public string? NfsRootSquash { get; set; }

    /// <summary>
    /// Gets or sets the allowed subnets for public access (comma-separated list).
    /// </summary>
    public string? AllowedSubnets { get; set; }

    /// <summary>
    /// Gets or sets the tags for the file share (JSON format).
    /// </summary>
    public string? Tags { get; set; }
}
