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

    /// <summary>Schedule frequency: "Daily" or "Weekly".</summary>
    public string? ScheduleFrequency { get; set; }

    /// <summary>Comma-separated backup times in HH:mm (e.g. "02:00" or "02:00,14:00").</summary>
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

    /// <summary>
    /// True when the caller supplied any of the new IaasVM-parity fields.
    /// Used by <c>RsvBackupOperations.UpdatePolicyAsync</c> to decide whether to
    /// switch on the new merger path or keep the legacy schedule-time /
    /// retention-days path.
    /// </summary>
    public bool HasIaasVmExtendedFlags()
    {
        return !string.IsNullOrWhiteSpace(TimeZone)
            || !string.IsNullOrWhiteSpace(ScheduleFrequency)
            || !string.IsNullOrWhiteSpace(ScheduleTimes)
            || !string.IsNullOrWhiteSpace(ScheduleDaysOfWeek)
            || WeeklyRetentionWeeks > 0
            || !string.IsNullOrWhiteSpace(WeeklyRetentionDaysOfWeek)
            || MonthlyRetentionMonths > 0
            || !string.IsNullOrWhiteSpace(MonthlyRetentionWeekOfMonth)
            || !string.IsNullOrWhiteSpace(MonthlyRetentionDaysOfWeek)
            || !string.IsNullOrWhiteSpace(MonthlyRetentionDaysOfMonth)
            || YearlyRetentionYears > 0
            || !string.IsNullOrWhiteSpace(YearlyRetentionMonths)
            || !string.IsNullOrWhiteSpace(YearlyRetentionWeekOfMonth)
            || !string.IsNullOrWhiteSpace(YearlyRetentionDaysOfWeek)
            || !string.IsNullOrWhiteSpace(YearlyRetentionDaysOfMonth);
    }

    /// <summary>
    /// True when the caller supplied any input at all (legacy or new).
    /// If false, the update becomes a no-op.
    /// </summary>
    public bool HasAnyInput()
    {
        return !string.IsNullOrWhiteSpace(ScheduleTime)
            || !string.IsNullOrWhiteSpace(DailyRetentionDays)
            || HasIaasVmExtendedFlags();
    }
}
