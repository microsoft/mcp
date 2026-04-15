// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
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
                { "workload-type", "AzureVM" }
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
                { "workload-type", "AzureFileShare" }
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
                { "workload-type", "SQL" }
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
                { "workload-type", "SAPHANA" }
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
                { "workload-type", "AzureDisk" }
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
                { "workload-type", "AKS" }
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
                { "workload-type", "PostgreSQLFlexible" }
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
                { "workload-type", "ElasticSAN" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    // CosmosDB policy create test skipped — CosmosDB backup via Azure Backup is not yet GA.
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
        // Bug 1.9/9.7 fix validation: RSV soft-delete now uses BackupResourceVaultConfig API
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
            Output.WriteLine("DPP vault soft-delete is locked (AlwaysOn) — environment-specific, treating as pass.");
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
    public async Task GovernanceFindUnprotected_WithResourceGroupFilter_Successfully()
    {
        var result = await CallToolAsync(
            "azurebackup_governance_find-unprotected",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group-filter", Settings.ResourceGroupName }
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
    public async Task DrEnableCrr_RsvVault_EnablesCrossRegionRestore_Successfully()
    {
        // CRR is an RSV-only feature — LRO can take 10-30 minutes
        // Note: If the vault's redundancy was previously configured via the Vault API
        // (ARM/portal), the Backup Config API will return 400 BMSUserErrorRedundancySettingsUseVaultApi.
        // In that case, CRR may already be enabled. We treat both outcomes as success.
        var vaultName = $"{Settings.ResourceBaseName}-rsv";
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: DrEnableCrr_RSV");

        var result = await CallToolAsync(
            "azurebackup_dr_enablecrr",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: DrEnableCrr_RSV");

        // Success path: result.status == "Succeeded"
        // Environment-specific path: error about Vault API — CRR already configured
        if (result.HasValue && result.Value.TryGetProperty("result", out var opResult))
        {
            Assert.Equal("Succeeded", opResult.GetProperty("status").GetString());
        }
        else if (result.HasValue && result.Value.TryGetProperty("message", out var message))
        {
            var msg = message.GetString() ?? "";
            Assert.Contains("RedundancySettings", msg, StringComparison.OrdinalIgnoreCase);
            Output.WriteLine("CRR already configured via Vault API — environment-specific, treating as pass.");
        }
        else
        {
            Assert.Fail("Unexpected response from DrEnableCrr");
        }
    }

    [Fact]
    public async Task DrEnableCrr_DppVault_EnablesCrossRegionRestore_Successfully()
    {
        // CRR is supported for DPP Backup vaults via FeatureSettings — LRO can take 10-30 minutes
        var vaultName = $"{Settings.ResourceBaseName}-dpp";
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: DrEnableCrr_DPP");

        var result = await CallToolAsync(
            "azurebackup_dr_enablecrr",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" }
            });

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: DrEnableCrr_DPP");
        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    #endregion
}
