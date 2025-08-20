// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Acr.LiveTests;

[Trait("Area", "Acr")]
[Trait("Category", "Live")]
public class AcrCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
{
    [Theory]
    [InlineData(AuthMethod.Credential)]
    public async Task Should_list_acr_registries_by_subscription(AuthMethod authMethod)
    {
        // Arrange & Act
        var result = await CallToolAsync(
            "azmcp_acr_registry_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "auth-method", authMethod.ToString() }
            });

        // Assert
        var registries = result.AssertProperty("registries");
        Assert.Equal(JsonValueKind.Array, registries.ValueKind);

        var registryItems = registries.EnumerateArray().ToList();
        if (registryItems.Count == 0)
        {
            // Fallback: attempt with resource group filter (test RG hosts the registry via bicep)
            var rgResult = await CallToolAsync(
                "azmcp_acr_registry_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "auth-method", authMethod.ToString() }
                });

            var rgRegistries = rgResult.AssertProperty("registries");
            Assert.Equal(JsonValueKind.Array, rgRegistries.ValueKind);
            registryItems = [.. rgRegistries.EnumerateArray()];
        }

        Assert.NotEmpty(registryItems); // After fallback we must have the test registry

        foreach (var item in registryItems)
        {
            // Enforce new object shape { name, location?, loginServer?, skuName?, skuTier? }
            Assert.Equal(JsonValueKind.Object, item.ValueKind);
            Assert.True(item.TryGetProperty("name", out var nameProp));
            var objName = nameProp.GetString();
            Assert.False(string.IsNullOrWhiteSpace(objName));
            Assert.Matches("^[a-z0-9]+(-[a-z0-9]+)*$", objName!); // Basic ACR naming pattern

            if (item.TryGetProperty("location", out var locationProp))
            {
                Assert.False(string.IsNullOrWhiteSpace(locationProp.GetString()));
            }
            if (item.TryGetProperty("loginServer", out var loginServerProp))
            {
                Assert.False(string.IsNullOrWhiteSpace(loginServerProp.GetString()));
            }
        }
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        // Empty subscription should trigger validation failure (400) -> null results
        var result = await CallToolAsync(
            "azmcp_acr_registry_list",
            new()
            {
                { "subscription", "" }
            });

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_handle_invalid_subscription_gracefully()
    {
        // Invalid identifier should reach execution and return structured error details (HasValue)
        var result = await CallToolAsync(
            "azmcp_acr_registry_list",
            new()
            {
                { "subscription", "invalid-subscription" }
            });

        Assert.NotNull(result);
        var message = result.AssertProperty("message");
        Assert.Contains("invalid-subscription", message.GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_validate_required_subscription_parameter()
    {
        // Missing subscription option entirely should behave like other areas (validation -> null)
        var result = await CallToolAsync(
            "azmcp_acr_registry_list",
            new Dictionary<string, object?>());

        Assert.Null(result);
    }
}
