// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class ReplicationListOptions : BaseReplicationOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.Exclude)]
    public string? Exclude { get; set; }
}