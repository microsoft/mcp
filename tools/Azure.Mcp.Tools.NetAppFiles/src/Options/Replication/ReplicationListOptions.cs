// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationListOptions : BaseReplicationOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.ExcludeName)]
    public string? Exclude { get; set; }
}