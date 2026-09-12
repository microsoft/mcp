// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Sub-resource group IDs supported by Recovery Services vault private endpoints.</summary>
public enum AzureBackupPrivateEndpointGroupId
{
    /// <summary>Target the vault's primary region.</summary>
    [JsonStringEnumMemberName("AzureBackup")]
    Primary,

    /// <summary>Target the vault's paired region for Cross-Region Restore.</summary>
    [JsonStringEnumMemberName("AzureBackup_secondary")]
    Secondary
}
