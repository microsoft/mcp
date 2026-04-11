// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
    }

    // protecteditem_protect requires a real datasource (VM/database).
    // Add test when test-resources.bicep includes a VM.

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

        Assert.NotNull(result);
    }

    #endregion

    #region Protectable Item Tests

    [Fact]
    public async Task ProtectableItemList_ListsProtectableItems_Successfully()
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

        Assert.NotNull(result);
    }

    #endregion

    #region Governance Tests (RSV)

    [Fact]
    public async Task GovernanceSoftDelete_RsvVault_ConfiguresSuccessfully()
    {
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GovernanceImmutability_RsvVault_ConfiguresSuccessfully()
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

        Assert.NotNull(result);
    }

    #endregion

    #region Governance Tests (DPP)

    [Fact]
    public async Task GovernanceSoftDelete_DppVault_ConfiguresSuccessfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_governance_soft-delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "soft-delete", "On" },
                { "vault-type", "dpp" }
            });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GovernanceImmutability_DppVault_ConfiguresSuccessfully()
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
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

        Assert.NotNull(result);
    }

    #endregion

    #region Recovery Point Tests

    // recoverypoint_get requires a real protected item with recovery points.
    // Add test when test-resources.bicep includes a protected VM or disk.

    #endregion

    #region Backup Status Tests

    // backup_status requires a real datasource ARM resource ID.
    // Add test when test-resources.bicep includes a VM or database.

    #endregion

    #region DR Tests

    [Fact]
    public async Task DrEnableCrr_RsvVault_EnablesCrossRegionRestore_Successfully()
    {
        // CRR is an RSV-only feature — LRO can take 10-30 minutes
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
        Assert.NotNull(result);
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
        Assert.NotNull(result);
    }

    #endregion
}
