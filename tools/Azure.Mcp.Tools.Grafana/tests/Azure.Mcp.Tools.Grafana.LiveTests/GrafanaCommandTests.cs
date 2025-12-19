// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Grafana.LiveTests;

public class GrafanaCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    [Fact]
    public async Task Should_list_grafana_workspaces_by_subscription_id()
    {
        var result = await CallToolAsync(
            "grafana_list",
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
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var result = await CallToolAsync(
            "grafana_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var workspaces = result.AssertProperty("workspaces").EnumerateArray();
        var testWorkspace = workspaces.FirstOrDefault(w => w.GetProperty("name").GetString()?.StartsWith(Settings.ResourceBaseName) == true);

        Assert.True(testWorkspace.ValueKind != JsonValueKind.Undefined, $"Expected to find test Grafana workspace starting with '{Settings.ResourceBaseName}' in the subscription");

        // Verify workspace properties
        Assert.NotNull(testWorkspace.AssertProperty("name").GetString());
        Assert.NotNull(testWorkspace.AssertProperty("subscriptionId").GetString());
        Assert.NotNull(testWorkspace.AssertProperty("location").GetString());
        Assert.NotNull(testWorkspace.AssertProperty("resourceGroupName").GetString());
        Assert.NotNull(testWorkspace.AssertProperty("endpoint").GetString());
        Assert.NotNull(testWorkspace.AssertProperty("zoneRedundancy").GetString());
        Assert.NotNull(testWorkspace.AssertProperty("publicNetworkAccess").GetString());

        Assert.Equal(resourceGroupName, testWorkspace.AssertProperty("resourceGroupName").GetString());
        Assert.Equal(Settings.SubscriptionId, testWorkspace.AssertProperty("subscriptionId").GetString());
    }
}
