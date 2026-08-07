// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Replication;

public class BaseReplicationActionOptions : BaseReplicationOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }
}