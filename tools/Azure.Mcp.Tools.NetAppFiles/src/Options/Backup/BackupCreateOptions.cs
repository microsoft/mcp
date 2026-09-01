// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Backup;

public class BackupCreateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.BackupVault)]
    public string? BackupVault { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Backup)]
    public string? Backup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.VolumeResourceId)]
    public string? VolumeResourceId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Label)]
    public string? Label { get; set; }
}
