// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Backup;

public class BackupGetOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.BackupVault)]
    public string? BackupVault { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Backup)]
    public string? Backup { get; set; }
}
