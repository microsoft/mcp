// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for CloudEndpointDeleteCommand.
/// </summary>
public class CloudEndpointDeleteOptions : BaseStorageSyncOptions
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
}
