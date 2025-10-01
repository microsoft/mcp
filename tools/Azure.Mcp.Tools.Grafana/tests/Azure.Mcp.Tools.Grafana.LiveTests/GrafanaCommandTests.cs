// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Grafana.LiveTests;

public class GrafanaCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_grafana_workspaces_by_subscription_id()
    {
        var result = await CallToolAsync(
            "azmcp_grafana_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var workspaces = result.AssertProperty("workspaces");
        Assert.Equal(JsonValueKind.Array, workspaces.ValueKind);
        Assert.NotEmpty(workspaces.EnumerateArray());
    }

    [Fact]
    public async Task Should_include_test_grafana_workspace_in_list()
    {
        var result = await CallToolAsync(
            "azmcp_grafana_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var workspaces = result.AssertProperty("workspaces").EnumerateArray();
        var testWorkspace = workspaces.FirstOrDefault(w => w.GetProperty("name").GetString()?.StartsWith(Settings.ResourceBaseName) == true);

        Assert.True(testWorkspace.ValueKind != JsonValueKind.Undefined, $"Expected to find test Grafana workspace starting with '{Settings.ResourceBaseName}' in the subscription");

        // Verify workspace properties
        Assert.NotNull(testWorkspace.GetProperty("name").GetString());
        Assert.NotNull(testWorkspace.GetProperty("subscriptionId").GetString());
        Assert.NotNull(testWorkspace.GetProperty("location").GetString());
        Assert.NotNull(testWorkspace.GetProperty("resourceGroupName").GetString());
        Assert.NotNull(testWorkspace.GetProperty("endpoint").GetString());
        Assert.NotNull(testWorkspace.GetProperty("zoneRedundancy").GetString());
        Assert.NotNull(testWorkspace.GetProperty("publicNetworkAccess").GetString());

        Assert.Equal(Settings.ResourceGroupName, testWorkspace.GetProperty("resourceGroupName").GetString());
        Assert.Equal(Settings.SubscriptionId, testWorkspace.GetProperty("subscriptionId").GetString());
    }
}
