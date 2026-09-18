// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests;

public class NetAppFilesCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task AccountGet_ReturnsAccount()
    {
        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "location", "eastus" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_account_get",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task AccountCreate_ReturnsCreatedAccount()
    {
        var result = await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "location", "eastus" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task AccountUpdate_ReturnsUpdatedAccount()
    {
        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "location", "eastus" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_account_update",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "tags", "{\"recorded-test\":\"account-update\"}" }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task BackupPolicyCreate_ReturnsCreatedBackupPolicy()
    {
        var backupPolicyName = $"{Settings.ResourceBaseName}-backup-policy";
        var result = await CallToolAsync(
            "netappfiles_backuppolicy_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-policy", backupPolicyName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "daily-backups-to-keep", 2 },
                { "weekly-backups-to-keep", 1 },
                { "monthly-backups-to-keep", 1 },
                { "enabled", true },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var backupPolicy = result.AssertProperty("backupPolicy");
        Assert.Equal(JsonValueKind.Object, backupPolicy.ValueKind);
        Assert.Equal(backupPolicyName, backupPolicy.AssertProperty("name").GetString());
        backupPolicy.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], backupPolicy.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", backupPolicy.AssertProperty("provisioningState").GetString());
        Assert.Equal(2, backupPolicy.AssertProperty("dailyBackupsToKeep").GetInt32());
        Assert.Equal(1, backupPolicy.AssertProperty("weeklyBackupsToKeep").GetInt32());
        Assert.Equal(1, backupPolicy.AssertProperty("monthlyBackupsToKeep").GetInt32());
        Assert.True(backupPolicy.AssertProperty("isEnabled").GetBoolean());
    }

    [Fact]
    public async Task BackupPolicyGet_ReturnsBackupPolicy()
    {
        var backupPolicyName = $"{Settings.ResourceBaseName}-get-backup-policy";
        await CallToolAsync(
            "netappfiles_backuppolicy_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-policy", backupPolicyName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "daily-backups-to-keep", 2 },
                { "weekly-backups-to-keep", 1 },
                { "monthly-backups-to-keep", 1 },
                { "enabled", true },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_backuppolicy_get",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-policy", backupPolicyName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var backupPolicy = result.AssertProperty("backupPolicy");
        Assert.Equal(JsonValueKind.Object, backupPolicy.ValueKind);
        Assert.Equal(backupPolicyName, backupPolicy.AssertProperty("name").GetString());
        backupPolicy.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], backupPolicy.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", backupPolicy.AssertProperty("provisioningState").GetString());
        Assert.Equal(2, backupPolicy.AssertProperty("dailyBackupsToKeep").GetInt32());
        Assert.Equal(1, backupPolicy.AssertProperty("weeklyBackupsToKeep").GetInt32());
        Assert.Equal(1, backupPolicy.AssertProperty("monthlyBackupsToKeep").GetInt32());
        Assert.True(backupPolicy.AssertProperty("isEnabled").GetBoolean());
    }

    [Fact]
    public async Task BackupPolicyUpdate_ReturnsUpdatedBackupPolicy()
    {
        var backupPolicyName = $"{Settings.ResourceBaseName}-update-backup-policy";
        await CallToolAsync(
            "netappfiles_backuppolicy_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-policy", backupPolicyName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "daily-backups-to-keep", 2 },
                { "weekly-backups-to-keep", 1 },
                { "monthly-backups-to-keep", 1 },
                { "enabled", true },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_backuppolicy_update",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-policy", backupPolicyName },
                { "daily-backups-to-keep", 3 },
                { "enabled", false },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var backupPolicy = result.AssertProperty("backupPolicy");
        Assert.Equal(JsonValueKind.Object, backupPolicy.ValueKind);
        Assert.Equal(backupPolicyName, backupPolicy.AssertProperty("name").GetString());
        backupPolicy.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], backupPolicy.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", backupPolicy.AssertProperty("provisioningState").GetString());
        Assert.Equal(3, backupPolicy.AssertProperty("dailyBackupsToKeep").GetInt32());
        Assert.Equal(1, backupPolicy.AssertProperty("weeklyBackupsToKeep").GetInt32());
        Assert.Equal(1, backupPolicy.AssertProperty("monthlyBackupsToKeep").GetInt32());
        Assert.False(backupPolicy.AssertProperty("isEnabled").GetBoolean());
    }

    [Fact]
    public async Task BackupVaultCreate_ReturnsCreatedBackupVault()
    {
        var backupVaultName = $"{Settings.ResourceBaseName}-backup-vault";
        var result = await CallToolAsync(
            "netappfiles_backupvault_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-vault", backupVaultName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var backupVault = result.AssertProperty("backupVault");
        Assert.Equal(JsonValueKind.Object, backupVault.ValueKind);
        Assert.Equal(backupVaultName, backupVault.AssertProperty("name").GetString());
        backupVault.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], backupVault.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", backupVault.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task BackupVaultGet_ReturnsBackupVault()
    {
        var backupVaultName = $"{Settings.ResourceBaseName}-get-backup-vault";
        await CallToolAsync(
            "netappfiles_backupvault_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-vault", backupVaultName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_backupvault_get",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-vault", backupVaultName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var backupVault = result.AssertProperty("backupVault");
        Assert.Equal(JsonValueKind.Object, backupVault.ValueKind);
        Assert.Equal(backupVaultName, backupVault.AssertProperty("name").GetString());
        backupVault.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], backupVault.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", backupVault.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task BackupVaultUpdate_ReturnsUpdatedBackupVault()
    {
        var backupVaultName = $"{Settings.ResourceBaseName}-update-backup-vault";
        await CallToolAsync(
            "netappfiles_backupvault_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-vault", backupVaultName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_backupvault_update",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "backup-vault", backupVaultName },
                { "tags", "{\"recorded-test\":\"backup-vault-update\"}" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var backupVault = result.AssertProperty("backupVault");
        Assert.Equal(JsonValueKind.Object, backupVault.ValueKind);
        Assert.Equal(backupVaultName, backupVault.AssertProperty("name").GetString());
        backupVault.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], backupVault.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", backupVault.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task VolumeCreate_ReturnsCreatedVolume()
    {
        var result = await CallToolAsync(
            "netappfiles_volume_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", $"{Settings.ResourceBaseName}-volume" },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "subnet-id", Settings.DeploymentOutputs["NETAPP_SUBNET_ID"] },
                { "quota-gib", 100 },
                { "service-level", "Standard" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volume = result.AssertProperty("volume");
        Assert.Equal(JsonValueKind.Object, volume.ValueKind);
        Assert.Equal($"{Settings.ResourceBaseName}-volume", volume.AssertProperty("name").GetString());
        volume.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], volume.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volume.AssertProperty("provisioningState").GetString());
        Assert.Equal(100, volume.AssertProperty("quotaGib").GetInt64());
        Assert.Equal("Standard", volume.AssertProperty("serviceLevel").GetString());
    }

    [Fact]
    public async Task VolumeGet_ReturnsVolume()
    {
        var volumeName = $"{Settings.ResourceBaseName}-get-volume";
        await CallToolAsync(
            "netappfiles_volume_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "subnet-id", Settings.DeploymentOutputs["NETAPP_SUBNET_ID"] },
                { "quota-gib", 100 },
                { "service-level", "Standard" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_volume_get",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volume = result.AssertProperty("volume");
        Assert.Equal(JsonValueKind.Object, volume.ValueKind);
        Assert.Equal(volumeName, volume.AssertProperty("name").GetString());
        volume.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], volume.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volume.AssertProperty("provisioningState").GetString());
        Assert.Equal(100, volume.AssertProperty("quotaGib").GetInt64());
        Assert.Equal("Standard", volume.AssertProperty("serviceLevel").GetString());
    }

    [Fact]
    public async Task VolumeUpdate_ReturnsUpdatedVolume()
    {
        var volumeName = $"{Settings.ResourceBaseName}-update-volume";
        await CallToolAsync(
            "netappfiles_volume_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "subnet-id", Settings.DeploymentOutputs["NETAPP_SUBNET_ID"] },
                { "quota-gib", 100 },
                { "service-level", "Standard" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_volume_update",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "quota-gib", 200 },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volume = result.AssertProperty("volume");
        Assert.Equal(JsonValueKind.Object, volume.ValueKind);
        Assert.Equal(volumeName, volume.AssertProperty("name").GetString());
        volume.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], volume.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volume.AssertProperty("provisioningState").GetString());
        Assert.Equal(200, volume.AssertProperty("quotaGib").GetInt64());
        Assert.Equal("Standard", volume.AssertProperty("serviceLevel").GetString());
    }
}
