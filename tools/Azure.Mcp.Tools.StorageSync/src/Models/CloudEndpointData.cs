namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Represents Azure File Sync Cloud Endpoint data.
/// </summary>
public class CloudEndpointData
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
    /// Gets or sets the cloud endpoint properties.
    /// </summary>
    public CloudEndpointProperties? Properties { get; set; }
}

/// <summary>
/// Represents Cloud Endpoint properties.
/// </summary>
public class CloudEndpointProperties
{
    /// <summary>
    /// Gets or sets the Azure storage account resource ID.
    /// </summary>
    public string? StorageAccountResourceId { get; set; }

    /// <summary>
    /// Gets or sets the Azure file share name.
    /// </summary>
    public string? AzureFileShareName { get; set; }

    /// <summary>
    /// Gets or sets the partnership status.
    /// </summary>
    public string? PartnershipStatus { get; set; }
}
