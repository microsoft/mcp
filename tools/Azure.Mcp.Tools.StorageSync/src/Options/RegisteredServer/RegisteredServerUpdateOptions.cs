// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for RegisteredServerUpdateCommand.
/// </summary>
public class RegisteredServerUpdateOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    public string? StorageSyncServiceName { get; set; }

    /// <summary>
    /// Gets or sets the registered server ID.
    /// </summary>
    public string? RegisteredServerId { get; set; }
}
