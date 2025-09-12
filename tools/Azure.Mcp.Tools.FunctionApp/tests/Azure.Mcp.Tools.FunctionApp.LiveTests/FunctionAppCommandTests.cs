// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.LiveTests;

public sealed class FunctionAppCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
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

    [Fact]
    public async Task Should_get_specific_function_app()
    {
        // List to obtain a real function app and its resource group
        var listResult = await CallToolAsync(
            "azmcp_functionapp_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var functionApps = listResult.AssertProperty("results");
        Assert.True(functionApps.GetArrayLength() > 0, "Expected at least one Function App for get command test");

        var first = functionApps.EnumerateArray().First();
        var name = first.GetProperty("name").GetString()!;
        var resourceGroup = first.GetProperty("resourceGroupName").GetString()!;

        var getResult = await CallToolAsync(
            "azmcp_functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroup },
                { "function-app", name }
            });

        var functionApp = getResult.AssertProperty("functionApp");
        Assert.Equal(JsonValueKind.Object, functionApp.ValueKind);
        Assert.Equal(name, functionApp.GetProperty("name").GetString());
        Assert.Equal(resourceGroup, functionApp.GetProperty("resourceGroupName").GetString());
        // Common useful properties
        if (functionApp.TryGetProperty("location", out var loc))
        {
            Assert.False(string.IsNullOrWhiteSpace(loc.GetString()));
        }
    }

    [Fact]
    public async Task Should_handle_nonexistent_function_app_gracefully()
    {
        var result = await CallToolAsync(
            "azmcp_functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "nonexistent-rg" },
                { "function-app", "nonexistent-functionapp" }
            });

        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        Assert.True(errorDetails.TryGetProperty("message", out _));
        Assert.True(errorDetails.TryGetProperty("type", out var typeProperty));
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_parameters_for_get_command()
    {
        // Missing functionapp
        var missingName = await CallToolAsync(
            "azmcp_functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "rg-test" }
            });
        Assert.False(missingName.HasValue);

        // Missing resource-group
        var missingRg = await CallToolAsync(
            "azmcp_functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "function-app", "name-test" }
            });
        Assert.False(missingRg.HasValue);

        // Missing subscription
        var missingSub = await CallToolAsync(
            "azmcp_functionapp_get",
            new()
            {
                { "resource-group", "rg-test" },
                { "function-app", "name-test" }
            });
        Assert.False(missingSub.HasValue);
    }
}
