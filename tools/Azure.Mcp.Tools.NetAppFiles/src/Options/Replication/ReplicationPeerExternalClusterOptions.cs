// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationPeerExternalClusterOptions : BaseReplicationActionOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PeerIpAddressesName)]
    public string[]? PeerIpAddresses { get; set; }
}