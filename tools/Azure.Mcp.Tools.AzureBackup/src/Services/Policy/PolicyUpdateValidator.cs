// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Validator for the IaasVM-parity options accepted by
/// <c>azmcp azurebackup policy update</c>.
/// Enforces the cross-field dependencies that are checked client-side today
/// on <see cref="PolicyCreateValidator"/> so callers see actionable messages
/// before the request reaches the Recovery Services API.
/// </summary>
/// <remarks>
/// Validates supplied values before I/O. Dependencies on omitted values are
/// validated against the fetched policy by <see cref="IaasVmPolicyUpdater"/>.
/// </remarks>
public static class PolicyUpdateValidator
{
    public static PolicyValidationResult Validate(PolicyUpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return Validate(PolicyUpdateRequest.FromOptions(options));
    }

    public static PolicyValidationResult Validate(PolicyUpdateRequest options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var issues = new List<PolicyValidationIssue>();
        // Preserve the recorded client-side negative case for an explicit Weekly frequency.
        // Updates to an existing Weekly schedule can omit frequency and retain its days.
        if (IsWeekly(options.ScheduleFrequency) && string.IsNullOrWhiteSpace(options.ScheduleDaysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleDaysOfWeekName}",
                "Weekly schedules require --schedule-days-of-week."));
        }

        // Hourly partials are checked against the existing schedule by the merger.
        if (!string.IsNullOrWhiteSpace(options.ScheduleFrequency) &&
            !IsDaily(options.ScheduleFrequency) &&
            !IsWeekly(options.ScheduleFrequency) &&
            !Is(options.ScheduleFrequency, "Hourly"))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                "Unsupported schedule frequency. " +
                "Policy update supports Daily, Weekly, or Hourly on an existing Enhanced policy."));
        }

        // Weekly retention: weeks + days-of-week must be supplied together.
        var hasWeeklyWeeks = options.WeeklyRetentionWeeks > 0;
        var hasWeeklyDays = !string.IsNullOrWhiteSpace(options.WeeklyRetentionDaysOfWeek);
        if (hasWeeklyWeeks ^ hasWeeklyDays)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.WeeklyRetentionWeeksName}",
                "Weekly retention requires both --weekly-retention-weeks and --weekly-retention-days-of-week."));
        }

        // Monthly retention: months + a complete scheme.
        ValidateMonthly(options, issues);

        // Yearly retention: years + months + a complete scheme.
        ValidateYearly(options, issues);
        ValidateValues(options, issues);

        return issues.Count == 0 ? PolicyValidationResult.Ok : PolicyValidationResult.Fail(issues);
    }

    private static void ValidateMonthly(PolicyUpdateRequest options, List<PolicyValidationIssue> issues)
    {
        var hasMonths = options.MonthlyRetentionMonths > 0;
        var hasWeek = !string.IsNullOrWhiteSpace(options.MonthlyRetentionWeekOfMonth);
        var hasDaysOfWeek = !string.IsNullOrWhiteSpace(options.MonthlyRetentionDaysOfWeek);
        var hasDaysOfMonth = !string.IsNullOrWhiteSpace(options.MonthlyRetentionDaysOfMonth);
        var hasRelative = hasWeek || hasDaysOfWeek;
        var hasAbsolute = hasDaysOfMonth;

        if (!hasMonths && (hasRelative || hasAbsolute))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionMonthsName}",
                "Monthly retention flags require --monthly-retention-months."));
            return;
        }

        if (!hasMonths)
        {
            return;
        }

        if (!hasRelative && !hasAbsolute)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonthName}",
                "Monthly retention requires either the absolute scheme (--monthly-retention-days-of-month) or the relative scheme (--monthly-retention-week-of-month + --monthly-retention-days-of-week)."));
            return;
        }

        if (hasRelative && hasAbsolute)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonthName}",
                "Monthly retention accepts either the absolute scheme (--monthly-retention-days-of-month) OR the relative scheme (--monthly-retention-week-of-month + --monthly-retention-days-of-week), not both."));
            return;
        }

        if (hasRelative && !(hasWeek && hasDaysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.MonthlyRetentionWeekOfMonthName}",
                "Relative monthly retention requires both --monthly-retention-week-of-month and --monthly-retention-days-of-week."));
        }
    }

    private static void ValidateYearly(PolicyUpdateRequest options, List<PolicyValidationIssue> issues)
    {
        var hasYears = options.YearlyRetentionYears > 0;
        var hasMonths = !string.IsNullOrWhiteSpace(options.YearlyRetentionMonths);
        var hasWeek = !string.IsNullOrWhiteSpace(options.YearlyRetentionWeekOfMonth);
        var hasDaysOfWeek = !string.IsNullOrWhiteSpace(options.YearlyRetentionDaysOfWeek);
        var hasDaysOfMonth = !string.IsNullOrWhiteSpace(options.YearlyRetentionDaysOfMonth);
        var hasRelative = hasWeek || hasDaysOfWeek;
        var hasAbsolute = hasDaysOfMonth;

        if (!hasYears && (hasMonths || hasRelative || hasAbsolute))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionYearsName}",
                "Yearly retention flags require --yearly-retention-years."));
            return;
        }

        if (!hasYears)
        {
            return;
        }

        if (!hasMonths)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionMonthsName}",
                "Yearly retention requires --yearly-retention-months (e.g. 'January')."));
        }

        if (!hasRelative && !hasAbsolute)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionDaysOfMonthName}",
                "Yearly retention requires either the absolute scheme (--yearly-retention-days-of-month) or the relative scheme (--yearly-retention-week-of-month + --yearly-retention-days-of-week)."));
            return;
        }

        if (hasRelative && hasAbsolute)
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionDaysOfMonthName}",
                "Yearly retention accepts either the absolute scheme (--yearly-retention-days-of-month) OR the relative scheme (--yearly-retention-week-of-month + --yearly-retention-days-of-week), not both."));
            return;
        }

        if (hasRelative && !(hasWeek && hasDaysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.YearlyRetentionWeekOfMonthName}",
                "Relative yearly retention requires both --yearly-retention-week-of-month and --yearly-retention-days-of-week."));
        }
    }

    private static bool IsDaily(string? freq) => string.Equals(freq, "Daily", StringComparison.OrdinalIgnoreCase);
    private static bool IsWeekly(string? freq) => string.Equals(freq, "Weekly", StringComparison.OrdinalIgnoreCase);

    internal static bool Is(string? value, string expected) => string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    internal static DateTimeOffset ParseTime(string value)
    {
        if (!TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            throw new ArgumentException("Invalid schedule time. Specify exactly one HH:mm time.");
        }
        return new DateTimeOffset(2000, 1, 1, time.Hour, time.Minute, 0, TimeSpan.Zero);
    }

    internal static int ParsePositive(string value)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count <= 0)
        {
            throw new ArgumentException("Expected a positive integer.");
        }
        return count;
    }

    private static void ValidateValues(PolicyUpdateRequest o, List<PolicyValidationIssue> issues)
    {
        void Check(string flag, Action action)
        {
            try
            {
                action();
            }
            catch (ArgumentException ex)
            {
                issues.Add(new PolicyValidationIssue(flag, ex.Message));
            }
        }

        void Text(string flag, string? value, Action<string> action)
        {
            if (value is not null)
            {
                Check(flag, () =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException("A supplied value must not be empty.");
                    }
                    action(value);
                });
            }
        }

        void Csv(string flag, string? value, Action<string> parse) => Text(flag, value, v =>
        {
            if (v.Split(',').Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Empty list entries are not allowed.");
            }
            parse(v);
        });

        void EnumValue(string flag, string? value, params string[] allowed) => Text(flag, value, v =>
        {
            if (!allowed.Any(a => Is(v, a)))
            {
                throw new ArgumentException($"Allowed values: {string.Join(", ", allowed)}.");
            }
        });

        Text("--schedule-time", o.ScheduleTime, v => ParseTime(v));
        Text("--schedule-times", o.ScheduleTimes, v => ParseTime(v));
        Text("--hourly-window-start-time", o.HourlyWindowStartTime, v => ParseTime(v));
        Text("--daily-retention-days", o.DailyRetentionDays, v =>
        {
            if (!int.TryParse(v, NumberStyles.None, CultureInfo.InvariantCulture, out var days) || days <= 0 || days > 9999)
            {
                throw new ArgumentException("Invalid daily retention days. Expected 1-9999.");
            }
        });
        Text("--instant-rp-retention-days", o.InstantRpRetentionDays, v =>
        {
            if (ParsePositive(v) > 30) { throw new ArgumentException("Instant retention must be 1-30 days; Azure enforces schedule-specific limits."); }
        });
        Text("--archive-tier-after-days", o.ArchiveTierAfterDays, v =>
        {
            if (ParsePositive(v) < 45) { throw new ArgumentException("VM archive tiering requires at least 45 days."); }
        });
        Text("--time-zone", o.TimeZone, v =>
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(v, out _)) { throw new ArgumentException("Unknown time-zone identifier."); }
        });
        Text("--instant-rp-resource-group", o.InstantRpResourceGroup, v =>
        {
            if (v.Length > 50 || v.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '_' and not '-'))
            {
                throw new ArgumentException("Instant RP resource group prefix must be 1-50 letters, digits, underscores or hyphens.");
            }
        });
        EnumValue("--schedule-frequency", o.ScheduleFrequency, "Daily", "Weekly", "Hourly");
        EnumValue("--policy-sub-type", o.PolicySubType, "Standard", "Enhanced");
        EnumValue("--snapshot-consistency", o.SnapshotConsistency, "ApplicationConsistent", "CrashConsistent");
        EnumValue("--archive-tier-mode", o.ArchiveTierMode, "TierAfter", "TierRecommended");
        Csv("--schedule-days-of-week", o.ScheduleDaysOfWeek, v => RsvPolicyBuilder.ParseDaysOfWeek(v));
        Csv("--weekly-retention-days-of-week", o.WeeklyRetentionDaysOfWeek, v => RsvPolicyBuilder.ParseDaysOfWeek(v));
        Csv("--monthly-retention-days-of-week", o.MonthlyRetentionDaysOfWeek, v => RsvPolicyBuilder.ParseDaysOfWeek(v));
        Csv("--yearly-retention-days-of-week", o.YearlyRetentionDaysOfWeek, v => RsvPolicyBuilder.ParseDaysOfWeek(v));
        Csv("--monthly-retention-week-of-month", o.MonthlyRetentionWeekOfMonth, v => RsvPolicyBuilder.ParseWeeksOfMonth(v));
        Csv("--yearly-retention-week-of-month", o.YearlyRetentionWeekOfMonth, v => RsvPolicyBuilder.ParseWeeksOfMonth(v));
        Csv("--yearly-retention-months", o.YearlyRetentionMonths, v => RsvPolicyBuilder.ParseMonthsOfYear(v));
        static void DaysOfMonth(string v)
        {
            if (RsvPolicyBuilder.ParseDaysOfMonth(v).Count != v.Split(',').Length)
            {
                throw new ArgumentException("Days of month must be 1-28 or Last.");
            }
        }
        Csv("--monthly-retention-days-of-month", o.MonthlyRetentionDaysOfMonth, DaysOfMonth);
        Csv("--yearly-retention-days-of-month", o.YearlyRetentionDaysOfMonth, DaysOfMonth);
        Text("--policy-tags", o.PolicyTags, v => IaasVmPolicyUpdater.ParseTags(v));

        void Require(bool valid, string flag, string message)
        {
            if (!valid) { issues.Add(new PolicyValidationIssue(flag, message)); }
        }
        Require(o.WeeklyRetentionWeeks is >= 0 and <= 5163, "--weekly-retention-weeks", "Weekly retention must be 1-5163 weeks when supplied.");
        Require(o.MonthlyRetentionMonths is >= 0 and <= 1188, "--monthly-retention-months", "Monthly retention must be 1-1188 months when supplied.");
        Require(o.YearlyRetentionYears is >= 0 and <= 99, "--yearly-retention-years", "Yearly retention must be 1-99 years when supplied.");
        Require(o.HourlyIntervalHours is null or 4 or 6 or 8 or 12, "--hourly-interval-hours", "Hourly interval must be 4, 6, 8 or 12.");
        Require(o.HourlyWindowDurationHours is null or (>= 4 and <= 24), "--hourly-window-duration-hours", "Hourly window duration must be 4-24 hours.");
        Require(o.HourlyIntervalHours is null || o.HourlyWindowDurationHours is null || o.HourlyWindowDurationHours >= o.HourlyIntervalHours,
            "--hourly-window-duration-hours", "Window duration must be at least the interval.");
        Require(o.ScheduleTime is null || o.ScheduleTimes is null, "--schedule-times", "Use only one of --schedule-time and --schedule-times.");
        Require(!IsWeekly(o.ScheduleFrequency) || o.DailyRetentionDays is null, "--daily-retention-days", "Weekly schedules cannot have daily retention.");
        Require(o.ScheduleFrequency is null || IsWeekly(o.ScheduleFrequency) || o.ScheduleDaysOfWeek is null,
            "--schedule-days-of-week", "Schedule days apply only to Weekly.");
        bool hourlyFlags = o.HourlyIntervalHours.HasValue || o.HourlyWindowStartTime is not null || o.HourlyWindowDurationHours.HasValue;
        Require(!hourlyFlags || o.ScheduleFrequency is null || Is(o.ScheduleFrequency, "Hourly"), "--schedule-frequency", "Hourly flags require an Hourly schedule.");
        Require(!Is(o.ScheduleFrequency, "Hourly") || (o.ScheduleTime is null && o.ScheduleTimes is null), "--schedule-times", "Hourly schedules use --hourly-window-start-time.");
        Require(!o.SmartTier.HasValue || (o.ArchiveTierMode is null && o.ArchiveTierAfterDays is null), "--smart-tier", "Do not combine smart-tier with archive mode or days.");
        Require(!Is(o.ArchiveTierMode, "TierRecommended") || o.ArchiveTierAfterDays is null, "--archive-tier-after-days", "TierRecommended cannot specify archive days.");
    }
}
