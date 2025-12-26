namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for SyncGroupListCommand.
/// </summary>
public class SyncGroupListOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    public string? StorageSyncServiceName { get; set; }
}
