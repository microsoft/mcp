// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskGet command.
/// </summary>
public class DiskGetOptions : BaseComputeOptions
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    public string? Disk { get; set; }
}
