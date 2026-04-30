// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public class PolicyCreateOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.PolicyName)]
    public string? Policy { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.WorkloadTypeName)]
    public string? WorkloadType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ScheduleTimeName)]
    public string? ScheduleTime { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.DailyRetentionDaysName)]
    public string? DailyRetentionDays { get; set; }
}
