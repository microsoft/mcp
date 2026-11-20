// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests;

public class NetAppFilesCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    string location = "westus";
    int sizeInTiB = 1;
    string serviceLevel = "Standard";

    long tibInBytes = 1_099_511_627_776;

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers => new()
    {
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
        {
            Regex = "tenantId=(?<tenant>[0-9a-fA-F-]{36})",
            GroupForReplace = "tenant",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
        {
            Regex = "objectId=(?<object>[0-9a-fA-F-]{36})",
            GroupForReplace = "object",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
        {
            Regex = "objectId=(?<object>[0-9a-fA-F-]{36})/.*/(?<locationObject>[0-9a-fA-F-]{36})",
            GroupForReplace = "locationObject",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-routing-request-id")
        {
            Regex = ".*:.*:(?<routingId>[0-9a-fA-F-]{36})",
            GroupForReplace = "routingId",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-correlation-request-id")
        {
            Regex = "(?<correlationId>[0-9a-fA-F-]{36})",
            GroupForReplace = "correlationId",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
    };

    public override List<BodyKeySanitizer> BodyKeySanitizers => new()
    {
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..tags.Owner")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..tags.Owners")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..resourceName")
        {
            Regex = "/resourceGroups/(?<resourceGroup>[^/]+)",
            GroupForReplace = "resourceGroup",
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.poolId")
        {
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.resourceName")
        {
            Regex = "/resourceGroups/(?<resourceGroup>[^/]+)",
            GroupForReplace = "resourceGroup",
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..id")
        {
            Regex = "/resourceGroups/(?<resourceGroup>[^/]+)",
            GroupForReplace = "resourceGroup",
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.error.message")
        {
            Regex = "resource group '(?<resourceGroup>[^']+)'",
            GroupForReplace = "resourceGroup",
            Value = "Sanitized"
        }),
    };

    [Fact]
    public async Task AccountGet_ReturnsAccount()
    {
        var accountName = $"{Settings.ResourceBaseName}-account";
        var resourceGroupName = Settings.ResourceGroupName;

        var result = await CallToolAsync(
            "netappfiles_account_get",
            new()
            {
                { "account", accountName },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var accounts = result.AssertProperty("accounts");
        Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
        var account = accounts[0];
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("westus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task AccountGet_NoAccountsFilterReturnsAllAccounts()
    {
        string[] accountNames = [$"{Settings.ResourceBaseName}-account", $"{Settings.ResourceBaseName}-account2"];
        var resourceGroupName = Settings.ResourceGroupName;

        var result = await CallToolAsync(
            "netappfiles_account_get",
            new()
            {
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var accounts = result.AssertProperty("accounts").EnumerateArray().ToArray();
        foreach (var accountName in accountNames)
        {
            var foundAccount = accounts.FirstOrDefault(account => account.AssertProperty("id").GetString()?.EndsWith($"netAppAccounts/{accountName}") ?? false);
            Assert.Equal(JsonValueKind.Object, foundAccount.ValueKind);
            foundAccount.AssertProperty("id");
            Assert.Equal("westus", foundAccount.AssertProperty("location").GetString());
            Assert.Equal("Succeeded", foundAccount.AssertProperty("provisioningState").GetString());
        }

        Assert.True(accounts.Length >= accountNames.Length);
    }

    [Fact]
    public async Task AccountCreate_FailsOnExistingAccount()
    {
        var accountName = $"{Settings.ResourceBaseName}-account";
        var resourceGroupName = Settings.ResourceGroupName;

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

        var message = result.AssertProperty("message");
        Assert.Equal($"Conflict: NetApp account '{accountName}' already exists in resource group '{resourceGroupName}'.", message.GetString());
    }

    [Fact]
    public async Task AccountCreate_ReturnsCreatedAccount()
    {
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"test-Account-{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = Settings.ResourceGroupName;

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
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"test-Account-{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = Settings.ResourceGroupName;
        var updatedTagsString = "{\"recorded-test\":\"account-update\"}";

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
                { "tags", updatedTagsString }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
        Assert.Equal("account-update", account.AssertProperty("tags").AssertProperty("recorded-test").GetString());
    }

    [Fact]
    public async Task PoolCreate_ReturnsCreatedPool()
    {
        var accountName = $"{Settings.ResourceBaseName}-account";
        var poolName = RegisterOrRetrieveVariable("createdNetAppPool", $"test-Pool-{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = Settings.ResourceGroupName;

        var result = await CallToolAsync(
            "netappfiles_pool_create",
            new()
            {
                { "account", accountName },
                { "pool", poolName },
                { "size", sizeInTiB },
                { "service-level", serviceLevel },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var pool = result.AssertProperty("pool");
        Assert.Equal(JsonValueKind.Object, pool.ValueKind);
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal(location, pool.AssertProperty("location").GetString());
        Assert.Equal(1_099_511_627_776, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal(serviceLevel, pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task PoolGet_ReturnsPool()
    {
        var accountName = $"{Settings.ResourceBaseName}-account";
        var poolName = $"{Settings.ResourceBaseName}-pool";
        var resourceGroupName = Settings.ResourceGroupName;

        var result = await CallToolAsync(
            "netappfiles_pool_get",
            new()
            {
                { "account", accountName },
                { "pool", poolName },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var pools = result.AssertProperty("pools");
        Assert.Equal(JsonValueKind.Array, pools.ValueKind);
        var pool = pools[0];
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal(location, pool.AssertProperty("location").GetString());
        Assert.Equal(1_099_511_627_776, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal(serviceLevel, pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task PoolGet_NoPoolsFilterReturnsAllPools()
    {
        var accountName = $"{Settings.ResourceBaseName}-account";
        (string Name, int SizeInTib)[] poolInfos = [($"{Settings.ResourceBaseName}-pool", 1), ($"{Settings.ResourceBaseName}-pool2", 2)];
        var resourceGroupName = Settings.ResourceGroupName;

        var result = await CallToolAsync(
            "netappfiles_pool_get",
            new()
            {
                { "account", accountName },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var poolsResult = result.AssertProperty("pools");
        Assert.Equal(JsonValueKind.Array, poolsResult.ValueKind);
        var pools = poolsResult.EnumerateArray().ToArray();
        foreach (var poolInfo in poolInfos)
        {
            var foundPool = pools.FirstOrDefault(pool => pool.AssertProperty("id").GetString()?.EndsWith($"capacityPools/{poolInfo.Name}") ?? false);
            Assert.Equal(JsonValueKind.Object, foundPool.ValueKind);
            foundPool.AssertProperty("id");
            Assert.Equal("westus", foundPool.AssertProperty("location").GetString());
            Assert.Equal(poolInfo.SizeInTib * tibInBytes, foundPool.AssertProperty("sizeInBytes").GetInt64());
            Assert.Equal("Succeeded", foundPool.AssertProperty("provisioningState").GetString());
        }

        Assert.True(pools.Length >= poolInfos.Length);
    }

    [Fact]
    public async Task PoolUpdate_ReturnsUpdatedPool()
    {
        var accountName = $"{Settings.ResourceBaseName}-account";
        var poolName = RegisterOrRetrieveVariable("createdNetAppPool", $"test-Pool-{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = Settings.ResourceGroupName;

        await CallToolAsync(
            "netappfiles_pool_create",
            new()
            {
                { "account", accountName },
                { "pool", poolName },
                { "size", sizeInTiB },
                { "service-level", serviceLevel },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_pool_update",
            new()
            {
                { "account", accountName },
                { "pool", poolName },
                { "tags", "{\"recorded-test\":\"pool-update\"}" },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var pool = result.AssertProperty("pool");
        Assert.Equal(JsonValueKind.Object, pool.ValueKind);
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal(location, pool.AssertProperty("location").GetString());
        Assert.Equal(1_099_511_627_776, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal(serviceLevel, pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("pool-update", pool.AssertProperty("tags").AssertProperty("recorded-test").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }
}
