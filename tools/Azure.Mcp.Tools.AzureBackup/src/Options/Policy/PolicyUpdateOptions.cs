// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public sealed class PolicyUpdateOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Policy)]
    public required string Policy { get; set; }

    [Option(Description = "Backup schedule time in 24h HH:mm format (e.g., '02:00'). Used for policy update.")]
    public string? ScheduleTime { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.DailyRetentionDays)]
    public string? DailyRetentionDays { get; set; }
}
