// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupUpdateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.VolumeGroup)]
    public string? VolumeGroup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.GroupDescription)]
    public string? GroupDescription { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Ids)]
    public string[]? Ids { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Add)]
    public string[]? Add { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Set)]
    public string[]? Set { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Remove)]
    public string[]? Remove { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ForceString)]
    public bool ForceString { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.GroupMetaData)]
    public string? GroupMetaData { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Volumes)]
    public string? Volumes { get; set; }
}
