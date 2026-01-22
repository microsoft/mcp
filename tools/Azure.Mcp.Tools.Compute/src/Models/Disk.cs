// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Represents an Azure Managed Disk.
/// </summary>
public class Disk
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the disk.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource group containing the disk.
    /// </summary>
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the location of the disk.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the SKU name (e.g., Premium_LRS, Standard_LRS).
    /// </summary>
    public string? SkuName { get; set; }

    /// <summary>
    /// Gets or sets the SKU tier.
    /// </summary>
    public string? SkuTier { get; set; }

    /// <summary>
    /// Gets or sets the size of the disk in GB.
    /// </summary>
    public int? DiskSizeGB { get; set; }

    /// <summary>
    /// Gets or sets the disk state (e.g., Attached, Unattached).
    /// </summary>
    public string? DiskState { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset? TimeCreated { get; set; }

    /// <summary>
    /// Gets or sets the OS type (Windows or Linux) if this is an OS disk.
    /// </summary>
    public string? OSType { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    public string? ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the disk.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }
}
