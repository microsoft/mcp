// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp backup policy with commonly useful metadata.
/// </summary>
public sealed record BackupPolicyInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("dailyBackupsToKeep")] int? DailyBackupsToKeep,
    [property: JsonPropertyName("weeklyBackupsToKeep")] int? WeeklyBackupsToKeep,
    [property: JsonPropertyName("monthlyBackupsToKeep")] int? MonthlyBackupsToKeep,
    [property: JsonPropertyName("volumeBackupsCount")] int? VolumeBackupsCount,
    [property: JsonPropertyName("enabled")] bool? Enabled);
