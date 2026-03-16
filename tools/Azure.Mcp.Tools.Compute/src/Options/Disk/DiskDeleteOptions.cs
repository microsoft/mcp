// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskDelete command.
/// </summary>
public class DiskDeleteOptions : BaseComputeOptions
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    public string? DiskName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to force the deletion without confirmation.
    /// </summary>
    public bool Force { get; set; }
}
