// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;

public class SnapshotGetOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeName)]
    public string? Volume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotName)]
    public string? Snapshot { get; set; }
}
