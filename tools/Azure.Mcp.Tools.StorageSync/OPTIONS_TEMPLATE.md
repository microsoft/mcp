namespace Azure.Mcp.Tools.StorageSync.Options;

using Azure.Mcp.Core.Models;

/// <summary>
/// Template for command options classes.
/// Copy and customize for each command type.
/// </summary>
public class OptionsTemplate : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the name of the resource (varies by command type).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the ID of the resource (for RegisteredServer operations).
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Gets or sets optional resource properties for update operations.
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// Gets or sets tags for resource tagging.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets cloud tiering settings for server endpoints.
    /// </summary>
    public bool? CloudTiering { get; set; }

    /// <summary>
    /// Gets or sets volume free space percentage for cloud tiering.
    /// </summary>
    public int? VolumeFreeSpacePercent { get; set; }

    /// <summary>
    /// Gets or sets the age threshold in days for tiering old files.
    /// </summary>
    public int? TierFilesOlderThanDays { get; set; }

    /// <summary>
    /// Gets or sets the incoming traffic policy for the storage sync service.
    /// </summary>
    public string? IncomingTrafficPolicy { get; set; }

    /// <summary>
    /// Gets or sets the resource group for resource operations.
    /// </summary>
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the directory path for change detection operations.
    /// </summary>
    public string? DirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the list of file paths for change detection operations.
    /// </summary>
    public List<string>? FilePaths { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether change detection should be recursive.
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Gets or sets the storage account resource ID for cloud endpoints.
    /// </summary>
    public string? StorageAccountResourceId { get; set; }

    /// <summary>
    /// Gets or sets the Azure file share name for cloud endpoints.
    /// </summary>
    public string? AzureFileShareName { get; set; }

    /// <summary>
    /// Gets or sets the server resource ID for server endpoint operations.
    /// </summary>
    public string? ServerResourceId { get; set; }

    /// <summary>
    /// Gets or sets the local path on the server for server endpoints.
    /// </summary>
    public string? ServerLocalPath { get; set; }
}

// Usage example - copy this pattern for each command options class:
//
// namespace Azure.Mcp.Tools.StorageSync.Options;
//
// /// <summary>
// /// Options for StorageSyncServiceListCommand.
// /// </summary>
// public class StorageSyncServiceListOptions : BaseStorageSyncOptions
// {
//     /// <summary>
//     /// Gets or sets the resource group (optional for list operations).
//     /// </summary>
//     public string? ResourceGroup { get; set; }
// }
//
// namespace Azure.Mcp.Tools.StorageSync.Options;
//
// /// <summary>
// /// Options for StorageSyncServiceGetCommand.
// /// </summary>
// public class StorageSyncServiceGetOptions : BaseStorageSyncOptions
// {
//     /// <summary>
//     /// Gets or sets the resource group (required).
//     /// </summary>
//     public string? ResourceGroup { get; set; }
//
//     /// <summary>
//     /// Gets or sets the name of the storage sync service.
//     /// </summary>
//     public string? Name { get; set; }
// }
//
// ... and so on for each command type
