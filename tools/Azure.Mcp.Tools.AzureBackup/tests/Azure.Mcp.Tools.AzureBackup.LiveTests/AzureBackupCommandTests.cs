// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
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
}
