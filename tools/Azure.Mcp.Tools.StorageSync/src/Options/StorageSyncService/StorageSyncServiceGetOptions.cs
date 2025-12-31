// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for StorageSyncServiceGetCommand.
/// </summary>
public class StorageSyncServiceGetOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the name of the storage sync service.
    /// </summary>
    public string? Name { get; set; }
}
