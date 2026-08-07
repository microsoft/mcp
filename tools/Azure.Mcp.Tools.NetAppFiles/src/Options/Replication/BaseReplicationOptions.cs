// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class BaseReplicationOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeName)]
    public string? Volume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AcquirePolicyTokenName)]
    public bool AcquirePolicyToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ChangeReferenceName)]
    public string? ChangeReference { get; set; }
}