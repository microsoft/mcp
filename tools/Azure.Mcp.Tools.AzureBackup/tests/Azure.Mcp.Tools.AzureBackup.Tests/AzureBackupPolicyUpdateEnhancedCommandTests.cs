// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests;

// All mutations target newly-created, unassigned policies. Never attach these policies to a VM
// or update DefaultPolicy. Policies remain available for diagnosis and test-resource teardown.
public class AzureBackupPolicyUpdateEnhancedCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override CustomDefaultMatcher? TestMatcher => new()
    {
        ExcludedHeaders = "Authorization,Content-Type,x-ms-client-request-id",
        CompareBodies = true
    };

    // Match the existing Azure Backup sanitizers without masking schedule/retention payloads.
    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        new BodyRegexSanitizer(new BodyRegexSanitizerBody()
        {
            Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
            GroupForReplace = "host",
        })
    ];

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "(?i)AzureBackupRG_mcp-test",
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = @"[A-Za-z0-9._%+-]+@microsoft\.com",
            Value = "sanitized@example.com",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "72f988bf-86f1-41af-91ab-2d7cd011db47",
            Value = "00000000-0000-0000-0000-000000000000",
        })
    ];

    [Fact]
    public async Task PolicyUpdate_EnhancedHourly_PartialIntervalAndWindow_RoundTrips()
    {
        var name = await CreatePolicyAsync("hourly-partial", HourlySeed());
        var before = await GetPolicyAsync(name);
        AssertHourly(before, 4, "08:00", 12);

        await UpdatePolicyAsync(name, new() { ["hourly-interval-hours"] = 6 });
        var intervalUpdate = await GetPolicyAsync(name);
        AssertHourly(intervalUpdate, 6, "08:00", 12);
        AssertRetention(intervalUpdate, "Daily", 14, "Days");

        await UpdatePolicyAsync(name, new() { ["hourly-window-duration-hours"] = 18 });
        var durationUpdate = await GetPolicyAsync(name);
        AssertHourly(durationUpdate, 6, "08:00", 18);
        AssertRetention(durationUpdate, "Daily", 14, "Days");

        await UpdatePolicyAsync(name, new() { ["hourly-window-start-time"] = "09:30" });
        var startUpdate = await GetPolicyAsync(name);
        AssertHourly(startUpdate, 6, "09:30", 18);
        AssertRetention(startUpdate, "Daily", 14, "Days");
    }

    [Fact]
    public async Task PolicyUpdate_EnhancedDailyToHourly_RoundTrips()
    {
        var name = await CreatePolicyAsync("daily-hourly", new()
        {
            ["policy-sub-type"] = "Enhanced", ["schedule-frequency"] = "Daily", ["schedule-times"] = "02:00"
        });
        var before = await GetPolicyAsync(name);
        AssertSchedule(before, "V2", "Daily", "02:00");

        await UpdatePolicyAsync(name, new()
        {
            ["schedule-frequency"] = "Hourly", ["hourly-interval-hours"] = 6,
            ["hourly-window-start-time"] = "08:00", ["hourly-window-duration-hours"] = 18
        });

        var after = await GetPolicyAsync(name);
        AssertHourly(after, 6, "08:00", 18);
        AssertRetention(after, "Daily", 14, "Days");
        var times = RetentionSchedule(after, "Daily").AssertProperty("retentionTimes");
        Assert.Equal("08:00", Assert.Single(times.EnumerateArray()).GetString());
    }

    [Fact]
    public async Task PolicyUpdate_EnhancedHourlyRetentionOnly_PreservesSchedule_RoundTrips()
    {
        var name = await CreatePolicyAsync("hourly-retention", HourlySeed());
        var before = await GetPolicyAsync(name);

        await UpdatePolicyAsync(name, new() { ["daily-retention-days"] = "30" });

        var after = await GetPolicyAsync(name);
        Assert.Equal(Schedule(before).GetRawText(), Schedule(after).GetRawText());
        AssertHourly(after, 4, "08:00", 12);
        AssertRetention(after, "Daily", 30, "Days");
        Assert.Equal(30, after.AssertProperty("dailyRetentionDays").GetInt32());
    }

    [Fact]
    public async Task PolicyUpdate_EnhancedDailyRetentionOnly_PreservesSchedule_RoundTrips()
    {
        var name = await CreatePolicyAsync("daily-retention", new()
        {
            ["policy-sub-type"] = "Enhanced", ["schedule-frequency"] = "Daily", ["schedule-times"] = "03:30"
        });
        var before = await GetPolicyAsync(name);

        await UpdatePolicyAsync(name, new() { ["daily-retention-days"] = "30" });

        var after = await GetPolicyAsync(name);
        Assert.Equal(Schedule(before).GetRawText(), Schedule(after).GetRawText());
        AssertSchedule(after, "V2", "Daily", "03:30");
        AssertRetention(after, "Daily", 30, "Days");
    }

    [Fact]
    public async Task PolicyUpdate_StandardDailyToWeekly_ReconcilesRetention_RoundTrips()
    {
        var name = await CreatePolicyAsync("standard-weekly", new()
        {
            ["policy-sub-type"] = "Standard", ["schedule-frequency"] = "Daily", ["schedule-times"] = "02:00"
        });
        var before = await GetPolicyAsync(name);
        AssertSchedule(before, "V1", "Daily", "02:00");

        await UpdatePolicyAsync(name, new()
        {
            ["schedule-frequency"] = "Weekly", ["schedule-times"] = "03:30", ["schedule-days-of-week"] = "Sunday",
            ["instant-rp-retention-days"] = "5",
            ["weekly-retention-weeks"] = 8, ["weekly-retention-days-of-week"] = "Sunday"
        });

        var after = await GetPolicyAsync(name);
        AssertSchedule(after, "V1", "Weekly", "03:30");
        Assert.Equal(5, after.AssertProperty("details").AssertProperty("instantRPRetentionRangeInDays").GetInt32());
        Assert.Equal("Sunday", Assert.Single(Schedule(after).AssertProperty("scheduleRunDays").EnumerateArray()).GetString());
        AssertRetention(after, "Weekly", 8, "Weeks");
        Assert.Equal("Sunday", Assert.Single(RetentionSchedule(after, "Weekly").AssertProperty("daysOfWeek").EnumerateArray()).GetString());
        Assert.Equal("03:30", Assert.Single(RetentionSchedule(after, "Weekly").AssertProperty("retentionTimes").EnumerateArray()).GetString());
        Assert.DoesNotContain(RetentionSchedules(after), s => s.AssertProperty("frequency").GetString() == "Daily");
    }

    [Fact]
    public async Task PolicyUpdate_EnhancedInstantSnapshotResourceGroupAndTags_RoundTrips()
    {
        // Crash-consistent snapshots require Enhanced policies and regional service support.
        var seed = HourlySeed();
        seed["instant-rp-retention-days"] = "2";
        seed["policy-tags"] = "retain=original,replace=old";
        var name = await CreatePolicyAsync("instant-settings", seed);
        var prefix = RegisterOrRetrieveVariable("instantPrefix", $"upd-snap-{Random.Shared.NextInt64()}");
        var before = await GetPolicyAsync(name);

        await UpdatePolicyAsync(name, new()
        {
            ["instant-rp-retention-days"] = "4", ["instant-rp-resource-group"] = prefix,
            ["snapshot-consistency"] = "CrashConsistent", ["policy-tags"] = "replace=new,added=enhanced"
        });

        var after = await GetPolicyAsync(name);
        var details = after.AssertProperty("details");
        Assert.Equal(4, details.AssertProperty("instantRPRetentionRangeInDays").GetInt32());
        Assert.Equal(prefix, details.AssertProperty("instantRPResourceGroupNamePrefix").GetString());
        Assert.Equal("OnlyCrashConsistent", details.AssertProperty("snapshotConsistencyType").GetString());
        Assert.Equal(Schedule(before).GetRawText(), Schedule(after).GetRawText());
        AssertRetention(after, "Daily", 14, "Days");
        // policy_get currently omits ARM tags. Their merge semantics are asserted in
        // IaasVmPolicyUpdaterTests; CompareBodies=true also guards the tag-bearing PUT.
    }

    [Fact]
    public async Task PolicyUpdate_StandardArchiveModeAndDays_RoundTrips()
    {
        // Requires an archive-capable RSV region. Long monthly/yearly retention provides
        // eligible policy tiers; actual archive movement is not attempted by this test.
        var name = await CreatePolicyAsync("archive-mode", ArchiveSeed());
        var before = await GetPolicyAsync(name);

        await UpdatePolicyAsync(name, new() { ["archive-tier-mode"] = "TierAfter", ["archive-tier-after-days"] = "180" });
        var after = await GetPolicyAsync(name);
        AssertTiering(after, "TierAfter", 180, "Days");
        Assert.Equal(Schedule(before).GetRawText(), Schedule(after).GetRawText());
        AssertRetention(after, "Monthly", 24, "Months");
        AssertRetention(after, "Yearly", 5, "Years");

        await UpdatePolicyAsync(name, new() { ["archive-tier-after-days"] = "190" });
        var partial = await GetPolicyAsync(name);
        AssertTiering(partial, "TierAfter", 190, "Days");
        AssertRetention(partial, "Monthly", 24, "Months");
        AssertRetention(partial, "Yearly", 5, "Years");
    }

    [Fact]
    public async Task PolicyUpdate_SmartTier_OmittedPreservesAndFalseDisables_RoundTrips()
    {
        // Requires archive/smart-tier support. An unassigned policy is used even when
        // toggling tiering so existing protected items and recovery points are unaffected.
        var name = await CreatePolicyAsync("smart-tier", ArchiveSeed());

        await UpdatePolicyAsync(name, new() { ["smart-tier"] = true });
        var enabled = await GetPolicyAsync(name);
        AssertTiering(enabled, "TierRecommended", 0, "Invalid");

        await UpdatePolicyAsync(name, new() { ["daily-retention-days"] = "60" });
        var omitted = await GetPolicyAsync(name);
        AssertTiering(omitted, "TierRecommended", 0, "Invalid");
        AssertRetention(omitted, "Daily", 60, "Days");
        Assert.Equal(Schedule(enabled).GetRawText(), Schedule(omitted).GetRawText());

        await UpdatePolicyAsync(name, new() { ["smart-tier"] = false });
        var disabled = await GetPolicyAsync(name);
        AssertTiering(disabled, "DoNotTier", 0, "Invalid");
        AssertRetention(disabled, "Daily", 60, "Days");
        AssertRetention(disabled, "Monthly", 24, "Months");
    }

    private Dictionary<string, object?> Parameters(string name) => new()
    {
        ["subscription"] = Settings.SubscriptionId,
        ["resource-group"] = Settings.ResourceGroupName,
        ["vault"] = $"{Settings.ResourceBaseName}-rsv",
        ["vault-type"] = "rsv",
        ["policy"] = name
    };

    private async Task<string> CreatePolicyAsync(string suffix, Dictionary<string, object?> seed)
    {
        var name = RegisterOrRetrieveVariable("isolatedPolicyName", $"test-upd-{suffix}-{Random.Shared.NextInt64()}");
        var parameters = Parameters(name);
        parameters["workload-type"] = "AzureVM";
        parameters["daily-retention-days"] = "14";
        parameters["time-zone"] = "UTC";
        foreach (var (key, value) in seed) { parameters[key] = value; }
        var result = await CallToolAsync("azurebackup_policy_create", parameters);
        Assert.Equal("Succeeded", result.AssertProperty("result").AssertProperty("status").GetString());
        return name;
    }

    private async Task UpdatePolicyAsync(string name, Dictionary<string, object?> changes)
    {
        var parameters = Parameters(name);
        foreach (var (key, value) in changes) { parameters[key] = value; }
        var result = await CallToolAsync("azurebackup_policy_update", parameters);
        Assert.Equal("Succeeded", result.AssertProperty("result").AssertProperty("status").GetString());
    }

    private async Task<JsonElement> GetPolicyAsync(string name)
    {
        var result = await CallToolAsync("azurebackup_policy_get", Parameters(name));
        var policy = Assert.Single(result.AssertProperty("policies").EnumerateArray());
        // The default recording sanitizer replaces resource names in response bodies.
        Assert.False(string.IsNullOrWhiteSpace(policy.AssertProperty("name").GetString()));
        Assert.Equal("rsv", policy.AssertProperty("vaultType").GetString());
        Assert.Equal(0, policy.AssertProperty("protectedItemsCount").GetInt32());
        return policy;
    }

    private static Dictionary<string, object?> HourlySeed() => new()
    {
        ["policy-sub-type"] = "Enhanced", ["schedule-frequency"] = "Hourly",
        ["hourly-interval-hours"] = 4, ["hourly-window-start-time"] = "08:00", ["hourly-window-duration-hours"] = 12
    };

    private static Dictionary<string, object?> ArchiveSeed() => new()
    {
        ["policy-sub-type"] = "Standard", ["daily-retention-days"] = "30",
        ["weekly-retention-weeks"] = 12, ["weekly-retention-days-of-week"] = "Sunday",
        ["monthly-retention-months"] = 24, ["monthly-retention-week-of-month"] = "First", ["monthly-retention-days-of-week"] = "Sunday",
        ["yearly-retention-years"] = 5, ["yearly-retention-months"] = "January", ["yearly-retention-week-of-month"] = "First", ["yearly-retention-days-of-week"] = "Sunday"
    };

    private static JsonElement Schedule(JsonElement policy) => policy.AssertProperty("details").AssertProperty("schedulePolicy");

    private static JsonElement[] RetentionSchedules(JsonElement policy) =>
        policy.AssertProperty("details").AssertProperty("retentionPolicy").AssertProperty("schedules").EnumerateArray().ToArray();

    private static JsonElement RetentionSchedule(JsonElement policy, string frequency) =>
        Assert.Single(RetentionSchedules(policy), schedule => schedule.AssertProperty("frequency").GetString() == frequency);

    private static void AssertRetention(JsonElement policy, string frequency, int count, string durationType)
    {
        var retention = RetentionSchedule(policy, frequency);
        Assert.Equal(count, retention.AssertProperty("durationCount").GetInt32());
        Assert.Equal(durationType, retention.AssertProperty("durationType").GetString());
    }

    private static void AssertSchedule(JsonElement policy, string type, string frequency, string time)
    {
        Assert.Equal(type, policy.AssertProperty("details").AssertProperty("policyType").GetString());
        var schedule = Schedule(policy);
        Assert.Equal(type == "V2" ? "SimpleSchedulePolicyV2" : "SimpleSchedulePolicy", schedule.AssertProperty("schedulePolicyType").GetString());
        Assert.Equal(frequency, schedule.AssertProperty("scheduleRunFrequency").GetString());
        Assert.Equal(time, Assert.Single(schedule.AssertProperty("scheduleRunTimes").EnumerateArray()).GetString());
    }

    private static void AssertHourly(JsonElement policy, int interval, string start, int duration)
    {
        Assert.Equal("V2", policy.AssertProperty("details").AssertProperty("policyType").GetString());
        var schedule = Schedule(policy);
        Assert.Equal("SimpleSchedulePolicyV2", schedule.AssertProperty("schedulePolicyType").GetString());
        Assert.Equal("Hourly", schedule.AssertProperty("scheduleRunFrequency").GetString());
        var hourly = schedule.AssertProperty("hourlySchedule");
        Assert.Equal(interval, hourly.AssertProperty("interval").GetInt32());
        Assert.Equal(start, hourly.AssertProperty("scheduleWindowStartTime").GetString());
        Assert.Equal(duration, hourly.AssertProperty("scheduleWindowDurationInHours").GetInt32());
    }

    private static void AssertTiering(JsonElement policy, string mode, int duration, string durationType)
    {
        var tier = Assert.Single(policy.AssertProperty("details").AssertProperty("tieringPolicies").EnumerateArray(),
            entry => entry.AssertProperty("sourceTier").GetString() == "ArchivedRP");
        Assert.Equal(mode, tier.AssertProperty("tieringMode").GetString());
        Assert.Equal(duration, tier.AssertProperty("durationValue").GetInt32());
        Assert.Equal(durationType, tier.AssertProperty("durationType").GetString());
    }
}