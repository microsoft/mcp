// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class BaseReplicationActionOptions : BaseReplicationOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }
}