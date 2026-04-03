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

    #region Vault Tests

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
    public async Task VaultGet_GetsSingleVault_Successfully()
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

    #endregion

    #region Policy Tests

    [Fact]
    public async Task PolicyGet_ListsPolicies_Successfully()
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
    public async Task PolicyCreate_CreatesPolicy_Successfully()
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

    #endregion

    #region Protected Item Tests

    [Fact]
    public async Task ProtectedItemGet_ListsProtectedItems_Successfully()
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

    #region Protectable Item Tests

    [Fact]
    public async Task ProtectableItemList_ListsProtectableItems_Successfully()
    {
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

    #region Governance Tests

    [Fact]
    public async Task GovernanceSoftDelete_ChecksConfiguration_Successfully()
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
    public async Task GovernanceImmutability_ChecksConfiguration_Successfully()
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

    #endregion

    #region Job Tests

    [Fact]
    public async Task JobGet_ListsJobs_Successfully()
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

    #region Recovery Point Tests

    // recoverypoint_get requires a real protected item with recovery points.
    // Add test when test-resources.bicep includes a protected VM.

    #endregion

    #region Backup Status Tests

    // backup_status requires a real datasource ARM resource ID.
    // Add test when test-resources.bicep includes a VM or database.

    #endregion

    #region DR Tests

    [Fact]
    public async Task DrEnableCrr_EnablesCrossRegionRestore_Successfully()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_dr_enablecrr",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        Assert.NotNull(result);
    }

    #endregion
}
