// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Linq;
using Azure.ResourceManager.RecoveryServicesBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Pure builder that translates a <see cref="PolicyCreateRequest"/> into a
/// <see cref="BackupGenericProtectionPolicy"/> ready to be wrapped in a
/// <c>BackupProtectionPolicyData</c> and submitted to the Recovery Services SDK.
/// </summary>
/// <remarks>
/// AOT-safe: no reflection, no dynamic dispatch.
/// All inputs are nullable strings  -  when a flag is omitted the builder falls back
/// to the long-standing default behavior so existing minimal-flag invocations keep
/// working unchanged: daily 02:00 UTC, 30 days retention.
/// More elaborate retention shapes (weekly / monthly / yearly / hourly / archive)
/// are produced only when the caller supplies the corresponding flags.
/// </remarks>
public static class RsvPolicyBuilder
{
    private const int DefaultDailyRetentionDays = 30;
    private const int DefaultLogFrequencyMinutes = 60;
    private const int DefaultLogRetentionDays = 15;
    private const string DefaultTimeZone = "UTC";

    public static BackupGenericProtectionPolicy Build(PolicyCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = RsvDatasourceRegistry.ResolveOrDefault(request.WorkloadType);

        return profile.PolicyType switch
        {
            RsvPolicyType.IaasVm => BuildIaasVm(request),
            RsvPolicyType.VmWorkload => BuildVmWorkload(request, profile.ApiWorkloadType ?? "SQLDataBase"),
            RsvPolicyType.AzureFileShare => BuildFileShare(request),
            _ => BuildIaasVm(request),
        };
    }

    private static IaasVmProtectionPolicy BuildIaasVm(PolicyCreateRequest req)
    {
        var scheduleTimes = ParseScheduleTimes(req.ScheduleTimes);
        var timeZone = string.IsNullOrWhiteSpace(req.TimeZone) ? DefaultTimeZone : req.TimeZone!;
        var policyType = ParseIaasVmPolicyType(req.PolicySubType);
        var iaasFreq = ParseScheduleRunType(req.ScheduleFrequency, defaultDaily: true);

        var schedule = BuildIaasVmSchedule(req, policyType, scheduleTimes);
        var retention = BuildLongTermRetention(req, scheduleTimes, iaasFreq);

        var policy = new IaasVmProtectionPolicy
        {
            SchedulePolicy = schedule,
            RetentionPolicy = retention,
            TimeZone = timeZone,
            PolicyType = policyType,
        };

        if (TryParsePositiveInt(req.InstantRpRetentionDays, out var instantRpDays))
        {
            policy.InstantRPRetentionRangeInDays = instantRpDays;
        }

        if (!string.IsNullOrWhiteSpace(req.InstantRpResourceGroup))
        {
            policy.InstantRPDetails = new InstantRPAdditionalDetails
            {
                AzureBackupRGNamePrefix = req.InstantRpResourceGroup,
            };
        }

        if (!string.IsNullOrWhiteSpace(req.SnapshotConsistency))
        {
            policy.SnapshotConsistencyType = new IaasVmSnapshotConsistencyType(req.SnapshotConsistency!);
        }

        var tiering = BuildTieringPolicy(req);
        if (tiering is not null)
        {
            policy.TieringPolicy["ArchivedRP"] = tiering;
        }

        // Smart tier (ML-recommended archive) overrides any explicit archive flag for VM workloads.
        // Backend requires Duration=0 + DurationType=Invalid when TieringMode is TierRecommended (matches Az CLI shape).
        if (req.SmartTier)
        {
            policy.TieringPolicy["ArchivedRP"] = new BackupTieringPolicy
            {
                TieringMode = TieringMode.TierRecommended,
                DurationValue = 0,
                DurationType = RetentionDurationType.Invalid,
            };
        }

        return policy;
    }

    private static FileShareProtectionPolicy BuildFileShare(PolicyCreateRequest req)
    {
        var scheduleTimes = ParseScheduleTimes(req.ScheduleTimes);
        // AFS supports the same Daily / Weekly / Hourly schedule shapes as IaasVm via SimpleSchedulePolicyV2.
        // When --schedule-frequency Hourly is supplied, emit a V2 hourly schedule; otherwise fall back
        // to the simple daily/weekly shape that the long-standing AFS policies use.
        var freq = ParseScheduleRunType(req.ScheduleFrequency, defaultDaily: true);
        BackupSchedulePolicy schedule = freq == ScheduleRunType.Hourly
            ? new SimpleSchedulePolicyV2 { ScheduleRunFrequency = freq, HourlySchedule = BuildHourlySchedule(req) }
            : BuildSimpleSchedule(req, scheduleTimes);

        var retention = BuildLongTermRetention(req, scheduleTimes, freq);

        return new FileShareProtectionPolicy
        {
            WorkLoadType = new BackupWorkloadType("AzureFileShare"),
            SchedulePolicy = schedule,
            RetentionPolicy = retention,
            TimeZone = string.IsNullOrWhiteSpace(req.TimeZone) ? DefaultTimeZone : req.TimeZone!,
        };
    }

    private static VmWorkloadProtectionPolicy BuildVmWorkload(PolicyCreateRequest req, string apiWorkloadType)
    {
        var scheduleTimes = ParseScheduleTimes(req.ScheduleTimes);
        var timeZone = string.IsNullOrWhiteSpace(req.TimeZone) ? DefaultTimeZone : req.TimeZone!;

        var policy = new VmWorkloadProtectionPolicy
        {
            WorkLoadType = new BackupWorkloadType(apiWorkloadType),
            Settings = new BackupCommonSettings
            {
                TimeZone = timeZone,
                IsCompression = req.IsCompression,
                IsSqlCompression = req.IsSqlCompression,
            },
        };

        // Full sub-policy is mandatory.
        policy.SubProtectionPolicy.Add(BuildVmWorkloadFullSubPolicy(req, scheduleTimes));

        // Differential is opt-in.
        if (!string.IsNullOrWhiteSpace(req.DifferentialScheduleDaysOfWeek) || req.DifferentialRetentionDays > 0)
        {
            policy.SubProtectionPolicy.Add(BuildVmWorkloadDifferentialSubPolicy(req, scheduleTimes));
        }

        // Incremental is opt-in.
        if (!string.IsNullOrWhiteSpace(req.IncrementalScheduleDaysOfWeek) || req.IncrementalRetentionDays > 0)
        {
            policy.SubProtectionPolicy.Add(BuildVmWorkloadIncrementalSubPolicy(req, scheduleTimes));
        }

        // Log sub-policy: emitted by default for compatibility with existing live tests.
        policy.SubProtectionPolicy.Add(BuildVmWorkloadLogSubPolicy(req));

        // SAPHANA snapshot/instance backup is enabled by attaching SnapshotBackupAdditionalDetails
        // to the Full sub-policy (matches Az CLI behavior for `az backup policy set` with
        // --snapshot-instant-rp-retention-days). Not a separate sub-policy.
        if (req.EnableSnapshotBackup)
        {
            AttachSnapshotDetailsToFullSubPolicy(policy, req);
        }

        return policy;
    }

    private static SubProtectionPolicy BuildVmWorkloadFullSubPolicy(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        var fullSchedule = BuildVmWorkloadFullSchedule(req, scheduleTimes);
        var fullFreq = ParseScheduleRunType(req.FullScheduleFrequency, defaultDaily: true);
        var fullRetention = BuildLongTermRetention(req, scheduleTimes, fullFreq);

        var sub = new SubProtectionPolicy
        {
            PolicyType = new SubProtectionPolicyType("Full"),
            SchedulePolicy = fullSchedule,
            RetentionPolicy = fullRetention,
        };

        var tiering = BuildTieringPolicy(req);
        if (tiering is not null)
        {
            sub.TieringPolicy["ArchivedRP"] = tiering;
        }

        return sub;
    }

    private static SubProtectionPolicy BuildVmWorkloadSnapshotSubPolicy(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        // Retained for backward-compatibility with existing unit tests; not currently invoked by Build().
        var schedule = new SimpleSchedulePolicy { ScheduleRunFrequency = ScheduleRunType.Daily };
        foreach (var t in scheduleTimes)
        {
            schedule.ScheduleRunTimes.Add(t);
        }

        var snapshotDays = TryParsePositiveInt(req.SnapshotInstantRpRetentionDays, out var rpDays) ? rpDays : 2;
        var retention = new SimpleRetentionPolicy
        {
            RetentionDuration = new RetentionDuration { Count = snapshotDays, DurationType = RetentionDurationType.Days },
        };

        var sub = new SubProtectionPolicy
        {
            PolicyType = new SubProtectionPolicyType("SnapshotCopyOnlyFull"),
            SchedulePolicy = schedule,
            RetentionPolicy = retention,
        };

        var details = new SnapshotBackupAdditionalDetails
        {
            InstantRpRetentionRangeInDays = snapshotDays,
        };
        if (!string.IsNullOrWhiteSpace(req.SnapshotInstantRpResourceGroup))
        {
            details.InstantRPDetails = req.SnapshotInstantRpResourceGroup;
        }
        sub.SnapshotBackupAdditionalDetails = details;

        return sub;
    }

    private static void AttachSnapshotDetailsToFullSubPolicy(VmWorkloadProtectionPolicy policy, PolicyCreateRequest req)
    {
        // Per Az CLI: snapshot backup for SAPHANA is enabled by adding SnapshotBackupAdditionalDetails
        // to the Full sub-policy, not by introducing a new SnapshotFull/SnapshotCopyOnlyFull sub-policy.
        var full = policy.SubProtectionPolicy.FirstOrDefault(s => s.PolicyType?.ToString() == "Full");
        if (full is null)
        {
            return;
        }

        var details = new SnapshotBackupAdditionalDetails();
        if (TryParsePositiveInt(req.SnapshotInstantRpRetentionDays, out var rp))
        {
            details.InstantRpRetentionRangeInDays = rp;
        }
        if (!string.IsNullOrWhiteSpace(req.SnapshotInstantRpResourceGroup))
        {
            details.InstantRPDetails = req.SnapshotInstantRpResourceGroup;
        }
        full.SnapshotBackupAdditionalDetails = details;
    }

    private static SubProtectionPolicy BuildVmWorkloadDifferentialSubPolicy(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        var schedule = new SimpleSchedulePolicy { ScheduleRunFrequency = ScheduleRunType.Weekly };
        foreach (var day in ParseDaysOfWeek(req.DifferentialScheduleDaysOfWeek))
        {
            schedule.ScheduleRunDays.Add(day);
        }
        foreach (var t in scheduleTimes)
        {
            schedule.ScheduleRunTimes.Add(t);
        }

        var days = req.DifferentialRetentionDays > 0 ? req.DifferentialRetentionDays : DefaultDailyRetentionDays;
        var retention = new SimpleRetentionPolicy
        {
            RetentionDuration = new RetentionDuration { Count = days, DurationType = RetentionDurationType.Days },
        };

        return new SubProtectionPolicy
        {
            PolicyType = new SubProtectionPolicyType("Differential"),
            SchedulePolicy = schedule,
            RetentionPolicy = retention,
        };
    }

    private static SubProtectionPolicy BuildVmWorkloadIncrementalSubPolicy(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        var schedule = new SimpleSchedulePolicy { ScheduleRunFrequency = ScheduleRunType.Weekly };
        foreach (var day in ParseDaysOfWeek(req.IncrementalScheduleDaysOfWeek))
        {
            schedule.ScheduleRunDays.Add(day);
        }
        foreach (var t in scheduleTimes)
        {
            schedule.ScheduleRunTimes.Add(t);
        }

        var days = req.IncrementalRetentionDays > 0 ? req.IncrementalRetentionDays : DefaultDailyRetentionDays;
        var retention = new SimpleRetentionPolicy
        {
            RetentionDuration = new RetentionDuration { Count = days, DurationType = RetentionDurationType.Days },
        };

        return new SubProtectionPolicy
        {
            PolicyType = new SubProtectionPolicyType("Incremental"),
            SchedulePolicy = schedule,
            RetentionPolicy = retention,
        };
    }

    private static SubProtectionPolicy BuildVmWorkloadLogSubPolicy(PolicyCreateRequest req)
    {
        var freq = req.LogFrequencyMinutes > 0 ? req.LogFrequencyMinutes : DefaultLogFrequencyMinutes;
        var retentionDays = req.LogRetentionDays > 0 ? req.LogRetentionDays : DefaultLogRetentionDays;

        return new SubProtectionPolicy
        {
            PolicyType = new SubProtectionPolicyType("Log"),
            SchedulePolicy = new LogSchedulePolicy { ScheduleFrequencyInMins = freq },
            RetentionPolicy = new SimpleRetentionPolicy
            {
                RetentionDuration = new RetentionDuration { Count = retentionDays, DurationType = RetentionDurationType.Days },
            },
        };
    }

    private static BackupSchedulePolicy BuildVmWorkloadFullSchedule(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        var freq = ParseScheduleRunType(req.FullScheduleFrequency, defaultDaily: true);
        var schedule = new SimpleSchedulePolicy { ScheduleRunFrequency = freq };

        if (freq == ScheduleRunType.Weekly)
        {
            var days = ParseDaysOfWeek(req.FullScheduleDaysOfWeek);
            if (days.Count == 0)
            {
                days.Add(BackupDayOfWeek.Sunday);
            }
            foreach (var d in days)
            {
                schedule.ScheduleRunDays.Add(d);
            }
        }

        foreach (var t in scheduleTimes)
        {
            schedule.ScheduleRunTimes.Add(t);
        }

        return schedule;
    }

    private static BackupSchedulePolicy BuildIaasVmSchedule(PolicyCreateRequest req, IaasVmPolicyType? policyType, IList<DateTimeOffset> scheduleTimes)
    {
        var freq = ParseScheduleRunType(req.ScheduleFrequency, defaultDaily: true);
        var isHourly = freq == ScheduleRunType.Hourly;
        var isWeekly = freq == ScheduleRunType.Weekly;

        // PolicyType.V2 + Hourly -> SimpleSchedulePolicyV2 with HourlySchedule.
        if (policyType == IaasVmPolicyType.V2 || isHourly)
        {
            var v2 = new SimpleSchedulePolicyV2 { ScheduleRunFrequency = freq };

            if (isHourly)
            {
                v2.HourlySchedule = BuildHourlySchedule(req);
            }
            else if (isWeekly)
            {
                v2.WeeklySchedule = BuildWeeklyV2(req, scheduleTimes);
            }
            else
            {
                foreach (var t in scheduleTimes)
                {
                    v2.ScheduleRunTimes.Add(t);
                }
            }

            return v2;
        }

        var schedule = new SimpleSchedulePolicy { ScheduleRunFrequency = freq };
        if (isWeekly)
        {
            var days = ParseDaysOfWeek(req.ScheduleDaysOfWeek);
            if (days.Count == 0)
            {
                days.Add(BackupDayOfWeek.Sunday);
            }
            foreach (var d in days)
            {
                schedule.ScheduleRunDays.Add(d);
            }
        }
        foreach (var t in scheduleTimes)
        {
            schedule.ScheduleRunTimes.Add(t);
        }
        return schedule;
    }

    private static BackupSchedulePolicy BuildSimpleSchedule(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        var freq = ParseScheduleRunType(req.ScheduleFrequency, defaultDaily: true);
        var schedule = new SimpleSchedulePolicy { ScheduleRunFrequency = freq };
        if (freq == ScheduleRunType.Weekly)
        {
            var days = ParseDaysOfWeek(req.ScheduleDaysOfWeek);
            if (days.Count == 0)
            {
                days.Add(BackupDayOfWeek.Sunday);
            }
            foreach (var d in days)
            {
                schedule.ScheduleRunDays.Add(d);
            }
        }
        foreach (var t in scheduleTimes)
        {
            schedule.ScheduleRunTimes.Add(t);
        }
        return schedule;
    }

    private static BackupHourlySchedule BuildHourlySchedule(PolicyCreateRequest req)
    {
        var hs = new BackupHourlySchedule();
        if (req.HourlyIntervalHours > 0)
        {
            hs.Interval = req.HourlyIntervalHours;
        }
        if (DateTimeOffset.TryParseExact(req.HourlyWindowStartTime ?? string.Empty, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var startTime))
        {
            hs.ScheduleWindowStartOn = NormalizeToHourMinute(startTime);
        }
        if (req.HourlyWindowDurationHours > 0)
        {
            hs.ScheduleWindowDuration = req.HourlyWindowDurationHours;
        }
        return hs;
    }

    private static BackupWeeklySchedule BuildWeeklyV2(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes)
    {
        var ws = new BackupWeeklySchedule();
        var days = ParseDaysOfWeek(req.ScheduleDaysOfWeek);
        if (days.Count == 0)
        {
            days.Add(BackupDayOfWeek.Sunday);
        }
        foreach (var d in days)
        {
            ws.ScheduleRunDays.Add(d);
        }
        foreach (var t in scheduleTimes)
        {
            ws.ScheduleRunTimes.Add(t);
        }
        return ws;
    }

    private static LongTermRetentionPolicy BuildLongTermRetention(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes, ScheduleRunType? scheduleFrequency = null)
    {
        var retention = new LongTermRetentionPolicy();

        // DailyRetentionSchedule only makes sense when the Full backup schedule is Daily (or Hourly).
        // For a Weekly schedule the RSV API rejects DailySchedule alongside WeeklySchedule (Az CLI omits it).
        // Honor an explicit --daily-retention-days regardless of frequency for backward compatibility.
        var dailyExplicit = TryParsePositiveInt(req.DailyRetentionDays, out var dd);
        var isWeekly = scheduleFrequency == ScheduleRunType.Weekly;
        if (dailyExplicit || !isWeekly)
        {
            var dailyDays = dailyExplicit ? dd : DefaultDailyRetentionDays;
            var daily = new DailyRetentionSchedule
            {
                RetentionDuration = new RetentionDuration { Count = dailyDays, DurationType = RetentionDurationType.Days },
            };
            foreach (var t in scheduleTimes)
            {
                daily.RetentionTimes.Add(t);
            }
            retention.DailySchedule = daily;
        }

        if (req.WeeklyRetentionWeeks > 0)
        {
            var weekly = new WeeklyRetentionSchedule
            {
                RetentionDuration = new RetentionDuration { Count = req.WeeklyRetentionWeeks, DurationType = RetentionDurationType.Weeks },
            };
            var dow = ParseDaysOfWeek(req.WeeklyRetentionDaysOfWeek);
            if (dow.Count == 0)
            {
                // Auto-align: when schedule is Weekly, weekly retention days must match schedule days.
                // Fall back to schedule days first, then Sunday as final default.
                var scheduleDow = IsWeeklyFrequency(scheduleFrequency)
                    ? ParseDaysOfWeek(req.ScheduleDaysOfWeek ?? req.FullScheduleDaysOfWeek)
                    : [];
                dow = scheduleDow.Count > 0 ? scheduleDow : [BackupDayOfWeek.Sunday];
            }
            foreach (var d in dow)
            {
                weekly.DaysOfTheWeek.Add(d);
            }
            foreach (var t in scheduleTimes)
            {
                weekly.RetentionTimes.Add(t);
            }
            retention.WeeklySchedule = weekly;
        }

        if (req.MonthlyRetentionMonths > 0)
        {
            retention.MonthlySchedule = BuildMonthlyRetention(req, scheduleTimes, req.MonthlyRetentionMonths);
        }

        if (req.YearlyRetentionYears > 0)
        {
            retention.YearlySchedule = BuildYearlyRetention(req, scheduleTimes, req.YearlyRetentionYears);
        }

        return retention;
    }

    private static MonthlyRetentionSchedule BuildMonthlyRetention(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes, int months)
    {
        var monthly = new MonthlyRetentionSchedule
        {
            RetentionDuration = new RetentionDuration { Count = months, DurationType = RetentionDurationType.Months },
        };

        if (!string.IsNullOrWhiteSpace(req.MonthlyRetentionDaysOfMonth))
        {
            monthly.RetentionScheduleFormatType = RetentionScheduleFormat.Daily;
            foreach (var day in ParseDaysOfMonth(req.MonthlyRetentionDaysOfMonth))
            {
                monthly.RetentionScheduleDailyDaysOfTheMonth.Add(day);
            }
        }
        else
        {
            monthly.RetentionScheduleFormatType = RetentionScheduleFormat.Weekly;
            monthly.RetentionScheduleWeekly = new WeeklyRetentionFormat();
            var dow = ParseDaysOfWeek(req.MonthlyRetentionDaysOfWeek);
            if (dow.Count == 0)
            {
                dow.Add(BackupDayOfWeek.Sunday);
            }
            foreach (var d in dow)
            {
                monthly.RetentionScheduleWeekly.DaysOfTheWeek.Add(d);
            }
            var weeks = ParseWeeksOfMonth(req.MonthlyRetentionWeekOfMonth);
            if (weeks.Count == 0)
            {
                weeks.Add(BackupWeekOfMonth.First);
            }
            foreach (var w in weeks)
            {
                monthly.RetentionScheduleWeekly.WeeksOfTheMonth.Add(w);
            }
        }

        foreach (var t in scheduleTimes)
        {
            monthly.RetentionTimes.Add(t);
        }
        return monthly;
    }

    private static YearlyRetentionSchedule BuildYearlyRetention(PolicyCreateRequest req, IList<DateTimeOffset> scheduleTimes, int years)
    {
        var yearly = new YearlyRetentionSchedule
        {
            RetentionDuration = new RetentionDuration { Count = years, DurationType = RetentionDurationType.Years },
        };

        var months = ParseMonthsOfYear(req.YearlyRetentionMonths);
        if (months.Count == 0)
        {
            months.Add(BackupMonthOfYear.January);
        }
        foreach (var m in months)
        {
            yearly.MonthsOfYear.Add(m);
        }

        if (!string.IsNullOrWhiteSpace(req.YearlyRetentionDaysOfMonth))
        {
            yearly.RetentionScheduleFormatType = RetentionScheduleFormat.Daily;
            foreach (var day in ParseDaysOfMonth(req.YearlyRetentionDaysOfMonth))
            {
                yearly.RetentionScheduleDailyDaysOfTheMonth.Add(day);
            }
        }
        else
        {
            yearly.RetentionScheduleFormatType = RetentionScheduleFormat.Weekly;
            yearly.RetentionScheduleWeekly = new WeeklyRetentionFormat();
            var dow = ParseDaysOfWeek(req.YearlyRetentionDaysOfWeek);
            if (dow.Count == 0)
            {
                dow.Add(BackupDayOfWeek.Sunday);
            }
            foreach (var d in dow)
            {
                yearly.RetentionScheduleWeekly.DaysOfTheWeek.Add(d);
            }
            var weeks = ParseWeeksOfMonth(req.YearlyRetentionWeekOfMonth);
            if (weeks.Count == 0)
            {
                weeks.Add(BackupWeekOfMonth.First);
            }
            foreach (var w in weeks)
            {
                yearly.RetentionScheduleWeekly.WeeksOfTheMonth.Add(w);
            }
        }

        foreach (var t in scheduleTimes)
        {
            yearly.RetentionTimes.Add(t);
        }
        return yearly;
    }

    private static BackupTieringPolicy? BuildTieringPolicy(PolicyCreateRequest req)
    {
        var modeText = req.ArchiveTierMode;
        var hasDays = TryParsePositiveInt(req.ArchiveTierAfterDays, out var afterDays);

        if (string.IsNullOrWhiteSpace(modeText) && !hasDays)
        {
            return null;
        }

        var mode = string.IsNullOrWhiteSpace(modeText)
            ? TieringMode.TierAfter
            : new TieringMode(modeText!);

        var policy = new BackupTieringPolicy
        {
            TieringMode = mode,
        };

        if (hasDays)
        {
            policy.DurationValue = afterDays;
            policy.DurationType = RetentionDurationType.Days;
        }

        return policy;
    }

    // ===== Parsers =====

    internal static IList<DateTimeOffset> ParseScheduleTimes(string? csv)
    {
        var list = new List<DateTimeOffset>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            list.Add(new DateTimeOffset(DateTime.UtcNow.Date.AddHours(2), TimeSpan.Zero));
            return list;
        }

        foreach (var raw in csv!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (DateTimeOffset.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                list.Add(NormalizeToHourMinute(parsed));
            }
        }

        if (list.Count == 0)
        {
            list.Add(new DateTimeOffset(DateTime.UtcNow.Date.AddHours(2), TimeSpan.Zero));
        }
        return list;
    }

    internal static List<BackupDayOfWeek> ParseDaysOfWeek(string? csv)
    {
        var list = new List<BackupDayOfWeek>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            return list;
        }

        foreach (var raw in csv!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            list.Add(NormalizeDayOfWeek(Capitalize(raw)));
        }
        return list;
    }

    private static BackupDayOfWeek NormalizeDayOfWeek(string text) => text.ToUpperInvariant() switch
    {
        "MON" or "MONDAY" => BackupDayOfWeek.Monday,
        "TUE" or "TUES" or "TUESDAY" => BackupDayOfWeek.Tuesday,
        "WED" or "WEDNESDAY" => BackupDayOfWeek.Wednesday,
        "THU" or "THUR" or "THURS" or "THURSDAY" => BackupDayOfWeek.Thursday,
        "FRI" or "FRIDAY" => BackupDayOfWeek.Friday,
        "SAT" or "SATURDAY" => BackupDayOfWeek.Saturday,
        "SUN" or "SUNDAY" => BackupDayOfWeek.Sunday,
        _ => throw new ArgumentException($"Unrecognized day of week: '{text}'. Valid values: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday (or 3-letter abbreviations)."),
    };

    internal static List<BackupMonthOfYear> ParseMonthsOfYear(string? csv)
    {
        var list = new List<BackupMonthOfYear>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            return list;
        }

        foreach (var raw in csv!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            list.Add(NormalizeMonthOfYear(raw));
        }
        return list;
    }

    private static BackupMonthOfYear NormalizeMonthOfYear(string text) => text.ToUpperInvariant() switch
    {
        "JAN" or "JANUARY" => BackupMonthOfYear.January,
        "FEB" or "FEBRUARY" => BackupMonthOfYear.February,
        "MAR" or "MARCH" => BackupMonthOfYear.March,
        "APR" or "APRIL" => BackupMonthOfYear.April,
        "MAY" => BackupMonthOfYear.May,
        "JUN" or "JUNE" => BackupMonthOfYear.June,
        "JUL" or "JULY" => BackupMonthOfYear.July,
        "AUG" or "AUGUST" => BackupMonthOfYear.August,
        "SEP" or "SEPT" or "SEPTEMBER" => BackupMonthOfYear.September,
        "OCT" or "OCTOBER" => BackupMonthOfYear.October,
        "NOV" or "NOVEMBER" => BackupMonthOfYear.November,
        "DEC" or "DECEMBER" => BackupMonthOfYear.December,
        _ => throw new ArgumentException($"Unrecognized month: '{text}'. Valid values: January - December (or 3-letter abbreviations)."),
    };

    internal static List<BackupWeekOfMonth> ParseWeeksOfMonth(string? csv)
    {
        var list = new List<BackupWeekOfMonth>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            return list;
        }

        foreach (var raw in csv!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            list.Add(raw.ToUpperInvariant() switch
            {
                "FIRST" or "1" => BackupWeekOfMonth.First,
                "SECOND" or "2" => BackupWeekOfMonth.Second,
                "THIRD" or "3" => BackupWeekOfMonth.Third,
                "FOURTH" or "4" => BackupWeekOfMonth.Fourth,
                "LAST" => BackupWeekOfMonth.Last,
                _ => throw new ArgumentException($"Unrecognized week of month: '{raw}'. Valid values: First, Second, Third, Fourth, Last (or 1-4)."),
            });
        }
        return list;
    }

    internal static List<BackupDay> ParseDaysOfMonth(string? csv)
    {
        var list = new List<BackupDay>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            return list;
        }

        foreach (var raw in csv!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.Equals(raw, "Last", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new BackupDay { IsLast = true });
            }
            else if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dayNum) && dayNum >= 1 && dayNum <= 28)
            {
                list.Add(new BackupDay { Date = dayNum });
            }
        }
        return list;
    }

    private static IaasVmPolicyType? ParseIaasVmPolicyType(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        return text.Trim().ToUpperInvariant() switch
        {
            "V1" or "STANDARD" => IaasVmPolicyType.V1,
            "V2" or "ENHANCED" => IaasVmPolicyType.V2,
            _ => new IaasVmPolicyType(text!),
        };
    }

    private static ScheduleRunType ParseScheduleRunType(string? text, bool defaultDaily)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return defaultDaily ? ScheduleRunType.Daily : ScheduleRunType.Weekly;
        }
        return text.Trim().ToUpperInvariant() switch
        {
            "DAILY" => ScheduleRunType.Daily,
            "WEEKLY" => ScheduleRunType.Weekly,
            "HOURLY" => ScheduleRunType.Hourly,
            _ => defaultDaily ? ScheduleRunType.Daily : ScheduleRunType.Weekly,
        };
    }

    private static bool TryParsePositiveInt(string? text, out int value)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value > 0)
        {
            return true;
        }
        value = 0;
        return false;
    }

    private static bool IsWeeklyFrequency(ScheduleRunType? frequency)
        => frequency == ScheduleRunType.Weekly;

    private static bool HasAnyText(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return true;
            }
        }
        return false;
    }

    private static string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        return char.ToUpperInvariant(text[0]) + text[1..].ToLowerInvariant();
    }

    private static DateTimeOffset NormalizeToHourMinute(DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, TimeSpan.Zero);
    }
}
