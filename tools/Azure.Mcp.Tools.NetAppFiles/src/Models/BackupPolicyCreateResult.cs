// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Result of a backup policy create operation.
/// </summary>
public sealed record BackupPolicyCreateResult(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("dailyBackupsToKeep")] int? DailyBackupsToKeep,
    [property: JsonPropertyName("weeklyBackupsToKeep")] int? WeeklyBackupsToKeep,
    [property: JsonPropertyName("monthlyBackupsToKeep")] int? MonthlyBackupsToKeep,
    [property: JsonPropertyName("enabled")] bool? Enabled);
