// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;

public class BackupVaultCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupVaultName)]
    public string? BackupVault { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AcquirePolicyTokenName)]
    public bool AcquirePolicyToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ChangeReferenceName)]
    public string? ChangeReference { get; set; }
}
