// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.DeviceRegistry.LiveTests;

public class DeviceRegistryCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_list_deviceregistry_namespaces_by_subscription()
    {
        var result = await CallToolAsync(
            "deviceregistry_namespace_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var namespaces = result.AssertProperty("namespaces");
        Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);
        Assert.NotEmpty(namespaces.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_deviceregistry_namespaces_by_subscription_name()
    {
        var result = await CallToolAsync(
            "deviceregistry_namespace_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var namespaces = result.AssertProperty("namespaces");
        Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);
        Assert.NotEmpty(namespaces.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_deviceregistry_namespaces_by_resource_group()
    {
        var result = await CallToolAsync(
            "deviceregistry_namespace_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var namespaces = result.AssertProperty("namespaces");
        Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);
        Assert.NotEmpty(namespaces.EnumerateArray());

        foreach (var ns in namespaces.EnumerateArray())
        {
            var name = ns.GetProperty("name");
            Assert.NotNull(name.GetString());

            var location = ns.GetProperty("location");
            Assert.NotNull(location.GetString());
        }
    }

    [Fact]
    public async Task Should_list_deviceregistry_namespaces_with_tenant_id()
    {
        var result = await CallToolAsync(
            "deviceregistry_namespace_list",
            new()
            {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId }
            });

        var namespaces = result.AssertProperty("namespaces");
        Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);
        Assert.NotEmpty(namespaces.EnumerateArray());
    }
}
