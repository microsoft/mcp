// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Backup;

public class BackupGetOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupVaultName)]
    public string? BackupVault { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupName)]
    public string? Backup { get; set; }
}
