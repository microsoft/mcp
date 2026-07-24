// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationApproveOptions : BaseReplicationActionOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoteVolumeResourceIdName)]
    public string? RemoteVolumeResourceId { get; set; }
}