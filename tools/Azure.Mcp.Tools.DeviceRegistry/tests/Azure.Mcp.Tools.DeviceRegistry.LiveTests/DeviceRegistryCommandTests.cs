// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
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
    public async Task Should_return_namespace_with_required_properties()
    {
        var result = await CallToolAsync(
            "deviceregistry_namespace_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var namespaces = result.AssertProperty("namespaces");
        Assert.NotEmpty(namespaces.EnumerateArray());

        // Verify each namespace has all expected properties
        foreach (var ns in namespaces.EnumerateArray())
        {
            // Required: name, id, location
            Assert.True(ns.TryGetProperty("name", out var name));
            Assert.NotEmpty(name.GetString()!);

            Assert.True(ns.TryGetProperty("id", out var id));
            Assert.Contains("deviceregistry/namespaces", id.GetString()!, StringComparison.OrdinalIgnoreCase);

            Assert.True(ns.TryGetProperty("location", out var location));
            Assert.NotEmpty(location.GetString()!);

            // Additional properties: provisioningState, type, resourceGroup
            Assert.True(ns.TryGetProperty("provisioningState", out var state));
            Assert.NotNull(state.GetString());

            Assert.True(ns.TryGetProperty("type", out var type));
            Assert.Contains("deviceregistry", type.GetString()!, StringComparison.OrdinalIgnoreCase);

            Assert.True(ns.TryGetProperty("resourceGroup", out var rg));
            Assert.NotNull(rg.GetString());
        }
    }

    [Fact]
    public async Task Should_return_namespace_with_uuid()
    {
        var result = await CallToolAsync(
            "deviceregistry_namespace_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var namespaces = result.AssertProperty("namespaces");
        Assert.NotEmpty(namespaces.EnumerateArray());

        // Verify namespace has uuid property (unique identifier)
        foreach (var ns in namespaces.EnumerateArray())
        {
            Assert.True(ns.TryGetProperty("uuid", out var uuid));
            Assert.NotNull(uuid.GetString());
            // UUID should be a valid GUID format
            Assert.True(Guid.TryParse(uuid.GetString(), out _), "UUID should be a valid GUID");
        }
    }
}
