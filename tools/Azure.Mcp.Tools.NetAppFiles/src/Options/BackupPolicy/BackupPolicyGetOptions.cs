// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;

public class BackupPolicyGetOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupPolicyName)]
    public string? BackupPolicy { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }
}
