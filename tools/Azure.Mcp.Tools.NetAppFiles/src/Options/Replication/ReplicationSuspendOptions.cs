// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationSuspendOptions : BaseReplicationActionOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.ForceBreakReplication)]
    public bool ForceBreakReplication { get; set; }
}