// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationReestablishOptions : BaseReplicationActionOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.SourceVolumeIdName)]
    public string? SourceVolumeId { get; set; }
}