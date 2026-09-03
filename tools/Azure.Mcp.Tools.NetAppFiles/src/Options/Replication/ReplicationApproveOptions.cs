// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationApproveOptions : BaseReplicationActionOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.RemoteVolumeResourceId)]
    public string? RemoteVolumeResourceId { get; set; }
}