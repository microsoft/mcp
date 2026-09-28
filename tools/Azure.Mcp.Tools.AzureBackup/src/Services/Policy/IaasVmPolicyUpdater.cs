// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json.Nodes;
using Azure.ResourceManager.RecoveryServicesBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Merges VM policy updates into the fetched SDK model. Never rebuilds a policy from create
/// defaults: unmentioned settings modeled by the SDK are retained, subject to schedule
/// reconciliation. Preservation of unknown wire fields is not guaranteed.
/// </summary>
internal static class IaasVmPolicyUpdater
{
    internal static void ValidateWorkload(BackupGenericProtectionPolicy policy, PolicyUpdateRequest request)
    {
        if (policy is not IaasVmProtectionPolicy && request.HasIaasVmExtendedFlags())
        {
            throw new ArgumentException("VM-only update options cannot be applied to this policy. Other RSV workloads support only --schedule-time and --daily-retention-days.");
        }
    }

    internal static void Apply(IaasVmProtectionPolicy policy, PolicyUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(request);
        var validation = PolicyUpdateValidator.Validate(request);
        if (!validation.IsValid)
        {
            throw new ArgumentException(string.Join(" ", validation.Issues.Select(i => $"[{i.Flag}] {i.Message}")));
        }
        if (!request.HasAnyInput())
        {
            return;
        }

        var v1 = policy.SchedulePolicy as SimpleSchedulePolicy;
        var v2 = policy.SchedulePolicy as SimpleSchedulePolicyV2;
        if (v1 is null && v2 is null)
        {
            throw new ArgumentException("The existing VM schedule type is unsupported; it cannot be safely updated.");
        }
        bool enhanced = v2 is not null;
        if ((policy.PolicyType is not null && policy.PolicyType != IaasVmPolicyType.V1 && policy.PolicyType != IaasVmPolicyType.V2)
            || (policy.PolicyType == IaasVmPolicyType.V1 && enhanced)
            || (policy.PolicyType == IaasVmPolicyType.V2 && !enhanced))
        {
            throw new ArgumentException("The existing VM subtype and schedule are inconsistent or unsupported.");
        }
        if (request.PolicySubType is not null && PolicyUpdateValidator.Is(request.PolicySubType, "Enhanced") != enhanced)
        {
            throw new ArgumentException("--policy-sub-type must match the existing Standard/Enhanced subtype. Update does not migrate policies.");
        }

        var currentFrequency = v1 is not null ? v1.ScheduleRunFrequency : v2!.ScheduleRunFrequency;
        if (currentFrequency != ScheduleRunType.Daily && currentFrequency != ScheduleRunType.Weekly && currentFrequency != ScheduleRunType.Hourly)
        {
            throw new ArgumentException("The existing schedule frequency is unsupported.");
        }
        var frequency = request.ScheduleFrequency is null ? currentFrequency
            : PolicyUpdateValidator.Is(request.ScheduleFrequency, "Hourly") ? ScheduleRunType.Hourly
            : PolicyUpdateValidator.Is(request.ScheduleFrequency, "Weekly") ? ScheduleRunType.Weekly : ScheduleRunType.Daily;
        bool hourly = frequency == ScheduleRunType.Hourly;
        bool weekly = frequency == ScheduleRunType.Weekly;
        bool hourlyFlags = request.HourlyIntervalHours.HasValue || request.HourlyWindowStartTime is not null || request.HourlyWindowDurationHours.HasValue;
        bool scheduleChanged = request.ScheduleFrequency is not null || request.ScheduleTime is not null || request.ScheduleTimes is not null
            || request.ScheduleDaysOfWeek is not null || hourlyFlags;
        bool retentionChanged = request.DailyRetentionDays is not null || request.WeeklyRetentionWeeks > 0
            || request.MonthlyRetentionMonths > 0 || request.YearlyRetentionYears > 0;
        bool scheduleOrRetentionChanged = scheduleChanged || retentionChanged;
        if (hourly && !enhanced)
        {
            throw new ArgumentException("Hourly schedules require an existing Enhanced (V2) policy.");
        }
        if ((!hourly && hourlyFlags) || (hourly && (request.ScheduleTimes is not null || request.ScheduleTime is not null)))
        {
            throw new ArgumentException("Hourly options apply only to Hourly schedules, which use --hourly-window-start-time instead of schedule-times.");
        }
        if (!weekly && request.ScheduleDaysOfWeek is not null)
        {
            throw new ArgumentException("--schedule-days-of-week applies only to Weekly schedules; specify --schedule-frequency Weekly to transition.");
        }

        var times = ExistingTimes(v1, v2, currentFrequency);
        var suppliedTime = request.ScheduleTimes ?? request.ScheduleTime;
        if (suppliedTime is not null)
        {
            times = [PolicyUpdateValidator.ParseTime(suppliedTime)];
        }
        if (scheduleOrRetentionChanged && !hourly && times.Count != 1)
        {
            throw new ArgumentException("Daily/Weekly VM schedules require exactly one schedule time. Supply --schedule-times when no unambiguous existing time is available.");
        }

        var days = request.ScheduleDaysOfWeek is not null ? RsvPolicyBuilder.ParseDaysOfWeek(request.ScheduleDaysOfWeek)
            : v1?.ScheduleRunDays.ToList() ?? v2?.WeeklySchedule?.ScheduleRunDays.ToList() ?? [];
        if (scheduleOrRetentionChanged && weekly && days.Count == 0)
        {
            throw new ArgumentException("Weekly schedules require --schedule-days-of-week when the existing schedule has no days.");
        }

        var oldHourly = currentFrequency == ScheduleRunType.Hourly ? v2?.HourlySchedule : null;
        if (hourly && currentFrequency != ScheduleRunType.Hourly
            && (request.HourlyIntervalHours is null || request.HourlyWindowStartTime is null || request.HourlyWindowDurationHours is null))
        {
            throw new ArgumentException("Transitioning to Hourly requires --hourly-interval-hours, --hourly-window-start-time and --hourly-window-duration-hours.");
        }
        var interval = request.HourlyIntervalHours ?? oldHourly?.Interval;
        var duration = request.HourlyWindowDurationHours ?? oldHourly?.ScheduleWindowDuration;
        var start = request.HourlyWindowStartTime is null ? oldHourly?.ScheduleWindowStartOn
            : ParseHourlyWindowStartTime(request.HourlyWindowStartTime, request.TimeZone ?? policy.TimeZone ?? "UTC");
        if (scheduleChanged && hourly && (interval is not (4 or 6 or 8 or 12) || duration is not (>= 4 and <= 24) || duration < interval || start is null))
        {
            throw new ArgumentException("The assembled hourly schedule requires interval 4/6/8/12, a start time, and duration between the interval and 24 hours.");
        }

        var retention = policy.RetentionPolicy as LongTermRetentionPolicy
            ?? throw new ArgumentException("The existing VM retention type is unsupported; it cannot be safely replaced.");
        if (weekly && request.DailyRetentionDays is not null)
        {
            throw new ArgumentException("Weekly schedules cannot have --daily-retention-days.");
        }
        if (scheduleOrRetentionChanged && weekly && retention.DailySchedule?.RetentionDuration?.Count > 0 && request.WeeklyRetentionWeeks <= 0)
        {
            throw new ArgumentException("Removing daily retention for a Weekly schedule requires explicit weekly retention weeks and days to avoid silently shortening retention.");
        }
        if (scheduleOrRetentionChanged && weekly && request.WeeklyRetentionWeeks <= 0 && retention.WeeklySchedule?.RetentionDuration?.Count is not > 0)
        {
            throw new ArgumentException("Weekly schedules require weekly retention.");
        }
        if (scheduleOrRetentionChanged && !weekly && request.DailyRetentionDays is null && retention.DailySchedule?.RetentionDuration?.Count is not > 0)
        {
            throw new ArgumentException("Daily/Hourly schedules require --daily-retention-days when the existing policy has no daily retention.");
        }
        if (scheduleOrRetentionChanged && weekly)
        {
            var retainedDays = request.WeeklyRetentionWeeks > 0
                ? RsvPolicyBuilder.ParseDaysOfWeek(request.WeeklyRetentionDaysOfWeek)
                : retention.WeeklySchedule?.DaysOfTheWeek.ToList() ?? [];
            if (retainedDays.Count == 0 || retainedDays.Any(d => !days.Contains(d)))
            {
                throw new ArgumentException("Weekly retention days must be included in the backup schedule days. Update retention selectors explicitly if needed.");
            }
        }

        int? instantDays = request.InstantRpRetentionDays is null ? policy.InstantRPRetentionRangeInDays : PolicyUpdateValidator.ParsePositive(request.InstantRpRetentionDays);
        if (request.InstantRpRetentionDays is not null && instantDays is not (>= 1 and <= 30))
        {
            throw new ArgumentException("Instant RP retention must be 1-30 days; Azure enforces schedule-specific limits.");
        }
        if (!enhanced && weekly
            && (scheduleOrRetentionChanged || request.InstantRpRetentionDays is not null || request.InstantRpResourceGroup is not null)
            && instantDays != 5)
        {
            throw new ArgumentException("Standard Weekly policies require effective instant RP retention of 5 days. Specify --instant-rp-retention-days 5; omission preserves the existing value.");
        }
        var archiveDays = request.ArchiveTierAfterDays is null ? (int?)null : PolicyUpdateValidator.ParsePositive(request.ArchiveTierAfterDays);
        policy.TieringPolicy.TryGetValue("ArchivedRP", out var archive);
        var requestedMode = request.SmartTier.HasValue ? (request.SmartTier.Value ? TieringMode.TierRecommended : TieringMode.DoNotTier)
            : request.ArchiveTierMode is not null ? (PolicyUpdateValidator.Is(request.ArchiveTierMode, "TierAfter") ? TieringMode.TierAfter : TieringMode.TierRecommended)
            : archiveDays.HasValue ? TieringMode.TierAfter : (TieringMode?)null;
        var effectiveArchiveDays = archiveDays ?? (archive?.TieringMode == TieringMode.TierAfter && archive.DurationType == RetentionDurationType.Days ? archive.DurationValue : null);
        if (requestedMode == TieringMode.TierAfter && effectiveArchiveDays is not >= 45)
        {
            throw new ArgumentException("TierAfter requires --archive-tier-after-days of at least 45, or existing TierAfter days.");
        }

        // All preconditions are checked before changing the fetched SDK model.
        if (request.TimeZone is not null) { policy.TimeZone = request.TimeZone; }
        if (scheduleChanged)
        {
            if (v1 is not null)
            {
                v1.ScheduleRunFrequency = frequency;
                if (weekly && currentFrequency != ScheduleRunType.Weekly)
                {
                    v1.ScheduleWeeklyFrequency = 1;
                }
                Replace(v1.ScheduleRunTimes, times);
                Replace(v1.ScheduleRunDays, weekly ? days : []);
            }
            else
            {
                v2!.ScheduleRunFrequency = frequency;
                if (hourly)
                {
                    v2.HourlySchedule ??= new BackupHourlySchedule();
                    v2.HourlySchedule.Interval = interval;
                    v2.HourlySchedule.ScheduleWindowStartOn = start;
                    v2.HourlySchedule.ScheduleWindowDuration = duration;
                    v2.ScheduleRunTimes.Clear();
                    v2.WeeklySchedule = null;
                }
                else if (weekly)
                {
                    v2.WeeklySchedule ??= new BackupWeeklySchedule();
                    Replace(v2.WeeklySchedule.ScheduleRunDays, days);
                    Replace(v2.WeeklySchedule.ScheduleRunTimes, times);
                    v2.ScheduleRunTimes.Clear();
                    v2.HourlySchedule = null;
                }
                else
                {
                    // The SDK exposes dailySchedule.scheduleRunTimes as a flattened collection.
                    Replace(v2.ScheduleRunTimes, times);
                    v2.WeeklySchedule = null;
                    v2.HourlySchedule = null;
                }
                if (hourly || weekly)
                {
                    // DailySchedule is internal in this SDK version. Remove the inactive wire
                    // branch via the typed persistence interface (no reflection/AOT metadata).
                    var model = (IPersistableModel<SimpleSchedulePolicyV2>)v2;
                    var options = new ModelReaderWriterOptions("J");
                    var json = JsonNode.Parse(model.Write(options).ToString())!.AsObject();
                    json.Remove("dailySchedule");
                    policy.SchedulePolicy = model.Create(BinaryData.FromString(json.ToJsonString()), options);
                }
            }
        }

        // Hourly policies may carry retention times. Do not erase these on unrelated updates.
        var retentionTimes = hourly && start is { } hourlyStart ? new List<DateTimeOffset> { hourlyStart } : times;
        if (scheduleOrRetentionChanged && weekly)
        {
            retention.DailySchedule = null;
        }
        else if (request.DailyRetentionDays is not null)
        {
            bool newDailySchedule = retention.DailySchedule is null;
            retention.DailySchedule ??= new DailyRetentionSchedule();
            retention.DailySchedule.RetentionDuration ??= new RetentionDuration();
            retention.DailySchedule.RetentionDuration.Count = PolicyUpdateValidator.ParsePositive(request.DailyRetentionDays);
            retention.DailySchedule.RetentionDuration.DurationType = RetentionDurationType.Days;
            if (newDailySchedule || scheduleChanged)
            {
                Replace(retention.DailySchedule.RetentionTimes, retentionTimes);
            }
        }
        RsvBackupOperations.MergeIaasVmRetention(retention, request, retentionTimes, scheduleChanged);
        if (scheduleChanged)
        {
            // Reconcile retained times only when the backup schedule was explicitly updated.
            if (retention.DailySchedule is not null) { Replace(retention.DailySchedule.RetentionTimes, retentionTimes); }
            if (retention.WeeklySchedule is not null) { Replace(retention.WeeklySchedule.RetentionTimes, retentionTimes); }
            if (retention.MonthlySchedule is not null) { Replace(retention.MonthlySchedule.RetentionTimes, retentionTimes); }
            if (retention.YearlySchedule is not null) { Replace(retention.YearlySchedule.RetentionTimes, retentionTimes); }
        }
        if (request.InstantRpRetentionDays is not null) { policy.InstantRPRetentionRangeInDays = instantDays; }
        if (request.InstantRpResourceGroup is not null)
        {
            policy.InstantRPDetails ??= new InstantRPAdditionalDetails();
            policy.InstantRPDetails.AzureBackupRGNamePrefix = request.InstantRpResourceGroup;
        }
        if (request.SnapshotConsistency is not null)
        {
            policy.SnapshotConsistencyType = PolicyUpdateValidator.Is(request.SnapshotConsistency, "CrashConsistent")
                ? IaasVmSnapshotConsistencyType.OnlyCrashConsistent : (IaasVmSnapshotConsistencyType?)null;
        }
        if (requestedMode is not null)
        {
            archive ??= new BackupTieringPolicy();
            archive.TieringMode = requestedMode;
            archive.DurationValue = requestedMode == TieringMode.TierAfter ? effectiveArchiveDays : 0;
            archive.DurationType = requestedMode == TieringMode.TierAfter ? RetentionDurationType.Days : RetentionDurationType.Invalid;
            policy.TieringPolicy["ArchivedRP"] = archive;
        }
    }

    private static DateTimeOffset ParseHourlyWindowStartTime(string value, string timeZoneId)
    {
        var parsed = PolicyUpdateValidator.ParseTime(value);
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZone))
        {
            throw new ArgumentException("Cannot convert --hourly-window-start-time: unknown effective time zone. Supply a valid --time-zone.");
        }

        // Enhanced hourly windows require UTC, unlike the existing Daily/Weekly parsing:
        // https://learn.microsoft.com/azure/backup/backup-azure-vms-enhanced-policy#tab/powershell
        // HH:mm has no date. Use the existing deterministic 2000-01-01 baseline, not today.
        var local = DateTime.SpecifyKind(parsed.DateTime, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(local) || timeZone.IsAmbiguousTime(local))
        {
            throw new ArgumentException("--hourly-window-start-time is invalid or ambiguous in the effective time zone on the 2000-01-01 baseline. Specify an unambiguous local time.");
        }

        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, timeZone));
    }

    private static List<DateTimeOffset> ExistingTimes(SimpleSchedulePolicy? v1, SimpleSchedulePolicyV2? v2, ScheduleRunType? frequency)
    {
        if (v1 is not null) { return v1.ScheduleRunTimes.ToList(); }
        if (frequency == ScheduleRunType.Hourly)
        {
            return v2?.HourlySchedule?.ScheduleWindowStartOn is { } start ? [start] : [];
        }
        if (frequency == ScheduleRunType.Weekly) { return v2?.WeeklySchedule?.ScheduleRunTimes.ToList() ?? []; }
        // ScheduleRunTimes is the SDK's flattened dailySchedule.scheduleRunTimes property.
        return v2?.ScheduleRunTimes.ToList() ?? [];
    }

    private static void Replace<T>(IList<T> destination, IEnumerable<T> values)
    {
        destination.Clear();
        foreach (var value in values) { destination.Add(value); }
    }

    internal static Dictionary<string, string> ParseTags(string csv)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in csv.Split(','))
        {
            int separator = pair.IndexOf('=');
            if (separator <= 0 || string.IsNullOrWhiteSpace(pair[..separator]))
            {
                throw new ArgumentException("Policy tags must be comma-separated key=value pairs.");
            }
            var key = pair[..separator].Trim();
            var value = pair[(separator + 1)..].Trim();
            if (key.Length > 512 || value.Length > 256 || key.IndexOfAny(['<', '>', '%', '&', '\\', '?', '/']) >= 0 || !result.TryAdd(key, value))
            {
                throw new ArgumentException("Policy tags contain duplicate keys, invalid characters or excessive lengths.");
            }
        }
        if (result.Count > 50)
        {
            throw new ArgumentException("An ARM resource can have at most 50 tags.");
        }
        return result;
    }

    internal static void MergeTags(IDictionary<string, string> destination, string? csv)
    {
        if (csv is null) { return; }
        var tags = ParseTags(csv);
        if (destination.Keys.Concat(tags.Keys).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 50)
        {
            throw new ArgumentException("An ARM resource can have at most 50 tags.");
        }
        foreach (var (key, value) in tags)
        {
            var existingKey = destination.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
            destination[existingKey ?? key] = value;
        }
    }
}