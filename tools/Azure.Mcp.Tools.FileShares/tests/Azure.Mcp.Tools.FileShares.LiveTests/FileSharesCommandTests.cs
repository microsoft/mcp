// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.LiveTests;

/// <summary>
/// Live tests for FileShares commands.
/// These tests exercise the actual Azure FileShares resource provider with real resources.
/// </summary>
public class FileSharesCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public override List<BodyRegexSanitizer> BodyRegexSanitizers => [
        // Sanitizes all URLs to remove actual service names
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
          GroupForReplace = "host",
        })
    ];

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
        Assert.NotEmpty(fileShares.EnumerateArray());
    }

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
        Assert.NotEqual(JsonValueKind.Null, fileShare.ValueKind);

        var name = fileShare.GetProperty("name");
        Assert.Equal(Settings.ResourceBaseName, name.GetString());

        var location = fileShare.GetProperty("location");
        Assert.NotEqual(JsonValueKind.Null, location.ValueKind);

        var provisioningState = fileShare.GetProperty("provisioningState");
        Assert.NotEqual(JsonValueKind.Null, provisioningState.ValueKind);
    }

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
        Assert.NotEqual(JsonValueKind.Null, fileShare.ValueKind);

        var name = fileShare.GetProperty("name");
        Assert.Equal(Settings.ResourceBaseName, name.GetString());
    }

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
        Assert.NotEqual(JsonValueKind.Null, fileShare.ValueKind);

        var name = fileShare.GetProperty("name");
        Assert.Equal(Settings.ResourceBaseName, name.GetString());
    }

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
        Assert.NotEmpty(snapshots.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_file_share_snapshot_details()
    {
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
            Assert.NotEqual(JsonValueKind.Null, snapshot.ValueKind);

            var name = snapshot.GetProperty("name");
            Assert.Equal(snapshotName, name.GetString());
        }
    }

    [Fact]
    public async Task Should_get_file_share_usage_data()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get_usage_data",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var usageData = result.AssertProperty("usageData");
        Assert.NotEqual(JsonValueKind.Null, usageData.ValueKind);
    }

    private new const string TenantNameReason = "Tenant name resolution is not supported for service principals";
}
