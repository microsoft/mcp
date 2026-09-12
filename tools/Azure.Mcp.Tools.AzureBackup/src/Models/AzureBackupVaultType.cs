// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Azure Backup vault implementations.</summary>
public enum AzureBackupVaultType
{
    /// <summary>Recovery Services vault.</summary>
    [JsonStringEnumMemberName("rsv")]
    Rsv,

    /// <summary>Data Protection Backup vault.</summary>
    [JsonStringEnumMemberName("dpp")]
    Dpp
}
