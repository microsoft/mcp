// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;

public class BackupPolicyCreateOptions : BaseNetAppFilesOptions
{
    [Option(Description = "The name of the Azure NetApp Files account (e.g., 'myanfaccount').")]
    public new required string Account { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public new required string ResourceGroup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BackupPolicy)]
    public required string BackupPolicy { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public required string Location { get; set; }

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

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }
}
