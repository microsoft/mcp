// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
/// Only fires when the caller supplies at least one of the new IaasVM-parity
/// flags. When the caller uses only legacy <c>--schedule-time</c> and
/// <c>--daily-retention-days</c>, the update path bypasses this validator.
/// </remarks>
public static class PolicyUpdateValidator
{
    public static PolicyValidationResult Validate(PolicyUpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var issues = new List<PolicyValidationIssue>();

        // Weekly schedule requires days-of-week.
        if (IsWeekly(options.ScheduleFrequency) && string.IsNullOrWhiteSpace(options.ScheduleDaysOfWeek))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleDaysOfWeekName}",
                "Weekly schedules require --schedule-days-of-week."));
        }

        // Reject unsupported frequencies (Hourly requires PolicySubType=Enhanced which update does not touch).
        if (!string.IsNullOrWhiteSpace(options.ScheduleFrequency) &&
            !IsDaily(options.ScheduleFrequency) &&
            !IsWeekly(options.ScheduleFrequency))
        {
            issues.Add(new PolicyValidationIssue(
                $"--{AzureBackupOptionDefinitions.ScheduleFrequencyName}",
                $"Unsupported schedule frequency '{options.ScheduleFrequency}'. " +
                "Policy update supports only 'Daily' or 'Weekly'. Recreate the policy to switch to Hourly."));
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

        return issues.Count == 0 ? PolicyValidationResult.Ok : PolicyValidationResult.Fail(issues);
    }

    private static void ValidateMonthly(PolicyUpdateOptions options, List<PolicyValidationIssue> issues)
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

    private static void ValidateYearly(PolicyUpdateOptions options, List<PolicyValidationIssue> issues)
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
}
