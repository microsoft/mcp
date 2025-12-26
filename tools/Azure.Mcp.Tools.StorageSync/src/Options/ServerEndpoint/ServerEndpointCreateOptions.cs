// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for ServerEndpointCreateCommand.
/// </summary>
public class ServerEndpointCreateOptions : BaseStorageSyncOptions
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
    /// Gets or sets the server resource ID.
    /// </summary>
    public string? ServerResourceId { get; set; }

    /// <summary>
    /// Gets or sets the server local path.
    /// </summary>
    public string? ServerLocalPath { get; set; }
}
