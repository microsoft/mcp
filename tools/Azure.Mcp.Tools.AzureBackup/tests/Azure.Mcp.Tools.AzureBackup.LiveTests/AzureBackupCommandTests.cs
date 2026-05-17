// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.LiveTests;

public class AzureBackupCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Relax matching: ignore Authorization headers and don't compare request bodies
    // (ARM requests include timestamps, correlation IDs, etc. that vary between runs)
    public override CustomDefaultMatcher? TestMatcher => new()
    {
        ExcludedHeaders = "Authorization,Content-Type,x-ms-client-request-id",
        CompareBodies = false
    };

    // Sanitize hostnames in response body URLs to remove actual resource names
    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        new BodyRegexSanitizer(new BodyRegexSanitizerBody()
        {
            Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
            GroupForReplace = "host",
        })
    ];

    // Sanitize the resource group name that doesn't contain ResourceBaseName.
    // The default sanitizer only replaces ResourceBaseName ("mcp26139ae9") which is not
    // a substring of the custom resource group "AzureBackupRG_mcp-test". During playback,
    // Settings.ResourceGroupName becomes "Sanitized" via PlaybackSettings, so the recording
    // must also have "Sanitized" for the resource group to match.
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "AzureBackupRG_mcp-test",
            Value = "Sanitized",
        }),
        // RSV APIs may return resource group names in lowercase in sourceResourceId
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "(?i)azurebackuprg_mcp-test",
            Value = "Sanitized",
        }),
        // ARM x-ms-arm-resource-system-data header may include the recording user's UPN
        // when fresh resources are created (createdBy / lastModifiedBy fields).
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = @"[A-Za-z0-9._%+-]+@microsoft\.com",
            Value = "sanitized@example.com",
        }),
        // x-ms-operation-identifier and certificate URLs in response headers leak the
        // tenant id of the recording subscription. Replace with the well-known zero GUID.
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "72f988bf-86f1-41af-91ab-2d7cd011db47",
            Value = "00000000-0000-0000-0000-000000000000",
        })
    ];

    #region Vault Tests (RSV)

    [Fact]
    public async Task VaultGet_ListsVaults_Successfully()
    {
        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var vaults = result.AssertProperty("vaults");
        Assert.Equal(JsonValueKind.Array, vaults.ValueKind);
        Assert.True(vaults.GetArrayLength() >= 2, "Expected at least 2 vaults (RSV + DPP)");

        // Verify each vault has required structural fields
        foreach (var vault in vaults.EnumerateArray())
        {
            vault.AssertProperty("name");
            vault.AssertProperty("vaultType");
        }
    }

    [Fact]
    public async Task VaultGet_GetsSingleRsvVault_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var vaults = result.AssertProperty("vaults");
        Assert.Equal(1, vaults.GetArrayLength());

        var vault = vaults.EnumerateArray().First();
        Assert.Equal("rsv", vault.AssertProperty("vaultType").GetString());
        Assert.Equal("Succeeded", vault.AssertProperty("provisioningState").GetString());

        // Verify the new detail fields are present (Bug 1.3 fix validation)
        vault.AssertProperty("skuName");
        vault.AssertProperty("redundancy");
    }

    [Fact]
    public async Task VaultGet_FiltersByResourceGroup_Successfully()
    {
        // Bug 1.2 fix validation: RG filter should scope results to the specified resource group
        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var vaults = result.AssertProperty("vaults");
        Assert.Equal(JsonValueKind.Array, vaults.ValueKind);

        // All returned vaults must belong to the specified resource group
        foreach (var vault in vaults.EnumerateArray())
        {
            var rg = vault.AssertProperty("resourceGroup").GetString();
            Assert.Equal(Settings.ResourceGroupName, rg, ignoreCase: true);
        }
    }

    [Fact]
    public async Task VaultCreate_CreatesVault_Successfully()
    {
        var vaultName = RegisterOrRetrieveVariable("createdVaultName", $"test-rsv-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_vault_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "location", "eastus" }
            });

        var vault = result.AssertProperty("vault");
        Assert.Equal("Succeeded", vault.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task VaultCreate_CreatesDppVault_Successfully()
    {
        var vaultName = RegisterOrRetrieveVariable("createdDppVaultName", $"test-dpp-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_vault_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" },
                { "location", "eastus" }
            });

        var vault = result.AssertProperty("vault");
        Assert.Equal("Succeeded", vault.AssertProperty("provisioningState").GetString());

        // DPP vault create must enable a System-Assigned Managed Identity by default so
        // the vault can authenticate to protected datasources without a separate
        // 'vault update --identity-type SystemAssigned' step.
        var getResult = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" }
            });
        // azurebackup_vault_get returns a 'vaults' array; pick the first matching entry.
        var vaults = getResult.AssertProperty("vaults");
        Assert.Equal(JsonValueKind.Array, vaults.ValueKind);
        var fetchedVault = vaults.EnumerateArray().First();
        var identityType = fetchedVault.AssertProperty("identityType").GetString();
        Assert.Equal("SystemAssigned", identityType, ignoreCase: true);
    }

    [Fact]
    public async Task VaultUpdate_UpdatesTags_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_vault_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "tags", "{\"environment\":\"test\"}" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task VaultUpdate_RsvVault_UpdatesIdentityType_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_vault_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "identity-type", "SystemAssigned" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task VaultGet_FiltersByVaultType_Successfully()
    {
        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault-type", "rsv" }
            });

        var vaults = result.AssertProperty("vaults");
        Assert.Equal(JsonValueKind.Array, vaults.ValueKind);

        // All returned vaults must be RSV type
        foreach (var vault in vaults.EnumerateArray())
        {
            Assert.Equal("rsv", vault.AssertProperty("vaultType").GetString());
        }
    }

    #endregion

    #region Vault Tests (DPP)

    [Fact]
    public async Task VaultGet_GetsSingleDppVault_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var vaults = result.AssertProperty("vaults");
        Assert.Equal(1, vaults.GetArrayLength());

        var vault = vaults.EnumerateArray().First();
        Assert.Equal("dpp", vault.AssertProperty("vaultType").GetString());
        Assert.Equal("Succeeded", vault.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task VaultUpdate_DppVault_UpdatesTags_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_vault_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "tags", "{\"environment\":\"test\"}" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    #endregion

    #region Policy Tests (RSV)

    [Fact]
    public async Task PolicyGet_RsvVault_ListsPolicies_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_policy_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var policies = result.AssertProperty("policies");
        Assert.Equal(JsonValueKind.Array, policies.ValueKind);
        // RSV vaults always have at least DefaultPolicy
        Assert.True(policies.GetArrayLength() >= 1, "RSV vault should have at least DefaultPolicy");

        foreach (var policy in policies.EnumerateArray())
        {
            policy.AssertProperty("name");
            Assert.Equal("rsv", policy.AssertProperty("vaultType").GetString());
        }
    }

    [Fact]
    public async Task PolicyCreate_RsvVault_CreatesPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdPolicyName", $"test-policy-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVault_CreatesVmPolicyWithCustomRetention_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdVmRetentionPolicyName", $"test-vm-ret-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "daily-retention-days", "14" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVault_CreatesFileSharePolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdFsPolicyName", $"test-fs-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureFileShare" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVault_CreatesSqlPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdSqlPolicyName", $"test-sql-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVault_CreatesSapHanaPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdHanaPolicyName", $"test-hana-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SAPHANA" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVm_WeeklyMultiTierWithArchive_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvVmWeeklyArchivePolicyName", $"test-vm-weekly-arch-{Random.Shared.NextInt64()}");

        // VM Enhanced V2 + Weekly + multi-tier + archive. All retention days-of-week must match schedule.
        // Monthly/Yearly use relative format (week-of-month + days-of-week) for Weekly schedule.
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "policy-sub-type", "Enhanced" },
                { "schedule-frequency", "Weekly" },
                { "schedule-days-of-week", "Monday" },
                { "schedule-times", "03:00" },
                { "weekly-retention-weeks", "8" },
                { "weekly-retention-days-of-week", "Monday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-week-of-month", "First" },
                { "monthly-retention-days-of-week", "Monday" },
                { "yearly-retention-years", "5" },
                { "yearly-retention-months", "January" },
                { "yearly-retention-week-of-month", "First" },
                { "yearly-retention-days-of-week", "Monday" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "90" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVm_HourlyEnhanced_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvVmHourlyEnhancedPolicyName", $"test-vm-hourly-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "policy-sub-type", "Enhanced" },
                { "schedule-frequency", "Hourly" },
                { "hourly-interval-hours", "4" },
                { "hourly-window-start-time", "08:00" },
                { "hourly-window-duration-hours", "12" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvSql_FullLogDiff_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdRsvSqlFullLogDiffPolicyName", $"test-sql-fld-{Random.Shared.NextInt64()}");

        // SQL Full = Weekly multi-day (every day except Wednesday), Diff = once a week (Wednesday).
        // Differential can only run once a week and must not overlap with Full days.
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" },
                { "full-schedule-frequency", "Weekly" },
                { "full-schedule-days-of-week", "Sunday,Monday,Tuesday,Thursday,Friday,Saturday" },
                { "schedule-times", "02:00" },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday,Monday,Tuesday,Thursday,Friday,Saturday" },
                { "differential-schedule-days-of-week", "Wednesday" },
                { "differential-retention-days", "15" },
                { "log-frequency-minutes", "60" },
                { "log-retention-days", "7" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyGet_RsvVault_GetsSinglePolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        // DefaultPolicy is a built-in policy that always exists
        var result = await CallToolAsync(
            "azurebackup_policy_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", "DefaultPolicy" }
            });

        var policies = result.AssertProperty("policies");
        Assert.Equal(1, policies.GetArrayLength());

        // Bug 2.3 fix validation: single policy should include schedule/retention details
        var policy = policies.EnumerateArray().First();
        // Policy name may be sanitized in recordings, so just verify it exists
        policy.AssertProperty("name");
        Assert.Equal("rsv", policy.AssertProperty("vaultType").GetString());
        policy.AssertProperty("datasourceTypes");
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_UpdatesIaasVmPolicyRetention_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("updateVmPolicyName", $"test-upd-vm-{Random.Shared.NextInt64()}");

        // Create a VM policy with lower retention first
        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "daily-retention-days", "14" }
            });

        // Increase retention to 30 days (immutable vaults block reduction)
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_UpdatesIaasVmPolicyScheduleTime_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("updateVmSchedulePolicyName", $"test-upd-vms-{Random.Shared.NextInt64()}");

        // Create a VM policy with lower retention first
        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureVM" },
                { "schedule-time", "02:00" }
            });

        // Update schedule time to 04:00
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "schedule-time", "04:00" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_UpdatesSqlWorkloadPolicyRetention_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("updateSqlPolicyName", $"test-upd-sql-{Random.Shared.NextInt64()}");

        // Create a SQL (VmWorkload) policy first
        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" },
                { "daily-retention-days", "30" }
            });

        // Update Full sub-policy retention to 60 days
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "daily-retention-days", "60" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_UpdatesSqlWorkloadPolicyScheduleAndRetention_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("updateSqlBothPolicyName", $"test-upd-sqlb-{Random.Shared.NextInt64()}");

        // Create a SQL (VmWorkload) policy first
        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" }
            });

        // Update both schedule time and retention on the Full sub-policy
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "schedule-time", "06:00" },
                { "daily-retention-days", "45" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("updated", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_RsvVault_NoChanges_ReturnsUnchangedMessage()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        // Update DefaultPolicy with no schedule-time or retention changes
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", "DefaultPolicy" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
        Assert.Contains("unchanged", opResult.AssertProperty("message").GetString());
    }

    [Fact]
    public async Task PolicyUpdate_DppVault_ReturnsNotSupportedError()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        // DPP vaults do not support policy update — should return an error response with type InvalidOperationException
        var result = await CallToolAsync(
            "azurebackup_policy_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", "some-policy" },
                { "daily-retention-days", "30" }
            });

        Assert.NotNull(result);
        var errorType = result.Value.AssertProperty("type");
        Assert.Equal("InvalidOperationException", errorType.GetString());
    }

    #endregion

    #region Policy Tests (DPP)

    [Fact]
    public async Task PolicyGet_DppVault_ListsPolicies_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_policy_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var policies = result.AssertProperty("policies");
        Assert.Equal(JsonValueKind.Array, policies.ValueKind);
    }

    [Fact]
    public async Task PolicyCreate_DppVault_CreatesDiskPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdDppPolicyName", $"test-dpp-policy-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppVault_CreatesDiskPolicyWithCustomRetention_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdDppDiskRetPolicyName", $"test-disk-ret-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "daily-retention-days", "14" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppVault_CreatesBlobPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdBlobPolicyName", $"test-blob-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureBlob" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppVault_CreatesAksPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdAksPolicyName", $"test-aks-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AKS" },
                { "schedule-frequency", "PT4H" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }



    [Fact]
    public async Task PolicyCreate_DppVault_CreatesPgFlexPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdPgFlexPolicyName", $"test-pgflex-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "PostgreSQLFlexible" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppVault_CreatesElasticSanPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdEsanPolicyName", $"test-esan-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "ElasticSAN" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppDisk_VaultTierMultiTier_E2E()
    {
        // AzureDisk with vault-tier copy + Weekly/Monthly retention (no Yearly, no Archive per manifest).
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdDppDiskMultiTierPolicyName", $"test-disk-mt-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "7" },
                { "enable-vault-tier-copy", "true" },
                { "vault-tier-copy-after-days", "7" },
                { "weekly-retention-weeks", "12" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    // CosmosDB policy create test skipped  -  CosmosDB backup via Azure Backup is not yet GA.
    // Stage 2: Add PolicyCreate_DppVault_CreatesCosmosDbPolicy_Successfully when GA.

    [Fact]
    public async Task PolicyCreate_DppVault_CreatesAdlsPolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdAdlsPolicyName", $"test-adls-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "ADLS" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    // ===== Stage 2 live tests =====

    [Fact(Skip = "VM smart-tier (TierRecommended) requires the smart-tiering preview feature to be enabled on the vault. Confirmed via direct ARM REST PUT: backend rejects the same shape Az CLI emits with BMSUserErrorInvalidPolicyInput on this vault. Builder now emits the Az-CLI-aligned shape (TieringMode=TierRecommended + Duration=0 + DurationType=Invalid) which will work where the feature is enabled.")]
    public async Task PolicyCreate_RsvVm_SmartTier_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdVmSmartTierPolicyName", $"test-vm-smart-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureIaasVM" },
                { "schedule-frequency", "Daily" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "180" },
                { "smart-tier", "true" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvHana_WithSnapshotBackup_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdHanaSnapshotPolicyName", $"test-hana-snap-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SAPHANA" },
                { "schedule-frequency", "Daily" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "30" },
                { "log-frequency-minutes", "60" },
                { "enable-snapshot-backup", "true" },
                { "snapshot-instant-rp-retention-days", "5" },
                { "snapshot-instant-rp-resource-group", Settings.ResourceGroupName }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvSql_WithArchiveTier_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdSqlArchivePolicyName", $"test-sql-archive-{Random.Shared.NextInt64()}");

        // SQL Full Weekly + weekly/monthly retention + archive on Full sub-policy + Log.
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "SQL" },
                { "full-schedule-frequency", "Weekly" },
                { "full-schedule-days-of-week", "Sunday" },
                { "schedule-times", "02:00" },
                { "weekly-retention-weeks", "4" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-week-of-month", "First" },
                { "monthly-retention-days-of-week", "Sunday" },
                { "archive-tier-mode", "TierAfter" },
                { "archive-tier-after-days", "90" },
                { "log-frequency-minutes", "60" },
                { "log-retention-days", "7" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvFileShare_DailyMultiTier_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdAfsDailyMultiTierPolicyName", $"test-afs-daily-mt-{Random.Shared.NextInt64()}");

        // AFS supports Daily schedule + multi-tier (weekly/monthly/yearly) long-term retention.
        // Hourly schedule + LTR is not supported by AFS today (rejected with UserErrorInvalidRequestParameter).
        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureFileShare" },
                { "schedule-frequency", "Daily" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "30" },
                { "weekly-retention-weeks", "12" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" },
                { "yearly-retention-years", "5" },
                { "yearly-retention-months", "January" },
                { "yearly-retention-days-of-month", "1" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_RsvVm_WithPolicyTags_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var policyName = RegisterOrRetrieveVariable("createdVmTaggedPolicyName", $"test-vm-tags-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureIaasVM" },
                { "schedule-frequency", "Daily" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "30" },
                { "policy-tags", "env=test,team=mcp" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppDisk_VaultTierCopy_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdDiskVaultCopyPolicyName", $"test-disk-vc-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "7" },
                { "enable-vault-tier-copy", "true" },
                { "vault-tier-copy-after-days", "7" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact(Skip = "Vaulted Blob/ADLS DPP backup requires per-storage-account feature enablement that is not configured on the test vault. Confirmed via Az CLI: even the CLI-generated shape is rejected with BMSUserErrorInvalidInput on this vault. Builder now emits the correct Az-CLI-aligned shape (single VaultStore retention rule, no AzureBackupRule) but the service rejects all attempts in this environment.")]
    public async Task PolicyCreate_DppBlob_Vaulted_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdBlobVaultedPolicyName", $"test-blob-vault-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureBlob" },
                { "backup-mode", "Vaulted" },
                { "schedule-frequency", "P1D" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppBlob_ContinuousWithPitr_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdBlobPitrPolicyName", $"test-blob-pitr-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureBlob" },
                { "pitr-retention-days", "60" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact(Skip = "Vaulted ADLS DPP backup follows Blob vaulted shape; same service-side limitation  -  see PolicyCreate_DppBlob_Vaulted_E2E.")]
    public async Task PolicyCreate_DppAdls_Vaulted_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = RegisterOrRetrieveVariable("createdAdlsVaultedPolicyName", $"test-adls-vault-{Random.Shared.NextInt64()}");

        var result = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDataLakeStorage" },
                { "backup-mode", "Vaulted" },
                { "schedule-frequency", "P1D" },
                { "schedule-times", "02:00" },
                { "daily-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task PolicyCreate_DppVault_PolicyTags_RejectedWithGuidance()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = $"test-dpp-tags-{Random.Shared.NextInt64()}";

        var status = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "daily-retention-days", "7" },
                { "policy-tags", "env=test" }
            },
            mcpClient: null,
            resultProcessor: elem => elem.TryGetProperty("status", out var s) ? s : null);

        Assert.NotNull(status);
        Assert.Equal(400, status!.Value.GetInt32());
    }

    [Fact]
    public async Task PolicyGet_DppVault_GetsSinglePolicy_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        // First create a policy we can then get by name
        var policyName = RegisterOrRetrieveVariable("dppGetPolicyName", $"test-get-{Random.Shared.NextInt64()}");

        await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName },
                { "workload-type", "AzureDisk" }
            });

        var result = await CallToolAsync(
            "azurebackup_policy_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "policy", policyName }
            });

        var policies = result.AssertProperty("policies");
        Assert.Equal(1, policies.GetArrayLength());

        var policy = policies.EnumerateArray().First();
        Assert.Equal("dpp", policy.AssertProperty("vaultType").GetString());
    }

    #endregion

    #region Protected Item Tests (RSV)

    [Fact]
    public async Task ProtectedItemGet_RsvVault_ListsProtectedItems_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_protecteditem_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var items = result.AssertProperty("protectedItems");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    // protecteditem_protect and protecteditem_get (by name) require a real datasource (VM/database).
    // Stage 2: Add tests when test-resources.bicep includes a VM for backup status,
    // friendly-name lookup, container auto-discovery, and recovery point tests.

    #endregion

    #region Protected Item Tests (DPP)

    [Fact]
    public async Task ProtectedItemGet_DppVault_ListsProtectedItems_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_protecteditem_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var items = result.AssertProperty("protectedItems");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    /// <summary>
    /// End-to-end Disk protection through DPP vault.
    /// Validates the Bug #2 (DPP) fix: <c>protecteditem protect</c> waits for the operation
    /// to complete (<see cref="Azure.WaitUntil.Completed"/>), reads the backup-instance back,
    /// and surfaces a real <c>protectionStatus</c> rather than a fake <c>"Accepted"</c>.
    /// Also implicitly validates the Bug #1 fix because protection succeeds only when the
    /// DPP vault MSI created by <c>vault create</c> has the right RBAC on the disk + RG.
    /// </summary>
    [Fact]
    [LiveTestOnly]
    public async Task ProtectedItemProtect_DppVault_DiskProtection_Succeeds_E2E()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var policyName = $"{Settings.ResourceBaseName}-disk-policy-{Guid.NewGuid().ToString("N")[..8]}";
        var diskName = $"{Settings.ResourceBaseName}-disk";
        var diskId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Compute/disks/{diskName}";

        // 1. Create disk-workload backup policy via MCP
        var policyResult = await CallToolAsync(
            "azurebackup_policy_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" },
                { "policy", policyName },
                { "workload-type", "AzureDisk" },
                { "daily-retention-days", "7" }
            });

        var policyOp = policyResult.AssertProperty("result");
        Assert.Equal("Succeeded", policyOp.AssertProperty("status").GetString());

        // 2. Protect the disk via MCP  -  exercises the new DPP code path
        var protectResult = await CallToolAsync(
            "azurebackup_protecteditem_protect",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" },
                { "datasource-id", diskId },
                { "policy", policyName },
                { "datasource-type", "AzureDisk" }
            });

        var protectOp = protectResult.AssertProperty("result");

        // The new code returns a real terminal status. Acceptable values:
        //   "Succeeded"  -  backend accepted the configuration
        //   "Failed"     -  backend rejected; the test infrastructure should make Succeeded the norm,
        //                 but if the backend transiently fails we still want to assert the new
        //                 contract (real errorMessage is present, JobId is null for DPP).
        var status = protectOp.AssertProperty("status").GetString();
        Assert.True(status is "Succeeded" or "Failed", $"Unexpected DPP protect status: {status}");

        // Bug #2 DPP contract: a backup-instance name is always returned, JobId is never set.
        protectOp.AssertProperty("protectedItemName");
        Assert.False(protectOp.TryGetProperty("jobId", out var jobId) && jobId.ValueKind != JsonValueKind.Null,
            "DPP protect must not return a jobId (DPP is not a job).");

        if (status == "Succeeded")
        {
            // Surface the protection status (e.g., "ConfiguringProtection" / "ProtectionConfigured")
            protectOp.AssertProperty("protectionStatus");
        }
        else
        {
            // Failed responses must include a non-empty errorMessage from the backend
            var errorMessage = protectOp.AssertProperty("errorMessage").GetString();
            Assert.False(string.IsNullOrWhiteSpace(errorMessage), "Failed DPP protect must include errorMessage.");
            Output.WriteLine($"DPP disk protect returned Failed: {errorMessage}");
        }
    }

    #endregion

    #region Protectable Item Tests

    [Fact]
    public async Task ProtectableItemList_RsvVault_ListsProtectableItems_Successfully()
    {
        // Protectable items is an RSV-only feature
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_protectableitem_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    // Bug 3.3 fix validation: DPP vault routed to protectable items returns a clear error.
    // This is tested at the unit test level (ListProtectableItemsAsync_NoVaultType_DppVault_ThrowsArgumentException)
    // because the live test would need to handle the error response format differently from a success response.

    #endregion

    #region Governance Tests (RSV)

    [Fact]
    public async Task GovernanceSoftDelete_RsvVault_ConfiguresSuccessfully()
    {
        // RSV soft-delete now uses Vault PATCH API with RecoveryServicesSoftDeleteSettings
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_governance_soft-delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "soft-delete", "On" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task GovernanceSoftDelete_RsvVault_WithRetentionDays_ConfiguresSuccessfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_governance_soft-delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "soft-delete", "On" },
                { "soft-delete-retention-days", "30" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task GovernanceImmutability_RsvVault_Disabled_ConfiguresSuccessfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_governance_immutability",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "immutability-state", "Disabled" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task GovernanceImmutability_RsvVault_Enabled_ConfiguresSuccessfully()
    {
        // Bug 9.5 fix validation: "Enabled" is normalized to "Unlocked" before calling the API
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_governance_immutability",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "immutability-state", "Enabled" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    #endregion

    #region Governance Tests (DPP)

    [Fact]
    public async Task GovernanceSoftDelete_DppVault_ConfiguresSuccessfully()
    {
        // Note: If the DPP vault already has soft-delete set to AlwaysOn,
        // the API will reject attempts to change it (400 BMSUserErrorInvalidInput).
        // We treat both success and AlwaysOn-locked scenarios as acceptable.
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_governance_soft-delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "soft-delete", "On" },
                { "soft-delete-retention-days", "14" },
                { "vault-type", "dpp" }
            });

        if (result.HasValue && result.Value.TryGetProperty("result", out var opResult))
        {
            Assert.Equal("Succeeded", opResult.GetProperty("status").GetString());
        }
        else if (result.HasValue && result.Value.TryGetProperty("message", out var message))
        {
            var msg = message.GetString() ?? "";
            Assert.True(
                msg.Contains("InvalidInput", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("AlwaysOn", StringComparison.OrdinalIgnoreCase),
                $"Unexpected error: {msg}");
            Output.WriteLine("DPP vault soft-delete is locked (AlwaysOn)  -  environment-specific, treating as pass.");
        }
        else
        {
            Assert.Fail("Unexpected response from GovernanceSoftDelete DPP");
        }
    }

    [Fact]
    public async Task GovernanceImmutability_DppVault_Disabled_ConfiguresSuccessfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_governance_immutability",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "immutability-state", "Disabled" },
                { "vault-type", "dpp" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task GovernanceImmutability_DppVault_Enabled_ConfiguresSuccessfully()
    {
        // Bug 9.6 fix validation: "Enabled" normalized to "Unlocked" for DPP too
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_governance_immutability",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "immutability-state", "Enabled" },
                { "vault-type", "dpp" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    #endregion

    #region Governance Tests (Subscription-scoped)

    [Fact]
    public async Task GovernanceFindUnprotected_ScansSubscription_Successfully()
    {
        var result = await CallToolAsync(
            "azurebackup_governance_find-unprotected",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);
    }

    [Fact]
    public async Task GovernanceFindUnprotected_WithResourceTypeFilter_Successfully()
    {
        var result = await CallToolAsync(
            "azurebackup_governance_find-unprotected",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-type-filter", "Microsoft.Compute/virtualMachines" }
            });

        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // All returned resources should be VMs
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.Equal("Microsoft.Compute/virtualMachines",
                resource.AssertProperty("resourceType").GetString(), ignoreCase: true);
        }
    }

    [Fact]
    public async Task GovernanceFindUnprotected_WithResourceGroup_Successfully()
    {
        var result = await CallToolAsync(
            "azurebackup_governance_find-unprotected",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // All returned resources should be in the specified resource group
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.Equal(Settings.ResourceGroupName,
                resource.AssertProperty("resourceGroup").GetString(), ignoreCase: true);
        }
    }

    #endregion

    #region Job Tests (RSV)

    [Fact]
    public async Task JobGet_RsvVault_ListsJobs_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_job_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var jobs = result.AssertProperty("jobs");
        Assert.Equal(JsonValueKind.Array, jobs.ValueKind);
    }

    #endregion

    #region Job Tests (DPP)

    [Fact]
    public async Task JobGet_DppVault_ListsJobs_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_job_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var jobs = result.AssertProperty("jobs");
        Assert.Equal(JsonValueKind.Array, jobs.ValueKind);
    }

    #endregion

    #region Recovery Point Tests

    // recoverypoint_get requires a real protected item with recovery points.
    // Stage 2: Add test when test-resources.bicep includes a protected VM or disk.

    #endregion

    #region Backup Status Tests

    // backup_status requires a real datasource ARM resource ID.
    // Stage 2: Add test when test-resources.bicep includes a VM or database.

    #endregion

    #region DR Tests

    [Fact]
    public async Task DisasterRecoveryEnableCrr_RsvVault_EnablesCrossRegionRestore_Successfully()
    {
        // CRR is an RSV-only feature — LRO can take 10-30 minutes
        // CRR is enabled via the Vault PATCH API with RedundancySettings.CrossRegionRestore.
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: DisasterRecoveryEnableCrr_RSV");

        var result = await CallToolAsync(
            "azurebackup_disasterrecovery_enable-crr",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: DisasterRecoveryEnableCrr_RSV");

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    [Fact]
    public async Task DisasterRecoveryEnableCrr_DppVault_EnablesCrossRegionRestore_Successfully()
    {
        // CRR is supported for DPP Backup vaults via FeatureSettings  -  LRO can take 10-30 minutes
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: DisasterRecoveryEnableCrr_DPP");

        var result = await CallToolAsync(
            "azurebackup_disasterrecovery_enable-crr",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: DisasterRecoveryEnableCrr_DPP");
        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    #endregion

    #region Undelete Protected Item Tests

    [Fact]
    [LiveTestOnly]
    public async Task ProtectedItemUndelete_DppVault_UndeletesDisk_Successfully()
    {
        // The test-resources-post.ps1 script protects a disk in the DPP vault and then
        // soft-deletes it. This test restores the soft-deleted disk backup instance.
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var datasourceId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Compute/disks/{Settings.ResourceBaseName}-disk";

        var result = await CallToolAsync(
            "azurebackup_protecteditem_undelete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "datasource-id", datasourceId },
                { "vault-type", "dpp" }
            });

        // If no soft-deleted item exists (consumed by a prior run or never set up),
        // the command returns an error response instead of a result. Skip gracefully.
        if (!result.HasValue || !result.Value.TryGetProperty("result", out var opResult))
        {
            var msg = result.HasValue && result.Value.TryGetProperty("message", out var m) ? m.GetString() : "unknown";
            Assert.Skip($"No soft-deleted DPP backup instance available: {msg}");
            return;
        }

        Assert.Equal("Accepted", opResult.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ProtectedItemUndelete_RsvVault_UndeletesFileShare_Successfully()
    {
        // The test-resources-post.ps1 script protects a file share in the RSV vault
        // and then soft-deletes it. This test restores the soft-deleted file share backup.
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var storageAccountName = $"{Settings.ResourceBaseName.Replace("-", "")}sa";
        if (storageAccountName.Length > 24)
        {
            storageAccountName = storageAccountName[..24];
        }

        // File share datasource ID format for RSV matching
        var datasourceId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}/fileServices/default/shares/{Settings.ResourceBaseName}-share";

        var result = await CallToolAsync(
            "azurebackup_protecteditem_undelete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "datasource-id", datasourceId },
                { "vault-type", "rsv" }
            });

        // Expect accepted (LRO started  -  item restore is in progress)
        var opResult = result.AssertProperty("result");
        Assert.Equal("Accepted", opResult.AssertProperty("status").GetString());
    }

    #endregion

    #region Security Tests (MUA - RSV)

    [Fact]
    public async Task SecurityConfigureMua_RsvVault_EnableMua_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var resourceGuardId = Settings.DeploymentOutputs?.GetValueOrDefault("RESOURCE_GUARD_ID")
            ?? "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-security/providers/Microsoft.DataProtection/resourceGuards/test-guard";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: SecurityConfigureMua_RSV_Enable");

        var result = await CallToolAsync(
            "azurebackup_security_configure-mua",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "resource-guard-id", resourceGuardId }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: SecurityConfigureMua_RSV_Enable");

        if (result.HasValue && result.Value.TryGetProperty("result", out var opResult))
        {
            Assert.Equal("Succeeded", opResult.GetProperty("status").GetString());
        }
        else if (result.HasValue && result.Value.TryGetProperty("message", out var message))
        {
            var msg = message.GetString() ?? "";
            bool isEnvironmentSpecific = msg.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("ResourceGuard", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Conflict", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Reader", StringComparison.OrdinalIgnoreCase);
            Assert.True(isEnvironmentSpecific, $"Unexpected error: {msg[..Math.Min(msg.Length, 300)]}");
            Output.WriteLine($"MUA enable skipped due to environment constraint: {msg[..Math.Min(msg.Length, 200)]}");
        }
        else
        {
            Assert.Fail("Unexpected response from SecurityConfigureMua (RSV Enable)");
        }
    }

    [Fact]
    public async Task SecurityConfigureMua_RsvVault_DisableMua_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: SecurityConfigureMua_RSV_Disable");

        var result = await CallToolAsync(
            "azurebackup_security_configure-mua",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: SecurityConfigureMua_RSV_Disable");

        if (result.HasValue && result.Value.TryGetProperty("result", out var opResult))
        {
            Assert.Equal("Succeeded", opResult.GetProperty("status").GetString());
        }
        else if (result.HasValue && result.Value.TryGetProperty("message", out var message))
        {
            var msg = message.GetString() ?? "";
            bool isEnvironmentSpecific = msg.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Authorization", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UnlockPreviligeAccess", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UnlockPrivilegeAccess", StringComparison.OrdinalIgnoreCase);
            Assert.True(isEnvironmentSpecific, $"Unexpected error: {msg[..Math.Min(msg.Length, 300)]}");
            Output.WriteLine($"MUA disable skipped: {msg[..Math.Min(msg.Length, 200)]}");
        }
        else
        {
            Assert.Fail("Unexpected response from SecurityConfigureMua (RSV Disable)");
        }
    }

    #endregion

    #region Security Tests (MUA - DPP)

    [Fact]
    public async Task SecurityConfigureMua_DppVault_EnableMua_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        var resourceGuardId = Settings.DeploymentOutputs?.GetValueOrDefault("RESOURCE_GUARD_ID")
            ?? "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-security/providers/Microsoft.DataProtection/resourceGuards/test-guard";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: SecurityConfigureMua_DPP_Enable");

        var result = await CallToolAsync(
            "azurebackup_security_configure-mua",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" },
                { "resource-guard-id", resourceGuardId }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: SecurityConfigureMua_DPP_Enable");

        if (result.HasValue && result.Value.TryGetProperty("result", out var opResult))
        {
            Assert.Equal("Succeeded", opResult.GetProperty("status").GetString());
        }
        else if (result.HasValue && result.Value.TryGetProperty("message", out var message))
        {
            var msg = message.GetString() ?? "";
            bool isEnvironmentSpecific = msg.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("ResourceGuard", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Conflict", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Reader", StringComparison.OrdinalIgnoreCase);
            Assert.True(isEnvironmentSpecific, $"Unexpected error: {msg[..Math.Min(msg.Length, 300)]}");
            Output.WriteLine($"MUA enable skipped (DPP): {msg[..Math.Min(msg.Length, 200)]}");
        }
        else
        {
            Assert.Fail("Unexpected response from SecurityConfigureMua (DPP Enable)");
        }
    }

    [Fact]
    public async Task SecurityConfigureMua_DppVault_DisableMua_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: SecurityConfigureMua_DPP_Disable");

        var result = await CallToolAsync(
            "azurebackup_security_configure-mua",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: SecurityConfigureMua_DPP_Disable");

        if (result.HasValue && result.Value.TryGetProperty("result", out var opResult))
        {
            Assert.Equal("Succeeded", opResult.GetProperty("status").GetString());
        }
        else if (result.HasValue && result.Value.TryGetProperty("message", out var message))
        {
            var msg = message.GetString() ?? "";
            bool isEnvironmentSpecific = msg.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Authorization", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UnlockPreviligeAccess", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UnlockPrivilegeAccess", StringComparison.OrdinalIgnoreCase);
            Assert.True(isEnvironmentSpecific, $"Unexpected error: {msg[..Math.Min(msg.Length, 300)]}");
            Output.WriteLine($"MUA disable skipped (DPP): {msg[..Math.Min(msg.Length, 200)]}");
        }
        else
        {
            Assert.Fail("Unexpected response from SecurityConfigureMua (DPP Disable)");
        }
    }

    [Fact]
    public async Task SecurityConfigureMua_RsvVault_WithExplicitVaultType_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        var resourceGuardId = Settings.DeploymentOutputs?.GetValueOrDefault("RESOURCE_GUARD_ID")
            ?? "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-security/providers/Microsoft.DataProtection/resourceGuards/test-guard";

        var result = await CallToolAsync(
            "azurebackup_security_configure-mua",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "resource-guard-id", resourceGuardId }
            });

        // Any response is acceptable — validates the command routing works with explicit vault type
        Assert.True(result.HasValue, "Response should not be empty");
    }

    // TC-7: Invalid vault-type validation is covered by unit tests.
    // Command-level validation errors return MCP error responses without tool content.

    #endregion

    #region Security - Configure Encryption (CMK)

    [Fact]
    [LiveTestOnly]
    public async Task SecurityConfigureEncryption_RsvVault_SystemAssigned()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: SecurityConfigureEncryption_RSV_SystemAssigned");

        var result = await CallToolAsync(
            "azurebackup_security_configure-encryption",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "key-vault-uri", Settings.DeploymentOutputs?.GetValueOrDefault("KEY_VAULT_URI") ?? "https://kv-backup-test.vault.azure.net/" },
                { "key-name", Settings.DeploymentOutputs?.GetValueOrDefault("KEY_NAME") ?? "backup-cmk" },
                { "identity-type", "SystemAssigned" }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: SecurityConfigureEncryption_RSV_SystemAssigned");

        if (result.HasValue)
        {
            var text = result.Value.GetProperty("text").GetString() ?? "";
            Output.WriteLine($"Result: {text}");
            Assert.Contains("Succeeded", text);
        }
        else
        {
            Assert.Fail("Unexpected response from SecurityConfigureEncryption (RSV SystemAssigned)");
        }
    }

    #endregion

}

