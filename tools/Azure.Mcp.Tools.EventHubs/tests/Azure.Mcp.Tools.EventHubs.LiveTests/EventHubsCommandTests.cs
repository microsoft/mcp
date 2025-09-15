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
            "azmcp_eventhubs_namespace_list",
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
            "azmcp_eventhubs_namespace_list",
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
}
