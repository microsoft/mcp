// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;

public class BackupPolicyCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupPolicyName)]
    public string? BackupPolicy { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.DailyBackupsToKeepName)]
    public int? DailyBackupsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.WeeklyBackupsToKeepName)]
    public int? WeeklyBackupsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyBackupsToKeepName)]
    public int? MonthlyBackupsToKeep { get; set; }
}
