// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Selective disk backup modes for IaaS virtual machines.</summary>
public enum AzureBackupDiskListSetting
{
    /// <summary>Back up only the listed data disk LUNs.</summary>
    [JsonStringEnumMemberName("include")]
    Include,

    /// <summary>Back up all data disks except the listed LUNs.</summary>
    [JsonStringEnumMemberName("exclude")]
    Exclude,

    /// <summary>Clear the selective disk configuration.</summary>
    [JsonStringEnumMemberName("resetexclusionsettings")]
    ResetExclusionSettings
}
