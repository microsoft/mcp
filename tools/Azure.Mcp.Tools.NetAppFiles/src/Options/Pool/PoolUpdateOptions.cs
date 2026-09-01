// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Pool;

public class PoolUpdateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.Pool)]
    public string? Pool { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Size)]
    public long? Size { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.QosType)]
    public string? QosType { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CoolAccess)]
    public bool? CoolAccess { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ServiceLevel)]
    public string? ServiceLevel { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SizeInBytes)]
    public long? SizeInBytes { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CustomThroughputMibps)]
    public long? CustomThroughputMibps { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }
}
