// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Aks.LiveTests;

public sealed class NodepoolCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_nodepools_for_cluster()
    {
        // Get a real cluster to target
        var listResult = await CallToolAsync(
            "azmcp_aks_cluster_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = listResult.AssertProperty("clusters");
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster for testing nodepool list command");

        var firstCluster = clusters.EnumerateArray().First();
        var clusterName = firstCluster.GetProperty("name").GetString()!;
        var resourceGroupName = firstCluster.GetProperty("resourceGroupName").GetString()!;

        // List node pools for that cluster
        var nodepoolResult = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "cluster", clusterName }
            });

        var nodePools = nodepoolResult.AssertProperty("nodePools");
        Assert.Equal(JsonValueKind.Array, nodePools.ValueKind);
        Assert.True(nodePools.GetArrayLength() > 0, "Expected at least one node pool in the cluster");

        // Validate properties exist on each node pool
        foreach (var pool in nodePools.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, pool.ValueKind);
            var nameProperty = pool.AssertProperty("name");
            Assert.False(string.IsNullOrEmpty(nameProperty.GetString()));

            if (pool.TryGetProperty("mode", out var modeProperty))
            {
                Assert.False(string.IsNullOrEmpty(modeProperty.GetString()));
            }

            if (pool.TryGetProperty("provisioningState", out var stateProperty))
            {
                Assert.False(string.IsNullOrEmpty(stateProperty.GetString()));
            }

            if (pool.TryGetProperty("osDiskSizeGB", out var osDiskSize))
            {
                Assert.True(osDiskSize.ValueKind is JsonValueKind.Number or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("osDiskType", out var osDiskType))
            {
                Assert.True(osDiskType.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("kubeletDiskType", out var kubeletDiskType))
            {
                Assert.True(kubeletDiskType.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("maxPods", out var maxPods))
            {
                Assert.True(maxPods.ValueKind is JsonValueKind.Number or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("type", out var typeProperty))
            {
                Assert.True(typeProperty.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("enableAutoScaling", out var autoScale))
            {
                Assert.True(autoScale.ValueKind is JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("powerState", out var powerState) && powerState.ValueKind == JsonValueKind.Object)
            {
                if (powerState.TryGetProperty("code", out var code))
                {
                    Assert.True(code.ValueKind is JsonValueKind.String or JsonValueKind.Null);
                }
            }
            if (pool.TryGetProperty("currentOrchestratorVersion", out var currVer))
            {
                Assert.True(currVer.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("enableNodePublicIP", out var pubIP))
            {
                Assert.True(pubIP.ValueKind is JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("scaleSetPriority", out var priority))
            {
                Assert.True(priority.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("nodeLabels", out var labels))
            {
                Assert.True(labels.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("nodeTaints", out var taints))
            {
                Assert.True(taints.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("osSKU", out var osSku))
            {
                Assert.True(osSku.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("nodeImageVersion", out var imgVer))
            {
                Assert.True(imgVer.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
        }
    }

    [Fact]
    public async Task Should_handle_nonexistent_cluster_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "nonexistent-rg" },
                { "cluster", "nonexistent-cluster" }
            });

        // Should return runtime error details in results
        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_parameters()
    {
        // Missing cluster
        var r1 = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "rg" }
            });
        Assert.False(r1.HasValue);

        // Missing resource-group
        var r2 = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", "cluster" }
            });
        Assert.False(r2.HasValue);

        // Missing subscription
        var r3 = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "resource-group", "rg" },
                { "cluster", "cluster" }
            });
        Assert.False(r3.HasValue);
    }

    [Fact]
    public async Task Should_handle_invalid_subscription_gracefully()
    {
        // Use obviously invalid subscription ID to ensure failure is surfaced
        var result = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "subscription", "invalid-subscription" },
                { "resource-group", "rg" },
                { "cluster", "cluster" }
            });

        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_aks_nodepool_list",
            new()
            {
                { "subscription", "" },
                { "resource-group", "rg" },
                { "cluster", "cluster" }
            });

        // Should return validation error response with no results
        Assert.False(result.HasValue);
    }
}
