// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public sealed class PolicyUpdateOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Policy)]
    public required string Policy { get; set; }

    // ===== Legacy back-compat =====

    [Option(Description = "Backup schedule time in 24h HH:mm format (e.g., '02:00'). Legacy single-time flag; prefer --schedule-times for the new IaasVM parity surface.")]
    public string? ScheduleTime { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.DailyRetentionDays)]
    public string? DailyRetentionDays { get; set; }

    // ===== IaasVM policy update parity (RSV Azure VM only) =====
    // Every field below is optional. Setting any one of them opts into the extended
    // update path that reshapes the existing IaasVmProtectionPolicy schedule and/or
    // retention using the same builder rules as `azurebackup policy create`. Other
    // workload types (VmWorkload SQL/HANA/ASE, FileShare) ignore these fields and
    // continue to honour --schedule-time and --daily-retention-days only.

    [Option(Description = "Windows time-zone identifier for the backup schedule (e.g., 'UTC', 'Pacific Standard Time'). RSV Azure VM only.")]
    public string? TimeZone { get; set; }

    [Option(Description = "Backup schedule frequency: 'Daily' or 'Weekly'. Hourly, PolicySubType, and V2 schedules are not supported by update. RSV Azure VM only.")]
    public string? ScheduleFrequency { get; set; }

    [Option(Description = "Comma-separated list of backup times in 24h HH:mm format (e.g., '02:00' or '02:00,14:00'). Interpreted in --time-zone. RSV Azure VM only.")]
    public string? ScheduleTimes { get; set; }

    [Option(Description = "Comma-separated days of the week the backup should run (e.g., 'Monday,Wednesday,Friday'). Required for Weekly schedules. RSV Azure VM only.")]
    public string? ScheduleDaysOfWeek { get; set; }

    [Option(Description = "Number of weeks to keep weekly recovery points. Pair with --weekly-retention-days-of-week. RSV Azure VM only.")]
    public int WeeklyRetentionWeeks { get; set; }

    [Option(Description = "Comma-separated days of the week tagged for weekly retention (e.g., 'Sunday'). Required with --weekly-retention-weeks. RSV Azure VM only.")]
    public string? WeeklyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Number of months to keep monthly recovery points. Combine with EITHER --monthly-retention-days-of-month (absolute) OR --monthly-retention-week-of-month + --monthly-retention-days-of-week (relative). RSV Azure VM only.")]
    public int MonthlyRetentionMonths { get; set; }

    [Option(Description = "Week of the month for monthly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --monthly-retention-days-of-week. RSV Azure VM only.")]
    public string? MonthlyRetentionWeekOfMonth { get; set; }

    [Option(Description = "Comma-separated days of the week for monthly retention (e.g., 'Sunday'). Use with --monthly-retention-week-of-month. RSV Azure VM only.")]
    public string? MonthlyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the month for monthly retention (1-28 or 'Last'; e.g., '1,15,Last'). Mutually exclusive with --monthly-retention-week-of-month. RSV Azure VM only.")]
    public string? MonthlyRetentionDaysOfMonth { get; set; }

    [Option(Description = "Number of years to keep yearly recovery points. Combine with --yearly-retention-months and either --yearly-retention-days-of-month OR --yearly-retention-week-of-month + --yearly-retention-days-of-week. RSV Azure VM only.")]
    public int YearlyRetentionYears { get; set; }

    [Option(Description = "Comma-separated months tagged for yearly retention (e.g., 'January' or 'January,July'). RSV Azure VM only.")]
    public string? YearlyRetentionMonths { get; set; }

    [Option(Description = "Week of the month for yearly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --yearly-retention-days-of-week. RSV Azure VM only.")]
    public string? YearlyRetentionWeekOfMonth { get; set; }

    [Option(Description = "Comma-separated days of the week for yearly retention (e.g., 'Sunday'). Use with --yearly-retention-week-of-month. RSV Azure VM only.")]
    public string? YearlyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the month for yearly retention (1-28 or 'Last'). Mutually exclusive with --yearly-retention-week-of-month. RSV Azure VM only.")]
    public string? YearlyRetentionDaysOfMonth { get; set; }
}
