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
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"testacct{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", accountName },
                { "location", "eastus" },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_account_get",
            new()
            {
                { "account", accountName },
                { "resource-group", resourceGroupName },
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
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"testacct{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", accountName },
                { "location", "eastus" },
                { "resource-group", resourceGroupName },
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
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"testacct{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", accountName },
                { "location", "eastus" },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_account_update",
            new()
            {
                { "account", accountName },
                { "resource-group", resourceGroupName },
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
}