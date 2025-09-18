// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Redis.LiveTests;

public class RedisCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_redis_caches_by_subscription_id()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cache_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var caches = result.AssertProperty("caches");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_name()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cache_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var caches = result.AssertProperty("caches");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_id_with_tenant_id()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cache_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var caches = result.AssertProperty("caches");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_id_with_tenant_name()
    {
        Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

        var result = await CallToolAsync(
            "azmcp_redis_cache_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantName }
            });

        var caches = result.AssertProperty("caches");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_with_retry_policy()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cache_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "retry-max-retries", 3 },
                { "retry-delay-seconds", 2 }
            });

        var caches = result.AssertProperty("caches");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_clusters_by_subscription_id()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cluster_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = result.AssertProperty("clusters");
        Assert.Equal(JsonValueKind.Array, clusters.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_clusters_by_subscription_name()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cluster_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var clusters = result.AssertProperty("clusters");
        Assert.Equal(JsonValueKind.Array, clusters.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_clusters_by_subscription_id_with_tenant_id()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cluster_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var clusters = result.AssertProperty("clusters");
        Assert.Equal(JsonValueKind.Array, clusters.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_access_policies()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cache_accesspolicy_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "cache", Settings.ResourceBaseName }
            });

        var policies = result.AssertProperty("accessPolicyAssignments");
        Assert.Equal(JsonValueKind.Array, policies.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_access_policies_with_tenant_id()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cache_accesspolicy_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "cache", Settings.ResourceBaseName },
                { "tenant", Settings.TenantId }
            });

        var policies = result.AssertProperty("accessPolicyAssignments");
        Assert.Equal(JsonValueKind.Array, policies.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_databases()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cluster_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "cluster", Settings.ResourceBaseName }
            });

        var databases = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_databases_with_retry_policy()
    {
        var result = await CallToolAsync(
            "azmcp_redis_cluster_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "cluster", Settings.ResourceBaseName },
                { "retry-max-retries", 2 },
                { "retry-delay-seconds", 1 }
            });

        var databases = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);
    }

}
