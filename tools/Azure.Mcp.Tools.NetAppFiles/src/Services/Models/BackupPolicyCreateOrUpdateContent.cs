// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp backup policy via ARM generic resource API.
/// </summary>
internal sealed class BackupPolicyCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public BackupPolicyCreateProperties? Properties { get; set; }
}

internal sealed class BackupPolicyCreateProperties
{
    [JsonPropertyName("dailyBackupsToKeep")]
    public int? DailyBackupsToKeep { get; set; }

    [JsonPropertyName("weeklyBackupsToKeep")]
    public int? WeeklyBackupsToKeep { get; set; }

    [JsonPropertyName("monthlyBackupsToKeep")]
    public int? MonthlyBackupsToKeep { get; set; }
}
