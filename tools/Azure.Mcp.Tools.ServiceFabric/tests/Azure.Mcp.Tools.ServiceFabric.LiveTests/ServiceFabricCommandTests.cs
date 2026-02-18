// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.ServiceFabric.LiveTests;

public class ServiceFabricCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_GetNodes_Successfully()
    {
        // Arrange
        var clusterName = Settings.ResourceBaseName;

        var result = await CallToolAsync(
            "servicefabric_managedcluster_node_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "cluster", clusterName }
            });

        // Assert
        var nodes = result.AssertProperty("Nodes");
        Assert.Equal(JsonValueKind.Array, nodes.ValueKind);

        foreach (var node in nodes.EnumerateArray())
        {
            node.AssertProperty("id");
            var properties = node.AssertProperty("properties");
            properties.AssertProperty("Name");

            if (properties.TryGetProperty("Type", out var type))
            {
                Assert.Equal(JsonValueKind.String, type.ValueKind);
            }

            if (properties.TryGetProperty("NodeStatus", out var status))
            {
                Assert.Equal(JsonValueKind.Number, status.ValueKind);
            }
        }
    }
}
