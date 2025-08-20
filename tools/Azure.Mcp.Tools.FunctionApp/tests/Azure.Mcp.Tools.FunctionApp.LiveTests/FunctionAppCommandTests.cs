// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.LiveTests;

public sealed class FunctionAppCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
{

    [Fact]
    public async Task Should_list_function_apps_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_functionapp_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var functionApps = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, functionApps.ValueKind);

        Assert.True(functionApps.GetArrayLength() >= 2, "Expected at least two Function Apps in the test environment");

        foreach (var functionApp in functionApps.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, functionApp.ValueKind);

            Assert.True(functionApp.TryGetProperty("name", out var nameProperty));
            Assert.False(string.IsNullOrEmpty(nameProperty.GetString()));

            Assert.True(functionApp.TryGetProperty("resourceGroupName", out var rgProperty));
            Assert.False(string.IsNullOrEmpty(rgProperty.GetString()));

            Assert.True(functionApp.TryGetProperty("appServicePlanName", out var aspProperty));
            Assert.False(string.IsNullOrEmpty(aspProperty.GetString()));

            if (functionApp.TryGetProperty("location", out var locationProperty))
            {
                Assert.False(string.IsNullOrEmpty(locationProperty.GetString()));
            }

            if (functionApp.TryGetProperty("status", out var statusProperty))
            {
                Assert.False(string.IsNullOrEmpty(statusProperty.GetString()));
            }
        }
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_functionapp_list",
            new()
            {
                { "subscription", "" }
            });

        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task Should_handle_invalid_subscription_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_functionapp_list",
            new()
            {
                { "subscription", "invalid-subscription" }
            });

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
            "azmcp_functionapp_list",
            new Dictionary<string, object?>());

        Assert.False(result.HasValue);
    }
}
