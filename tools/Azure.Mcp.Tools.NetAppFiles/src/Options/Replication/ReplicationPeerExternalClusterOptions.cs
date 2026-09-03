// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationPeerExternalClusterOptions : BaseReplicationActionOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.PeerIpAddresses)]
    public string[]? PeerIpAddresses { get; set; }
}