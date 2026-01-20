// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for CloudEndpointCreateCommand.
/// </summary>
public class CloudEndpointCreateOptions : BaseStorageSyncOptions
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
    /// Gets or sets the storage account resource ID.
    /// </summary>
    public string? StorageAccountResourceId { get; set; }

    /// <summary>
    /// Gets or sets the Azure file share name.
    /// </summary>
    public string? AzureFileShareName { get; set; }
}
