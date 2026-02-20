// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskCreate command.
/// </summary>
public class DiskCreateOptions : BaseComputeOptions
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    public string? Disk { get; set; }

    /// <summary>
    /// Gets or sets the size of the disk in GB.
    /// </summary>
    public int? SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the storage SKU (e.g., Premium_LRS, Standard_LRS).
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Gets or sets the Azure region/location for the disk.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the Operating System type (Linux or Windows).
    /// </summary>
    public string? OsType { get; set; }

    /// <summary>
    /// Gets or sets the availability zone.
    /// </summary>
    public string? Zone { get; set; }

    /// <summary>
    /// Gets or sets the hypervisor generation (V1 or V2).
    /// </summary>
    public string? HyperVGeneration { get; set; }
}
