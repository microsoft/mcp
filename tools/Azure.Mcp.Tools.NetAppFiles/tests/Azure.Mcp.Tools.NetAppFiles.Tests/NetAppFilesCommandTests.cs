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
    public async Task PoolCreate_ReturnsCreatedPool()
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
            "netappfiles_pool_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "pool", $"{Settings.ResourceBaseName}-pool" },
                { "size", 4 },
                { "service-level", "Premium" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var pool = result.AssertProperty("pool");
        Assert.Equal(JsonValueKind.Object, pool.ValueKind);
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal("eastus", pool.AssertProperty("location").GetString());
        Assert.Equal(4_398_046_511_104, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal("Premium", pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task PoolGet_ReturnsPool()
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

        await CallToolAsync(
            "netappfiles_pool_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "pool", $"{Settings.ResourceBaseName}-pool" },
                { "size", 4 },
                { "service-level", "Premium" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_pool_get",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "pool", $"{Settings.ResourceBaseName}-pool" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var pool = result.AssertProperty("pool");
        Assert.Equal(JsonValueKind.Object, pool.ValueKind);
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal("eastus", pool.AssertProperty("location").GetString());
        Assert.Equal(4_398_046_511_104, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal("Premium", pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task PoolUpdate_ReturnsUpdatedPool()
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

        await CallToolAsync(
            "netappfiles_pool_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "pool", $"{Settings.ResourceBaseName}-pool" },
                { "size", 4 },
                { "service-level", "Premium" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_pool_update",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "pool", $"{Settings.ResourceBaseName}-pool" },
                { "tags", "{\"recorded-test\":\"pool-update\"}" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var pool = result.AssertProperty("pool");
        Assert.Equal(JsonValueKind.Object, pool.ValueKind);
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal("eastus", pool.AssertProperty("location").GetString());
        Assert.Equal(4_398_046_511_104, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal("Premium", pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("pool-update", pool.AssertProperty("tags").AssertProperty("recorded-test").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }
}
