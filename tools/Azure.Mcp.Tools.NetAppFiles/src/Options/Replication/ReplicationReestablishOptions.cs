// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationReestablishOptions : BaseReplicationActionOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.SourceVolumeId)]
    public string? SourceVolumeId { get; set; }
}