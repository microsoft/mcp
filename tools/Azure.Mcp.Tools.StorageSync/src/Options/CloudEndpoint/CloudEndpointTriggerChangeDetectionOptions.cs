// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for CloudEndpointTriggerChangeDetectionCommand.
/// </summary>
public class CloudEndpointTriggerChangeDetectionOptions : BaseStorageSyncOptions
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
    /// Gets or sets the cloud endpoint name.
    /// </summary>
    public string? CloudEndpointName { get; set; }

    /// <summary>
    /// Gets or sets the relative path to a directory Azure File share for which change detection is to be performed.
    /// </summary>
    public string? DirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the change detection mode. Applies to a directory specified in directoryPath parameter.
    /// </summary>
    public string? ChangeDetectionMode { get; set; }

    /// <summary>
    /// Gets or sets the array of relative paths on the Azure File share to be included in the change detection. Can be files and directories.
    /// </summary>
    public IList<string>? Paths { get; set; }
}
