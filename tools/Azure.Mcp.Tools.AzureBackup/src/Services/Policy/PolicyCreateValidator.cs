// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Validator for <c>azmcp azurebackup policy create</c> options. Surfaces shape and
/// missing-required-input problems before the request is forwarded to the Azure Backup
/// service, and also enforces selected workload-specific scheduling and retention
/// constraints that are validated client-side (for example, SQL differential/full
/// schedule and log-retention rules) to provide actionable error messages.
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

        // Rule D: CosmosDB pass-through  -  no special validator action; fall through to common rules.
        // (AKS gate removed in Stage 2  -  AKS now flows through normal DPP discrete validation.)

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

        // Storage workloads with --backup-mode unspecified or "Continuous" behave like DppContinuous  - 
        // no schedule/retention required. Vaulted-mode storage workloads use the regular validation.
        if (family == WorkloadFamily.DppStorageBackupMode && !IsVaultedBackupMode(options.BackupMode))
        {
            return;
        }

        // A.1  -  at least one schedule or retention input must be supplied.
        if (!HasAnyScheduleOrRetentionInput(options))
        {
            issues.Add(new PolicyValidationIssue(
                AnyFlag,
                "Provide at least one schedule or retention flag " +
                "(e.g. --schedule-frequency, --schedule-times, --daily-retention-days)."));
        }

        // A.2  -  Weekly schedule requires --schedule-days-of-week.
        if (IsRsvWeekly(options.ScheduleFrequency) && string.IsNullOrWhiteSpace(options.ScheduleDaysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleDaysOfWeekName}",
                "Weekly schedules require --schedule-days-of-week."));
        }

        // A.3  -  Hourly schedule requires all three hourly inputs.
        if (IsRsvHourly(options.ScheduleFrequency))
        {
            if (options.HourlyIntervalHours <= 0 ||
                string.IsNullOrWhiteSpace(options.HourlyWindowStartTime) ||
                options.HourlyWindowDurationHours <= 0)
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                    "Hourly schedules require --hourly-interval-hours, --hourly-window-start-time, and --hourly-window-duration-hours."));
            }
        }

        // A.4  -  Weekly retention requires both weeks and days-of-week.
        if (IsPartialWeeklyRetention(options))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.WeeklyRetentionWeeksName}",
                "Weekly retention requires both --weekly-retention-weeks and --weekly-retention-days-of-week."));
        }

        // A.5  -  Monthly retention requires months plus a complete relative OR absolute scheme.
        ValidateMonthlyRetention(options, issues);

        // A.6  -  Yearly retention requires years, months, plus a complete relative OR absolute scheme.
        ValidateYearlyRetention(options, issues);

        // A.7  -  Archive tier requires both --archive-tier-after-days and --archive-tier-mode.
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

        // A.8  -  Archive tier (ArchiveStore) is not supported by any DPP (Backup vault) datasource
        // today: AzureDisk, AKS, PostgreSQL Flexible Server, PostgreSQL, Cosmos DB, Elastic SAN,
        // AzureBlob, AzureDataLakeStorage. Reject any archive flag for these workloads with an
        // actionable message rather than letting the request reach the service and get rejected
        // as BMSUserErrorInvalidInput.
        if ((hasArchiveDays || hasArchiveMode) && IsArchiveUnsupportedDppFamily(family))
        {
            var workloadLabel = DescribeArchiveUnsupportedWorkload((options.WorkloadType ?? string.Empty).Trim(), family);
            issues.Add(new PolicyValidationIssue(
                hasArchiveMode
                    ? $"--{AzureBackupOptionDefinitions.ArchiveTierModeName}"
                    : $"--{AzureBackupOptionDefinitions.ArchiveTierAfterDaysName}",
                $"Archive tier is not supported for {workloadLabel} today. " +
                "Remove --archive-tier-after-days and --archive-tier-mode."));
        }

        // A.9  -  AzureDisk (DPP) does not support Yearly retention. Per the Disk DPP manifest,
        // allowedRetentionTagNames is [Daily, Weekly, Monthly] only. Reject --yearly-* with an
        // actionable message rather than letting the request reach the service and get rejected
        // as BMSUserErrorInvalidInput.
        if (IsAzureDiskWorkload(options.WorkloadType) && HasAnyYearlyRetentionInput(options))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionYearsName}",
                "AzureDisk does not support Yearly retention. " +
                "Allowed retention tags are Daily, Weekly, Monthly. " +
                "Remove --yearly-retention-* flags."));
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
        if (options.IncrementalRetentionDays > 0 &&
            !IsSapWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.IncrementalRetentionDaysName}",
                "--incremental-retention-days is supported only for SAPHANA / SAPASE workloads."));
        }

        // --is-sql-compression is SQL-only.
        if (options.IsSqlCompression &&
            !IsSqlWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.IsSqlCompressionName}",
                "--is-sql-compression is supported only for SQL workloads."));
        }

        // SQL Full and Differential cannot run on the same day. Detect the common cases:
        //   * Full schedule is Daily (runs every day) AND --differential-schedule-days-of-week is set.
        //   * Full schedule is Weekly AND any Differential day overlaps a Full day.
        if (IsSqlWorkload(options.WorkloadType) &&
            !string.IsNullOrWhiteSpace(options.DifferentialScheduleDaysOfWeek))
        {
            var fullFreq = (options.FullScheduleFrequency ?? string.Empty).Trim();
            if (string.Equals(fullFreq, "Daily", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(fullFreq))
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeekName}",
                    "SQL Full and Differential backups cannot run on the same day. " +
                    "Use --full-schedule-frequency Weekly with --full-schedule-days-of-week and choose Differential days that do not overlap."));
            }
            else if (string.Equals(fullFreq, "Weekly", StringComparison.OrdinalIgnoreCase) &&
                     !string.IsNullOrWhiteSpace(options.FullScheduleDaysOfWeek) &&
                     HasDayOverlap(options.FullScheduleDaysOfWeek, options.DifferentialScheduleDaysOfWeek))
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeekName}",
                    "SQL Full and Differential backups cannot run on the same day. " +
                    "Choose Differential days that do not overlap with --full-schedule-days-of-week."));
            }

            // SQL Differential supports exactly one day per week.
            var diffDays = SplitDays(options.DifferentialScheduleDaysOfWeek);
            if (diffDays.Count > 1)
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeekName}",
                    "SQL Differential backup supports exactly one day per week. " +
                    "Specify a single day in --differential-schedule-days-of-week."));
            }
        }

        // SQL Log retention constraints:
        //   * Minimum 7 days (service rejects lower values with UserErrorLogRetentionNotInValidRangeInPolicy).
        //   * Must be less than Differential retention (UserErrorLogRetentionMoreThanDiffRetentionInPolicy).
        if (IsSqlWorkload(options.WorkloadType))
        {
            if (options.LogRetentionDays > 0 && options.LogRetentionDays < 7)
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.LogRetentionDaysName}",
                    "SQL Log retention minimum is 7 days."));
            }

            if (options.LogRetentionDays > 0 &&
                options.DifferentialRetentionDays > 0 &&
                options.LogRetentionDays >= options.DifferentialRetentionDays)
            {
                issues.Add(new PolicyValidationIssue(
                    $"--{AzureBackupOptionDefinitions.LogRetentionDaysName}",
                    "SQL Log retention must be less than Differential retention. " +
                    $"Current: log={options.LogRetentionDays} days, differential={options.DifferentialRetentionDays} days."));
            }
        }

        // Hourly schedules are RSV only.
        if (IsRsvHourly(options.ScheduleFrequency) && !IsRsvFamily(family))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                "Hourly schedules are supported only for RSV workloads."));
        }

        // DPP schedule-frequency validation: DPP repeating intervals require ISO-8601 durations
        // (or the friendly aliases Daily/Weekly which we normalize). Reject unrecognized values
        // so users get a clear message instead of an opaque Azure API error.
        if (IsDppFamily(family) && !string.IsNullOrWhiteSpace(options.ScheduleFrequency) &&
            !IsKnownDppScheduleFrequency(options.ScheduleFrequency!))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                $"Unrecognized DPP schedule frequency '{options.ScheduleFrequency}'. " +
                "Valid values: Daily, Weekly, P1D, P1W, P2W, P1M, PT4H, PT6H, PT8H, PT12H."));
        }

        // Continuous DPP rejects schedule, retention, and archive flags. This covers both the legacy
        // DppContinuous family and DppStorageBackupMode workloads when --backup-mode is unset/Continuous.
        var isContinuousStorage = family == WorkloadFamily.DppContinuous ||
            (family == WorkloadFamily.DppStorageBackupMode && !IsVaultedBackupMode(options.BackupMode));
        if (isContinuousStorage && HasAnyScheduleRetentionOrArchiveInput(options))
        {
            issues.Add(new PolicyValidationIssue(
                AnyFlag,
                "Continuous DPP workloads (AzureBlob, AzureDataLakeStorage) do not accept schedule, retention, or archive flags."));
        }

        // ===== Stage 2 shape rules =====

        // --smart-tier is RSV VM only.
        if (options.SmartTier && family != WorkloadFamily.RsvVm)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.SmartTierName}",
                "--smart-tier is supported only for RSV VM workloads."));
        }

        // Snapshot/instance backup flags are SAPHANA only.
        if (options.EnableSnapshotBackup && !IsHanaWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue($"--{AzureBackupOptionDefinitions.EnableSnapshotBackupName}", $"--{AzureBackupOptionDefinitions.EnableSnapshotBackupName} is supported only for SAPHANA workloads."));
        }
        var snapshotFlags = new (string? value, string flag)[]
        {
            (options.SnapshotInstantRpRetentionDays, $"--{AzureBackupOptionDefinitions.SnapshotInstantRpRetentionDaysName}"),
            (options.SnapshotInstantRpResourceGroup, $"--{AzureBackupOptionDefinitions.SnapshotInstantRpResourceGroupName}"),
        };
        foreach (var (value, flag) in snapshotFlags)
        {
            if (!string.IsNullOrWhiteSpace(value) && !IsHanaWorkload(options.WorkloadType))
            {
                issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for SAPHANA workloads."));
            }
        }

        // Vault-tier copy flags are DPP AzureDisk only.
        if (options.EnableVaultTierCopy && !IsAzureDiskWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue($"--{AzureBackupOptionDefinitions.EnableVaultTierCopyName}", $"--{AzureBackupOptionDefinitions.EnableVaultTierCopyName} is supported only for DPP AzureDisk workloads."));
        }
        if (options.VaultTierCopyAfterDays > 0 && !IsAzureDiskWorkload(options.WorkloadType))
        {
            issues.Add(new PolicyValidationIssue($"--{AzureBackupOptionDefinitions.VaultTierCopyAfterDaysName}", $"--{AzureBackupOptionDefinitions.VaultTierCopyAfterDaysName} is supported only for DPP AzureDisk workloads."));
        }

        // Vault-tier copy partial input check.
        if (options.EnableVaultTierCopy && options.VaultTierCopyAfterDays <= 0)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.VaultTierCopyAfterDaysName}",
                "--enable-vault-tier-copy requires --vault-tier-copy-after-days."));
        }

        // --backup-mode is DPP storage only (Blob / ADLS).
        if (!string.IsNullOrWhiteSpace(options.BackupMode) && family != WorkloadFamily.DppStorageBackupMode)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.BackupModeName}",
                "--backup-mode is supported only for DPP AzureBlob and AzureDataLakeStorage workloads."));
        }

        // --pitr-retention-days is DPP storage continuous only.
        if (options.PitrRetentionDays > 0 &&
            family != WorkloadFamily.DppStorageBackupMode &&
            family != WorkloadFamily.DppContinuous)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.PitrRetentionDaysName}",
                "--pitr-retention-days is supported only for DPP AzureBlob and AzureDataLakeStorage continuous backups."));
        }

        // --policy-tags is RSV only (DPP backup policies are sub-resources of the vault and the SDK
        // does not expose Tags on DataProtectionBackupPolicyData).
        if (!string.IsNullOrWhiteSpace(options.PolicyTags) && !IsRsvFamily(family))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.PolicyTagsName}",
                "--policy-tags is supported only for Recovery Services vault (RSV) policies."));
        }

    }

    // ----- Helpers -----

    internal enum WorkloadFamily
    {
        Unknown,
        RsvVm,
        RsvVmWorkload,
        RsvFileShare,
        DppDiscrete,            // AzureDisk, ElasticSAN, PostgreSQLFlexible, CosmosDB
        DppContinuous,          // (legacy) AzureBlob, AzureDataLakeStorage when --backup-mode unspecified
        DppStorageBackupMode,   // AzureBlob, ADLS  -  mode-driven (Continuous default vs Vaulted). Vaulted Azure Files lives under RSV (workload-type AzureFileShare).
        DppAks,                 // AKS  -  has AKS-specific flags
        Aks,                    // (legacy alias retained for backward compatibility in tests)
    }

    private static bool IsRsvFamily(WorkloadFamily f) =>
        f is WorkloadFamily.RsvVm or WorkloadFamily.RsvVmWorkload or WorkloadFamily.RsvFileShare;

    private static bool IsDppFamily(WorkloadFamily f) =>
        f is WorkloadFamily.DppDiscrete or WorkloadFamily.DppContinuous or WorkloadFamily.DppStorageBackupMode or WorkloadFamily.DppAks or WorkloadFamily.Aks;

    private static WorkloadFamily ClassifyWorkload(string workloadType)
    {
        if (string.IsNullOrWhiteSpace(workloadType))
        {
            return WorkloadFamily.Unknown;
        }

        return workloadType.ToLowerInvariant() switch
        {
            "vm" or "azurevm" or "iaasvm" or "azureiaasvm" or "virtualmachine" or "iaasvmcontainer" => WorkloadFamily.RsvVm,
            "sql" or "sqldatabase" or "sqldb" or "mssql" or "azuresql" or "saphana" or "saphanadatabase" or "saphanadb" or "hana" or "sapase" or "ase" or "sybase" => WorkloadFamily.RsvVmWorkload,
            "azurefileshare" or "fileshare" or "afs" => WorkloadFamily.RsvFileShare,
            "azuredisk" or "disk" or "elasticsan" or "esan" or "postgresqlflexible" or "postgres" or "pgflex" or "cosmosdb" or "cosmos" => WorkloadFamily.DppDiscrete,
            "aks" or "kubernetes" or "kubernetescluster" => WorkloadFamily.DppAks,
            "azureblob" or "blob" or "adls" or "azuredatalakestorage" or "datalake" or "datalakestorage" => WorkloadFamily.DppStorageBackupMode,
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

    private static bool IsHanaWorkload(string? workloadType) =>
        workloadType is not null &&
        (workloadType.Equals("SAPHANA", StringComparison.OrdinalIgnoreCase) ||
         workloadType.Equals("SAPHanaDatabase", StringComparison.OrdinalIgnoreCase));

    private static bool IsAzureDiskWorkload(string? workloadType) =>
        workloadType is not null &&
        (workloadType.Equals("AzureDisk", StringComparison.OrdinalIgnoreCase) ||
         workloadType.Equals("Disk", StringComparison.OrdinalIgnoreCase));

    // No DPP (Backup vault) datasource supports ArchiveStore today. This includes:
    // AzureDisk, AKS, PostgreSQL Flexible Server, PostgreSQL, Cosmos DB, Elastic SAN,
    // AzureBlob, and AzureDataLakeStorage. Reject --archive-tier-* for any DPP workload
    // with an actionable per-datasource message rather than letting the request fail at
    // the service with BMSUserErrorInvalidInput.
    private static bool IsArchiveUnsupportedDppFamily(WorkloadFamily family) =>
        IsDppFamily(family);

    private static string DescribeArchiveUnsupportedWorkload(string workloadType, WorkloadFamily family)
    {
        if (family is WorkloadFamily.DppAks or WorkloadFamily.Aks)
        {
            return "AKS";
        }

        return workloadType.ToLowerInvariant() switch
        {
            "azuredisk" or "disk" => "AzureDisk",
            "elasticsan" or "esan" => "Elastic SAN",
            "postgresqlflexible" or "pgflex" => "PostgreSQL Flexible Server",
            "postgres" => "PostgreSQL",
            "cosmosdb" or "cosmos" => "Cosmos DB",
            "azureblob" or "blob" => "AzureBlob",
            "adls" or "azuredatalakestorage" or "datalake" or "datalakestorage" => "AzureDataLakeStorage",
            _ => "this DPP workload",
        };
    }

    private static bool IsVaultedBackupMode(string? backupMode) =>
        !string.IsNullOrWhiteSpace(backupMode) &&
        backupMode.Trim().Equals("Vaulted", StringComparison.OrdinalIgnoreCase);

    private static bool IsRsvWeekly(string? frequency) =>
        string.Equals(frequency, "Weekly", StringComparison.OrdinalIgnoreCase);

    private static bool IsRsvHourly(string? frequency) =>
        string.Equals(frequency, "Hourly", StringComparison.OrdinalIgnoreCase);

    private static bool IsKnownDppScheduleFrequency(string frequency) => frequency.Trim().ToUpperInvariant() switch
    {
        "DAILY" or "WEEKLY" or "P1D" or "P1W" or "P2W" or "P1M" or "PT4H" or "PT6H" or "PT8H" or "PT12H" => true,
        _ => false,
    };

    private static bool HasDayOverlap(string? daysA, string? daysB)
    {
        if (string.IsNullOrWhiteSpace(daysA) || string.IsNullOrWhiteSpace(daysB))
        {
            return false;
        }
        var a = SplitDays(daysA!);
        var b = SplitDays(daysB!);
        return a.Overlaps(b);
    }

    private static HashSet<string> SplitDays(string csv) =>
        new(csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.OrdinalIgnoreCase);

    private static bool IsPartialWeeklyRetention(PolicyCreateOptions o)
    {
        var weeks = o.WeeklyRetentionWeeks > 0;
        var days = !string.IsNullOrWhiteSpace(o.WeeklyRetentionDaysOfWeek);
        return weeks ^ days;
    }

    private static void ValidateMonthlyRetention(PolicyCreateOptions o, List<PolicyValidationIssue> issues)
    {
        var months = o.MonthlyRetentionMonths > 0;
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
        var years = o.YearlyRetentionYears > 0;
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
        o.HourlyIntervalHours > 0 ||
        !string.IsNullOrWhiteSpace(o.HourlyWindowStartTime) ||
        o.HourlyWindowDurationHours > 0 ||
        !string.IsNullOrWhiteSpace(o.DailyRetentionDays) ||
        o.WeeklyRetentionWeeks > 0 ||
        !string.IsNullOrWhiteSpace(o.WeeklyRetentionDaysOfWeek) ||
        o.MonthlyRetentionMonths > 0 ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionWeekOfMonth) ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.MonthlyRetentionDaysOfMonth) ||
        o.YearlyRetentionYears > 0 ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionMonths) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionWeekOfMonth) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfMonth) ||
        !string.IsNullOrWhiteSpace(o.ArchiveTierAfterDays) ||
        !string.IsNullOrWhiteSpace(o.ArchiveTierMode) ||
        !string.IsNullOrWhiteSpace(o.FullScheduleFrequency) ||
        o.LogFrequencyMinutes > 0;

    private static bool HasAnyScheduleRetentionOrArchiveInput(PolicyCreateOptions o) =>
        HasAnyScheduleOrRetentionInput(o);

    private static bool HasAnyYearlyRetentionInput(PolicyCreateOptions o) =>
        o.YearlyRetentionYears > 0 ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionMonths) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionWeekOfMonth) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfWeek) ||
        !string.IsNullOrWhiteSpace(o.YearlyRetentionDaysOfMonth);

    private static void EnsureFamily(string? value, string flag, WorkloadFamily actual, WorkloadFamily required, string requiredLabel, List<PolicyValidationIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(value) && actual != required)
        {
            issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for {requiredLabel} workloads."));
        }
    }

    private static void EnsureFamily(int value, string flag, WorkloadFamily actual, WorkloadFamily required, string requiredLabel, List<PolicyValidationIssue> issues)
    {
        if (value > 0 && actual != required)
        {
            issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for {requiredLabel} workloads."));
        }
    }

    private static void EnsureFamily(bool value, string flag, WorkloadFamily actual, WorkloadFamily required, string requiredLabel, List<PolicyValidationIssue> issues)
    {
        if (value && actual != required)
        {
            issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for {requiredLabel} workloads."));
        }
    }

    private static void EnsureDpp(string? value, string flag, WorkloadFamily actual, List<PolicyValidationIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(value) && IsRsvFamily(actual))
        {
            issues.Add(new PolicyValidationIssue(flag, $"{flag} is supported only for DPP (Backup vault) workloads."));
        }
    }
}
