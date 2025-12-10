namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Represents Azure File Sync Sync Group data.
/// </summary>
public class SyncGroupData
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the sync group properties.
    /// </summary>
    public SyncGroupProperties? Properties { get; set; }
}

/// <summary>
/// Represents Sync Group properties.
/// </summary>
public class SyncGroupProperties
{
    /// <summary>
    /// Gets or sets the sync state.
    /// </summary>
    public string? SyncState { get; set; }
}
