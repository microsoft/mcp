// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationSuspendOptions : BaseReplicationActionOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.ForceBreakReplicationName)]
    public bool ForceBreakReplication { get; set; }
}