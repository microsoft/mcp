// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.LiveTests;

/// <summary>
/// Live tests for FileShares commands.
/// These tests exercise the actual Azure FileShares resource provider with real resources.
/// </summary>
public class FileSharesCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    /// <summary>
    /// Tests listing file shares by subscription ID.
    /// </summary>
    [Fact]
    public async Task Should_list_file_shares_by_subscription_id()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var fileShares = result.AssertProperty("fileShares");
        Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);
        Assert.NotEmpty(fileShares.EnumerateArray());
    }

    /// <summary>
    /// Tests listing file shares by subscription name.
    /// </summary>
    [Fact]
    public async Task Should_list_file_shares_by_subscription_name()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var fileShares = result.AssertProperty("fileShares");
        Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);
        Assert.NotEmpty(fileShares.EnumerateArray());
    }

    /// <summary>
    /// Tests listing file shares by resource group.
    /// </summary>
    [Fact]
    public async Task Should_list_file_shares_by_resource_group()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceGroupName }
            });

        var fileShares = result.AssertProperty("fileShares");
        Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);
        // File shares should exist in the test resource group
        Assert.NotEmpty(fileShares.EnumerateArray());
    }

    /// <summary>
    /// Tests getting a specific file share by name.
    /// </summary>
    [Fact]
    public async Task Should_get_file_share_details_by_subscription_and_name()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var fileShare = result.AssertProperty("fileShare");
        Assert.NotNull(fileShare);

        var name = fileShare.GetProperty("name");
        Assert.NotNull(name.GetString());

        var location = fileShare.GetProperty("location");
        Assert.NotNull(location.GetString());

        var provisioningState = fileShare.GetProperty("provisioningState");
        Assert.NotNull(provisioningState.GetString());
    }

    /// <summary>
    /// Tests getting file share details with tenant ID.
    /// </summary>
    [Fact]
    public async Task Should_get_file_share_details_with_tenant_id()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "tenant", Settings.TenantId }
            });

        var fileShare = result.AssertProperty("fileShare");
        Assert.NotNull(fileShare);

        var name = fileShare.GetProperty("name");
        Assert.NotNull(name.GetString());
    }

    /// <summary>
    /// Tests getting file share details with tenant name (if not service principal).
    /// </summary>
    [Fact]
    public async Task Should_get_file_share_details_with_tenant_name()
    {
        Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

        var result = await CallToolAsync(
            "fileshares_fileshare_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "tenant", Settings.TenantName }
            });

        var fileShare = result.AssertProperty("fileShare");
        Assert.NotNull(fileShare);

        var name = fileShare.GetProperty("name");
        Assert.NotNull(name.GetString());
    }

    /// <summary>
    /// Tests listing file share snapshots.
    /// </summary>
    [Fact]
    public async Task Should_list_file_share_snapshots()
    {
        var result = await CallToolAsync(
            "fileshares_snapshot_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceGroupName },
                { "fileShareName", Settings.ResourceBaseName }
            });

        var snapshots = result.AssertProperty("snapshots");
        Assert.Equal(JsonValueKind.Array, snapshots.ValueKind);
        // Snapshots should exist from test resource deployment
        Assert.NotEmpty(snapshots.EnumerateArray());
    }

    /// <summary>
    /// Tests getting a specific file share snapshot.
    /// </summary>
    [Fact]
    public async Task Should_get_file_share_snapshot_details()
    {
        // First list snapshots to get a snapshot name
        var listResult = await CallToolAsync(
            "fileshares_snapshot_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resourceGroup", Settings.ResourceGroupName },
                { "fileShareName", Settings.ResourceBaseName }
            });

        var snapshots = listResult.AssertProperty("snapshots");
        var snapshotArray = snapshots.EnumerateArray().ToList();

        if (snapshotArray.Count > 0)
        {
            var firstSnapshot = snapshotArray[0];
            var snapshotName = firstSnapshot.GetProperty("name").GetString();

            var result = await CallToolAsync(
                "fileshares_snapshot_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resourceGroup", Settings.ResourceGroupName },
                    { "fileShareName", Settings.ResourceBaseName },
                    { "name", snapshotName }
                });

            var snapshot = result.AssertProperty("snapshot");
            Assert.NotNull(snapshot);

            var name = snapshot.GetProperty("name");
            Assert.Equal(snapshotName, name.GetString());
        }
    }

    /// <summary>
    /// Tests getting file share usage data.
    /// </summary>
    [Fact]
    public async Task Should_get_file_share_usage_data()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get_usage_data",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "location", Settings.Location }
            });

        var usageData = result.AssertProperty("usageData");
        Assert.NotNull(usageData);
    }

    /// <summary>
    /// Reason for skipping tenant name test when using service principal.
    /// </summary>
    private const string TenantNameReason = "Tenant name resolution is not supported for service principals";
}
