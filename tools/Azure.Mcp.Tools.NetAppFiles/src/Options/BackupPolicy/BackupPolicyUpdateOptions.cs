// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;

public class BackupPolicyUpdateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.BackupPolicy)]
    public string? BackupPolicy { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DailyBackupsToKeep)]
    public int? DailyBackupsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.WeeklyBackupsToKeep)]
    public int? WeeklyBackupsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.MonthlyBackupsToKeep)]
    public int? MonthlyBackupsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Enabled)]
    public bool? Enabled { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Ids)]
    public string[]? Ids { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Add)]
    public string[]? Add { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Set)]
    public string[]? Set { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Remove)]
    public string[]? Remove { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ForceString)]
    public bool ForceString { get; set; }
}
