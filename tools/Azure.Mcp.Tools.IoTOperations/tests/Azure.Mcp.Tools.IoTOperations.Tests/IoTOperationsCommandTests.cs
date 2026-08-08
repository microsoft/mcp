// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.IoTOperations.Tests;

public class IoTOperationsCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    [LiveTestOnly]
    public async Task Should_list_iotoperations_instances_by_subscription()
    {
        var result = await CallToolAsync(
            "iotoperations_instance_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var instances = result.AssertProperty("instances");
        Assert.Equal(JsonValueKind.Array, instances.ValueKind);
        Assert.NotEmpty(instances.EnumerateArray());
    }

    [Fact]
    [LiveTestOnly]
    public async Task Should_return_instance_with_required_properties()
    {
        var result = await CallToolAsync(
            "iotoperations_instance_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var instances = result.AssertProperty("instances");
        Assert.NotEmpty(instances.EnumerateArray());

        foreach (var instance in instances.EnumerateArray())
        {
            Assert.True(instance.TryGetProperty("name", out var name));
            Assert.NotEmpty(name.GetString()!);

            Assert.True(instance.TryGetProperty("id", out var id));
            Assert.Contains("iotoperations/instances", id.GetString()!, StringComparison.OrdinalIgnoreCase);

            Assert.True(instance.TryGetProperty("location", out var location));
            Assert.NotEmpty(location.GetString()!);

            Assert.True(instance.TryGetProperty("type", out var type));
            Assert.Contains("iotoperations", type.GetString()!, StringComparison.OrdinalIgnoreCase);

            Assert.True(instance.TryGetProperty("resourceGroup", out var rg));
            Assert.NotNull(rg.GetString());
        }
    }

    [Fact]
    [LiveTestOnly]
    public async Task Should_get_iotoperations_instance()
    {
        var listResult = await CallToolAsync(
            "iotoperations_instance_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var instances = listResult.AssertProperty("instances");
        var first = instances.EnumerateArray().First();
        var name = first.GetProperty("name").GetString();
        var resourceGroup = first.GetProperty("resourceGroup").GetString();

        var result = await CallToolAsync(
            "iotoperations_instance_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "resource-group", resourceGroup! },
                { "instance", name! }
            });

        var instance = result.AssertProperty("instance");
        Assert.Equal(JsonValueKind.Object, instance.ValueKind);
        Assert.Equal(name, instance.GetProperty("name").GetString());
    }
}
