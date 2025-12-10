namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Represents Azure File Sync Registered Server data.
/// </summary>
public class RegisteredServerData
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
    /// Gets or sets the registered server properties.
    /// </summary>
    public RegisteredServerProperties? Properties { get; set; }
}

/// <summary>
/// Represents Registered Server properties.
/// </summary>
public class RegisteredServerProperties
{
    /// <summary>
    /// Gets or sets the server ID.
    /// </summary>
    public string? ServerId { get; set; }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the management endpoint URL.
    /// </summary>
    public string? ManagementEndpointUrl { get; set; }

    /// <summary>
    /// Gets or sets the agent version.
    /// </summary>
    public string? AgentVersion { get; set; }

    /// <summary>
    /// Gets or sets the discovery status.
    /// </summary>
    public string? DiscoveryStatus { get; set; }
}
