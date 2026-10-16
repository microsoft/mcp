// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services.Policy;

public class IaasVmPolicyUpdaterTests
{
    private static DateTimeOffset At(int hour, int minute = 0) => new(2025, 1, 1, hour, minute, 0, TimeSpan.Zero);

    // Build independent SDK fixtures rather than relying on create's defaults/normalization.
    private static IaasVmProtectionPolicy Policy(bool enhanced = false, string frequency = "Daily")
    {
        var retention = new LongTermRetentionPolicy();
        if (frequency == "Weekly")
        {
            retention.WeeklySchedule = new WeeklyRetentionSchedule
            {
                RetentionDuration = new RetentionDuration { Count = 4, DurationType = RetentionDurationType.Weeks }
            };
            retention.WeeklySchedule.DaysOfTheWeek.Add(BackupDayOfWeek.Sunday);
            retention.WeeklySchedule.RetentionTimes.Add(At(2));
        }
        else
        {
            retention.DailySchedule = new DailyRetentionSchedule
            {
                RetentionDuration = new RetentionDuration { Count = 14, DurationType = RetentionDurationType.Days }
            };
            if (frequency != "Hourly")
            { retention.DailySchedule.RetentionTimes.Add(At(2)); }
        }

        BackupSchedulePolicy schedule;
        if (enhanced)
        {
            var v2 = new SimpleSchedulePolicyV2 { ScheduleRunFrequency = new ScheduleRunType(frequency) };
            if (frequency == "Hourly")
            {
                v2.HourlySchedule = new BackupHourlySchedule { Interval = 4, ScheduleWindowStartOn = At(8), ScheduleWindowDuration = 12 };
            }
            else if (frequency == "Weekly")
            {
                v2.WeeklySchedule = new BackupWeeklySchedule();
                v2.WeeklySchedule.ScheduleRunDays.Add(BackupDayOfWeek.Sunday);
                v2.WeeklySchedule.ScheduleRunTimes.Add(At(2));
            }
            else
            {
                v2.ScheduleRunTimes.Add(At(2));
            }
            schedule = v2;
        }
        else
        {
            var v1 = new SimpleSchedulePolicy { ScheduleRunFrequency = new ScheduleRunType(frequency), ScheduleWeeklyFrequency = 1 };
            v1.ScheduleRunTimes.Add(At(2));
            if (frequency == "Weekly")
            { v1.ScheduleRunDays.Add(BackupDayOfWeek.Sunday); }
            schedule = v1;
        }

        return new IaasVmProtectionPolicy
        {
            PolicyType = enhanced ? IaasVmPolicyType.V2 : IaasVmPolicyType.V1,
            SchedulePolicy = schedule,
            RetentionPolicy = retention,
            TimeZone = "UTC",
            InstantRPRetentionRangeInDays = !enhanced && frequency == "Weekly" ? 5 : 2,
            InstantRPDetails = new InstantRPAdditionalDetails { AzureBackupRGNamePrefix = "original", AzureBackupRGNameSuffix = "suffix" },
            SnapshotConsistencyType = null
        };
    }

    private static LongTermRetentionPolicy Retention(IaasVmProtectionPolicy policy) => Assert.IsType<LongTermRetentionPolicy>(policy.RetentionPolicy);

    private static void AssertNoDailySchedule(SimpleSchedulePolicyV2 schedule)
    {
        using var json = JsonDocument.Parse(((IPersistableModel<SimpleSchedulePolicyV2>)schedule).Write(ModelReaderWriterOptions.Json).ToString());
        Assert.False(json.RootElement.TryGetProperty("dailySchedule", out _));
    }

    private static PolicyUpdateRequest WeeklyTransition() => new()
    {
        ScheduleFrequency = "Weekly",
        ScheduleDaysOfWeek = "Sunday,Wednesday",
        ScheduleTimes = "03:30",
        WeeklyRetentionWeeks = 8,
        WeeklyRetentionDaysOfWeek = "Sunday"
    };

    private static PolicyUpdateRequest HourlyTransition() => new()
    {
        ScheduleFrequency = "Hourly",
        HourlyIntervalHours = 6,
        HourlyWindowStartTime = "09:30",
        HourlyWindowDurationHours = 18
    };

    [Theory]
    [InlineData(false, "Daily")]
    [InlineData(false, "Weekly")]
    [InlineData(true, "Daily")]
    [InlineData(true, "Weekly")]
    [InlineData(true, "Hourly")]
    public void Apply_NoInput_PreservesModelInstances(bool enhanced, string frequency)
    {
        var policy = Policy(enhanced, frequency);
        var schedule = policy.SchedulePolicy;
        var retention = policy.RetentionPolicy;
        var instant = policy.InstantRPDetails;

        IaasVmPolicyUpdater.Apply(policy, new());

        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(retention, policy.RetentionPolicy);
        Assert.Same(instant, policy.InstantRPDetails);
        Assert.Equal("UTC", policy.TimeZone);
        Assert.Empty(policy.TieringPolicy);
    }

    [Theory]
    [InlineData(false, "Daily")]
    [InlineData(false, "Weekly")]
    [InlineData(true, "Daily")]
    [InlineData(true, "Weekly")]
    [InlineData(true, "Hourly")]
    public void Apply_RetentionOnly_PreservesScheduleAndUnmentionedFields(bool enhanced, string frequency)
    {
        var policy = Policy(enhanced, frequency);
        var schedule = policy.SchedulePolicy;
        var retention = policy.RetentionPolicy;
        var request = frequency == "Weekly"
            ? new PolicyUpdateRequest { WeeklyRetentionWeeks = 8, WeeklyRetentionDaysOfWeek = "Sunday" }
            : new PolicyUpdateRequest { DailyRetentionDays = "30" };

        IaasVmPolicyUpdater.Apply(policy, request);

        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(retention, policy.RetentionPolicy);
        Assert.Equal(enhanced ? IaasVmPolicyType.V2 : IaasVmPolicyType.V1, policy.PolicyType);
        Assert.Equal("UTC", policy.TimeZone);
        Assert.Equal(!enhanced && frequency == "Weekly" ? 5 : 2, policy.InstantRPRetentionRangeInDays);
        Assert.Equal("original", policy.InstantRPDetails.AzureBackupRGNamePrefix);
        Assert.Null(policy.SnapshotConsistencyType);
        if (schedule is SimpleSchedulePolicy v1)
        {
            Assert.Equal(new ScheduleRunType(frequency), v1.ScheduleRunFrequency);
            Assert.Equal(At(2), Assert.Single(v1.ScheduleRunTimes));
            Assert.Equal(1, v1.ScheduleWeeklyFrequency);
        }
        else
        {
            var v2 = Assert.IsType<SimpleSchedulePolicyV2>(schedule);
            Assert.Equal(new ScheduleRunType(frequency), v2.ScheduleRunFrequency);
            if (frequency == "Hourly")
            {
                Assert.Equal(4, v2.HourlySchedule.Interval);
                Assert.Equal(At(8), v2.HourlySchedule.ScheduleWindowStartOn);
                Assert.Equal(12, v2.HourlySchedule.ScheduleWindowDuration);
                Assert.Empty(Retention(policy).DailySchedule.RetentionTimes);
            }
            else
            {
                Assert.Equal(At(2), Assert.Single(frequency == "Daily" ? v2.ScheduleRunTimes : v2.WeeklySchedule.ScheduleRunTimes));
            }
        }
        if (frequency == "Weekly")
        {
            Assert.Null(Retention(policy).DailySchedule);
            Assert.Equal(8, Retention(policy).WeeklySchedule.RetentionDuration.Count);
            Assert.Equal(BackupDayOfWeek.Sunday, Assert.Single(Retention(policy).WeeklySchedule.DaysOfTheWeek));
        }
        else
        { Assert.Equal(30, Retention(policy).DailySchedule.RetentionDuration.Count); }
    }

    [Fact]
    public void Apply_V2DailyTimesAliasRetentionOnly_PreservesSchedule()
    {
        var policy = Policy(true);
        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        // The SDK exposes dailySchedule.scheduleRunTimes as a flattened collection.
        schedule.ScheduleRunTimes.Clear();
        schedule.ScheduleRunTimes.Add(At(5));
        var times = schedule.ScheduleRunTimes;

        IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" });

        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(times, schedule.ScheduleRunTimes);
        Assert.Equal(At(5), Assert.Single(schedule.ScheduleRunTimes));
        Assert.Equal(30, Retention(policy).DailySchedule.RetentionDuration.Count);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Apply_DailyTimePartial_SynchronizesRetentionTimes(bool enhanced, bool legacyFlag)
    {
        var policy = Policy(enhanced);
        var schedule = policy.SchedulePolicy;
        var request = legacyFlag ? new PolicyUpdateRequest { ScheduleTime = "05:30" } : new PolicyUpdateRequest { ScheduleTimes = "05:30" };

        IaasVmPolicyUpdater.Apply(policy, request);

        Assert.Same(schedule, policy.SchedulePolicy);
        var times = enhanced ? Assert.IsType<SimpleSchedulePolicyV2>(schedule).ScheduleRunTimes : Assert.IsType<SimpleSchedulePolicy>(schedule).ScheduleRunTimes;
        var time = Assert.Single(times);
        Assert.Equal(5, time.Hour);
        Assert.Equal(30, time.Minute);
        Assert.Equal(time, Assert.Single(Retention(policy).DailySchedule.RetentionTimes));
        Assert.Equal(14, Retention(policy).DailySchedule.RetentionDuration.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Apply_DailyToWeekly_RemovesDailyRetentionAndReconcilesDays(bool enhanced)
    {
        var policy = Policy(enhanced);
        var schedule = policy.SchedulePolicy;
        var retention = policy.RetentionPolicy;
        var request = WeeklyTransition();
        if (!enhanced)
        {
            request.InstantRpRetentionDays = "5";
            Assert.IsType<SimpleSchedulePolicy>(schedule).ScheduleWeeklyFrequency = 0;
        }

        IaasVmPolicyUpdater.Apply(policy, request);

        Assert.Same(retention, policy.RetentionPolicy);
        Assert.Equal(enhanced ? 2 : 5, policy.InstantRPRetentionRangeInDays);
        if (!enhanced)
        { Assert.Same(schedule, policy.SchedulePolicy); }
        var days = enhanced ? Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).WeeklySchedule.ScheduleRunDays : Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy).ScheduleRunDays;
        Assert.Equal(new[] { BackupDayOfWeek.Sunday, BackupDayOfWeek.Wednesday }, days);
        Assert.Null(Retention(policy).DailySchedule);
        Assert.Equal(8, Retention(policy).WeeklySchedule.RetentionDuration.Count);
        var retainedTime = Assert.Single(Retention(policy).WeeklySchedule.RetentionTimes);
        Assert.Equal(3, retainedTime.Hour);
        Assert.Equal(30, retainedTime.Minute);
        if (enhanced)
        {
            var v2 = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
            Assert.Equal(ScheduleRunType.Weekly, v2.ScheduleRunFrequency);
            Assert.Equal(retainedTime, Assert.Single(v2.WeeklySchedule.ScheduleRunTimes));
            AssertNoDailySchedule(v2);
            Assert.Null(v2.HourlySchedule);
        }
        else
        {
            var v1 = Assert.IsType<SimpleSchedulePolicy>(schedule);
            Assert.Equal(ScheduleRunType.Weekly, v1.ScheduleRunFrequency);
            Assert.Equal(1, v1.ScheduleWeeklyFrequency);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(2, null)]
    [InlineData(5, "2")]
    [InlineData(5, "4")]
    [InlineData(5, "6")]
    [InlineData(5, "30")]
    public void Apply_StandardDailyToWeekly_InvalidEffectiveInstantDays_RejectsBeforeMutation(int? existingDays, string? requestedDays)
    {
        var policy = Policy();
        policy.InstantRPRetentionRangeInDays = existingDays;
        var schedule = Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy);
        schedule.ScheduleWeeklyFrequency = 0;
        var retention = Retention(policy);
        var request = WeeklyTransition();
        request.InstantRpRetentionDays = requestedDays;
        request.TimeZone = "Pacific Standard Time";

        var error = Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, request));

        Assert.Contains("--instant-rp-retention-days 5", error.Message);
        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(retention, policy.RetentionPolicy);
        Assert.Equal(ScheduleRunType.Daily, schedule.ScheduleRunFrequency);
        Assert.Equal(0, schedule.ScheduleWeeklyFrequency);
        Assert.Equal(At(2), Assert.Single(schedule.ScheduleRunTimes));
        Assert.Empty(schedule.ScheduleRunDays);
        Assert.Equal(14, retention.DailySchedule.RetentionDuration.Count);
        Assert.Null(retention.WeeklySchedule);
        Assert.Equal(existingDays, policy.InstantRPRetentionRangeInDays);
        Assert.Equal("UTC", policy.TimeZone);
    }

    [Fact]
    public void Apply_StandardDailyToWeekly_OmittedInstantDays_PreservesExistingFive()
    {
        var policy = Policy();
        policy.InstantRPRetentionRangeInDays = 5;
        var request = WeeklyTransition();

        IaasVmPolicyUpdater.Apply(policy, request);

        Assert.Null(request.InstantRpRetentionDays);
        Assert.Equal(5, policy.InstantRPRetentionRangeInDays);
        Assert.Equal(ScheduleRunType.Weekly, Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy).ScheduleRunFrequency);
    }

    [Theory]
    [InlineData("time")]
    [InlineData("legacy-time")]
    [InlineData("days")]
    [InlineData("weekly-retention")]
    [InlineData("monthly-retention")]
    [InlineData("yearly-retention")]
    [InlineData("instant-days")]
    [InlineData("instant-prefix")]
    public void Apply_StandardWeeklyPartial_RequiresEffectiveInstantDaysFive(string scenario)
    {
        var policy = Policy(false, "Weekly");
        policy.InstantRPRetentionRangeInDays = 2;
        var request = scenario switch
        {
            "time" => new PolicyUpdateRequest { ScheduleTimes = "03:00" },
            "legacy-time" => new PolicyUpdateRequest { ScheduleTime = "03:00" },
            "days" => new PolicyUpdateRequest { ScheduleDaysOfWeek = "Sunday,Monday" },
            "weekly-retention" => new PolicyUpdateRequest { WeeklyRetentionWeeks = 8, WeeklyRetentionDaysOfWeek = "Sunday" },
            "monthly-retention" => new PolicyUpdateRequest { MonthlyRetentionMonths = 12, MonthlyRetentionDaysOfMonth = "1" },
            "yearly-retention" => new PolicyUpdateRequest { YearlyRetentionYears = 5, YearlyRetentionMonths = "January", YearlyRetentionDaysOfMonth = "1" },
            "instant-days" => new PolicyUpdateRequest { InstantRpRetentionDays = "4" },
            "instant-prefix" => new PolicyUpdateRequest { InstantRpResourceGroup = "updated-rg" },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var model = (IPersistableModel<IaasVmProtectionPolicy>)policy;
        var before = model.Write(ModelReaderWriterOptions.Json).ToString();

        var error = Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, request));

        Assert.Contains("--instant-rp-retention-days 5", error.Message);
        Assert.Equal(before, model.Write(ModelReaderWriterOptions.Json).ToString());

        request.InstantRpRetentionDays = "5";
        IaasVmPolicyUpdater.Apply(policy, request);
        Assert.Equal(5, policy.InstantRPRetentionRangeInDays);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void Apply_StandardWeeklyUnrelatedUpdate_DoesNotValidateOrChangeInstantDays(int? instantDays, bool timeZoneOnly)
    {
        var policy = Policy(false, "Weekly");
        policy.InstantRPRetentionRangeInDays = instantDays;
        var schedule = policy.SchedulePolicy;
        var retention = policy.RetentionPolicy;

        IaasVmPolicyUpdater.Apply(policy, timeZoneOnly
            ? new() { TimeZone = "Pacific Standard Time" }
            : new() { PolicyTags = "owner=backup" });

        Assert.Equal(instantDays, policy.InstantRPRetentionRangeInDays);
        Assert.Equal(timeZoneOnly ? "Pacific Standard Time" : "UTC", policy.TimeZone);
        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(retention, policy.RetentionPolicy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Apply_WeeklyToDaily_RequiresDailyRetentionAndClearsWeeklySchedule(bool enhanced)
    {
        var policy = Policy(enhanced, "Weekly");
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { ScheduleFrequency = "Daily" }));

        IaasVmPolicyUpdater.Apply(policy, new() { ScheduleFrequency = "Daily", DailyRetentionDays = "30", ScheduleTimes = "04:00" });

        Assert.Equal(30, Retention(policy).DailySchedule.RetentionDuration.Count);
        Assert.Equal(4, Retention(policy).WeeklySchedule.RetentionDuration.Count);
        Assert.Equal(4, Assert.Single(Retention(policy).WeeklySchedule.RetentionTimes).Hour);
        if (enhanced)
        {
            var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
            Assert.Equal(ScheduleRunType.Daily, schedule.ScheduleRunFrequency);
            Assert.Null(schedule.WeeklySchedule);
            Assert.Equal(4, Assert.Single(schedule.ScheduleRunTimes).Hour);
        }
        else
        {
            var schedule = Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy);
            Assert.Equal(ScheduleRunType.Daily, schedule.ScheduleRunFrequency);
            Assert.Empty(schedule.ScheduleRunDays);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Apply_WeeklyPartials_PreserveUnmentionedTimeAndDays(bool enhanced)
    {
        var policy = Policy(enhanced, "Weekly");
        IaasVmPolicyUpdater.Apply(policy, new() { ScheduleTimes = "06:00" });
        IaasVmPolicyUpdater.Apply(policy, new() { ScheduleDaysOfWeek = "Sunday,Monday" });

        var times = enhanced ? Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).WeeklySchedule.ScheduleRunTimes : Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy).ScheduleRunTimes;
        var days = enhanced ? Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).WeeklySchedule.ScheduleRunDays : Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy).ScheduleRunDays;
        Assert.Equal(6, Assert.Single(times).Hour);
        Assert.Equal(new[] { BackupDayOfWeek.Sunday, BackupDayOfWeek.Monday }, days);
        Assert.Equal(BackupDayOfWeek.Sunday, Assert.Single(Retention(policy).WeeklySchedule.DaysOfTheWeek));
    }

    [Theory]
    [InlineData("interval")]
    [InlineData("start")]
    [InlineData("duration")]
    public void Apply_HourlyPartial_PreservesOtherWindowFields(string field)
    {
        var policy = Policy(true, "Hourly");
        var retention = policy.RetentionPolicy;
        var request = field switch
        {
            "interval" => new PolicyUpdateRequest { HourlyIntervalHours = 6 },
            "start" => new PolicyUpdateRequest { HourlyWindowStartTime = "10:30" },
            _ => new PolicyUpdateRequest { HourlyWindowDurationHours = 24 }
        };

        IaasVmPolicyUpdater.Apply(policy, request);

        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        var hourly = Assert.IsType<BackupHourlySchedule>(schedule.HourlySchedule);
        Assert.Same(retention, policy.RetentionPolicy);
        AssertNoDailySchedule(schedule);
        Assert.Null(schedule.WeeklySchedule);
        Assert.Equal(ScheduleRunType.Hourly, schedule.ScheduleRunFrequency);
        Assert.Equal(field == "interval" ? 6 : 4, hourly.Interval);
        Assert.Equal(field == "duration" ? 24 : 12, hourly.ScheduleWindowDuration);
        Assert.Equal(field == "start" ? 10 : 8, hourly.ScheduleWindowStartOn!.Value.Hour);
        Assert.Equal(field == "start" ? 30 : 0, hourly.ScheduleWindowStartOn.Value.Minute);
        Assert.Equal(hourly.ScheduleWindowStartOn, Assert.Single(Retention(policy).DailySchedule.RetentionTimes));
        Assert.Equal(14, Retention(policy).DailySchedule.RetentionDuration.Count);
    }

    [Theory]
    [InlineData("India Standard Time", null, "08:00", 2000, 1, 1, 2, 30)]
    [InlineData("Pacific Standard Time", "India Standard Time", "08:00", 2000, 1, 1, 2, 30)]
    [InlineData("India Standard Time", "UTC", "08:00", 2000, 1, 1, 8, 0)]
    [InlineData("UTC", null, "08:00", 2000, 1, 1, 8, 0)]
    [InlineData(null, null, "08:00", 2000, 1, 1, 8, 0)]
    [InlineData("India Standard Time", null, "02:00", 1999, 12, 31, 20, 30)]
    [InlineData("Pacific Standard Time", null, "20:00", 2000, 1, 2, 4, 0)]
    public void Apply_HourlyStart_ConvertsEffectiveLocalTimeToUtc(
        string? existingTimeZone, string? suppliedTimeZone, string localTime,
        int year, int month, int day, int hour, int minute)
    {
        var policy = Policy(true, "Hourly");
        policy.TimeZone = existingTimeZone;
        var expected = new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero);

        IaasVmPolicyUpdater.Apply(policy, new() { HourlyWindowStartTime = localTime, TimeZone = suppliedTimeZone });

        var hourly = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).HourlySchedule;
        Assert.True(expected.EqualsExact(hourly.ScheduleWindowStartOn!.Value));
        Assert.True(expected.EqualsExact(Assert.Single(Retention(policy).DailySchedule.RetentionTimes)));
        Assert.Equal(suppliedTimeZone ?? existingTimeZone, policy.TimeZone);
        Assert.Equal(4, hourly.Interval);
        Assert.Equal(12, hourly.ScheduleWindowDuration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("India Standard Time")]
    public void Apply_TransitionToHourly_ConvertsNewStartUsingEffectiveTimeZone(string? suppliedTimeZone)
    {
        var policy = Policy(true);
        policy.TimeZone = suppliedTimeZone is null ? "India Standard Time" : "Pacific Standard Time";
        var request = HourlyTransition();
        request.TimeZone = suppliedTimeZone;

        IaasVmPolicyUpdater.Apply(policy, request);

        var expected = new DateTimeOffset(2000, 1, 1, 4, 0, 0, TimeSpan.Zero);
        var hourly = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).HourlySchedule;
        Assert.True(expected.EqualsExact(hourly.ScheduleWindowStartOn!.Value));
        Assert.True(expected.EqualsExact(Assert.Single(Retention(policy).DailySchedule.RetentionTimes)));
    }

    [Theory]
    [InlineData("interval")]
    [InlineData("duration")]
    [InlineData("retention")]
    [InlineData("time-zone")]
    public void Apply_HourlyStartOmitted_PreservesExactStoredTimestamp(string scenario)
    {
        var policy = Policy(true, "Hourly");
        policy.TimeZone = "India Standard Time";
        var original = new DateTimeOffset(2025, 7, 12, 2, 30, 45, TimeSpan.Zero);
        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        schedule.HourlySchedule.ScheduleWindowStartOn = original;
        var before = ((IPersistableModel<SimpleSchedulePolicyV2>)schedule).Write(ModelReaderWriterOptions.Json).ToString();
        var request = scenario switch
        {
            "interval" => new PolicyUpdateRequest { HourlyIntervalHours = 6 },
            "duration" => new PolicyUpdateRequest { HourlyWindowDurationHours = 24 },
            "retention" => new PolicyUpdateRequest { DailyRetentionDays = "30" },
            _ => new PolicyUpdateRequest { TimeZone = "Pacific Standard Time" }
        };

        IaasVmPolicyUpdater.Apply(policy, request);

        var after = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        Assert.True(original.EqualsExact(after.HourlySchedule.ScheduleWindowStartOn!.Value));
        if (scenario is "retention" or "time-zone")
        {
            Assert.Same(schedule, after);
            Assert.Equal(before, ((IPersistableModel<SimpleSchedulePolicyV2>)after).Write(ModelReaderWriterOptions.Json).ToString());
            Assert.Empty(Retention(policy).DailySchedule.RetentionTimes);
        }
        else
        {
            Assert.True(original.EqualsExact(Assert.Single(Retention(policy).DailySchedule.RetentionTimes)));
        }
    }

    [Fact]
    public void Apply_HourlyStart_UnknownExistingTimeZoneRejectsBeforeMutation()
    {
        var policy = Policy(true, "Hourly");
        policy.TimeZone = "Unknown time zone";
        var model = (IPersistableModel<IaasVmProtectionPolicy>)policy;
        var before = model.Write(ModelReaderWriterOptions.Json).ToString();

        var error = Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { HourlyWindowStartTime = "08:00" }));

        Assert.Contains("--time-zone", error.Message);
        Assert.Equal(before, model.Write(ModelReaderWriterOptions.Json).ToString());

        // An explicit valid zone supersedes an unsupported fetched identifier.
        IaasVmPolicyUpdater.Apply(policy, new() { HourlyWindowStartTime = "08:00", TimeZone = "India Standard Time" });
        Assert.Equal("India Standard Time", policy.TimeZone);
        Assert.Equal(new DateTimeOffset(2000, 1, 1, 2, 30, 0, TimeSpan.Zero),
            Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).HourlySchedule.ScheduleWindowStartOn);
    }

    [Theory]
    [InlineData(false, "Daily")]
    [InlineData(false, "Weekly")]
    [InlineData(true, "Daily")]
    [InlineData(true, "Weekly")]
    public void Apply_DailyWeeklyTime_KeepsExistingClockEncodingWithNonUtcTimeZone(bool enhanced, string frequency)
    {
        var policy = Policy(enhanced, frequency);

        IaasVmPolicyUpdater.Apply(policy, new() { ScheduleTimes = "08:00", TimeZone = "India Standard Time" });

        var times = policy.SchedulePolicy is SimpleSchedulePolicy v1 ? v1.ScheduleRunTimes
            : frequency == "Weekly" ? Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).WeeklySchedule.ScheduleRunTimes
            : Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).ScheduleRunTimes;
        Assert.True(new DateTimeOffset(2000, 1, 1, 8, 0, 0, TimeSpan.Zero).EqualsExact(Assert.Single(times)));
    }

    [Theory]
    [InlineData("Daily")]
    [InlineData("Weekly")]
    public void Apply_EnhancedToHourly_ReconcilesSchedulesAndRetentionTimes(string frequency)
    {
        var policy = Policy(true, frequency);
        var request = HourlyTransition();
        request.DailyRetentionDays = "30";
        request.WeeklyRetentionWeeks = 8;
        request.WeeklyRetentionDaysOfWeek = "Sunday";
        request.MonthlyRetentionMonths = 12;
        request.MonthlyRetentionDaysOfMonth = "1";
        request.YearlyRetentionYears = 5;
        request.YearlyRetentionMonths = "January";
        request.YearlyRetentionDaysOfMonth = "1";

        IaasVmPolicyUpdater.Apply(policy, request);

        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        Assert.Equal(ScheduleRunType.Hourly, schedule.ScheduleRunFrequency);
        AssertNoDailySchedule(schedule);
        Assert.Null(schedule.WeeklySchedule);
        Assert.Equal(6, schedule.HourlySchedule.Interval);
        Assert.Equal(18, schedule.HourlySchedule.ScheduleWindowDuration);
        Assert.Equal(9, schedule.HourlySchedule.ScheduleWindowStartOn!.Value.Hour);
        var retention = Retention(policy);
        Assert.Equal(schedule.HourlySchedule.ScheduleWindowStartOn, Assert.Single(retention.DailySchedule.RetentionTimes));
        Assert.Equal(schedule.HourlySchedule.ScheduleWindowStartOn, Assert.Single(retention.WeeklySchedule.RetentionTimes));
        Assert.Equal(schedule.HourlySchedule.ScheduleWindowStartOn, Assert.Single(retention.MonthlySchedule.RetentionTimes));
        Assert.Equal(schedule.HourlySchedule.ScheduleWindowStartOn, Assert.Single(retention.YearlySchedule.RetentionTimes));
        Assert.Equal(12, retention.MonthlySchedule.RetentionDuration.Count);
        Assert.Equal(5, retention.YearlySchedule.RetentionDuration.Count);
    }

    [Theory]
    [InlineData("Daily")]
    [InlineData("Weekly")]
    public void Apply_HourlyToNonHourly_RemovesHourlyWindow(string frequency)
    {
        var policy = Policy(true, "Hourly");
        var request = frequency == "Weekly" ? WeeklyTransition() : new PolicyUpdateRequest { ScheduleFrequency = "Daily" };

        IaasVmPolicyUpdater.Apply(policy, request);

        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        Assert.Equal(new ScheduleRunType(frequency), schedule.ScheduleRunFrequency);
        Assert.Null(schedule.HourlySchedule);
        var times = frequency == "Daily" ? schedule.ScheduleRunTimes : schedule.WeeklySchedule.ScheduleRunTimes;
        Assert.Equal(frequency == "Daily" ? 8 : 3, Assert.Single(times).Hour);
    }

    [Theory]
    [InlineData("interval")]
    [InlineData("start")]
    [InlineData("duration")]
    public void Apply_HourlyTransitionMissingField_RejectsWithoutMutation(string missing)
    {
        var policy = Policy(true);
        var request = HourlyTransition();
        if (missing == "interval")
        { request.HourlyIntervalHours = null; }
        if (missing == "start")
        { request.HourlyWindowStartTime = null; }
        if (missing == "duration")
        { request.HourlyWindowDurationHours = null; }
        request.TimeZone = "Pacific Standard Time";

        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, request));

        Assert.Equal("UTC", policy.TimeZone);
        Assert.Equal(ScheduleRunType.Daily, Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).ScheduleRunFrequency);
        Assert.Equal(14, Retention(policy).DailySchedule.RetentionDuration.Count);
    }

    [Fact]
    public void Apply_HourlyPartialInvalidAgainstExistingWindow_Rejects()
    {
        var policy = Policy(true, "Hourly");
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { HourlyWindowDurationHours = 6, HourlyIntervalHours = 8 }));
        IaasVmPolicyUpdater.Apply(policy, new() { HourlyWindowDurationHours = 4 });
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { HourlyIntervalHours = 8 }));
        Assert.Equal(4, Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy).HourlySchedule.Interval);
    }

    [Fact]
    public void Apply_WeeklyReconciliation_RejectsMissingDaysRetentionAndSelectorMismatch()
    {
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(Policy(), new() { ScheduleFrequency = "Weekly" }));
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(Policy(), new() { ScheduleFrequency = "Weekly", ScheduleDaysOfWeek = "Sunday" }));
        var mismatched = WeeklyTransition();
        mismatched.WeeklyRetentionDaysOfWeek = "Monday";
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(Policy(), mismatched));
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(Policy(false, "Weekly"), new() { ScheduleDaysOfWeek = "Monday" }));
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(Policy(false, "Weekly"), new() { DailyRetentionDays = "30" }));
    }

    [Theory]
    [InlineData(false, "30")]
    [InlineData(true, "7")]
    public void Apply_InstantSnapshotAndTimeZone_PreservesSuffixAndSchedule(bool enhanced, string instantDays)
    {
        var policy = Policy(enhanced);
        var schedule = policy.SchedulePolicy;
        var instant = policy.InstantRPDetails;

        IaasVmPolicyUpdater.Apply(policy, new()
        {
            PolicySubType = enhanced ? "Enhanced" : "Standard",
            InstantRpRetentionDays = instantDays,
            InstantRpResourceGroup = "updated-rg",
            SnapshotConsistency = "CrashConsistent",
            TimeZone = "Pacific Standard Time"
        });

        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(instant, policy.InstantRPDetails);
        Assert.Equal(enhanced ? 7 : 30, policy.InstantRPRetentionRangeInDays);
        Assert.Equal("updated-rg", instant.AzureBackupRGNamePrefix);
        Assert.Equal("suffix", instant.AzureBackupRGNameSuffix);
        Assert.Equal(IaasVmSnapshotConsistencyType.OnlyCrashConsistent, policy.SnapshotConsistencyType);
        Assert.Equal("Pacific Standard Time", policy.TimeZone);
        Assert.Equal(14, Retention(policy).DailySchedule.RetentionDuration.Count);
    }

    [Fact]
    public void Apply_InstantDetailsAbsent_CreatesDetails()
    {
        var policy = Policy();
        policy.InstantRPDetails = null;
        IaasVmPolicyUpdater.Apply(policy, new() { InstantRpResourceGroup = "snapshots" });
        Assert.NotNull(policy.InstantRPDetails);
        Assert.Equal("snapshots", policy.InstantRPDetails.AzureBackupRGNamePrefix);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public void Apply_SmartTier_DistinguishesOmittedFromExplicitFalse(bool? smartTier)
    {
        var policy = Policy();
        var archive = new BackupTieringPolicy { TieringMode = TieringMode.TierAfter, DurationValue = 90, DurationType = RetentionDurationType.Days };
        policy.TieringPolicy["ArchivedRP"] = archive;
        var other = new BackupTieringPolicy { TieringMode = TieringMode.DoNotTier };
        policy.TieringPolicy["OtherTier"] = other;

        IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30", SmartTier = smartTier });

        Assert.Same(archive, policy.TieringPolicy["ArchivedRP"]);
        Assert.Same(other, policy.TieringPolicy["OtherTier"]);
        Assert.Equal(smartTier is null ? TieringMode.TierAfter : smartTier.Value ? TieringMode.TierRecommended : TieringMode.DoNotTier, archive.TieringMode);
        Assert.Equal(smartTier is null ? 90 : 0, archive.DurationValue);
        Assert.Equal(smartTier is null ? RetentionDurationType.Days : RetentionDurationType.Invalid, archive.DurationType);
    }

    [Fact]
    public void Apply_ArchivePartial_MergesExistingDaysAndSupportsRecommendedMode()
    {
        var policy = Policy();
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { ArchiveTierMode = "TierAfter" }));
        IaasVmPolicyUpdater.Apply(policy, new() { ArchiveTierAfterDays = "90" });
        IaasVmPolicyUpdater.Apply(policy, new() { ArchiveTierMode = "TierAfter" });
        Assert.Equal(90, policy.TieringPolicy["ArchivedRP"].DurationValue);
        IaasVmPolicyUpdater.Apply(policy, new() { ArchiveTierAfterDays = "120" });
        Assert.Equal(120, policy.TieringPolicy["ArchivedRP"].DurationValue);
        IaasVmPolicyUpdater.Apply(policy, new() { ArchiveTierMode = "TierRecommended" });
        Assert.Equal(TieringMode.TierRecommended, policy.TieringPolicy["ArchivedRP"].TieringMode);
        Assert.Equal(0, policy.TieringPolicy["ArchivedRP"].DurationValue);
        Assert.Equal(RetentionDurationType.Invalid, policy.TieringPolicy["ArchivedRP"].DurationType);
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { ArchiveTierMode = "TierAfter" }));
    }

    [Fact]
    public void MergeTags_OverwritesCaseInsensitiveKeysAndPreservesUnmentionedTags()
    {
        var tags = new Dictionary<string, string> { ["Owner"] = "original", ["keep"] = "value" };
        IaasVmPolicyUpdater.MergeTags(tags, null);
        Assert.Equal(2, tags.Count);
        IaasVmPolicyUpdater.MergeTags(tags, " owner = updated ,new=value=with=equals,empty=");
        Assert.Equal(4, tags.Count);
        Assert.Equal("updated", tags["Owner"]);
        Assert.Equal("value", tags["keep"]);
        Assert.Equal("value=with=equals", tags["new"]);
        Assert.Equal(string.Empty, tags["empty"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("key")]
    [InlineData("=value")]
    [InlineData("key=value,")]
    [InlineData("key=value,KEY=duplicate")]
    [InlineData("bad/key=value")]
    [InlineData("bad?key=value")]
    public void MergeTags_InvalidInput_DoesNotPartiallyMutate(string csv)
    {
        var tags = new Dictionary<string, string> { ["keep"] = "value" };
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.MergeTags(tags, csv));
        Assert.Equal("value", Assert.Single(tags).Value);
    }

    [Fact]
    public void MergeTags_EnforcesLengthsAndCombinedTagLimit()
    {
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.ParseTags($"{new string('k', 513)}=v"));
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.ParseTags($"key={new string('v', 257)}"));
        var tags = Enumerable.Range(0, 50).ToDictionary(i => $"key{i}", _ => "old");
        IaasVmPolicyUpdater.MergeTags(tags, "KEY0=updated");
        Assert.Equal("updated", tags["key0"]);
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.MergeTags(tags, "key0=bad,new=value"));
        Assert.Equal(50, tags.Count);
        Assert.Equal("updated", tags["key0"]);
    }

    [Theory]
    [InlineData("AzureFileShare")]
    [InlineData("MSSQL")]
    [InlineData("SAPHANA")]
    public void ValidateWorkload_RejectsVmOnlyFieldsButAllowsLegacyFields(string workload)
    {
        var policy = RsvPolicyBuilder.Build(new() { Policy = "p", WorkloadType = workload });
        IaasVmPolicyUpdater.ValidateWorkload(policy, new() { ScheduleTime = "03:00", DailyRetentionDays = "30" });
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.ValidateWorkload(policy, new() { HourlyIntervalHours = 6 }));
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.ValidateWorkload(policy, new() { SmartTier = false }));
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.ValidateWorkload(policy, new() { PolicyTags = "key=value" }));
    }

    [Theory]
    [InlineData(false, "Enhanced")]
    [InlineData(true, "Standard")]
    public void Apply_SubtypeMigration_Rejects(bool enhanced, string subtype)
    {
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(Policy(enhanced), new() { PolicySubType = subtype }));
    }

    [Fact]
    public void Apply_UnsupportedOrInconsistentModels_RejectsWithoutReplacingThem()
    {
        var policy = Policy();
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, HourlyTransition()));
        policy.PolicyType = IaasVmPolicyType.V2;
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" }));
        policy.PolicyType = new IaasVmPolicyType("Future");
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" }));
        policy.PolicyType = IaasVmPolicyType.V1;
        policy.SchedulePolicy = new LogSchedulePolicy();
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" }));
        Assert.IsType<LogSchedulePolicy>(policy.SchedulePolicy);
        policy = Policy();
        policy.RetentionPolicy = new SimpleRetentionPolicy();
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" }));
        Assert.IsType<SimpleRetentionPolicy>(policy.RetentionPolicy);
        policy = Policy();
        Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy).ScheduleRunFrequency = new ScheduleRunType("Future");
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" }));
    }

    [Theory]
    [InlineData("empty-time-entry")]
    [InlineData("bad-time")]
    [InlineData("both-times")]
    [InlineData("negative-retention")]
    [InlineData("zero-retention")]
    [InlineData("too-much-retention")]
    [InlineData("invalid-interval")]
    [InlineData("invalid-duration")]
    [InlineData("hourly-on-daily")]
    [InlineData("hourly-with-time")]
    [InlineData("daily-with-days")]
    [InlineData("invalid-day")]
    [InlineData("empty-day")]
    [InlineData("invalid-month-day")]
    [InlineData("negative-weekly")]
    [InlineData("invalid-subtype")]
    [InlineData("invalid-snapshot")]
    [InlineData("invalid-timezone")]
    [InlineData("invalid-prefix")]
    [InlineData("instant-over-limit")]
    [InlineData("archive-too-early")]
    [InlineData("archive-copy-on-expiry")]
    [InlineData("smart-and-mode")]
    [InlineData("false-and-days")]
    [InlineData("recommended-with-days")]
    public void Apply_InvalidInput_RejectsBeforeMutation(string scenario)
    {
        var request = scenario switch
        {
            "empty-time-entry" => new PolicyUpdateRequest { ScheduleTimes = "02:00,,14:00" },
            "bad-time" => new PolicyUpdateRequest { ScheduleTime = "25:00" },
            "both-times" => new PolicyUpdateRequest { ScheduleTime = "02:00", ScheduleTimes = "03:00" },
            "negative-retention" => new PolicyUpdateRequest { DailyRetentionDays = "-1" },
            "zero-retention" => new PolicyUpdateRequest { DailyRetentionDays = "0" },
            "too-much-retention" => new PolicyUpdateRequest { DailyRetentionDays = "10000" },
            "invalid-interval" => new PolicyUpdateRequest { HourlyIntervalHours = 5 },
            "invalid-duration" => new PolicyUpdateRequest { HourlyWindowDurationHours = 25 },
            "hourly-on-daily" => new PolicyUpdateRequest { HourlyIntervalHours = 6 },
            "hourly-with-time" => new PolicyUpdateRequest { ScheduleFrequency = "Hourly", ScheduleTimes = "02:00" },
            "daily-with-days" => new PolicyUpdateRequest { ScheduleDaysOfWeek = "Sunday" },
            "invalid-day" => new PolicyUpdateRequest { ScheduleDaysOfWeek = "InvalidDay" },
            "empty-day" => new PolicyUpdateRequest { ScheduleDaysOfWeek = "Sunday," },
            "invalid-month-day" => new PolicyUpdateRequest { MonthlyRetentionMonths = 12, MonthlyRetentionDaysOfMonth = "29" },
            "negative-weekly" => new PolicyUpdateRequest { WeeklyRetentionWeeks = -1 },
            "invalid-subtype" => new PolicyUpdateRequest { PolicySubType = "V3" },
            "invalid-snapshot" => new PolicyUpdateRequest { SnapshotConsistency = "Unknown" },
            "invalid-timezone" => new PolicyUpdateRequest { TimeZone = "Not/A/Zone" },
            "invalid-prefix" => new PolicyUpdateRequest { InstantRpResourceGroup = "bad/prefix" },
            "instant-over-limit" => new PolicyUpdateRequest { InstantRpRetentionDays = "31" },
            "archive-too-early" => new PolicyUpdateRequest { ArchiveTierAfterDays = "44" },
            "archive-copy-on-expiry" => new PolicyUpdateRequest { ArchiveTierMode = "CopyOnExpiry" },
            "smart-and-mode" => new PolicyUpdateRequest { SmartTier = true, ArchiveTierMode = "TierAfter" },
            "false-and-days" => new PolicyUpdateRequest { SmartTier = false, ArchiveTierAfterDays = "90" },
            "recommended-with-days" => new PolicyUpdateRequest { ArchiveTierMode = "TierRecommended", ArchiveTierAfterDays = "90" },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var policy = Policy(true);
        var schedule = policy.SchedulePolicy;
        var retention = policy.RetentionPolicy;

        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, request));

        Assert.Same(schedule, policy.SchedulePolicy);
        Assert.Same(retention, policy.RetentionPolicy);
        Assert.Equal(ScheduleRunType.Daily, Assert.IsType<SimpleSchedulePolicyV2>(schedule).ScheduleRunFrequency);
        Assert.Equal(At(2), Assert.Single(Assert.IsType<SimpleSchedulePolicyV2>(schedule).ScheduleRunTimes));
        Assert.Equal(14, Retention(policy).DailySchedule.RetentionDuration.Count);
        Assert.Equal("UTC", policy.TimeZone);
        Assert.Empty(policy.TieringPolicy);
    }

    [Fact]
    public void Apply_HourlyRetentionOnly_PreservesExistingRetentionTimesAndWindow()
    {
        var policy = Policy(true, "Hourly");
        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        var hourly = schedule.HourlySchedule;
        var daily = Retention(policy).DailySchedule;
        daily.RetentionTimes.Add(At(8));
        daily.RetentionTimes.Add(At(12));

        IaasVmPolicyUpdater.Apply(policy, new() { DailyRetentionDays = "30" });

        Assert.Same(hourly, schedule.HourlySchedule);
        Assert.Equal(new[] { At(8), At(12) }, daily.RetentionTimes);
        Assert.Equal(30, daily.RetentionDuration.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Apply_ApplicationConsistent_ClearsCrashOnlyOverride(bool enhanced)
    {
        var policy = Policy(enhanced);
        policy.SnapshotConsistencyType = IaasVmSnapshotConsistencyType.OnlyCrashConsistent;

        IaasVmPolicyUpdater.Apply(policy, new() { SnapshotConsistency = "ApplicationConsistent" });

        Assert.Null(policy.SnapshotConsistencyType);
    }

    [Theory]
    [InlineData(false, "Daily")]
    [InlineData(false, "Weekly")]
    [InlineData(true, "Daily")]
    [InlineData(true, "Weekly")]
    public void Apply_MultipleScheduleTimes_ReconcilesEveryRetentionTier(bool enhanced, string frequency)
    {
        var policy = Policy(enhanced, frequency);
        var retention = Retention(policy);
        retention.WeeklySchedule ??= new WeeklyRetentionSchedule
        {
            RetentionDuration = new RetentionDuration { Count = 4, DurationType = RetentionDurationType.Weeks }
        };
        if (retention.WeeklySchedule.DaysOfTheWeek.Count == 0)
        {
            retention.WeeklySchedule.DaysOfTheWeek.Add(BackupDayOfWeek.Sunday);
        }
        retention.MonthlySchedule = new MonthlyRetentionSchedule();
        retention.YearlySchedule = new YearlyRetentionSchedule();

        IaasVmPolicyUpdater.Apply(policy, new() { ScheduleTimes = "02:00, 14:30" });

        var expected = PolicyUpdateValidator.ParseTimes("02:00,14:30");
        var actual = policy.SchedulePolicy is SimpleSchedulePolicy v1 ? v1.ScheduleRunTimes
            : frequency == "Daily" ? ((SimpleSchedulePolicyV2)policy.SchedulePolicy).ScheduleRunTimes
            : ((SimpleSchedulePolicyV2)policy.SchedulePolicy).WeeklySchedule.ScheduleRunTimes;
        Assert.Equal(expected, actual);
        if (frequency == "Daily")
        { Assert.Equal(expected, retention.DailySchedule.RetentionTimes); }
        Assert.Equal(expected, retention.WeeklySchedule.RetentionTimes);
        Assert.Equal(expected, retention.MonthlySchedule.RetentionTimes);
        Assert.Equal(expected, retention.YearlySchedule.RetentionTimes);

        var model = (IPersistableModel<IaasVmProtectionPolicy>)policy;
        var before = model.Write(ModelReaderWriterOptions.Json).ToString();
        IaasVmPolicyUpdater.Apply(policy, frequency == "Daily"
            ? new() { DailyRetentionDays = "30" }
            : new() { WeeklyRetentionWeeks = 8, WeeklyRetentionDaysOfWeek = "Sunday" });
        Assert.Equal(expected, actual);
        Assert.Equal(expected, retention.WeeklySchedule.RetentionTimes);
        Assert.Equal(expected, retention.MonthlySchedule.RetentionTimes);
        Assert.Equal(expected, retention.YearlySchedule.RetentionTimes);
        Assert.NotEqual(before, model.Write(ModelReaderWriterOptions.Json).ToString());
    }

    [Theory]
    [InlineData("Daily", "Weekly")]
    [InlineData("Daily", "Hourly")]
    [InlineData("Weekly", "Daily")]
    [InlineData("Weekly", "Hourly")]
    [InlineData("Hourly", "Daily")]
    [InlineData("Hourly", "Weekly")]
    public void Apply_V2Transition_SerializesOnlyActiveScheduleBranch(string from, string to)
    {
        var policy = Policy(true, from);
        var request = to == "Hourly" ? HourlyTransition()
            : to == "Weekly" ? WeeklyTransition()
            : new PolicyUpdateRequest { ScheduleFrequency = "Daily", ScheduleTimes = "04:00" };
        if (from == "Weekly")
        { request.DailyRetentionDays = "30"; }

        IaasVmPolicyUpdater.Apply(policy, request);

        var model = (IPersistableModel<SimpleSchedulePolicyV2>)Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        using var json = JsonDocument.Parse(model.Write(ModelReaderWriterOptions.Json).ToString());
        foreach (var frequency in new[] { "Daily", "Weekly", "Hourly" })
        {
            var present = json.RootElement.TryGetProperty($"{frequency.ToLowerInvariant()}Schedule", out var branch)
                && branch.ValueKind != JsonValueKind.Null;
            Assert.Equal(frequency == to, present);
        }
    }

    [Fact]
    public void Apply_HourlyInterval24_RejectsWithoutMutation()
    {
        var policy = Policy(true, "Hourly");
        // A live probe with a 24-hour interval and window returned BMSUserErrorInvalidPolicyInput.
        Assert.Throws<ArgumentException>(() => IaasVmPolicyUpdater.Apply(policy, new() { HourlyIntervalHours = 24, HourlyWindowDurationHours = 24 }));
        var hourly = ((SimpleSchedulePolicyV2)policy.SchedulePolicy).HourlySchedule;
        Assert.Equal(4, hourly.Interval);
        Assert.Equal(12, hourly.ScheduleWindowDuration);
        Assert.Equal(At(8), hourly.ScheduleWindowStartOn);
    }

    [Fact]
    public void Apply_NullArguments_Rejects()
    {
        Assert.Throws<ArgumentNullException>(() => IaasVmPolicyUpdater.Apply(null!, new()));
        Assert.Throws<ArgumentNullException>(() => IaasVmPolicyUpdater.Apply(Policy(), null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public void FromOptions_MapsNewFieldsAndPreservesNullableSmartTier(bool? smartTier)
    {
        var options = new PolicyUpdateOptions
        {
            Vault = "test-vault",
            ResourceGroup = "test-rg",
            Policy = "isolated",
            HourlyIntervalHours = 6,
            HourlyWindowStartTime = "08:00",
            HourlyWindowDurationHours = 18,
            PolicySubType = "Enhanced",
            InstantRpRetentionDays = "4",
            InstantRpResourceGroup = "snapshot-rg",
            SnapshotConsistency = "CrashConsistent",
            ArchiveTierAfterDays = "90",
            ArchiveTierMode = "TierAfter",
            SmartTier = smartTier,
            PolicyTags = "key=value"
        };

        var request = PolicyUpdateRequest.FromOptions(options);

        Assert.Equal(options.Policy, request.Policy);
        Assert.Equal(options.HourlyIntervalHours, request.HourlyIntervalHours);
        Assert.Equal(options.HourlyWindowStartTime, request.HourlyWindowStartTime);
        Assert.Equal(options.HourlyWindowDurationHours, request.HourlyWindowDurationHours);
        Assert.Equal(options.PolicySubType, request.PolicySubType);
        Assert.Equal(options.InstantRpRetentionDays, request.InstantRpRetentionDays);
        Assert.Equal(options.InstantRpResourceGroup, request.InstantRpResourceGroup);
        Assert.Equal(options.SnapshotConsistency, request.SnapshotConsistency);
        Assert.Equal(options.ArchiveTierAfterDays, request.ArchiveTierAfterDays);
        Assert.Equal(options.ArchiveTierMode, request.ArchiveTierMode);
        Assert.Equal(options.PolicyTags, request.PolicyTags);
        Assert.Equal(smartTier, request.SmartTier);
        var smartOnly = new PolicyUpdateRequest { SmartTier = smartTier };
        Assert.Equal(smartTier.HasValue, smartOnly.HasAnyInput());
        Assert.Equal(smartTier.HasValue, smartOnly.HasIaasVmExtendedFlags());
    }
}
