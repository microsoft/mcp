// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for ServerEndpointUpdateCommand.
/// </summary>
public class ServerEndpointUpdateOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    public string? StorageSyncServiceName { get; set; }

    /// <summary>
    /// Gets or sets the sync group name.
    /// </summary>
    public string? SyncGroupName { get; set; }

    /// <summary>
    /// Gets or sets the server endpoint name.
    /// </summary>
    public string? ServerEndpointName { get; set; }

    /// <summary>
    /// Gets or sets whether cloud tiering is enabled.
    /// </summary>
    public bool? CloudTiering { get; set; }

    /// <summary>
    /// Gets or sets the volume free space percentage to maintain (1-99).
    /// </summary>
    public int? VolumeFreeSpacePercent { get; set; }

    /// <summary>
    /// Gets or sets the number of days after which files should be tiered if not accessed.
    /// </summary>
    public int? TierFilesOlderThanDays { get; set; }

    /// <summary>
    /// Gets or sets the local cache mode (DownloadNewAndModifiedFiles or UpdateLocallyCachedFiles).
    /// </summary>
    public string? LocalCacheMode { get; set; }
}
