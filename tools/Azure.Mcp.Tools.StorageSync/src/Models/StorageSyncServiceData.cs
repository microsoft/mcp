namespace Azure.Mcp.Tools.StorageSync.Models;

using System.Collections.Generic;

/// <summary>
/// Represents Azure Storage Sync service data.
/// </summary>
public class StorageSyncServiceData
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
    /// Gets or sets the resource location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the resource tags.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the service properties.
    /// </summary>
    public StorageSyncServiceProperties? Properties { get; set; }
}

/// <summary>
/// Represents Storage Sync service properties.
/// </summary>
public class StorageSyncServiceProperties
{
    /// <summary>
    /// Gets or sets the incoming traffic policy.
    /// </summary>
    public string? IncomingTrafficPolicy { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    public string? ProvisioningState { get; set; }
}
