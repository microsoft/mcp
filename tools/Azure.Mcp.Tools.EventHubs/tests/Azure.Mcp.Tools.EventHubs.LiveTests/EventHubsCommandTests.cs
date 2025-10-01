// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
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
        Assert.True(namespaceArray.Count >= 1, "Should contain at least our test Event Hubs namespace");

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
    public async Task Should_GetSingleNamespace_Successfully()
    {
        // Test getting a single namespace by name and resource group
        var result = await CallToolAsync(
            "azmcp_eventhubs_namespace_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "namespace", Settings.ResourceBaseName }
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
        var location = namespaceData.AssertProperty("location");
        Assert.False(string.IsNullOrEmpty(location.GetString()));

        Assert.True(namespaceData.TryGetProperty("status", out var status));
        Assert.False(string.IsNullOrEmpty(status.GetString()));

        Assert.True(namespaceData.TryGetProperty("provisioningState", out var provisioningState));
        Assert.False(string.IsNullOrEmpty(provisioningState.GetString()));

        // Verify SKU information is present and detailed
        var sku = namespaceData.AssertProperty("sku");
        Assert.Equal(JsonValueKind.Object, sku.ValueKind);

        var skuName = sku.AssertProperty("name");
        Assert.False(string.IsNullOrEmpty(skuName.GetString()));

        var skuTier = sku.AssertProperty("tier");
        Assert.False(string.IsNullOrEmpty(skuTier.GetString()));

        // Verify timestamps are present
        var creationTime = namespaceData.AssertProperty("creationTime");
        Assert.NotEqual(JsonValueKind.Null, creationTime.ValueKind);

        // Verify service endpoint is present
        var serviceBusEndpoint = namespaceData.AssertProperty("serviceBusEndpoint");
        Assert.False(string.IsNullOrEmpty(serviceBusEndpoint.GetString()));
        Assert.Contains(".servicebus.windows.net", serviceBusEndpoint.GetString());

        // Verify metric ID is present
        var metricId = namespaceData.AssertProperty("metricId");
        Assert.False(string.IsNullOrEmpty(metricId.GetString()));
        Assert.Contains(Settings.SubscriptionId, metricId.GetString());
        Assert.Contains(Settings.ResourceBaseName, metricId.GetString());

        // Verify feature flags are present (even if false/null)
        Assert.True(namespaceData.TryGetProperty("isAutoInflateEnabled", out _));
        Assert.True(namespaceData.TryGetProperty("kafkaEnabled", out _));
        Assert.True(namespaceData.TryGetProperty("zoneRedundant", out _));
    }
}
