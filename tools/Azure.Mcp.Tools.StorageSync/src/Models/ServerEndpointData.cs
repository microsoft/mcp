namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Represents Azure File Sync Server Endpoint data.
/// </summary>
public class ServerEndpointData
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
    /// Gets or sets the server endpoint properties.
    /// </summary>
    public ServerEndpointProperties? Properties { get; set; }
}

/// <summary>
/// Represents Server Endpoint properties.
/// </summary>
public class ServerEndpointProperties
{
    /// <summary>
    /// Gets or sets the server resource identifier.
    /// </summary>
    public string? ServerResourceId { get; set; }

    /// <summary>
    /// Gets or sets the local server path.
    /// </summary>
    public string? ServerLocalPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether cloud tiering is enabled.
    /// </summary>
    public bool? CloudTiering { get; set; }

    /// <summary>
    /// Gets or sets the volume free space percentage for cloud tiering.
    /// </summary>
    public int? VolumeFreeSpacePercent { get; set; }

    /// <summary>
    /// Gets or sets the age in days for tiering old files.
    /// </summary>
    public int? TierFilesOlderThanDays { get; set; }

    /// <summary>
    /// Gets or sets the sync status.
    /// </summary>
    public string? SyncStatus { get; set; }
}
