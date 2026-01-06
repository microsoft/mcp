// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for SyncGroupGetCommand.
/// </summary>
public class SyncGroupGetOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    public string? StorageSyncServiceName { get; set; }

    /// <summary>
    /// Gets or sets the sync group name.
    /// </summary>
    public string? SyncGroupName { get; set; }
}
