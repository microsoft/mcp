// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Options;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Azure VM backup policy limits enforced by the Recovery Services service. Out-of-range values are
/// rejected by Azure with <c>UserErrorInvalidRequestParameter</c> and an unnamed parameter
/// (<c>NO_PARAM</c>), so they are validated client-side with messages that name the flag, the value,
/// and the allowed range.
/// </summary>
/// <remarks>
/// Limits: https://learn.microsoft.com/azure/backup/backup-instant-restore-capability#configure-snapshot-retention,
/// https://learn.microsoft.com/azure/backup/backup-azure-vms-enhanced-policy, and
/// https://learn.microsoft.com/azure/backup/backup-azure-vm-backup-faq.
/// </remarks>
internal static class IaasVmPolicyLimits
{
    internal const int StandardInstantRpMaxDays = 5;
    internal const int StandardWeeklyInstantRpDays = 5;
    internal const int EnhancedInstantRpMaxDays = 30;
    internal const int DailyRetentionMinDays = 7;
    internal const int DailyRetentionMaxDays = 9999;
    internal const int WeeklyRetentionMaxWeeks = 5163;
    internal const int MonthlyRetentionMaxMonths = 1188;
    internal const int YearlyRetentionMaxYears = 99;

    private const string InstantRpFlag = $"--{AzureBackupOptionDefinitions.InstantRpRetentionDaysName}";
    private const string DailyRetentionFlag = $"--{AzureBackupOptionDefinitions.DailyRetentionDaysName}";

    /// <summary>
    /// Returns an issue when <paramref name="days"/> violates the instant restore limits for the
    /// policy shape, or <c>null</c> when it is valid.
    /// </summary>
    /// <param name="days">Effective instant restore snapshot retention in days.</param>
    /// <param name="enhanced">True for Enhanced (V2) policies, including Hourly schedules.</param>
    /// <param name="weekly">True when the backup schedule is Weekly.</param>
    /// <param name="dailyRetentionDays">Effective daily retention, or <c>null</c> when unknown or not applicable.</param>
    /// <param name="hourlySnapshotsPerDay">Snapshots per day for an Hourly schedule, or <c>null</c> for other schedules.</param>
    internal static PolicyValidationIssue? ValidateInstantRpRetention(int days, bool enhanced, bool weekly, int? dailyRetentionDays, int? hourlySnapshotsPerDay = null)
    {
        if (enhanced)
        {
            if (days is < 1 or > EnhancedInstantRpMaxDays)
            {
                return new(InstantRpFlag, $"{days} days is outside the range allowed for Enhanced (V2) policies: 1-{EnhancedInstantRpMaxDays} days.");
            }
            if (hourlySnapshotsPerDay is int snapshots && MaxHourlyInstantRpDays(snapshots) is int hourlyMax && days > hourlyMax)
            {
                return new(InstantRpFlag, $"{days} days exceeds the maximum of {hourlyMax} days for Enhanced Hourly policies that take {snapshots} snapshots per day. Lower {InstantRpFlag}, or use a longer interval or shorter window so fewer snapshots are taken each day.");
            }
        }
        else if (weekly)
        {
            if (days != StandardWeeklyInstantRpDays)
            {
                return new(InstantRpFlag, $"{days} days is not allowed for Standard (V1) Weekly policies, which require exactly {StandardWeeklyInstantRpDays} days. Specify {InstantRpFlag} {StandardWeeklyInstantRpDays}.");
            }
        }
        else if (days is < 1 or > StandardInstantRpMaxDays)
        {
            return new(InstantRpFlag, $"{days} days is outside the range allowed for Standard (V1) policies: 1-{StandardInstantRpMaxDays} days. Use an Enhanced (V2) policy for up to {EnhancedInstantRpMaxDays} days.");
        }

        if (!weekly && dailyRetentionDays is int retention && days > retention)
        {
            return new(InstantRpFlag, $"{days} days exceeds the daily retention of {retention} days. Instant restore retention cannot be longer than daily retention; lower {InstantRpFlag} or raise {DailyRetentionFlag}.");
        }

        return null;
    }

    /// <summary>
    /// Returns an issue when a supplied long-term retention value is outside the Azure VM limits, or
    /// <c>null</c> when every supplied value is valid. Zero or <c>null</c> means "not supplied".
    /// </summary>
    internal static PolicyValidationIssue? ValidateLongTermRetention(int? dailyDays, int weeklyWeeks, int monthlyMonths, int yearlyYears)
    {
        if (dailyDays is int daily && daily is < DailyRetentionMinDays or > DailyRetentionMaxDays)
        {
            return new(DailyRetentionFlag, $"{daily} days is outside the range allowed for Azure VM policies: {DailyRetentionMinDays}-{DailyRetentionMaxDays} days.");
        }
        if (weeklyWeeks > WeeklyRetentionMaxWeeks)
        {
            return new($"--{AzureBackupOptionDefinitions.WeeklyRetentionWeeksName}", $"{weeklyWeeks} weeks exceeds the maximum of {WeeklyRetentionMaxWeeks} weeks for Azure VM policies.");
        }
        if (monthlyMonths > MonthlyRetentionMaxMonths)
        {
            return new($"--{AzureBackupOptionDefinitions.MonthlyRetentionMonthsName}", $"{monthlyMonths} months exceeds the maximum of {MonthlyRetentionMaxMonths} months for Azure VM policies.");
        }
        if (yearlyYears > YearlyRetentionMaxYears)
        {
            return new($"--{AzureBackupOptionDefinitions.YearlyRetentionYearsName}", $"{yearlyYears} years exceeds the maximum of {YearlyRetentionMaxYears} years for Azure VM policies.");
        }
        return null;
    }

    internal static string Format(PolicyValidationIssue issue) => $"[{issue.Flag}] {issue.Message}";

    /// <summary>
    /// Number of snapshots an Hourly schedule takes per day: one at the window start and one every
    /// <paramref name="intervalHours"/> within the window. Returns <c>null</c> for an incomplete schedule.
    /// </summary>
    internal static int? HourlySnapshotsPerDay(int? intervalHours, int? windowDurationHours) =>
        intervalHours is int interval and > 0 && windowDurationHours is int duration and > 0
            ? (duration + interval - 1) / interval
            : null;

    // The documented maximum falls as snapshots per day rise: every 4 hours over 24 hours (6 snapshots)
    // allows 17 days and every 6 hours (4 snapshots) allows 22. Only these documented values are enforced,
    // as upper bounds, so a value Azure accepts is never rejected.
    private static int? MaxHourlyInstantRpDays(int snapshotsPerDay) => snapshotsPerDay switch
    {
        >= 6 => 17,
        >= 4 => 22,
        _ => null,
    };
}
