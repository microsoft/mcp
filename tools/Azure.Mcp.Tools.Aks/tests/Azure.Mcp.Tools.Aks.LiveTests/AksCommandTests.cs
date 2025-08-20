// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Aks.LiveTests;

public sealed class AksCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
{

    [Fact]
    public async Task Should_list_aks_clusters_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = result.AssertProperty("clusters");
        Assert.Equal(JsonValueKind.Array, clusters.ValueKind);

        // Verify we have at least one cluster in the test environment
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster in the test environment");

        // Check each cluster is an object with expected properties
        foreach (var cluster in clusters.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, cluster.ValueKind);

            // Verify required properties exist
            Assert.True(cluster.TryGetProperty("name", out var nameProperty));
            Assert.False(string.IsNullOrEmpty(nameProperty.GetString()));

            // Verify optional but commonly present properties
            if (cluster.TryGetProperty("location", out var locationProperty))
            {
                Assert.False(string.IsNullOrEmpty(locationProperty.GetString()));
            }

            if (cluster.TryGetProperty("kubernetesVersion", out var versionProperty))
            {
                Assert.False(string.IsNullOrEmpty(versionProperty.GetString()));
            }

            if (cluster.TryGetProperty("provisioningState", out var stateProperty))
            {
                Assert.False(string.IsNullOrEmpty(stateProperty.GetString()));
            }
        }
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_list",
            new()
            {
                { "subscription", "" }
            });

        // Should return validation error response with no results
        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task Should_handle_invalid_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_list",
            new()
            {
                { "subscription", "invalid-subscription" }
            });

        // Should return runtime error response with error details in results
        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        Assert.True(errorDetails.TryGetProperty("message", out _));
        Assert.True(errorDetails.TryGetProperty("type", out var typeProperty));
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_subscription_parameter()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_list",
            new Dictionary<string, object?>());

        // Should return error response for missing subscription (no results)
        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task Should_get_specific_aks_cluster()
    {
        // First, get a list of clusters to find one we can test against
        var listResult = await CallToolAsync(
            "azmcp_aks_cluster_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = listResult.AssertProperty("clusters");
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster for testing get command");

        // Get the first cluster's details
        var firstCluster = clusters.EnumerateArray().First();
        var clusterName = firstCluster.GetProperty("name").GetString()!;
        var resourceGroupName = firstCluster.GetProperty("resourceGroupName").GetString()!;

        // Now test the get command
        var getResult = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "cluster", clusterName }
            });

        var cluster = getResult.AssertProperty("cluster");
        Assert.Equal(JsonValueKind.Object, cluster.ValueKind);

        // Verify the cluster details
        Assert.True(cluster.TryGetProperty("name", out var nameProperty));
        Assert.Equal(clusterName, nameProperty.GetString());

        Assert.True(cluster.TryGetProperty("resourceGroupName", out var rgProperty));
        Assert.Equal(resourceGroupName, rgProperty.GetString());

        // Verify other common properties exist
        Assert.True(cluster.TryGetProperty("subscriptionId", out _));
        Assert.True(cluster.TryGetProperty("location", out _));
    }

    [Fact]
    public async Task Should_handle_nonexistent_cluster_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "nonexistent-rg" },
                { "cluster", "nonexistent-cluster" }
            });

        // Should return runtime error response with error details
        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        Assert.True(errorDetails.TryGetProperty("message", out _));
        Assert.True(errorDetails.TryGetProperty("type", out var typeProperty));
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_parameters_for_get_command()
    {
        // Test missing cluster
        var result1 = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "test-rg" }
            });
        Assert.False(result1.HasValue);

        // Test missing resource-group
        var result2 = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", "test-cluster" }
            });
        Assert.False(result2.HasValue);

        // Test missing subscription
        var result3 = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "resource-group", "test-rg" },
                { "cluster", "test-cluster" }
            });
        Assert.False(result3.HasValue);
    }
}
