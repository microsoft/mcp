// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Redis.LiveTests;

public class RedisCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public override List<BodyKeySanitizer> BodyKeySanitizers { get; } = new List<BodyKeySanitizer>
    {
        new(new BodyKeySanitizerBody("$..displayName")
        {
            Value = "Sanitized"
        }),
    };

    public override List<BodyRegexSanitizer> BodyRegexSanitizers => new()
    {
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = "\"domains\"\\s*:\\s*\\[(?s)(?<domains>.*?)\\]",
          GroupForReplace = "domains",
          Value = "\"contoso.com\""
        })
    };

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_id()
    {
        var result = await CallToolAsync(
            "redis_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var caches = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_name()
    {
        var result = await CallToolAsync(
            "redis_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var caches = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_id_with_tenant_id()
    {
        var result = await CallToolAsync(
            "redis_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var caches = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_by_subscription_id_with_tenant_name()
    {
        Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

        var result = await CallToolAsync(
            "redis_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantName }
            });

        var caches = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }

    [Fact]
    public async Task Should_list_redis_caches_with_retry_policy()
    {
        var result = await CallToolAsync(
            "redis_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "retry-max-retries", 3 },
                { "retry-delay-seconds", 2 }
            });

        var caches = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, caches.ValueKind);
    }
}
