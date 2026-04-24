// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.ResourceManager.DataProtectionBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// Pure builder that translates a <see cref="PolicyCreateRequest"/> into a
/// <see cref="RuleBasedBackupPolicy"/> for Data Protection (DPP) vaults.
/// </summary>
/// <remarks>
/// AOT-safe: no reflection, no dynamic dispatch.
/// All inputs are nullable strings — when a flag is omitted the builder falls back
/// to the long-standing default behavior so existing minimal-flag invocations keep
/// working unchanged.
/// Multi-tier retention is opt-in: when <c>--weekly-retention-weeks</c>,
/// <c>--monthly-retention-months</c>, or <c>--yearly-retention-years</c> is supplied
/// the builder emits an extra <see cref="DataProtectionRetentionRule"/> + matching
/// <see cref="DataProtectionBackupTaggingCriteria"/> per tier, mirroring what the
/// portal/azure-cli generate.
/// Archive tiering is opt-in: when <c>--archive-tier-after-days</c> or
/// <c>--archive-tier-mode</c> is supplied the vault-store rule gains a
/// <see cref="TargetCopySetting"/> to <see cref="DataStoreType.ArchiveStore"/>.
/// </remarks>
public static class DppPolicyBuilder
{
    public static RuleBasedBackupPolicy Build(PolicyCreateRequest request, DppDatasourceProfile profile)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(profile);

        var dataStoreType = profile.UsesOperationalStore
            ? DataStoreType.OperationalStore
            : DataStoreType.VaultStore;

        var rules = new List<DataProtectionBasePolicyRule>
        {
            BuildDefaultRetentionRule(request, profile, dataStoreType),
        };

        // Per-tier retention rules (opt-in via positive weeks/months/years).
        if (TryParsePositiveInt(request.WeeklyRetentionWeeks, out var weeks))
        {
            rules.Add(BuildTierRetentionRule("Weekly", TimeSpan.FromDays(weeks * 7), dataStoreType, request));
        }
        if (TryParsePositiveInt(request.MonthlyRetentionMonths, out var months))
        {
            rules.Add(BuildTierRetentionRule("Monthly", TimeSpan.FromDays(months * 30), dataStoreType, request));
        }
        if (TryParsePositiveInt(request.YearlyRetentionYears, out var years))
        {
            rules.Add(BuildTierRetentionRule("Yearly", TimeSpan.FromDays(years * 365), dataStoreType, request));
        }

        if (!profile.IsContinuousBackup)
        {
            rules.Add(BuildBackupRule(request, profile, dataStoreType));
        }

        return new RuleBasedBackupPolicy([profile.ArmResourceType], rules);
    }

    private static DataProtectionRetentionRule BuildDefaultRetentionRule(
        PolicyCreateRequest request,
        DppDatasourceProfile profile,
        DataStoreType dataStoreType)
    {
        var retentionDays = TryParsePositiveInt(request.DailyRetentionDays, out var dd)
            ? dd
            : profile.DefaultRetentionDays;

        var lifeCycle = new SourceLifeCycle(
            new DataProtectionBackupAbsoluteDeleteSetting(TimeSpan.FromDays(retentionDays)),
            new DataStoreInfoBase(dataStoreType, "DataStoreInfoBase"));

        AppendArchiveCopyIfRequested(lifeCycle, request);

        return new DataProtectionRetentionRule("Default", [lifeCycle])
        {
            IsDefault = true,
        };
    }

    private static DataProtectionRetentionRule BuildTierRetentionRule(
        string tierName,
        TimeSpan duration,
        DataStoreType dataStoreType,
        PolicyCreateRequest request)
    {
        var lifeCycle = new SourceLifeCycle(
            new DataProtectionBackupAbsoluteDeleteSetting(duration),
            new DataStoreInfoBase(dataStoreType, "DataStoreInfoBase"));

        AppendArchiveCopyIfRequested(lifeCycle, request);

        return new DataProtectionRetentionRule(tierName, [lifeCycle])
        {
            IsDefault = false,
        };
    }

    private static void AppendArchiveCopyIfRequested(SourceLifeCycle lifeCycle, PolicyCreateRequest request)
    {
        var hasMode = !string.IsNullOrWhiteSpace(request.ArchiveTierMode);
        var hasDays = TryParsePositiveInt(request.ArchiveTierAfterDays, out var afterDays);

        if (!hasMode && !hasDays)
        {
            return;
        }

        DataProtectionBackupCopySetting copySetting = request.ArchiveTierMode?.Trim().ToUpperInvariant() switch
        {
            "COPYONEXPIRY" => new CopyOnExpirySetting(),
            "TIERAFTER" or null or "" when hasDays => new CustomCopySetting { Duration = TimeSpan.FromDays(afterDays) },
            "TIERAFTER" => new CopyOnExpirySetting(),
            _ when hasDays => new CustomCopySetting { Duration = TimeSpan.FromDays(afterDays) },
            _ => new CopyOnExpirySetting(),
        };

        lifeCycle.TargetDataStoreCopySettings.Add(
            new TargetCopySetting(copySetting, new DataStoreInfoBase(DataStoreType.ArchiveStore, "DataStoreInfoBase")));
    }

    private static DataProtectionBackupRule BuildBackupRule(
        PolicyCreateRequest request,
        DppDatasourceProfile profile,
        DataStoreType dataStoreType)
    {
        var scheduleStartTime = ParseScheduleStartTime(request.ScheduleTimes);
        var scheduleInterval = string.IsNullOrWhiteSpace(request.ScheduleFrequency)
            ? profile.ScheduleInterval
            : request.ScheduleFrequency!;

        var repeatingInterval = $"R/{scheduleStartTime:yyyy-MM-ddTHH:mm:ss+00:00}/{scheduleInterval}";
        var schedule = new DataProtectionBackupSchedule([repeatingInterval])
        {
            TimeZone = string.IsNullOrWhiteSpace(request.TimeZone) ? "UTC" : request.TimeZone!,
        };

        var taggingCriteria = BuildTaggingCriteria(request);

        var triggerContext = new ScheduleBasedBackupTriggerContext(schedule, taggingCriteria);

        return new DataProtectionBackupRule(
            profile.BackupRuleName,
            new DataStoreInfoBase(dataStoreType, "DataStoreInfoBase"),
            triggerContext)
        {
            BackupParameters = new DataProtectionBackupSettings(profile.BackupType),
        };
    }

    private static List<DataProtectionBackupTaggingCriteria> BuildTaggingCriteria(PolicyCreateRequest request)
    {
        // The default rule is always present; portal/cli uses TaggingPriority=99 with IsDefault=true.
        var list = new List<DataProtectionBackupTaggingCriteria>
        {
            new(true, 99, new DataProtectionBackupRetentionTag("Default")),
        };

        long priority = 25;

        if (TryParsePositiveInt(request.WeeklyRetentionWeeks, out _))
        {
            list.Add(BuildTierTagging("Weekly", priority, BackupAbsoluteMarker.FirstOfWeek));
            priority -= 5;
        }
        if (TryParsePositiveInt(request.MonthlyRetentionMonths, out _))
        {
            list.Add(BuildTierTagging("Monthly", priority, BackupAbsoluteMarker.FirstOfMonth));
            priority -= 5;
        }
        if (TryParsePositiveInt(request.YearlyRetentionYears, out _))
        {
            list.Add(BuildTierTagging("Yearly", priority, BackupAbsoluteMarker.FirstOfYear));
        }

        return list;
    }

    private static DataProtectionBackupTaggingCriteria BuildTierTagging(string tierName, long priority, BackupAbsoluteMarker marker)
    {
        var criteria = new ScheduleBasedBackupCriteria();
        criteria.AbsoluteCriteria.Add(marker);

        return new DataProtectionBackupTaggingCriteria(false, priority, new DataProtectionBackupRetentionTag(tierName))
        {
            Criteria = { criteria },
        };
    }

    private static DateTimeOffset ParseScheduleStartTime(string? scheduleTimes)
    {
        var raw = scheduleTimes;
        if (string.IsNullOrWhiteSpace(raw))
        {
            raw = "02:00";
        }
        else
        {
            var first = raw!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (first.Length > 0)
            {
                raw = first[0];
            }
        }

        var parts = raw!.Split(':');
        var hour = parts.Length > 0 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var h) ? h : 2;
        var minute = parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m) ? m : 0;

        var now = DateTimeOffset.UtcNow;
        return new DateTimeOffset(now.Year, now.Month, now.Day, hour, minute, 0, TimeSpan.Zero);
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
}
