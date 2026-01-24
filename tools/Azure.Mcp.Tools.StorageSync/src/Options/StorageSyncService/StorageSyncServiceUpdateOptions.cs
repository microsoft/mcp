// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for StorageSyncServiceUpdateCommand.
/// </summary>
public class StorageSyncServiceUpdateOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the name of the storage sync service.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the incoming traffic policy.
    /// </summary>
    public string? IncomingTrafficPolicy { get; set; }

    /// <summary>
    /// Gets or sets tags for the resource.
    /// </summary>
    public Dictionary<string, object>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the managed service identity type.
    /// </summary>
    public string? IdentityType { get; set; }
}
