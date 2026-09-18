// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;

public sealed class SnapshotPolicyUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account containing the snapshot policy.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the snapshot policy to update. Must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string SnapshotPolicy { get; set; }

    [Option(Description = "The Azure region for the snapshot policy (for example, 'eastus' or 'westus2'). The existing location is preserved when omitted.")]
    public string? Location { get; set; }

    [Option(Description = "Whether the snapshot policy is enabled.")]
    public bool? Enabled { get; set; }

    [Option(Description = "The minute within each hour when an hourly snapshot is taken. Valid values are 0-59.")]
    public int? HourlyMinute { get; set; }

    [Option(Description = "The number of hourly snapshots to retain. Valid values are 1-255.")]
    public int? HourlySnapshotsToKeep { get; set; }

    [Option(Description = "The UTC hour when a daily snapshot is taken. Valid values are 0-23.")]
    public int? DailyHour { get; set; }

    [Option(Description = "The minute when a daily snapshot is taken. Valid values are 0-59.")]
    public int? DailyMinute { get; set; }

    [Option(Description = "The number of daily snapshots to retain. Valid values are 1-255.")]
    public int? DailySnapshotsToKeep { get; set; }

    [Option(Description = "Comma-separated English weekday names when weekly snapshots are taken.")]
    public string? WeeklyDay { get; set; }

    [Option(Description = "The UTC hour when a weekly snapshot is taken. Valid values are 0-23.")]
    public int? WeeklyHour { get; set; }

    [Option(Description = "The minute when a weekly snapshot is taken. Valid values are 0-59.")]
    public int? WeeklyMinute { get; set; }

    [Option(Description = "The number of weekly snapshots to retain. Valid values are 1-255.")]
    public int? WeeklySnapshotsToKeep { get; set; }

    [Option(Description = "Comma-separated days of the month when monthly snapshots are taken. Valid values are 1-31.")]
    public string? MonthlyDaysOfMonth { get; set; }

    [Option(Description = "The UTC hour when a monthly snapshot is taken. Valid values are 0-23.")]
    public int? MonthlyHour { get; set; }

    [Option(Description = "The minute when a monthly snapshot is taken. Valid values are 0-59.")]
    public int? MonthlyMinute { get; set; }

    [Option(Description = "The number of monthly snapshots to retain. Valid values are 1-255.")]
    public int? MonthlySnapshotsToKeep { get; set; }

    [Option(Description = "Resource tags as a JSON key-value object. Providing an empty object removes all tags.")]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
