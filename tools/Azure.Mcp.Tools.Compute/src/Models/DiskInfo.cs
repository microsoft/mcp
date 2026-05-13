// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Represents an Azure Managed Disk.
/// </summary>
public class DiskInfo
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk.
    /// </summary>
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource group containing the disk.
    /// </summary>
    [JsonPropertyName("ResourceGroup")]
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the location of the disk.
    /// </summary>
    [JsonPropertyName("Location")]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the SKU name (e.g., Premium_LRS, Standard_LRS).
    /// </summary>
    [JsonPropertyName("SkuName")]
    public string? SkuName { get; set; }

    /// <summary>
    /// Gets or sets the SKU tier.
    /// </summary>
    [JsonPropertyName("SkuTier")]
    public string? SkuTier { get; set; }

    /// <summary>
    /// Gets or sets the size of the disk in GB.
    /// </summary>
    [JsonPropertyName("DiskSizeGB")]
    public int? DiskSizeGB { get; set; }

    /// <summary>
    /// Gets or sets the disk state (e.g., Attached, Unattached).
    /// </summary>
    [JsonPropertyName("DiskState")]
    public string? DiskState { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [JsonPropertyName("TimeCreated")]
    public DateTimeOffset? TimeCreated { get; set; }

    /// <summary>
    /// Gets or sets the OS type (Windows or Linux) if this is an OS disk.
    /// </summary>
    [JsonPropertyName("OSType")]
    public string? OSType { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    [JsonPropertyName("ProvisioningState")]
    public string? ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the disk.
    /// </summary>
    [JsonPropertyName("Tags")]
    public Dictionary<string, string>? Tags { get; set; }
}
