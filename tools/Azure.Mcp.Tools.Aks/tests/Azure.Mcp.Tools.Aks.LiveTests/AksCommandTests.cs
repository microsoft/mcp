// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Aks.LiveTests;

public sealed class AksCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{

    [Fact]
    public async Task Should_list_aks_clusters_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_get",
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
            var nameProperty = cluster.AssertProperty("name");
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

            // New enriched fields (presence and shape only)
            if (cluster.TryGetProperty("id", out var idProperty))
            {
                Assert.True(idProperty.ValueKind is JsonValueKind.String);
            }
            if (cluster.TryGetProperty("networkProfile", out var netProfile))
            {
                Assert.True(netProfile.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
                if (netProfile.ValueKind == JsonValueKind.Object)
                {
                    if (netProfile.TryGetProperty("loadBalancerProfile", out var lbProfile))
                    {
                        Assert.True(lbProfile.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
                    }
                }
            }
            if (cluster.TryGetProperty("windowsProfile", out var winProfile))
            {
                Assert.True(winProfile.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
            if (cluster.TryGetProperty("servicePrincipalProfile", out var spProfile))
            {
                Assert.True(spProfile.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
            if (cluster.TryGetProperty("addonProfiles", out var addons))
            {
                Assert.True(addons.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
            if (cluster.TryGetProperty("identityProfile", out var idProfile))
            {
                Assert.True(idProfile.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
            if (cluster.TryGetProperty("tags", out var tags))
            {
                Assert.True(tags.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
        }
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_aks_cluster_get",
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
            "azmcp_aks_cluster_get",
            new()
            {
                { "subscription", "invalid-subscription" }
            });

        // Should return runtime error response with error details in results
        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_subscription_parameter()
    {
        var result = await CallToolAsync("azmcp_aks_cluster_get", []);

        // Should return error response for missing subscription (no results)
        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task Should_get_specific_aks_cluster()
    {
        // First, get a list of clusters to find one we can test against
        var listResult = await CallToolAsync(
            "azmcp_aks_cluster_get",
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

        clusters = getResult.AssertProperty("clusters");
        Assert.Equal(JsonValueKind.Array, clusters.ValueKind);

        // Should return exactly one cluster
        Assert.Equal(1, clusters.GetArrayLength());
        var cluster = clusters.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, cluster.ValueKind);

        // Verify the cluster details
        var nameProperty = cluster.AssertProperty("name");
        Assert.Equal(clusterName, nameProperty.GetString());

        var rgProperty = cluster.AssertProperty("resourceGroupName");
        Assert.Equal(resourceGroupName, rgProperty.GetString());

        // Verify other common properties exist
        Assert.True(cluster.TryGetProperty("subscriptionId", out _));
        Assert.True(cluster.TryGetProperty("location", out _));

        // Enriched cluster checks
        Assert.True(cluster.TryGetProperty("id", out _));
        Assert.True(cluster.TryGetProperty("enableRbac", out _));
        Assert.True(cluster.TryGetProperty("skuName", out _));
        Assert.True(cluster.TryGetProperty("skuTier", out _));
        Assert.True(cluster.TryGetProperty("nodeResourceGroup", out _));
        Assert.True(cluster.TryGetProperty("maxAgentPools", out _));
        Assert.True(cluster.TryGetProperty("supportPlan", out _));

        // Profiles present or null
        Assert.True(cluster.TryGetProperty("networkProfile", out _));
        Assert.True(cluster.TryGetProperty("windowsProfile", out _));
        Assert.True(cluster.TryGetProperty("servicePrincipalProfile", out _));
        Assert.True(cluster.TryGetProperty("addonProfiles", out _));
        Assert.True(cluster.TryGetProperty("identityProfile", out _));

        // Get-specific should return agentPoolProfiles (we populate on Get)
        Assert.True(cluster.TryGetProperty("agentPoolProfiles", out var pools));
        Assert.Equal(JsonValueKind.Array, pools.ValueKind);
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
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_parameters_for_get_command()
    {
        // Test missing resource-group
        var result1 = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", "test-cluster" }
            });
        Assert.False(result1.HasValue);

        // Test missing subscription
        var result2 = await CallToolAsync(
            "azmcp_aks_cluster_get",
            new()
            {
                { "resource-group", "test-rg" },
                { "cluster", "test-cluster" }
            });
        Assert.False(result2.HasValue);
    }
}
