// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;

public class BackupPolicyUpdateOptions : BaseNetAppFilesOptions
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

    [JsonPropertyName(NetAppFilesOptionDefinitions.EnabledName)]
    public bool? Enabled { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AddName)]
    public string[]? Add { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SetName)]
    public string[]? Set { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoveName)]
    public string[]? Remove { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ForceStringName)]
    public bool ForceString { get; set; }
}
