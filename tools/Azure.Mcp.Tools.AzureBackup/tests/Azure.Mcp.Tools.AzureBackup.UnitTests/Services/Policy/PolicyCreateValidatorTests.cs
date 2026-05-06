// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Services.Policy;

public class PolicyCreateValidatorTests
{
    private static PolicyCreateOptions BaseOptions(string workload = "VM") => new()
    {
        Subscription = "sub",
        ResourceGroup = "rg",
        Vault = "v",
        Policy = "p",
        WorkloadType = workload,
    };

    // ----- Rule C: AKS support (Stage 2  -  AKS is now first-class) -----

    [Theory]
    [InlineData("AKS")]
    [InlineData("aks")]
    public void Validate_AksWorkload_WithDailyRetention_PassesThrough(string workload)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }



    // ----- Rule D: CosmosDB pass-through -----

    [Fact]
    public void Validate_CosmosDb_WithDailyRetention_PassesThrough()
    {
        var options = BaseOptions("CosmosDB");
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.1: at least one schedule/retention input -----

    [Fact]
    public void Validate_NoScheduleOrRetention_Fails()
    {
        var result = PolicyCreateValidator.Validate(BaseOptions("VM"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Message.StartsWith("Provide at least one schedule"));
    }

    [Theory]
    [InlineData("VM")]
    [InlineData("SQL")]
    [InlineData("AzureDisk")]
    [InlineData("PostgreSQLFlexible")]
    public void Validate_DailyRetentionAlone_Passes(string workload)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.2: Weekly requires --schedule-days-of-week -----

    [Fact]
    public void Validate_WeeklyWithoutDaysOfWeek_Fails()
    {
        var options = BaseOptions("VM");
        options.ScheduleFrequency = "Weekly";
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--schedule-days-of-week");
    }

    [Fact]
    public void Validate_WeeklyWithDaysOfWeek_Passes()
    {
        var options = BaseOptions("VM");
        options.ScheduleFrequency = "Weekly";
        options.ScheduleDaysOfWeek = "Sunday";
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.3: Hourly requires all three hourly inputs -----

    [Theory]
    [InlineData(0, "08:00", 12)]
    [InlineData(4, null, 12)]
    [InlineData(4, "08:00", 0)]
    [InlineData(0, null, 0)]
    public void Validate_HourlyWithMissingInputs_Fails(int interval, string? start, int duration)
    {
        var options = BaseOptions("VM");
        options.ScheduleFrequency = "Hourly";
        options.HourlyIntervalHours = interval;
        options.HourlyWindowStartTime = start;
        options.HourlyWindowDurationHours = duration;
        options.DailyRetentionDays = "7";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--schedule-frequency" && i.Message.Contains("Hourly"));
    }

    [Fact]
    public void Validate_HourlyWithAllInputs_Passes()
    {
        var options = BaseOptions("VM");
        options.PolicySubType = "Enhanced";
        options.ScheduleFrequency = "Hourly";
        options.HourlyIntervalHours = 4;
        options.HourlyWindowStartTime = "08:00";
        options.HourlyWindowDurationHours = 12;
        options.DailyRetentionDays = "7";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.4: Weekly retention partial -----

    [Theory]
    [InlineData(4, null)]
    [InlineData(0, "Sunday")]
    public void Validate_PartialWeeklyRetention_Fails(int weeks, string? days)
    {
        var options = BaseOptions("VM");
        options.WeeklyRetentionWeeks = weeks;
        options.WeeklyRetentionDaysOfWeek = days;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--weekly-retention-weeks" && i.Message.Contains("Weekly retention"));
    }

    [Fact]
    public void Validate_FullWeeklyRetention_Passes()
    {
        var options = BaseOptions("VM");
        options.WeeklyRetentionWeeks = 12;
        options.WeeklyRetentionDaysOfWeek = "Sunday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.5: Monthly retention partials and exclusivity -----

    [Fact]
    public void Validate_MonthlyMonthsAlone_Fails()
    {
        var options = BaseOptions("VM");
        options.MonthlyRetentionMonths = 12;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-months");
    }

    [Fact]
    public void Validate_MonthlyDayInputsWithoutMonths_Fails()
    {
        var options = BaseOptions("VM");
        options.MonthlyRetentionDaysOfMonth = "1";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-months" && i.Message.Contains("require"));
    }

    [Fact]
    public void Validate_MonthlyMixedRelativeAndAbsolute_Fails()
    {
        var options = BaseOptions("VM");
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionDaysOfMonth = "1";
        options.MonthlyRetentionWeekOfMonth = "First";
        options.MonthlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-days-of-month");
    }

    [Fact]
    public void Validate_MonthlyPartialRelative_Fails()
    {
        var options = BaseOptions("VM");
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionWeekOfMonth = "First";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-days-of-week");
    }

    [Fact]
    public void Validate_MonthlyAbsolute_Passes()
    {
        var options = BaseOptions("VM");
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionDaysOfMonth = "1,15,Last";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MonthlyRelative_Passes()
    {
        var options = BaseOptions("VM");
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionWeekOfMonth = "First";
        options.MonthlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.6: Yearly retention -----

    [Fact]
    public void Validate_YearlyYearsAlone_Fails()
    {
        var options = BaseOptions("VM");
        options.YearlyRetentionYears = 5;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--yearly-retention-months");
    }

    [Fact]
    public void Validate_YearlyAbsolute_Passes()
    {
        var options = BaseOptions("VM");
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionDaysOfMonth = "1";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_YearlyRelative_Passes()
    {
        var options = BaseOptions("VM");
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionWeekOfMonth = "First";
        options.YearlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.7: Archive tier pair -----

    [Theory]
    [InlineData("60", null)]
    [InlineData(null, "TierAfter")]
    public void Validate_ArchiveTierPartial_Fails(string? days, string? mode)
    {
        var options = BaseOptions("VM");
        options.DailyRetentionDays = "30";
        options.ArchiveTierAfterDays = days;
        options.ArchiveTierMode = mode;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Message.Contains("--archive-tier-after-days and --archive-tier-mode"));
    }

    [Fact]
    public void Validate_ArchiveTierBoth_Passes()
    {
        var options = BaseOptions("VM");
        options.DailyRetentionDays = "30";
        options.ArchiveTierAfterDays = "60";
        options.ArchiveTierMode = "TierAfter";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule B: shape rules (workload exclusivity) -----

    [Theory]
    [InlineData("SQL", "--policy-sub-type", "Enhanced")]
    [InlineData("AzureDisk", "--policy-sub-type", "Standard")]
    [InlineData("AzureDisk", "--instant-rp-retention-days", "5")]
    [InlineData("SQL", "--snapshot-consistency", "ApplicationConsistent")]
    [InlineData("PostgreSQLFlexible", "--instant-rp-resource-group", "rg")]
    public void Validate_RsvVmFlagsOnNonRsvVm_Fail(string workload, string flag, string value)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "7";
        switch (flag)
        {
            case "--policy-sub-type":
                options.PolicySubType = value;
                break;
            case "--instant-rp-retention-days":
                options.InstantRpRetentionDays = value;
                break;
            case "--snapshot-consistency":
                options.SnapshotConsistency = value;
                break;
            case "--instant-rp-resource-group":
                options.InstantRpResourceGroup = value;
                break;
        }

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == flag && i.Message.Contains("RSV VM"));
    }

    [Theory]
    [InlineData("VM", "--log-frequency-minutes", "60")]
    [InlineData("AzureDisk", "--full-schedule-frequency", "Weekly")]
    [InlineData("VM", "--differential-retention-days", "10")]
    [InlineData("VM", "--is-compression", "true")]
    public void Validate_RsvVmWorkloadFlagsOnNonRsvVmWorkload_Fail(string workload, string flag, string value)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "7";
        switch (flag)
        {
            case "--log-frequency-minutes":
                options.LogFrequencyMinutes = int.Parse(value);
                break;
            case "--full-schedule-frequency":
                options.FullScheduleFrequency = value;
                break;
            case "--differential-retention-days":
                options.DifferentialRetentionDays = int.Parse(value);
                break;
            case "--is-compression":
                options.IsCompression = bool.Parse(value);
                break;
        }

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == flag && i.Message.Contains("SQL / SAPHANA / SAPASE"));
    }

    [Theory]
    [InlineData("VM")]
    [InlineData("SQL")]
    public void Validate_IncrementalOnNonSap_Fails(string workload)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "7";
        options.IncrementalScheduleDaysOfWeek = "Tuesday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--incremental-schedule-days-of-week");
    }

    [Fact]
    public void Validate_IncrementalOnSapHana_Passes()
    {
        var options = BaseOptions("SAPHANA");
        options.DailyRetentionDays = "7";
        options.IncrementalScheduleDaysOfWeek = "Tuesday";
        options.IncrementalRetentionDays = 15;

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("VM")]
    [InlineData("SAPHANA")]
    public void Validate_IsSqlCompressionOnNonSql_Fails(string workload)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "7";
        options.IsSqlCompression = true;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--is-sql-compression");
    }

    [Theory]
    [InlineData("AzureDisk")]
    [InlineData("PostgreSQLFlexible")]
    public void Validate_HourlyOnDpp_Fails(string workload)
    {
        var options = BaseOptions(workload);
        options.ScheduleFrequency = "Hourly";
        options.HourlyIntervalHours = 4;
        options.HourlyWindowStartTime = "08:00";
        options.HourlyWindowDurationHours = 12;
        options.DailyRetentionDays = "7";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--schedule-frequency" && i.Message.Contains("RSV"));
    }

    // ----- Continuous DPP rejects schedule/retention -----

    [Theory]
    [InlineData("AzureBlob")]
    [InlineData("ADLS")]
    public void Validate_ContinuousDppWithScheduleOrRetention_Fails(string workload)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "7";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "policy" && i.Message.Contains("Continuous DPP"));
    }

    [Theory]
    [InlineData("AzureBlob")]
    [InlineData("ADLS")]
    public void Validate_ContinuousDppWithoutFlags_Passes(string workload)
    {
        var result = PolicyCreateValidator.Validate(BaseOptions(workload));

        Assert.True(result.IsValid);
    }

    // ----- Multiple issues surfaced together -----

    [Fact]
    public void Validate_MultipleProblems_AllSurfaced()
    {
        var options = BaseOptions("VM");
        options.ScheduleFrequency = "Weekly";          // missing --schedule-days-of-week
        options.WeeklyRetentionWeeks = 4;             // missing --weekly-retention-days-of-week
        options.ArchiveTierAfterDays = "60";            // missing --archive-tier-mode
        options.LogFrequencyMinutes = 60;             // RSV VmWorkload only

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.True(result.Issues.Count >= 4);
        Assert.Contains(result.Issues, i => i.Flag == "--schedule-days-of-week");
        Assert.Contains(result.Issues, i => i.Flag == "--weekly-retention-weeks");
        Assert.Contains(result.Issues, i => i.Flag == "--archive-tier-mode");
        Assert.Contains(result.Issues, i => i.Flag == "--log-frequency-minutes");
    }

    // ===== Stage 2 validator shape rules =====

    [Fact]
    public void Validate_SmartTier_RequiresVmWorkload()
    {
        var options = BaseOptions("MSSQL");
        options.DailyRetentionDays = "30";
        options.SmartTier = true;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--smart-tier");
    }

    [Fact]
    public void Validate_SmartTier_OnVm_PassesThrough()
    {
        var options = BaseOptions("VM");
        options.DailyRetentionDays = "30";
        options.SmartTier = true;

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_SnapshotBackup_RequiresHana()
    {
        var options = BaseOptions("MSSQL");
        options.DailyRetentionDays = "30";
        options.EnableSnapshotBackup = true;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--enable-snapshot-backup");
    }

    [Fact]
    public void Validate_SnapshotBackup_OnHana_PassesThrough()
    {
        var options = BaseOptions("SAPHANA");
        options.DailyRetentionDays = "30";
        options.EnableSnapshotBackup = true;
        options.SnapshotInstantRpRetentionDays = "5";
        options.SnapshotInstantRpResourceGroup = "snapRG";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_VaultTierCopy_RequiresAzureDisk()
    {
        var options = BaseOptions("AzureBlob");
        options.BackupMode = "Vaulted";
        options.DailyRetentionDays = "30";
        options.EnableVaultTierCopy = true;
        options.VaultTierCopyAfterDays = 7;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--enable-vault-tier-copy");
    }

    [Fact]
    public void Validate_VaultTierCopy_PartialInput_Fails()
    {
        var options = BaseOptions("AzureDisk");
        options.EnableVaultTierCopy = true;
        // Missing --vault-tier-copy-after-days.

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--vault-tier-copy-after-days");
    }

    [Fact]
    public void Validate_VaultTierCopy_OnDisk_PassesThrough()
    {
        var options = BaseOptions("AzureDisk");
        options.DailyRetentionDays = "30";
        options.EnableVaultTierCopy = true;
        options.VaultTierCopyAfterDays = 7;

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_BackupMode_RequiresStorageWorkload()
    {
        var options = BaseOptions("VM");
        options.DailyRetentionDays = "30";
        options.BackupMode = "Vaulted";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--backup-mode");
    }

    [Fact]
    public void Validate_BackupMode_VaultedOnBlob_PassesThrough()
    {
        var options = BaseOptions("AzureBlob");
        options.BackupMode = "Vaulted";
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PitrRetentionDays_RequiresStorageWorkload()
    {
        var options = BaseOptions("AzureDisk");
        options.PitrRetentionDays = 60;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--pitr-retention-days");
    }

    [Fact]
    public void Validate_PitrRetentionDays_OnBlob_PassesThrough()
    {
        var options = BaseOptions("AzureBlob");
        options.PitrRetentionDays = 60;

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PolicyTags_RejectedOnDpp()
    {
        var options = BaseOptions("AzureDisk");
        options.PolicyTags = "env=prod,team=backup";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--policy-tags");
    }

    [Fact]
    public void Validate_PolicyTags_AllowedOnRsv()
    {
        var options = BaseOptions("VM");
        options.DailyRetentionDays = "30";
        options.PolicyTags = "env=prod,team=backup";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }
    // ----- Rule: SQL Full/Diff day overlap -----

    [Fact]
    public void Validate_SqlDiffDailyFull_RejectsSameDayConflict()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.DifferentialScheduleDaysOfWeek = "Monday";
        // Full defaults to Daily (runs every day), so any Diff day overlaps.

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--differential-schedule-days-of-week" &&
            i.Message.Contains("cannot run on the same day"));
    }

    [Fact]
    public void Validate_SqlWeeklyFullDiffOverlap_RejectsSameDayConflict()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.FullScheduleFrequency = "Weekly";
        options.FullScheduleDaysOfWeek = "Sunday,Wednesday";
        options.DifferentialScheduleDaysOfWeek = "Wednesday"; // overlaps with Full

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--differential-schedule-days-of-week" &&
            i.Message.Contains("cannot run on the same day"));
    }

    [Fact]
    public void Validate_SqlWeeklyFullDiffNoOverlap_Passes()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.FullScheduleFrequency = "Weekly";
        options.FullScheduleDaysOfWeek = "Sunday";
        options.DifferentialScheduleDaysOfWeek = "Wednesday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_SqlDiffMultipleDays_Rejected()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.FullScheduleFrequency = "Weekly";
        options.FullScheduleDaysOfWeek = "Sunday";
        options.DifferentialScheduleDaysOfWeek = "Monday,Tuesday";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--differential-schedule-days-of-week" &&
            i.Message.Contains("exactly one day"));
    }

    // ----- SQL Log retention constraints -----

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    public void Validate_SqlLogRetentionBelowMinimum_Fails(int logDays)
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.LogRetentionDays = logDays;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--log-retention-days" &&
            i.Message.Contains("minimum is 7"));
    }

    [Fact]
    public void Validate_SqlLogRetention7Days_Passes()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.LogRetentionDays = 7;

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_SqlLogRetentionExceedsDiffRetention_Fails()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.LogRetentionDays = 15;
        options.DifferentialRetentionDays = 10;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--log-retention-days" &&
            i.Message.Contains("less than Differential"));
    }

    [Fact]
    public void Validate_SqlLogRetentionEqualsDiffRetention_Fails()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.LogRetentionDays = 10;
        options.DifferentialRetentionDays = 10;

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--log-retention-days" &&
            i.Message.Contains("less than Differential"));
    }

    [Fact]
    public void Validate_SqlLogRetentionBelowDiffRetention_Passes()
    {
        var options = BaseOptions("SQL");
        options.DailyRetentionDays = "30";
        options.LogRetentionDays = 7;
        options.DifferentialRetentionDays = 15;

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    // ----- Rule A.8: Archive tier rejected for DPP workloads -----

    [Theory]
    [InlineData("AzureDisk")]
    [InlineData("AKS")]
    [InlineData("PostgreSQLFlexible")]
    [InlineData("CosmosDB")]
    public void Validate_ArchiveTierOnDppWorkload_Fails(string workload)
    {
        var options = BaseOptions(workload);
        options.DailyRetentionDays = "30";
        options.ArchiveTierAfterDays = "60";
        options.ArchiveTierMode = "TierAfter";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Message.Contains("Archive tier is not supported"));
    }

    // ----- Rule A.9: Yearly retention rejected for AzureDisk -----

    [Fact]
    public void Validate_YearlyRetentionOnDisk_Fails()
    {
        var options = BaseOptions("AzureDisk");
        options.DailyRetentionDays = "30";
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionDaysOfMonth = "1";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--yearly-retention-years" &&
            i.Message.Contains("AzureDisk does not support Yearly"));
    }

    // ----- DPP schedule-frequency validation -----

    [Theory]
    [InlineData("Daily")]
    [InlineData("P1D")]
    [InlineData("PT4H")]
    [InlineData("P1W")]
    public void Validate_DppKnownScheduleFrequency_Passes(string frequency)
    {
        var options = BaseOptions("AzureDisk");
        options.ScheduleFrequency = frequency;
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_DppWeeklyScheduleFrequency_WithDaysOfWeek_Passes()
    {
        var options = BaseOptions("AzureDisk");
        options.ScheduleFrequency = "Weekly";
        options.ScheduleDaysOfWeek = "Sunday";
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Monthly")]
    [InlineData("BADVALUE")]
    [InlineData("Hourly")]
    public void Validate_DppUnknownScheduleFrequency_Fails(string frequency)
    {
        var options = BaseOptions("AzureDisk");
        options.ScheduleFrequency = frequency;
        options.DailyRetentionDays = "30";

        var result = PolicyCreateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Flag == "--schedule-frequency" &&
            i.Message.Contains("Unrecognized DPP schedule frequency"));
    }
}
