// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Aks.Tests;

public sealed class AksCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // ARM SDK calls for GetNodePools include the cluster name in the URL path.
    // In playback mode Settings.ResourceBaseName is "Sanitized", so the default
    // GeneralRegexSanitizer for ResourceBaseName is a no-op. A pattern-based
    // UriRegexSanitizer is needed, just like the resource-group one in the base class.
    public override List<UriRegexSanitizer> UriRegexSanitizers { get; } =
    [
        new(new UriRegexSanitizerBody
        {
            Regex = "managedClusters/([^?/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        })
    ];

    // ARG query bodies contain resource group and cluster names in KQL syntax like:
    //   resourceGroup =~ 'rg-name' and name =~ 'cluster-name'
    // The default GeneralRegexSanitizer for ResourceBaseName is a no-op in playback
    // (Settings.ResourceBaseName is "Sanitized"), so explicit body sanitizers are needed.
    public override List<BodyRegexSanitizer> BodyRegexSanitizers { get; } =
    [
        new(new BodyRegexSanitizerBody
        {
            Regex = "resourceGroup =~ '([^']+)'",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        new(new BodyRegexSanitizerBody
        {
            Regex = "name =~ '([^']+)'",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        // Also handle the path-format that appears in ARG response $id fields
        new(new BodyRegexSanitizerBody
        {
            Regex = "resource[Gg]roups/([^?/\"]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
    ];

    [Fact]
    public async Task Should_list_aks_clusters_by_subscription()
    {
        var result = await CallToolAsync(
            "aks_cluster_get",
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
    [LiveTestOnly]
    public async Task Should_validate_required_subscription_parameter()
    {
        // When subscription is omitted, falls back to default subscription from CLI profile
        var result = await CallToolAsync("aks_cluster_get", []);

        Assert.True(result.HasValue);
        var clusters = result.Value.AssertProperty("clusters");
        Assert.Equal(JsonValueKind.Array, clusters.ValueKind);
    }

    [Fact]
    public async Task Should_get_specific_aks_cluster()
    {
        // First, get a list of clusters to find one we can test against
        var listResult = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = listResult.AssertProperty("clusters");
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster for testing get command");

        // Get the first cluster's details
        var firstCluster = clusters.EnumerateArray().First();
        // Use response values directly: in playback these are "Sanitized" (matching the recording);
        // in record mode they are the real names which get sanitized when stored.
        var clusterName = firstCluster.GetProperty("name").GetString()!;
        var resourceGroupName = firstCluster.GetProperty("resourceGroupName").GetString()!;

        // Now test the get command
        var getResult = await CallToolAsync(
            "aks_cluster_get",
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
        cluster.AssertProperty("subscriptionId");
        cluster.AssertProperty("location");

        // Enriched cluster checks
        cluster.AssertProperty("id");
        cluster.AssertProperty("enableRbac");
        cluster.AssertProperty("skuName");
        cluster.AssertProperty("skuTier");
        cluster.AssertProperty("nodeResourceGroup");
        cluster.AssertProperty("maxAgentPools");
        cluster.AssertProperty("supportPlan");

        // Profiles present or null
        cluster.AssertProperty("networkProfile");
        cluster.AssertProperty("windowsProfile");
        cluster.AssertProperty("servicePrincipalProfile");
        cluster.AssertProperty("addonProfiles");
        cluster.AssertProperty("identityProfile");

        // Get-specific should return agentPoolProfiles (we populate on Get)
        var pools = cluster.AssertProperty("agentPoolProfiles");
        Assert.Equal(JsonValueKind.Array, pools.ValueKind);
    }

    [Fact]
    public async Task Should_handle_nonexistent_cluster_gracefully()
    {
        // First, get a list of clusters to find a resource group to test against
        var listResult = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = listResult.AssertProperty("clusters");
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster for testing get command");

        // Get the first cluster's resource group
        var firstCluster = clusters.EnumerateArray().First();
        // Use the response value directly: in playback this is "Sanitized" (matching the recording);
        // in record mode it's the real name which gets sanitized when stored.
        var resourceGroupName = firstCluster.GetProperty("resourceGroupName").GetString()!;

        // Attempt to get a non-existent cluster from that resource group
        // In playback, the recording stores the cluster name sanitized as "Sanitized", so we must use
        // "Sanitized" in playback to match. In record/live mode we use a name that doesn't exist in Azure.
        var nonExistentClusterName = TestMode == TestMode.Playback ? "Sanitized" : "nonexistent-cluster";
        var result = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "cluster", nonExistentClusterName }
            });

        // Should return list with zero clusters
        Assert.True(result.HasValue);
        var results = result.Value;
        var resultsClusters = results.AssertProperty("clusters");
        Assert.True(resultsClusters.GetArrayLength() == 0, "Expected no clusters for nonexistent cluster request");
    }

    [Fact]
    [LiveTestOnly]
    public async Task Should_validate_required_parameters_for_get_command()
    {
        // Test missing resource-group when cluster is specified - validation catches it
        var result1 = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", "test-cluster" }
            });
        Assert.False(result1.HasValue);

        // Test missing subscription - falls back to default, nonexistent rg returns error
        var result2 = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "resource-group", "test-rg" },
                { "cluster", "test-cluster" }
            });
        Assert.True(result2.HasValue);
        result2.Value.AssertProperty("message");
        var typeProperty = result2.Value.AssertProperty("type");
        Assert.Equal("RequestFailedException", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_get_nodepool_for_cluster()
    {
        // Get a real cluster to target
        var listResult = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = listResult.AssertProperty("clusters");
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster for testing nodepool get command");

        var firstCluster = clusters.EnumerateArray().First();
        var clusterName = RegisterOrRetrieveVariable("firstClusterName", firstCluster.GetProperty("name").GetString()!);
        var resourceGroupName = RegisterOrRetrieveVariable("firstResourceGroupName", firstCluster.GetProperty("resourceGroupName").GetString()!);

        // Find a node pool to query
        var nodepoolList = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "cluster", clusterName }
            });

        var nodePools = nodepoolList.AssertProperty("nodePools");
        Assert.True(nodePools.GetArrayLength() > 0, "Expected at least one node pool in the cluster");

        var firstPool = nodePools.EnumerateArray().First();
        var nodepoolName = RegisterOrRetrieveVariable("firstNodepoolName", firstPool.GetProperty("name").GetString()!);

        // Get details for that node pool
        var nodepoolGet = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "cluster", clusterName },
                { "nodepool", nodepoolName }
            });

        nodePools = nodepoolGet.AssertProperty("nodePools");
        Assert.Equal(JsonValueKind.Array, nodePools.ValueKind);
        Assert.Single(nodePools.EnumerateArray());

        var nodePool = nodePools.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, nodePool.ValueKind);
        Assert.Equal(TestMode == TestMode.Playback ? "Sanitized" : nodepoolName, nodePool.GetProperty("name").GetString());

        if (nodePool.TryGetProperty("mode", out var modeProperty))
        {
            Assert.False(string.IsNullOrEmpty(modeProperty.GetString()));
        }

        if (nodePool.TryGetProperty("provisioningState", out var stateProperty))
        {
            Assert.False(string.IsNullOrEmpty(stateProperty.GetString()));
        }

        nodePool.AssertProperty("orchestratorVersion");
        nodePool.AssertProperty("currentOrchestratorVersion");
        nodePool.AssertProperty("enableAutoScaling");
        nodePool.AssertProperty("maxPods");
        nodePool.AssertProperty("osSKU");
        nodePool.AssertProperty("nodeImageVersion");

        // Enriched node pool fields (presence/type checks)
        if (nodePool.TryGetProperty("tags", out var tags))
        {
            Assert.True(tags.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
        }
        if (nodePool.TryGetProperty("spotMaxPrice", out var spot))
        {
            Assert.True(spot.ValueKind is JsonValueKind.Number or JsonValueKind.Null);
        }
        if (nodePool.TryGetProperty("workloadRuntime", out var wr))
        {
            Assert.True(wr.ValueKind is JsonValueKind.String or JsonValueKind.Null);
        }
        if (nodePool.TryGetProperty("networkProfile", out var np))
        {
            Assert.True(np.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            if (np.ValueKind == JsonValueKind.Object)
            {
                if (np.TryGetProperty("allowedHostPorts", out var ahp))
                {
                    Assert.True(ahp.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
                }
                if (np.TryGetProperty("applicationSecurityGroups", out var asg))
                {
                    Assert.True(asg.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
                }
                if (np.TryGetProperty("nodePublicIPTags", out var ipt))
                {
                    Assert.True(ipt.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
                }
            }
        }
        if (nodePool.TryGetProperty("podSubnetID", out var podSubnet))
        {
            Assert.True(podSubnet.ValueKind is JsonValueKind.String or JsonValueKind.Null);
        }
        if (nodePool.TryGetProperty("vnetSubnetID", out var vnetSubnet))
        {
            Assert.True(vnetSubnet.ValueKind is JsonValueKind.String or JsonValueKind.Null);
        }
    }

    [Fact]
    public async Task Should_handle_nonexistent_nodepool_gracefully()
    {
        var result = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "nonexistent-rg" },
                { "cluster", "nonexistent-cluster" },
                { "nodepool", "nonexistent-nodepool" }
            });

        // Should return runtime error details in results
        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");

        Assert.Equal("RequestFailedException", typeProperty.GetString());
    }

    [Fact]
    [LiveTestOnly]
    public async Task Should_validate_required_parameters()
    {
        // Missing cluster - validation catches it, no results returned
        var r1 = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "rg" },
                { "nodepool", "np1" }
            });
        Assert.False(r1.HasValue);

        // Missing resource-group - validation catches it, no results returned
        var r2 = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", "cluster" },
                { "nodepool", "np1" }
            });
        Assert.False(r2.HasValue);

        // Missing subscription - falls back to default subscription, returns error for nonexistent resources
        var r3 = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "resource-group", "rg" },
                { "cluster", "cluster" },
                { "nodepool", "np1" }
            });
        Assert.True(r3.HasValue);
        r3.Value.AssertProperty("message");
        var r3Type = r3.Value.AssertProperty("type");
        Assert.Equal("RequestFailedException", r3Type.GetString());
    }

    [Fact]
    public async Task Should_handle_invalid_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", "not-a-real-sub" },
                { "resource-group", "rg" },
                { "cluster", "cluster" },
                { "nodepool", "np1" }
            });

        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("KeyNotFoundException", typeProperty.GetString());
    }

    [Fact]
    [LiveTestOnly]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "aks_nodepool_get",
            new()
            {
                { "subscription", "" },
                { "resource-group", "rg" },
                { "cluster", "cluster" },
                { "nodepool", "np1" }
            });

        // Empty subscription falls back to default, nonexistent resources return error
        Assert.True(result.HasValue);
        result.Value.AssertProperty("message");
        var typeProperty = result.Value.AssertProperty("type");
        Assert.Equal("RequestFailedException", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_list_nodepools_for_cluster()
    {
        // Get a real cluster to target
        var listResult = await CallToolAsync(
            "aks_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var clusters = listResult.AssertProperty("clusters");
        Assert.True(clusters.GetArrayLength() > 0, "Expected at least one AKS cluster for testing nodepool get command");

        var firstCluster = clusters.EnumerateArray().First();
        var clusterName = RegisterOrRetrieveVariable("firstClusterName", firstCluster.GetProperty("name").GetString()!);
        var resourceGroupName = RegisterOrRetrieveVariable("firstResourceGroupName", firstCluster.GetProperty("resourceGroupName").GetString()!);

        // List node pools for that cluster
        var nodepoolResult = await CallToolAsync(
            "aks_nodepool_get",
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

            // Enriched fields on node pool (optional checks)
            if (pool.TryGetProperty("tags", out var tags))
            {
                Assert.True(tags.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("spotMaxPrice", out var spot))
            {
                Assert.True(spot.ValueKind is JsonValueKind.Number or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("workloadRuntime", out var wr))
            {
                Assert.True(wr.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("networkProfile", out var np))
            {
                Assert.True(np.ValueKind is JsonValueKind.Object or JsonValueKind.Null);
                if (np.ValueKind == JsonValueKind.Object)
                {
                    if (np.TryGetProperty("allowedHostPorts", out var ahp))
                    {
                        Assert.True(ahp.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
                    }
                    if (np.TryGetProperty("applicationSecurityGroups", out var asg))
                    {
                        Assert.True(asg.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
                    }
                    if (np.TryGetProperty("nodePublicIPTags", out var ipt))
                    {
                        Assert.True(ipt.ValueKind is JsonValueKind.Array or JsonValueKind.Null);
                    }
                }
            }
            if (pool.TryGetProperty("podSubnetID", out var podSubnet))
            {
                Assert.True(podSubnet.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
            if (pool.TryGetProperty("vnetSubnetID", out var vnetSubnet))
            {
                Assert.True(vnetSubnet.ValueKind is JsonValueKind.String or JsonValueKind.Null);
            }
        }
    }
}
