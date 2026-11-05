// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Service-layer DTO for the <c>azmcp azurebackup policy update</c> command.
/// Mirrors the subset of <see cref="PolicyCreateRequest"/> that is meaningful to
/// apply as an in-place update against an existing RSV backup policy.
/// </summary>
/// <remarks>
/// Scope for the current stage of the parity work is <b>IaasVM (Azure VM) policies
/// only</b>. VmWorkload (SQL / SAP HANA / SAP ASE) and FileShare policies continue
/// to honour only <see cref="ScheduleTime"/> and <see cref="DailyRetentionDays"/>
/// for backward compatibility with existing recorded tests.
/// <para>
/// Every property is optional. Fields left <c>null</c> or zero preserve the
/// corresponding piece of the existing policy on the server; fields set by the
/// caller overwrite that piece using the same builder helpers that
/// <see cref="RsvPolicyBuilder"/> uses on create.
/// </para>
/// </remarks>
public sealed class PolicyUpdateRequest
{
    /// <summary>Required. Name of the policy being updated.</summary>
    public string Policy { get; set; } = string.Empty;

    // ===== Legacy back-compat (all workloads) =====

    /// <summary>Legacy single schedule time (24h HH:mm). Preserved for back-compat.</summary>
    public string? ScheduleTime { get; set; }

    /// <summary>Legacy daily retention override in days.</summary>
    public string? DailyRetentionDays { get; set; }

    // ===== IaasVM schedule (new) =====

    /// <summary>Windows time-zone identifier (e.g. "Pacific Standard Time").</summary>
    public string? TimeZone { get; set; }

    /// <summary>Schedule frequency: Daily, Weekly, or Hourly on an existing Enhanced policy.</summary>
    public string? ScheduleFrequency { get; set; }

    /// <summary>One Daily/Weekly backup time in HH:mm (e.g. "02:00").</summary>
    public string? ScheduleTimes { get; set; }

    /// <summary>Comma-separated days of the week (required with Weekly).</summary>
    public string? ScheduleDaysOfWeek { get; set; }

    // ===== IaasVM retention (new) =====

    public int WeeklyRetentionWeeks { get; set; }
    public string? WeeklyRetentionDaysOfWeek { get; set; }

    public int MonthlyRetentionMonths { get; set; }
    public string? MonthlyRetentionWeekOfMonth { get; set; }
    public string? MonthlyRetentionDaysOfWeek { get; set; }
    public string? MonthlyRetentionDaysOfMonth { get; set; }

    public int YearlyRetentionYears { get; set; }
    public string? YearlyRetentionMonths { get; set; }
    public string? YearlyRetentionWeekOfMonth { get; set; }
    public string? YearlyRetentionDaysOfWeek { get; set; }
    public string? YearlyRetentionDaysOfMonth { get; set; }

    public int? HourlyIntervalHours { get; set; }
    public string? HourlyWindowStartTime { get; set; }
    public int? HourlyWindowDurationHours { get; set; }
    public string? PolicySubType { get; set; }
    public string? InstantRpRetentionDays { get; set; }
    public string? InstantRpResourceGroup { get; set; }
    public string? SnapshotConsistency { get; set; }
    public string? ArchiveTierAfterDays { get; set; }
    public string? ArchiveTierMode { get; set; }
    public bool? SmartTier { get; set; }
    public string? PolicyTags { get; set; }

    internal static PolicyUpdateRequest FromOptions(Options.Policy.PolicyUpdateOptions options) => new()
    {
        Policy = options.Policy,
        ScheduleTime = options.ScheduleTime,
        DailyRetentionDays = options.DailyRetentionDays,
        TimeZone = options.TimeZone,
        ScheduleFrequency = options.ScheduleFrequency,
        ScheduleTimes = options.ScheduleTimes,
        ScheduleDaysOfWeek = options.ScheduleDaysOfWeek,
        WeeklyRetentionWeeks = options.WeeklyRetentionWeeks,
        WeeklyRetentionDaysOfWeek = options.WeeklyRetentionDaysOfWeek,
        MonthlyRetentionMonths = options.MonthlyRetentionMonths,
        MonthlyRetentionWeekOfMonth = options.MonthlyRetentionWeekOfMonth,
        MonthlyRetentionDaysOfWeek = options.MonthlyRetentionDaysOfWeek,
        MonthlyRetentionDaysOfMonth = options.MonthlyRetentionDaysOfMonth,
        YearlyRetentionYears = options.YearlyRetentionYears,
        YearlyRetentionMonths = options.YearlyRetentionMonths,
        YearlyRetentionWeekOfMonth = options.YearlyRetentionWeekOfMonth,
        YearlyRetentionDaysOfWeek = options.YearlyRetentionDaysOfWeek,
        YearlyRetentionDaysOfMonth = options.YearlyRetentionDaysOfMonth,
        HourlyIntervalHours = options.HourlyIntervalHours,
        HourlyWindowStartTime = options.HourlyWindowStartTime,
        HourlyWindowDurationHours = options.HourlyWindowDurationHours,
        PolicySubType = options.PolicySubType,
        InstantRpRetentionDays = options.InstantRpRetentionDays,
        InstantRpResourceGroup = options.InstantRpResourceGroup,
        SnapshotConsistency = options.SnapshotConsistency,
        ArchiveTierAfterDays = options.ArchiveTierAfterDays,
        ArchiveTierMode = options.ArchiveTierMode,
        SmartTier = options.SmartTier,
        PolicyTags = options.PolicyTags,
    };

    /// <summary>
    /// True when the caller supplied any of the new IaasVM-parity fields.
    /// Used to reject VM-only options on other workloads. All VM updates,
    /// including legacy schedule-time / retention-days, use the same merger.
    /// </summary>
    public bool HasIaasVmExtendedFlags()
    {
        return TimeZone is not null
            || ScheduleFrequency is not null
            || ScheduleTimes is not null
            || ScheduleDaysOfWeek is not null
            || WeeklyRetentionWeeks != 0
            || WeeklyRetentionDaysOfWeek is not null
            || MonthlyRetentionMonths != 0
            || MonthlyRetentionWeekOfMonth is not null
            || MonthlyRetentionDaysOfWeek is not null
            || MonthlyRetentionDaysOfMonth is not null
            || YearlyRetentionYears != 0
            || YearlyRetentionMonths is not null
            || YearlyRetentionWeekOfMonth is not null
            || YearlyRetentionDaysOfWeek is not null
            || YearlyRetentionDaysOfMonth is not null
            || HourlyIntervalHours.HasValue || HourlyWindowStartTime is not null || HourlyWindowDurationHours.HasValue
            || PolicySubType is not null || InstantRpRetentionDays is not null || InstantRpResourceGroup is not null
            || SnapshotConsistency is not null || ArchiveTierAfterDays is not null || ArchiveTierMode is not null
            || SmartTier.HasValue || PolicyTags is not null;
    }

    /// <summary>
    /// True when the caller supplied any input at all (legacy or new).
    /// If false, the update becomes a no-op.
    /// </summary>
    public bool HasAnyInput()
    {
        return ScheduleTime is not null
            || DailyRetentionDays is not null
            || HasIaasVmExtendedFlags();
    }
}
