// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Pure validator for <c>azmcp azurebackup policy create</c> options. Surfaces shape and
/// missing-required-input problems before the request is forwarded to the Azure Backup
/// service. Service-contract concerns (e.g. "Diff and Full cannot share a day") are
/// intentionally NOT enforced here — the service decides.
/// </summary>
public static class PolicyCreateValidator
{
    private const string AnyFlag = "policy";

    /// <summary>
    /// Validates the supplied options. Caller surfaces every issue at once with status 400
    /// when <see cref="PolicyValidationResult.IsValid"/> is false.
    /// </summary>
    public static PolicyValidationResult Validate(PolicyCreateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var issues = new List<PolicyValidationIssue>();
        var workload = (options.WorkloadType ?? string.Empty).Trim();
        var family = ClassifyWorkload(workload);

        // Rule C: AKS gate — defer until structured AKS support lands.
        if (family == WorkloadFamily.Aks)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.WorkloadTypeName}",
                "AKS is not yet supported via this command (label/namespace/hook selectors are pending). " +
                "Use 'az dataprotection backup-policy create' or the Azure portal in the meantime."));
            return PolicyValidationResult.Fail(issues);
        }

        // Rule D: CosmosDB pass-through — no special validator action; fall through to common rules.

        ValidateRequiredInputs(options, family, issues);
        ValidateShape(options, family, issues);

        return issues.Count == 0 ? PolicyValidationResult.Ok : PolicyValidationResult.Fail(issues);
    }

    // ----- Rule A: required-input combinations -----
    private static void ValidateRequiredInputs(PolicyCreateOptions options, WorkloadFamily family, List<PolicyValidationIssue> issues)
    {
        // Continuous DPP workloads (AzureBlob, ADLS) reject any schedule/retention/archive flag.
        // Reported as shape errors, not required-input errors.
        if (family == WorkloadFamily.DppContinuous)
        {
            return;
        }

        // A.1 — at least one schedule or retention input must be supplied.
        if (!HasAnyScheduleOrRetentionInput(options))
        {
            issues.Add(new PolicyValidationIssue(
                AnyFlag,
                "Provide at least one schedule or retention flag " +
                "(e.g. --schedule-frequency, --schedule-times, --daily-retention-days)."));
        }

        // A.2 — Weekly schedule requires --schedule-days-of-week.
        if (IsRsvWeekly(options.ScheduleFrequency) && string.IsNullOrWhiteSpace(options.ScheduleDaysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleDaysOfWeekName}",
                "Weekly schedules require --schedule-days-of-week."));
        }

        // A.3 — Hourly schedule requires all three hourly inputs.
        if (IsRsvHourly(options.ScheduleFrequency))
        {
            if (string.IsNullOrWhiteSpace(options.HourlyIntervalHours) ||
                string.IsNullOrWhiteSpace(options.HourlyWindowStartTime) ||
                string.IsNullOrWhiteSpace(options.HourlyWindowDurationHours))
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                    "Hourly schedules require --hourly-interval-hours, --hourly-window-start-time, and --hourly-window-duration-hours."));
            }
        }

        // A.4 — Weekly retention requires both weeks and days-of-week.
        if (IsPartialWeeklyRetention(options))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.WeeklyRetentionWeeksName}",
                "Weekly retention requires both --weekly-retention-weeks and --weekly-retention-days-of-week."));
        }

        // A.5 — Monthly retention requires months plus a complete relative OR absolute scheme.
        ValidateMonthlyRetention(options, issues);

        // A.6 — Yearly retention requires years, months, plus a complete relative OR absolute scheme.
        ValidateYearlyRetention(options, issues);

        // A.7 — Archive tier requires both --archive-tier-after-days and --archive-tier-mode.
        var hasArchiveDays = !string.IsNullOrWhiteSpace(options.ArchiveTierAfterDays);
        var hasArchiveMode = !string.IsNullOrWhiteSpace(options.ArchiveTierMode);
        if (hasArchiveDays ^ hasArchiveMode)
        {
            issues.Add(new PolicyValidationIssue(
                hasArchiveDays
                    ? $"--{AzureBackupOptionDefinitions.ArchiveTierModeName}"
                    : $"--{AzureBackupOptionDefinitions.ArchiveTierAfterDaysName}",
                "--archive-tier-after-days and --archive-tier-mode must be supplied together."));
        }

        // A.8 — DPP tier-duration flags require --data-store-type.
        var hasVaultDur = !string.IsNullOrWhiteSpace(options.VaultTierRetentionDuration);
        var hasArchiveDur = !string.IsNullOrWhiteSpace(options.ArchiveTierRetentionDuration);
        if ((hasVaultDur || hasArchiveDur) && string.IsNullOrWhiteSpace(options.DataStoreType))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.DataStoreTypeName}",
                "--data-store-type is required when --vault-tier-retention-duration or --archive-tier-retention-duration is supplied."));
        }
    }

    // ----- Rule B: shape (workload exclusivity) -----
    private static void ValidateShape(PolicyCreateOptions options, WorkloadFamily family, List<PolicyValidationIssue> issues)
    {
        // RSV-VM-only flags.
        EnsureFamily(options.PolicySubType,
            $"--{AzureBackupOptionDefinitions.PolicySubTypeName}",
            family, WorkloadFamily.RsvVm, "RSV VM", issues);
        EnsureFamily(options.InstantRpRetentionDays,
            $"--{AzureBackupOptionDefinitions.InstantRpRetentionDaysName}",
            family, WorkloadFamily.RsvVm, "RSV VM", issues);
        EnsureFamily(options.InstantRpResourceGroup,
            $"--{AzureBackupOptionDefinitions.InstantRpResourceGroupName}",
            family, WorkloadFamily.RsvVm, "RSV VM", issues);
        EnsureFamily(options.SnapshotConsistency,
            $"--{AzureBackupOptionDefinitions.SnapshotConsistencyName}",
            family, WorkloadFamily.RsvVm, "RSV VM", issues);

        // RSV VmWorkload (SQL / SAPHANA / SAPASE).
        EnsureFamily(options.FullScheduleFrequency,
            $"--{AzureBackupOptionDefinitions.FullScheduleFrequencyName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);
        EnsureFamily(options.FullScheduleDaysOfWeek,
            $"--{AzureBackupOptionDefinitions.FullScheduleDaysOfWeekName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);
        EnsureFamily(options.DifferentialScheduleDaysOfWeek,
            $"--{AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeekName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);
        EnsureFamily(options.DifferentialRetentionDays,
            $"--{AzureBackupOptionDefinitions.DifferentialRetentionDaysName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);
        EnsureFamily(options.LogFrequencyMinutes,
            $"--{AzureBackupOptionDefinitions.LogFrequencyMinutesName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);
        EnsureFamily(options.LogRetentionDays,
            $"--{AzureBackupOptionDefinitions.LogRetentionDaysName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);
        EnsureFamily(options.IsCompression,
            $"--{AzureBackupOptionDefinitions.IsCompressionName}",
            family, WorkloadFamily.RsvVmWorkload, "RSV SQL / SAPHANA / SAPASE", issues);

        // Incremental flags are SAPHANA / SAPASE only.
        if (!string.IsNullOrWhiteSpace(options.IncrementalScheduleDaysOfWeek) &&
            !IsSapWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.IncrementalScheduleDaysOfWeekName}",
                "--incremental-schedule-days-of-week is supported only for SAPHANA / SAPASE workloads."));
        }
        if (!string.IsNullOrWhiteSpace(options.IncrementalRetentionDays) &&
            !IsSapWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.IncrementalRetentionDaysName}",
                "--incremental-retention-days is supported only for SAPHANA / SAPASE workloads."));
        }

        // --is-sql-compression is SQL-only.
        if (!string.IsNullOrWhiteSpace(options.IsSqlCompression) &&
            !IsSqlWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.IsSqlCompressionName}",
                "--is-sql-compression is supported only for SQL workloads."));
        }

        // Hourly schedules are RSV only.
        if (IsRsvHourly(options.ScheduleFrequency) && !IsRsvFamily(family))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                "Hourly schedules are supported only for RSV workloads."));
        }

        // DPP-only flags.
        EnsureDpp(options.DataStoreType,
            $"--{AzureBackupOptionDefinitions.DataStoreTypeName}", family, issues);
        EnsureDpp(options.VaultTierRetentionDuration,
            $"--{AzureBackupOptionDefinitions.VaultTierRetentionDurationName}", family, issues);
        EnsureDpp(options.ArchiveTierRetentionDuration,
            $"--{AzureBackupOptionDefinitions.ArchiveTierRetentionDurationName}", family, issues);
        EnsureDpp(options.DatasourceTypes,
            $"--{AzureBackupOptionDefinitions.DatasourceTypesName}", family, issues);

        // Continuous DPP (Blob / ADLS) rejects every schedule, retention, and archive flag.
        if (family == WorkloadFamily.DppContinuous && HasAnyScheduleRetentionOrArchiveInput(options))
        {
            issues.Add(new PolicyValidationIssue(
                AnyFlag,
                "Continuous DPP workloads (AzureBlob, AzureDataLakeStorage) do not accept schedule, retention, or archive flags."));
        }
    }

    // ----- Helpers -----

    internal enum WorkloadFamily
    {
        Unknown,
        RsvVm,
        RsvVmWorkload,
        RsvFileShare,
        DppDiscrete,    // AzureDisk, ElasticSAN, PostgreSQLFlexible, CosmosDB
        DppContinuous,  // AzureBlob, AzureDataLakeStorage
        Aks,
    }

    private static bool IsRsvFamily(WorkloadFamily f) =>
        f is WorkloadFamily.RsvVm or WorkloadFamily.RsvVmWorkload or WorkloadFamily.RsvFileShare;

    private static WorkloadFamily ClassifyWorkload(string workloadType)
    {
        if (string.IsNullOrWhiteSpace(workloadType))
        {
            return WorkloadFamily.Unknown;
        }

        return workloadType.ToLowerInvariant() switch
        {
            "vm" or "azurevm" => WorkloadFamily.RsvVm,
            "sql" or "sqldatabase" or "saphana" or "saphanadatabase" or "sapase" => WorkloadFamily.RsvVmWorkload,
            "azurefileshare" => WorkloadFamily.RsvFileShare,
            "azuredisk" or "elasticsan" or "postgresqlflexible" or "cosmosdb" or "cosmos" => WorkloadFamily.DppDiscrete,
            "azureblob" or "adls" or "azuredatalakestorage" => WorkloadFamily.DppContinuous,
            "aks" => WorkloadFamily.Aks,
            _ => WorkloadFamily.Unknown,
        };
    }

    private static bool IsSapWorkload(string? workloadType) =>
        workloadType is not null &&
        (workloadType.Equals("SAPHANA", StringComparison.OrdinalIgnoreCase) ||
         workloadType.Equals("SAPHanaDatabase", StringComparison.OrdinalIgnoreCase) ||
         workloadType.Equals("SAPASE", StringComparison.OrdinalIgnoreCase));

    private static bool IsSqlWorkload(string? workloadType) =>
        workloadType is not null &&
        (workloadType.Equals("SQL", StringComparison.OrdinalIgnoreCase) ||
         workloadType.Equals("SQLDatabase", StringComparison.OrdinalIgnoreCase));

    private static bool IsRsvWeekly(string? frequency) =>
        string.Equals(frequency, "Weekly", StringComparison.OrdinalIgnoreCase);

    private static bool IsRsvHourly(string? frequency) =>
        string.Equals(frequency, "Hourly", StringComparison.OrdinalIgnoreCase);

    private static bool IsPartialWeeklyRetention(PolicyCreateOptions o)
    {
        var weeks = !string.IsNullOrWhiteSpace(o.WeeklyRetentionWeeks);
        var days = !string.IsNullOrWhiteSpace(o.WeeklyRetentionDaysOfWeek);
        return weeks ^ days;
    }

    private static void ValidateMonthlyRetention(PolicyCreateOptions o, List<PolicyValidationIssue> issues)
    {
        var months = !string.IsNullOrWhiteSpace(o.MonthlyRetentionMonths);
        var weekOf = !string.IsNullOrWhiteSpace(o.MonthlyRetentionWeekOfMonth);
        var daysOfWeek = !string.IsNullOrWhiteSpace(o.MonthlyRetentionDaysOfWeek);
        var daysOfMonth = !string.IsNullOrWhiteSpace(o.MonthlyRetentionDaysOfMonth);

        var anyMonthlyTagInput = weekOf || daysOfWeek || daysOfMonth;

        if (months && !anyMonthlyTagInput)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionMonthsName}",
                "Monthly retention requires either --monthly-retention-days-of-month (absolute) or " +
                "--monthly-retention-week-of-month + --monthly-retention-days-of-week (relative)."));
            return;
        }

        if (!months && anyMonthlyTagInput)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionMonthsName}",
                "Monthly retention day inputs require --monthly-retention-months."));
            return;
        }

        if (months && daysOfMonth && (weekOf || daysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonthName}",
                "Use either --monthly-retention-days-of-month (absolute) OR " +
                "--monthly-retention-week-of-month + --monthly-retention-days-of-week (relative), not both."));
        }
        else if (months && (weekOf ^ daysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                weekOf
                    ? $"--{AzureBackupOptionDefinitions.MonthlyRetentionDaysOfWeekName}"
                    : $"--{AzureBackupOptionDefinitions.MonthlyRetentionWeekOfMonthName}",
                "Relative monthly retention requires both --monthly-retention-week-of-month and --monthly-retention-days-of-week."));
        }
    }

    private static void ValidateYearlyRetention(PolicyCreateOptions o, List<PolicyValidationIssue> issues)
    {
        var years = !string.IsNullOrWhiteSpace(o.YearlyRetentionYears);
        var months = !string.IsNullOrWhiteSpace(o.YearlyRetentionMonths);
        var weekOf = !string.IsNullOrWhiteSpace(o.YearlyRetentionWeekOfMonth);
        var daysOfWeek = !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfWeek);
        var daysOfMonth = !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfMonth);

        var anyYearlyTagInput = months || weekOf || daysOfWeek || daysOfMonth;

        if (years && !months)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionMonthsName}",
                "Yearly retention requires --yearly-retention-months."));
        }

        if (years && months && !(weekOf || daysOfWeek || daysOfMonth))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionYearsName}",
                "Yearly retention requires either --yearly-retention-days-of-month (absolute) or " +
                "--yearly-retention-week-of-month + --yearly-retention-days-of-week (relative)."));
            return;
        }

        if (!years && anyYearlyTagInput)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionYearsName}",
                "Yearly retention inputs require --yearly-retention-years."));
            return;
        }

        if (years && daysOfMonth && (weekOf || daysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionDaysOfMonthName}",
                "Use either --yearly-retention-days-of-month (absolute) OR " +
                "--yearly-retention-week-of-month + --yearly-retention-days-of-week (relative), not both."));
        }
        else if (years && (weekOf ^ daysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                weekOf
                    ? $"--{AzureBackupOptionDefinitions.YearlyRetentionDaysOfWeekName}"
                    : $"--{AzureBackupOptionDefinitions.YearlyRetentionWeekOfMonthName}",
                "Relative yearly retention requires both --yearly-retention-week-of-month and --yearly-retention-days-of-week."));
        }
    }

    private static bool HasAnyScheduleOrRetentionInput(PolicyCreateOptions o) =>
        !string.IsNullOrWhiteSpace(o.ScheduleFrequency) ||
        !string.IsNullOrWhiteSpace(o.ScheduleTimes) ||
        !string.IsNullOrWhiteSpace(o.ScheduleDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.HourlyIntervalHours) ||
        !string.IsNullOrWhiteSpace(o.HourlyWindowStartTime) ||
        !string.IsNullOrWhiteSpace(o.HourlyWindowDurationHours) ||
        !string.IsNullOrWhiteSpace(o.DailyRetentionDays) ||
        !string.IsNullOrWhiteSpace(o.WeeklyRetentionWeeks) ||
        !string.IsNullOrWhiteSpace(o.WeeklyRetentionDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionMonths) ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionWeekOfMonth) ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionDaysOfMonth) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionYears) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionMonths) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionWeekOfMonth) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfMonth) ||
        !string.IsNullOrWhiteSpace(o.ArchiveTierAfterDays) ||
        !string.IsNullOrWhiteSpace(o.ArchiveTierMode) ||
        !string.IsNullOrWhiteSpace(o.VaultTierRetentionDuration) ||
        !string.IsNullOrWhiteSpace(o.ArchiveTierRetentionDuration) ||
        !string.IsNullOrWhiteSpace(o.DataStoreType) ||
        !string.IsNullOrWhiteSpace(o.FullScheduleFrequency) ||
        !string.IsNullOrWhiteSpace(o.LogFrequencyMinutes);

    private static bool HasAnyScheduleRetentionOrArchiveInput(PolicyCreateOptions o) =>
        HasAnyScheduleOrRetentionInput(o);

    private static void EnsureFamily(string? value, string flag, WorkloadFamily actual, WorkloadFamily required, string requiredLabel, List<PolicyValidationIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(value) && actual != required)
        {
            issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for {requiredLabel} workloads."));
        }
    }

    private static void EnsureDpp(string? value, string flag, WorkloadFamily actual, List<PolicyValidationIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(value) && actual is WorkloadFamily.RsvVm or WorkloadFamily.RsvVmWorkload or WorkloadFamily.RsvFileShare)
        {
            issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for DPP (Backup vault) workloads."));
        }
    }
}
