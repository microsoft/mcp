// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for StorageSyncServiceCreateCommand.
/// </summary>
public class StorageSyncServiceCreateOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the name of the storage sync service to create.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the location for the service.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets tags for the resource.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }
}
