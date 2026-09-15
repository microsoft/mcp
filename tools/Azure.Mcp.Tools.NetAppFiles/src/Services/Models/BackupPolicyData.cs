// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp backup policy from Resource Graph.
/// </summary>
internal sealed class BackupPolicyData
{
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }

    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName("properties")]
    public BackupPolicyProperties? Properties { get; set; }

    public static BackupPolicyData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.BackupPolicyData);
    }
}

internal sealed class BackupPolicyProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("dailyBackupsToKeep")]
    public int? DailyBackupsToKeep { get; set; }

    [JsonPropertyName("weeklyBackupsToKeep")]
    public int? WeeklyBackupsToKeep { get; set; }

    [JsonPropertyName("monthlyBackupsToKeep")]
    public int? MonthlyBackupsToKeep { get; set; }

    [JsonPropertyName("volumeBackupsCount")]
    public int? VolumeBackupsCount { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}
