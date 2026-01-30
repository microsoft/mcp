// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.Compute.Options;

/// <summary>
/// Static option definitions for Azure Compute commands.
/// </summary>
public static class ComputeOptionDefinitions
{
    public const string DiskName = "disk";

    /// <summary>
    /// The name of the disk.
    /// </summary>
    public static Option<string> Disk { get; } = new($"--{DiskName}")
    {
        Description = "The name of the disk",
        Required = false
    };
}
