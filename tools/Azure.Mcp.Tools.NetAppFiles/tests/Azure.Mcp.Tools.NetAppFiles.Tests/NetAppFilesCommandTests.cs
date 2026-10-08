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
    public async Task SnapshotPolicyCreate_ReturnsCreatedSnapshotPolicy()
    {
        var policyName = $"{Settings.ResourceBaseName}-snapshot-policy";
        var result = await CallToolAsync(
            "netappfiles_snapshotpolicy_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "snapshot-policy", policyName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "hourly-minute", 5 },
                { "hourly-snapshots-to-keep", 2 },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var policy = result.AssertProperty("snapshotPolicy");
        Assert.Equal(JsonValueKind.Object, policy.ValueKind);
        Assert.Equal(policyName, policy.AssertProperty("name").GetString());
        policy.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], policy.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", policy.AssertProperty("provisioningState").GetString());
        Assert.True(policy.AssertProperty("enabled").GetBoolean());
        Assert.Equal(5, policy.AssertProperty("hourlyMinute").GetInt32());
        Assert.Equal(2, policy.AssertProperty("hourlySnapshotsToKeep").GetInt32());
    }

    [Fact]
    public async Task SnapshotPolicyGet_ReturnsSnapshotPolicy()
    {
        var policyName = $"{Settings.ResourceBaseName}-get-snapshot-policy";
        await CallToolAsync(
            "netappfiles_snapshotpolicy_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "snapshot-policy", policyName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "hourly-minute", 5 },
                { "hourly-snapshots-to-keep", 2 },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_snapshotpolicy_get",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "snapshot-policy", policyName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var policy = result.AssertProperty("snapshotPolicy");
        Assert.Equal(JsonValueKind.Object, policy.ValueKind);
        Assert.Equal(policyName, policy.AssertProperty("name").GetString());
        policy.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], policy.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", policy.AssertProperty("provisioningState").GetString());
        Assert.True(policy.AssertProperty("enabled").GetBoolean());
        Assert.Equal(5, policy.AssertProperty("hourlyMinute").GetInt32());
        Assert.Equal(2, policy.AssertProperty("hourlySnapshotsToKeep").GetInt32());
    }

    [Fact]
    public async Task SnapshotPolicyUpdate_ReturnsUpdatedSnapshotPolicy()
    {
        var policyName = $"{Settings.ResourceBaseName}-update-snapshot-policy";
        await CallToolAsync(
            "netappfiles_snapshotpolicy_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "snapshot-policy", policyName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "hourly-minute", 5 },
                { "hourly-snapshots-to-keep", 2 },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_snapshotpolicy_update",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "snapshot-policy", policyName },
                { "enabled", false },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var policy = result.AssertProperty("snapshotPolicy");
        Assert.Equal(JsonValueKind.Object, policy.ValueKind);
        Assert.Equal(policyName, policy.AssertProperty("name").GetString());
        policy.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], policy.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", policy.AssertProperty("provisioningState").GetString());
        Assert.False(policy.AssertProperty("enabled").GetBoolean());
        Assert.Equal(5, policy.AssertProperty("hourlyMinute").GetInt32());
        Assert.Equal(2, policy.AssertProperty("hourlySnapshotsToKeep").GetInt32());
    }

    [Fact]
    public async Task SnapshotCreate_ReturnsCreatedSnapshot()
    {
        var volumeName = $"{Settings.ResourceBaseName}-snapshot-volume";
        var snapshotName = $"{Settings.ResourceBaseName}-snapshot";
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
            "netappfiles_snapshot_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "snapshot", snapshotName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var snapshot = result.AssertProperty("snapshot");
        Assert.Equal(JsonValueKind.Object, snapshot.ValueKind);
        Assert.Equal(snapshotName, snapshot.AssertProperty("name").GetString());
        snapshot.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], snapshot.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", snapshot.AssertProperty("provisioningState").GetString());
        snapshot.AssertProperty("snapshotId");
        snapshot.AssertProperty("created");
    }

    [Fact]
    public async Task SnapshotGet_ReturnsSnapshot()
    {
        var volumeName = $"{Settings.ResourceBaseName}-get-snapshot-volume";
        var snapshotName = $"{Settings.ResourceBaseName}-get-snapshot";
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

        await CallToolAsync(
            "netappfiles_snapshot_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "snapshot", snapshotName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_snapshot_get",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "snapshot", snapshotName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var snapshot = result.AssertProperty("snapshot");
        Assert.Equal(JsonValueKind.Object, snapshot.ValueKind);
        Assert.Equal(snapshotName, snapshot.AssertProperty("name").GetString());
        snapshot.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], snapshot.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", snapshot.AssertProperty("provisioningState").GetString());
        snapshot.AssertProperty("snapshotId");
        snapshot.AssertProperty("created");
    }

    [Fact]
    public async Task SnapshotUpdate_ReturnsUpdatedSnapshot()
    {
        var volumeName = $"{Settings.ResourceBaseName}-update-snapshot-volume";
        var snapshotName = $"{Settings.ResourceBaseName}-update-snapshot";
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

        await CallToolAsync(
            "netappfiles_snapshot_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "snapshot", snapshotName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_snapshot_update",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "snapshot", snapshotName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var snapshot = result.AssertProperty("snapshot");
        Assert.Equal(JsonValueKind.Object, snapshot.ValueKind);
        Assert.Equal(snapshotName, snapshot.AssertProperty("name").GetString());
        snapshot.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], snapshot.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", snapshot.AssertProperty("provisioningState").GetString());
        snapshot.AssertProperty("snapshotId");
        snapshot.AssertProperty("created");
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
