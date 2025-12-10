namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for RegisteredServerRegisterCommand.
/// </summary>
public class RegisteredServerRegisterOptions : BaseStorageSyncOptions
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
