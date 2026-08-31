// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests;

// Dedicated live-test class for the policy-update parity surface (weekly
// schedule + LTR retention flags added for parity with `az backup policy set`).
// Split into its own class so the recording session is isolated from the
// baseline PolicyUpdate live tests already present in AzureBackupCommandTests.
public class AzureBackupPolicyUpdateParityCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override CustomDefaultMatcher? TestMatcher => new()
    {
        ExcludedHeaders = "Authorization,Content-Type,x-ms-client-request-id",
        CompareBodies = false
    };

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
            Regex = "AzureBackupRG_mcp-test",
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "(?i)azurebackuprg_mcp-test",
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
    public async Task PolicyUpdate_RsvVault_AddsWeeklyRetention_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable(
            "updateVmWeeklyRetPolicyName",
            $"test-upd-vmwr-{Random.Shared.NextInt64()}");

        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "daily-retention-days", "7" }
            });

        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_AddsMonthlyRetentionAbsolute_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable(
            "updateVmMonthlyAbsPolicyName",
            $"test-upd-vmma-{Random.Shared.NextInt64()}");

        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "daily-retention-days", "7" }
            });

        // Absolute monthly: keep the 1st of each month for 12 months. Includes
        // the weekly parent required by the RSV LTR hierarchy.
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_AddsYearlyRetentionRelative_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable(
            "updateVmYearlyRelPolicyName",
            $"test-upd-vmyr-{Random.Shared.NextInt64()}");

        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "daily-retention-days", "7" }
            });

        // Relative yearly: first Sunday of January for 5 years, includes the
        // weekly + monthly parents required by the RSV LTR hierarchy.
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-week-of-month", "First" },
                { "monthly-retention-days-of-week", "Sunday" },
                { "yearly-retention-years", "5" },
                { "yearly-retention-months", "January" },
                { "yearly-retention-week-of-month", "First" },
                { "yearly-retention-days-of-week", "Sunday" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_InvalidWeeklyMissingDaysOfWeek_ReturnsValidationError()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        // Client-side validator failure: no ARM calls are made. Uses
        // DefaultPolicy which already exists so the vault lookup would
        // succeed if the validator ever regressed to a pass-through. The
        // validator failure sets `message` on the response envelope but no
        // `results`, so grab the whole root via the result processor.
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", "DefaultPolicy" },
                { "schedule-frequency", "Weekly" }
            },
            mcpClient: null,
            resultProcessor: root => root);

        Assert.NotNull(result);
        var message = result.Value.AssertProperty("message").GetString();
        Assert.Contains("--schedule-days-of-week", message);
    }
}
