// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public class GovernanceSoftDeleteOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.SoftDeleteName)]
    public string? SoftDeleteState { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.SoftDeleteRetentionDaysName)]
    public string? SoftDeleteRetentionDays { get; set; }
}
