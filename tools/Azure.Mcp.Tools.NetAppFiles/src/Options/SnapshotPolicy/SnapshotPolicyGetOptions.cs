// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;

public class SnapshotPolicyGetOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotPolicyName)]
    public string? SnapshotPolicy { get; set; }
}
