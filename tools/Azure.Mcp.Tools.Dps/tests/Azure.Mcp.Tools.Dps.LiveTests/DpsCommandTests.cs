// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using System.Text.Json;
using Xunit;

namespace Azure.Mcp.Tools.Dps.LiveTests;

[Trait("Toolset", "Dps")]
[Trait("Category", "Live")]
public class DpsCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_ListDpsInstances_Successfully()
    {
        // Arrange & Act
        var result = await CallToolAsync(
            "azmcp_dps_instance_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        // Assert
        var instances = result.AssertProperty("instances");
        Assert.Equal(JsonValueKind.Array, instances.ValueKind);

        // If instances exist, verify structure
        if (instances.GetArrayLength() > 0)
        {
            var firstInstance = instances[0];
            firstInstance.AssertProperty("name");
            firstInstance.AssertProperty("id");
            firstInstance.AssertProperty("resourceGroup");
            firstInstance.AssertProperty("location");

            // Optional properties
            if (firstInstance.TryGetProperty("provisioningState", out var provisioningState))
            {
                Assert.Equal(JsonValueKind.String, provisioningState.ValueKind);
            }

            if (firstInstance.TryGetProperty("sku", out var sku))
            {
                Assert.Equal(JsonValueKind.String, sku.ValueKind);
            }
        }
    }

    [Fact]
    public async Task Should_ListDpsInstances_WithResourceGroup()
    {
        // Arrange & Act
        var result = await CallToolAsync(
            "azmcp_dps_instance_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        // Assert
        var instances = result.AssertProperty("instances");
        Assert.Equal(JsonValueKind.Array, instances.ValueKind);
    }

}
