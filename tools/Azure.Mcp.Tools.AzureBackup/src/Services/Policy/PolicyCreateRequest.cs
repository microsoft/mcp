// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Service-layer DTO for the <c>azmcp azurebackup policy create</c> command.
/// Mirrors the user-facing flag surface (one-to-one with <c>PolicyCreateOptions</c>),
/// minus the ARM identity (vault / resource group / subscription / tenant) which is
/// passed as separate parameters to the service methods.
/// </summary>
/// <remarks>
/// All fields are nullable strings  -  the service layer is responsible for parsing,
/// validating, and translating them into SDK objects via the policy builders.
/// This decoupling keeps the System.CommandLine option-binding concerns out of the
/// service layer and makes builder unit tests easy to write.
/// </remarks>
public sealed class PolicyCreateRequest
{
    // Required identity
    public string Policy { get; set; } = string.Empty;
    public string WorkloadType { get; set; } = string.Empty;

    // Legacy daily retention (kept for backwards compatibility with existing live tests).
    public string? DailyRetentionDays { get; set; }

    // Common schedule flags
    public string? TimeZone { get; set; }
    public string? ScheduleFrequency { get; set; }
    public string? ScheduleTimes { get; set; }
    public string? ScheduleDaysOfWeek { get; set; }
    public string? HourlyIntervalHours { get; set; }
    public string? HourlyWindowStartTime { get; set; }
    public string? HourlyWindowDurationHours { get; set; }

    // Retention flags
    public string? WeeklyRetentionWeeks { get; set; }
    public string? WeeklyRetentionDaysOfWeek { get; set; }
    public string? MonthlyRetentionMonths { get; set; }
    public string? MonthlyRetentionWeekOfMonth { get; set; }
    public string? MonthlyRetentionDaysOfWeek { get; set; }
    public string? MonthlyRetentionDaysOfMonth { get; set; }
    public string? YearlyRetentionYears { get; set; }
    public string? YearlyRetentionMonths { get; set; }
    public string? YearlyRetentionWeekOfMonth { get; set; }
    public string? YearlyRetentionDaysOfWeek { get; set; }
    public string? YearlyRetentionDaysOfMonth { get; set; }
    public string? ArchiveTierAfterDays { get; set; }
    public string? ArchiveTierMode { get; set; }

    // RSV-VM only flags
    public string? PolicySubType { get; set; }
    public string? InstantRpRetentionDays { get; set; }
    public string? InstantRpResourceGroup { get; set; }
    public string? SnapshotConsistency { get; set; }

    // RSV-VmWorkload (SQL / SAPHANA / SAPASE) flags
    public string? FullScheduleFrequency { get; set; }
    public string? FullScheduleDaysOfWeek { get; set; }
    public string? DifferentialScheduleDaysOfWeek { get; set; }
    public string? DifferentialRetentionDays { get; set; }
    public string? IncrementalScheduleDaysOfWeek { get; set; }
    public string? IncrementalRetentionDays { get; set; }
    public string? LogFrequencyMinutes { get; set; }
    public string? LogRetentionDays { get; set; }
    public string? IsCompression { get; set; }
    public string? IsSqlCompression { get; set; }

    // ===== Stage 2 expansion =====

    public string? SmartTier { get; set; }
    public string? EnableSnapshotBackup { get; set; }
    public string? SnapshotInstantRpRetentionDays { get; set; }
    public string? SnapshotInstantRpResourceGroup { get; set; }
    public string? EnableVaultTierCopy { get; set; }
    public string? VaultTierCopyAfterDays { get; set; }
    public string? BackupMode { get; set; }
    public string? PitrRetentionDays { get; set; }
    public string? PolicyTags { get; set; }
    public string? AksSnapshotResourceGroup { get; set; }
    public string? AksIncludedNamespaces { get; set; }
    public string? AksExcludedNamespaces { get; set; }
    public string? AksLabelSelectors { get; set; }
    public string? AksIncludeClusterScopeResources { get; set; }
}
