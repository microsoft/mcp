// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.LiveTests;

public class EventHubsCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)

{

    [Fact]
    public async Task Should_ListNamespaces_Successfully()
    {
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        // Should successfully retrieve the list of namespaces
        var namespaces = result.AssertProperty("namespaces");
        Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);

        // Should contain at least our test namespace
        var namespaceArray = namespaces.EnumerateArray().ToList();
        Assert.True(namespaceArray.Count >= 1, "Should contain at least our test EventHubs namespace");

        // Verify that our test namespace exists
        var testNamespace = namespaceArray.FirstOrDefault(ns =>
            ns.GetProperty("name").GetString() == Settings.ResourceBaseName);
        Assert.NotEqual(default, testNamespace);

        // Verify namespace properties
        if (testNamespace.ValueKind != JsonValueKind.Undefined)
        {
            var nsName = testNamespace.GetProperty("name").GetString();
            Assert.Equal(Settings.ResourceBaseName, nsName);

            var nsId = testNamespace.GetProperty("id").GetString();
            Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", nsId);
            Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", nsId);
            Assert.Contains("/providers/Microsoft.EventHub/namespaces/", nsId);
            Assert.Contains(Settings.ResourceBaseName, nsId);

            var nsResourceGroup = testNamespace.GetProperty("resourceGroup").GetString();
            Assert.Equal(Settings.ResourceGroupName, nsResourceGroup);
        }
    }

    [Fact]
    public async Task Should_HandleEmptyResourceGroup_Gracefully()
    {
        // Test with a resource group that doesn't have EventHubs namespaces
        var emptyResourceGroupName = $"empty-rg-{Guid.NewGuid():N}[..10]";

        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", emptyResourceGroupName }
            });

        // Should successfully return an empty array or handle the non-existent resource group
        // Note: This might return an empty array or an error depending on how the service handles missing resource groups
        if (result.HasValue && result.Value.TryGetProperty("namespaces", out var namespaces))
        {
            Assert.Equal(JsonValueKind.Array, namespaces.ValueKind);
            var namespaceArray = namespaces.EnumerateArray().ToList();
            Assert.Empty(namespaceArray);
        }
        // If it returns an error instead, that's also acceptable behavior
    }

    [Fact]
    public async Task Should_GetSingleNamespaceWithComprehensiveMetadata_Successfully()
    {
        // Test getting a single namespace by name and resource group
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace-name", Settings.ResourceBaseName }
            });

        // Should successfully retrieve the single namespace with detailed metadata
        var namespaceData = result.AssertProperty("namespace");
        Assert.Equal(JsonValueKind.Object, namespaceData.ValueKind);

        // Verify basic properties
        var name = namespaceData.GetProperty("name").GetString();
        Assert.Equal(Settings.ResourceBaseName, name);

        var id = namespaceData.GetProperty("id").GetString();
        Assert.Contains($"/subscriptions/{Settings.SubscriptionId}", id);
        Assert.Contains($"/resourceGroups/{Settings.ResourceGroupName}", id);
        Assert.Contains("/providers/Microsoft.EventHub/namespaces/", id);
        Assert.Contains(Settings.ResourceBaseName, id);

        var resourceGroup = namespaceData.GetProperty("resourceGroup").GetString();
        Assert.Equal(Settings.ResourceGroupName, resourceGroup);

        // Verify comprehensive metadata fields are present
        Assert.True(namespaceData.TryGetProperty("location", out var location));
        Assert.NotNull(location.GetString());
        Assert.False(string.IsNullOrEmpty(location.GetString()));

        Assert.True(namespaceData.TryGetProperty("status", out var status));
        Assert.NotNull(status.GetString());

        Assert.True(namespaceData.TryGetProperty("provisioningState", out var provisioningState));
        Assert.NotNull(provisioningState.GetString());

        // Verify SKU information is present and detailed
        Assert.True(namespaceData.TryGetProperty("sku", out var sku));
        Assert.Equal(JsonValueKind.Object, sku.ValueKind);

        Assert.True(sku.TryGetProperty("name", out var skuName));
        Assert.NotNull(skuName.GetString());

        Assert.True(sku.TryGetProperty("tier", out var skuTier));
        Assert.NotNull(skuTier.GetString());

        // Verify timestamps are present
        Assert.True(namespaceData.TryGetProperty("creationTime", out var creationTime));
        Assert.NotEqual(JsonValueKind.Null, creationTime.ValueKind);

        // Verify service endpoint is present
        Assert.True(namespaceData.TryGetProperty("serviceBusEndpoint", out var serviceBusEndpoint));
        Assert.NotNull(serviceBusEndpoint.GetString());
        Assert.Contains(".servicebus.windows.net", serviceBusEndpoint.GetString());

        // Verify metric ID is present
        Assert.True(namespaceData.TryGetProperty("metricId", out var metricId));
        Assert.NotNull(metricId.GetString());
        Assert.Contains(Settings.SubscriptionId, metricId.GetString());
        Assert.Contains(Settings.ResourceBaseName, metricId.GetString());

        // Verify feature flags are present (even if false/null)
        Assert.True(namespaceData.TryGetProperty("isAutoInflateEnabled", out _));
        Assert.True(namespaceData.TryGetProperty("kafkaEnabled", out _));
        Assert.True(namespaceData.TryGetProperty("zoneRedundant", out _));

        // Verify tags property exists (may be null or empty)
        Assert.True(namespaceData.TryGetProperty("tags", out _));
    }

    
}
