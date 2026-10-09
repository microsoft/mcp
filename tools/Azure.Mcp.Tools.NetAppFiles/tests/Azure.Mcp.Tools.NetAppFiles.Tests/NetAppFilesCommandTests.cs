// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
    int sizeInTiB = 4;
    string serviceLevel = "Standard";

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
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.tags.Owner")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.tags.Owners")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.id")
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
    };

    [Fact]
    public async Task AccountGet_ReturnsAccount()
    {
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"test-Account-{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = Settings.ResourceGroupName;

        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", accountName },
                { "location", location },
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
        Assert.Equal(location, account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
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
                { "location", location },
                { "resource-group", resourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal(location, account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task AccountUpdate_ReturnsUpdatedAccount()
    {
        var accountName = RegisterOrRetrieveVariable("createdNetAppAccount", $"test-Account-{DateTime.UtcNow:MMddHHmmss}");
        var resourceGroupName = Settings.ResourceGroupName;

        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", accountName },
                { "location", location },
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
        Assert.Equal(location, account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
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
        Assert.Equal(4_398_046_511_104, pool.AssertProperty("sizeInBytes").GetInt64());
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

        var pool = result.AssertProperty("pool");
        Assert.Equal(JsonValueKind.Object, pool.ValueKind);
        pool.AssertProperty("name");
        pool.AssertProperty("id");
        Assert.Equal(location, pool.AssertProperty("location").GetString());
        Assert.Equal(4_398_046_511_104, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal(serviceLevel, pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
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
        Assert.Equal(4_398_046_511_104, pool.AssertProperty("sizeInBytes").GetInt64());
        Assert.Equal(serviceLevel, pool.AssertProperty("serviceLevel").GetString());
        Assert.Equal("pool-update", pool.AssertProperty("tags").AssertProperty("recorded-test").GetString());
        Assert.Equal("Succeeded", pool.AssertProperty("provisioningState").GetString());
    }
}
