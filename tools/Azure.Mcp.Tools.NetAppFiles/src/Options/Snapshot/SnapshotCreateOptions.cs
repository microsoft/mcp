// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;

public class SnapshotCreateOptions : BaseNetAppFilesOptions
{
    [Option(Description = "The name of the Azure NetApp Files account (e.g., 'myanfaccount').")]
    public new required string Account { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public new required string ResourceGroup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Pool)]
    public required string Pool { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Volume)]
    public required string Volume { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Snapshot)]
    public required string Snapshot { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AcquirePolicyToken)]
    public bool AcquirePolicyToken { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ChangeReference)]
    public string? ChangeReference { get; set; }
}
