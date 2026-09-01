// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Volume;

public class VolumeGetOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.Pool)]
    public string? Pool { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Volume)]
    public string? Volume { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Ids)]
    public string[]? Ids { get; set; }
}
