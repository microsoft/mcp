// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;

public class SnapshotUpdateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeName)]
    public string? Volume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotName)]
    public string? Snapshot { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AcquirePolicyTokenName)]
    public bool AcquirePolicyToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ChangeReferenceName)]
    public string? ChangeReference { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AddName)]
    public string[]? Add { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SetName)]
    public string[]? Set { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoveName)]
    public string[]? Remove { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ForceStringName)]
    public bool ForceString { get; set; }
}
