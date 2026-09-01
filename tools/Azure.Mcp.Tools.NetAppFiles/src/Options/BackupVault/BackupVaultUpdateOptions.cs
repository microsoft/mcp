// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;

public class BackupVaultUpdateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.BackupVault)]
    public string? BackupVault { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Ids)]
    public string[]? Ids { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AcquirePolicyToken)]
    public bool AcquirePolicyToken { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ChangeReference)]
    public string? ChangeReference { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Add)]
    public string[]? Add { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Set)]
    public string[]? Set { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Remove)]
    public string[]? Remove { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ForceString)]
    public bool ForceString { get; set; }
}
