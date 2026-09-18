// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;

public class BackupPolicyUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account containing the backup policy.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the Azure NetApp Files backup policy to update. Must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string BackupPolicy { get; set; }

    [Option(Description = "The updated number of daily backups to retain. Must be zero or greater.")]
    public int? DailyBackupsToKeep { get; set; }

    [Option(Description = "The updated number of weekly backups to retain. Must be zero or greater.")]
    public int? WeeklyBackupsToKeep { get; set; }

    [Option(Description = "The updated number of monthly backups to retain. Must be zero or greater.")]
    public int? MonthlyBackupsToKeep { get; set; }

    [Option(Description = "Whether the backup policy is enabled.")]
    public bool? Enabled { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
