namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for RegisteredServerListCommand.
/// </summary>
public class RegisteredServerListOptions : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    public string? StorageSyncServiceName { get; set; }
}
