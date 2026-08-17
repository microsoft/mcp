// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Vault;

// Recorded live tests covering the --expand feature on azurebackup_vault_get.
// Kept in a separate class from AzureBackupCommandTests so recordings for
// posture-field assertions live in their own test-scoped recording files and
// so pre-existing recordings for the vault-get tests remain undisturbed.
public class VaultGetExpandCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Match sanitizer configuration of AzureBackupCommandTests so recordings
    // in this class use the same request-matching and body-sanitization rules.
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
    public async Task VaultGet_WithoutExpand_OmitsPostureFields()
    {
        // Regression guard: default output shape must not include the extended
        // posture fields introduced by --expand (soft-delete state, MUA state,
        // encryption state, etc.). Existing consumers must see the same response
        // shape as before the --expand feature was added.
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName }
            });

        var vault = result.AssertProperty("vaults").EnumerateArray().First();

        foreach (var postureField in new[] {
            "muaState", "muaResourceGuardId",
            "crossRegionRestoreState", "publicNetworkAccess",
            "crossSubscriptionRestoreState", "enhancedSecurityState",
            "encryptionState", "encryptionKeyUri",
            "monitoringAlertState", "privateEndpointConnections" })
        {
            Assert.False(
                vault.TryGetProperty(postureField, out _),
                $"Field '{postureField}' must be omitted when --expand is not specified.");
        }
    }

    [Fact]
    public async Task VaultGet_WithExpandAll_RsvVault_ReturnsPostureFields()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv";

        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "expand", "all" }
            });

        var vault = result.AssertProperty("vaults").EnumerateArray().First();
        Assert.Equal("rsv", vault.AssertProperty("vaultType").GetString());

        // Network-scope: always populated on a live RSV vault.
        vault.AssertProperty("publicNetworkAccess");
        // Cross-region restore state (from RedundancySettings).
        vault.AssertProperty("crossRegionRestoreState");
        // MUA state must be "Enabled" (proxy present) or "Disabled" (no proxy).
        var muaState = vault.AssertProperty("muaState").GetString();
        Assert.True(muaState is "Enabled" or "Disabled",
            $"muaState must be 'Enabled' or 'Disabled', got '{muaState}'.");

        // enhancedSecurityState is only emitted when Security.EnhancedSecurityState
        // is configured on the vault. Assert that if present it is a non-empty string.
        if (vault.TryGetProperty("enhancedSecurityState", out var enhancedSecurity))
        {
            Assert.False(string.IsNullOrEmpty(enhancedSecurity.GetString()));
        }
    }

    [Fact]
    public async Task VaultGet_WithExpandAll_DppVault_ReturnsPostureFields()
    {
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_vault_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" },
                { "expand", "all" }
            });

        var vault = result.AssertProperty("vaults").EnumerateArray().First();
        Assert.Equal("dpp", vault.AssertProperty("vaultType").GetString());

        // DPP posture: cross-region restore state comes from FeatureSettings and is
        // always emitted on live DPP vaults.
        vault.AssertProperty("crossRegionRestoreState");
        // MUA proxy resource for DPP vaults — must resolve to Enabled or Disabled.
        var muaState = vault.AssertProperty("muaState").GetString();
        Assert.True(muaState is "Enabled" or "Disabled",
            $"muaState must be 'Enabled' or 'Disabled', got '{muaState}'.");

        // encryptionState is only emitted when SecuritySettings.EncryptionSettings
        // is configured (CMK). Assert that if present it is a non-empty string.
        if (vault.TryGetProperty("encryptionState", out var encryptionState))
        {
            Assert.False(string.IsNullOrEmpty(encryptionState.GetString()));
        }
    }
}
