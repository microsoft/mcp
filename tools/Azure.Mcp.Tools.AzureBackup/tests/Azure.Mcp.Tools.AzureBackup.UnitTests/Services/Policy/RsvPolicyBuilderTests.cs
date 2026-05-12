// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Services.Policy;

public class RsvPolicyBuilderTests
{
    [Fact]
    public void Build_VmMinimal_ProducesIaasVmDailyDefaults()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureIaasVM",
        };

        var policy = (IaasVmProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.Equal("UTC", policy.TimeZone);
        var schedule = Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy);
        Assert.Equal(ScheduleRunType.Daily, schedule.ScheduleRunFrequency);
        Assert.Single(schedule.ScheduleRunTimes);
        Assert.Equal(2, schedule.ScheduleRunTimes[0].Hour);

        var retention = Assert.IsType<LongTermRetentionPolicy>(policy.RetentionPolicy);
        Assert.NotNull(retention.DailySchedule);
        Assert.Equal(30, retention.DailySchedule!.RetentionDuration!.Count);
        Assert.Null(retention.WeeklySchedule);
        Assert.Null(retention.MonthlySchedule);
        Assert.Null(retention.YearlySchedule);
        Assert.Empty(policy.TieringPolicy);
    }

    [Fact]
    public void Build_VmHourlyEnhanced_ProducesV2WithHourlySchedule()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureIaasVM",
            PolicySubType = "Enhanced",
            ScheduleFrequency = "Hourly",
            HourlyIntervalHours = 4,
            HourlyWindowStartTime = "08:00",
            HourlyWindowDurationHours = 12,
            DailyRetentionDays = "7",
        };

        var policy = (IaasVmProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.Equal(IaasVmPolicyType.V2, policy.PolicyType);
        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        Assert.Equal(ScheduleRunType.Hourly, schedule.ScheduleRunFrequency);
        Assert.NotNull(schedule.HourlySchedule);
        Assert.Equal(4, schedule.HourlySchedule!.Interval);
        Assert.Equal(12, schedule.HourlySchedule!.ScheduleWindowDuration);
        Assert.Equal(8, schedule.HourlySchedule!.ScheduleWindowStartOn!.Value.Hour);
    }

    [Fact]
    public void Build_VmWeeklyMultiTierWithArchive_PopulatesRetentionAndTiering()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureIaasVM",
            ScheduleFrequency = "Weekly",
            ScheduleDaysOfWeek = "Monday,Wednesday",
            ScheduleTimes = "03:00",
            DailyRetentionDays = "10",
            WeeklyRetentionWeeks = 8,
            WeeklyRetentionDaysOfWeek = "Monday",
            MonthlyRetentionMonths = 12,
            MonthlyRetentionDaysOfMonth = "1,15",
            YearlyRetentionYears = 5,
            YearlyRetentionMonths = "January,July",
            YearlyRetentionWeekOfMonth = "First",
            YearlyRetentionDaysOfWeek = "Sunday",
            ArchiveTierAfterDays = "90",
            ArchiveTierMode = "TierAfter",
            InstantRpRetentionDays = "5",
            InstantRpResourceGroup = "rg-snap",
        };

        var policy = (IaasVmProtectionPolicy)RsvPolicyBuilder.Build(req);

        var schedule = Assert.IsType<SimpleSchedulePolicy>(policy.SchedulePolicy);
        Assert.Equal(ScheduleRunType.Weekly, schedule.ScheduleRunFrequency);
        Assert.Equal(2, schedule.ScheduleRunDays.Count);
        Assert.Equal(3, schedule.ScheduleRunTimes[0].Hour);

        var retention = Assert.IsType<LongTermRetentionPolicy>(policy.RetentionPolicy);
        Assert.Equal(10, retention.DailySchedule!.RetentionDuration!.Count);
        Assert.Equal(8, retention.WeeklySchedule!.RetentionDuration!.Count);
        Assert.Single(retention.WeeklySchedule.DaysOfTheWeek);
        Assert.Equal(BackupDayOfWeek.Monday, retention.WeeklySchedule.DaysOfTheWeek[0]);

        Assert.NotNull(retention.MonthlySchedule);
        Assert.Equal(RetentionScheduleFormat.Daily, retention.MonthlySchedule!.RetentionScheduleFormatType);
        Assert.Equal(2, retention.MonthlySchedule.RetentionScheduleDailyDaysOfTheMonth.Count);

        Assert.NotNull(retention.YearlySchedule);
        Assert.Equal(2, retention.YearlySchedule!.MonthsOfYear.Count);
        Assert.Equal(RetentionScheduleFormat.Weekly, retention.YearlySchedule.RetentionScheduleFormatType);
        Assert.Single(retention.YearlySchedule.RetentionScheduleWeekly!.DaysOfTheWeek);
        Assert.Single(retention.YearlySchedule.RetentionScheduleWeekly.WeeksOfTheMonth);

        Assert.True(policy.TieringPolicy.ContainsKey("ArchivedRP"));
        var tiering = policy.TieringPolicy["ArchivedRP"];
        Assert.Equal(TieringMode.TierAfter, tiering.TieringMode);
        Assert.Equal(90, tiering.DurationValue);

        Assert.Equal(5, policy.InstantRPRetentionRangeInDays);
        Assert.Equal("rg-snap", policy.InstantRPDetails!.AzureBackupRGNamePrefix);
    }

    [Fact]
    public void Build_SqlFullLogOnly_ProducesTwoSubPolicies()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "MSSQL",
            ScheduleTimes = "01:30",
            DailyRetentionDays = "20",
            LogFrequencyMinutes = 30,
            LogRetentionDays = 10,
        };

        var policy = (VmWorkloadProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.Equal(2, policy.SubProtectionPolicy.Count);
        Assert.Equal("Full", policy.SubProtectionPolicy[0].PolicyType.ToString());
        Assert.Equal("Log", policy.SubProtectionPolicy[1].PolicyType.ToString());

        var logSchedule = Assert.IsType<LogSchedulePolicy>(policy.SubProtectionPolicy[1].SchedulePolicy);
        Assert.Equal(30, logSchedule.ScheduleFrequencyInMins);
    }

    [Fact]
    public void Build_SqlFullDifferentialIncrementalLog_ProducesFourSubPolicies()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "MSSQL",
            FullScheduleFrequency = "Weekly",
            FullScheduleDaysOfWeek = "Sunday",
            DifferentialScheduleDaysOfWeek = "Wednesday",
            DifferentialRetentionDays = 30,
            IncrementalScheduleDaysOfWeek = "Monday,Tuesday",
            IncrementalRetentionDays = 7,
        };

        var policy = (VmWorkloadProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.Equal(4, policy.SubProtectionPolicy.Count);
        Assert.Equal("Full", policy.SubProtectionPolicy[0].PolicyType.ToString());
        Assert.Equal("Differential", policy.SubProtectionPolicy[1].PolicyType.ToString());
        Assert.Equal("Incremental", policy.SubProtectionPolicy[2].PolicyType.ToString());
        Assert.Equal("Log", policy.SubProtectionPolicy[3].PolicyType.ToString());

        var diffSchedule = Assert.IsType<SimpleSchedulePolicy>(policy.SubProtectionPolicy[1].SchedulePolicy);
        Assert.Equal(ScheduleRunType.Weekly, diffSchedule.ScheduleRunFrequency);
        Assert.Single(diffSchedule.ScheduleRunDays);

        var incSchedule = Assert.IsType<SimpleSchedulePolicy>(policy.SubProtectionPolicy[2].SchedulePolicy);
        Assert.Equal(2, incSchedule.ScheduleRunDays.Count);
    }

    [Fact]
    public void Build_AzureFileShare_ProducesFileShareProtectionPolicy()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureFileShare",
            DailyRetentionDays = "14",
        };

        var policy = (FileShareProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.Equal("AzureFileShare", policy.WorkLoadType.ToString());
        var retention = Assert.IsType<LongTermRetentionPolicy>(policy.RetentionPolicy);
        Assert.Equal(14, retention.DailySchedule!.RetentionDuration!.Count);
    }

    [Fact]
    public void Build_MonthlyRelativeFormat_UsesWeekAndDayOfWeek()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureIaasVM",
            MonthlyRetentionMonths = 6,
            MonthlyRetentionWeekOfMonth = "Last",
            MonthlyRetentionDaysOfWeek = "Friday",
        };

        var policy = (IaasVmProtectionPolicy)RsvPolicyBuilder.Build(req);
        var monthly = ((LongTermRetentionPolicy)policy.RetentionPolicy).MonthlySchedule;

        Assert.NotNull(monthly);
        Assert.Equal(RetentionScheduleFormat.Weekly, monthly!.RetentionScheduleFormatType);
        Assert.Equal(BackupDayOfWeek.Friday, monthly.RetentionScheduleWeekly!.DaysOfTheWeek[0]);
        Assert.Equal(BackupWeekOfMonth.Last, monthly.RetentionScheduleWeekly.WeeksOfTheMonth[0]);
    }

    // ===== Stage 2 tests =====

    [Fact]
    public void Build_VmSmartTier_SetsTierRecommended()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureIaasVM",
            DailyRetentionDays = "30",
            SmartTier = true,
        };

        var policy = (IaasVmProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.True(policy.TieringPolicy.ContainsKey("ArchivedRP"));
        Assert.Equal(TieringMode.TierRecommended, policy.TieringPolicy["ArchivedRP"].TieringMode);
    }

    [Fact]
    public void Build_VmSmartTier_OverridesArchiveTierAfterDays()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureIaasVM",
            DailyRetentionDays = "30",
            ArchiveTierAfterDays = "60",
            SmartTier = true,
        };

        var policy = (IaasVmProtectionPolicy)RsvPolicyBuilder.Build(req);

        Assert.Equal(TieringMode.TierRecommended, policy.TieringPolicy["ArchivedRP"].TieringMode);
    }

    [Fact]
    public void Build_HanaWithSnapshotBackup_AttachesSnapshotDetailsToFullSubPolicy()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "SAPHANA",
            DailyRetentionDays = "30",
            EnableSnapshotBackup = true,
            SnapshotInstantRpRetentionDays = "5",
            SnapshotInstantRpResourceGroup = "snapRG",
        };

        var policy = (VmWorkloadProtectionPolicy)RsvPolicyBuilder.Build(req);

        var full = policy.SubProtectionPolicy.First(sp => sp.PolicyType.ToString() == "Full");
        Assert.NotNull(full.SnapshotBackupAdditionalDetails);
        Assert.Equal(5, full.SnapshotBackupAdditionalDetails!.InstantRpRetentionRangeInDays);
        Assert.Equal("snapRG", full.SnapshotBackupAdditionalDetails.InstantRPDetails);
        // Snapshot is attached to Full, not a separate SnapshotFull sub-policy.
        Assert.DoesNotContain(policy.SubProtectionPolicy, sp => sp.PolicyType.ToString() == "SnapshotFull" || sp.PolicyType.ToString() == "SnapshotCopyOnlyFull");
    }

    [Fact]
    public void Build_HanaWithoutSnapshotBackup_DoesNotAttachSnapshotDetails()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "SAPHANA",
            DailyRetentionDays = "30",
        };

        var policy = (VmWorkloadProtectionPolicy)RsvPolicyBuilder.Build(req);

        var full = policy.SubProtectionPolicy.First(sp => sp.PolicyType.ToString() == "Full");
        Assert.Null(full.SnapshotBackupAdditionalDetails);
    }

    [Fact]
    public void Build_VmWorkloadFullSubPolicy_PropagatesArchiveTier()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "MSSQL",
            DailyRetentionDays = "30",
            ArchiveTierMode = "TierAfter",
            ArchiveTierAfterDays = "90",
        };

        var policy = (VmWorkloadProtectionPolicy)RsvPolicyBuilder.Build(req);
        var fullSub = policy.SubProtectionPolicy.First(sp => sp.PolicyType.ToString() == "Full");

        Assert.True(fullSub.TieringPolicy.ContainsKey("ArchivedRP"));
        Assert.Equal(TieringMode.TierAfter, fullSub.TieringPolicy["ArchivedRP"].TieringMode);
        Assert.Equal(90, fullSub.TieringPolicy["ArchivedRP"].DurationValue);
    }

    [Fact]
    public void Build_AzureFileShareHourly_ProducesV2WithHourlySchedule()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureFileShare",
            ScheduleFrequency = "Hourly",
            HourlyIntervalHours = 6,
            HourlyWindowStartTime = "09:00",
            HourlyWindowDurationHours = 12,
            DailyRetentionDays = "30",
        };

        var policy = (FileShareProtectionPolicy)RsvPolicyBuilder.Build(req);

        var schedule = Assert.IsType<SimpleSchedulePolicyV2>(policy.SchedulePolicy);
        Assert.Equal(ScheduleRunType.Hourly, schedule.ScheduleRunFrequency);
        Assert.Equal(6, schedule.HourlySchedule!.Interval);
        Assert.Equal(12, schedule.HourlySchedule.ScheduleWindowDuration);
    }

    [Fact]
    public void Build_AzureFileShareMultiTier_PopulatesWeeklyMonthlyYearly()
    {
        var req = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "AzureFileShare",
            DailyRetentionDays = "30",
            WeeklyRetentionWeeks = 12,
            WeeklyRetentionDaysOfWeek = "Sunday",
            MonthlyRetentionMonths = 12,
            MonthlyRetentionDaysOfMonth = "1",
            YearlyRetentionYears = 5,
            YearlyRetentionMonths = "January",
            YearlyRetentionDaysOfMonth = "1",
        };

        var policy = (FileShareProtectionPolicy)RsvPolicyBuilder.Build(req);
        var retention = Assert.IsType<LongTermRetentionPolicy>(policy.RetentionPolicy);

        Assert.Equal(30, retention.DailySchedule!.RetentionDuration!.Count);
        Assert.Equal(12, retention.WeeklySchedule!.RetentionDuration!.Count);
        Assert.Equal(12, retention.MonthlySchedule!.RetentionDuration!.Count);
        Assert.Equal(5, retention.YearlySchedule!.RetentionDuration!.Count);
    }

    // ===== Parse helpers =====

    [Theory]
    [InlineData("Monday", BackupDayOfWeek.Monday)]
    [InlineData("mon", BackupDayOfWeek.Monday)]
    [InlineData("TUESDAY", BackupDayOfWeek.Tuesday)]
    [InlineData("tue", BackupDayOfWeek.Tuesday)]
    [InlineData("Wed", BackupDayOfWeek.Wednesday)]
    [InlineData("thursday", BackupDayOfWeek.Thursday)]
    [InlineData("FRI", BackupDayOfWeek.Friday)]
    [InlineData("Saturday", BackupDayOfWeek.Saturday)]
    [InlineData("Sun", BackupDayOfWeek.Sunday)]
    public void ParseDaysOfWeek_ValidInput_Parses(string input, BackupDayOfWeek expected)
    {
        var result = RsvPolicyBuilder.ParseDaysOfWeek(input);
        Assert.Single(result);
        Assert.Equal(expected, result[0]);
    }

    [Fact]
    public void ParseDaysOfWeek_CommaSeparated_ParsesAll()
    {
        var result = RsvPolicyBuilder.ParseDaysOfWeek("Monday,Wednesday,Friday");
        Assert.Equal(3, result.Count);
        Assert.Equal(BackupDayOfWeek.Monday, result[0]);
        Assert.Equal(BackupDayOfWeek.Wednesday, result[1]);
        Assert.Equal(BackupDayOfWeek.Friday, result[2]);
    }

    [Fact]
    public void ParseDaysOfWeek_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Empty(RsvPolicyBuilder.ParseDaysOfWeek(null));
        Assert.Empty(RsvPolicyBuilder.ParseDaysOfWeek(""));
        Assert.Empty(RsvPolicyBuilder.ParseDaysOfWeek("  "));
    }

    [Fact]
    public void ParseDaysOfWeek_UnrecognizedValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RsvPolicyBuilder.ParseDaysOfWeek("Foobar"));
    }

    [Theory]
    [InlineData("January", BackupMonthOfYear.January)]
    [InlineData("jan", BackupMonthOfYear.January)]
    [InlineData("FEB", BackupMonthOfYear.February)]
    [InlineData("March", BackupMonthOfYear.March)]
    [InlineData("SEPTEMBER", BackupMonthOfYear.September)]
    [InlineData("sept", BackupMonthOfYear.September)]
    [InlineData("December", BackupMonthOfYear.December)]
    public void ParseMonthsOfYear_ValidInput_Parses(string input, BackupMonthOfYear expected)
    {
        var result = RsvPolicyBuilder.ParseMonthsOfYear(input);
        Assert.Single(result);
        Assert.Equal(expected, result[0]);
    }

    [Fact]
    public void ParseMonthsOfYear_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Empty(RsvPolicyBuilder.ParseMonthsOfYear(null));
        Assert.Empty(RsvPolicyBuilder.ParseMonthsOfYear(""));
    }

    [Fact]
    public void ParseMonthsOfYear_UnrecognizedValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RsvPolicyBuilder.ParseMonthsOfYear("Smarch"));
    }

    [Theory]
    [InlineData("First", BackupWeekOfMonth.First)]
    [InlineData("1", BackupWeekOfMonth.First)]
    [InlineData("Second", BackupWeekOfMonth.Second)]
    [InlineData("3", BackupWeekOfMonth.Third)]
    [InlineData("Fourth", BackupWeekOfMonth.Fourth)]
    [InlineData("Last", BackupWeekOfMonth.Last)]
    public void ParseWeeksOfMonth_ValidInput_Parses(string input, BackupWeekOfMonth expected)
    {
        var result = RsvPolicyBuilder.ParseWeeksOfMonth(input);
        Assert.Single(result);
        Assert.Equal(expected, result[0]);
    }

    [Fact]
    public void ParseWeeksOfMonth_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Empty(RsvPolicyBuilder.ParseWeeksOfMonth(null));
    }

    [Fact]
    public void ParseWeeksOfMonth_UnrecognizedValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RsvPolicyBuilder.ParseWeeksOfMonth("Fifth"));
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("15", 15)]
    [InlineData("28", 28)]
    public void ParseDaysOfMonth_ValidDayNumber_Parses(string input, int expectedDay)
    {
        var result = RsvPolicyBuilder.ParseDaysOfMonth(input);
        Assert.Single(result);
        Assert.Equal(expectedDay, result[0].Date);
        Assert.NotEqual(true, result[0].IsLast);
    }

    [Fact]
    public void ParseDaysOfMonth_Last_ParsesAsIsLast()
    {
        var result = RsvPolicyBuilder.ParseDaysOfMonth("Last");
        Assert.Single(result);
        Assert.True(result[0].IsLast);
    }

    [Fact]
    public void ParseDaysOfMonth_MultipleValues_ParsesAll()
    {
        var result = RsvPolicyBuilder.ParseDaysOfMonth("1,15,Last");
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ParseDaysOfMonth_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Empty(RsvPolicyBuilder.ParseDaysOfMonth(null));
    }

    [Fact]
    public void ParseScheduleTimes_NullOrEmpty_DefaultsTo0200()
    {
        var result = RsvPolicyBuilder.ParseScheduleTimes(null);
        Assert.Single(result);
        Assert.Equal(2, result[0].Hour);
        Assert.Equal(0, result[0].Minute);
    }

    [Fact]
    public void ParseScheduleTimes_ValidTime_Parses()
    {
        var result = RsvPolicyBuilder.ParseScheduleTimes("14:30");
        Assert.Single(result);
        Assert.Equal(14, result[0].Hour);
        Assert.Equal(30, result[0].Minute);
    }

    [Fact]
    public void ParseScheduleTimes_MultipleTimes_ParsesAll()
    {
        var result = RsvPolicyBuilder.ParseScheduleTimes("02:00,14:00");
        Assert.Equal(2, result.Count);
    }
}
