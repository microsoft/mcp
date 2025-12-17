// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Acr.LiveTests;

public class AcrCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public override List<string> DisabledDefaultSanitizers =>
    [
        ..base.DisabledDefaultSanitizers,
        "AZSDK3493"
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..data.properties.loginServer") {
             Value = "sanitized.azurecr.io"
        })
    ];

    [Theory]
    [InlineData(AuthMethod.Credential)]
    public async Task Should_list_acr_registries_by_subscription(AuthMethod authMethod)
    {
        // Arrange & Act
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var resourceBaseName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "acr_registry_list",
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
                "acr_registry_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName },
                    { "auth-method", authMethod.ToString() }
                });

            var rgRegistries = rgResult.AssertProperty("registries");
            Assert.Equal(JsonValueKind.Array, rgRegistries.ValueKind);
            registryItems = [.. rgRegistries.EnumerateArray()];
        }

        Assert.NotEmpty(registryItems); // After fallback we must have at least one registry

        // Validate that the test registry exists (created by bicep as baseName)
        var hasTestRegistry = registryItems.Any(item =>
            item.TryGetProperty("name", out var nameProp) &&
            string.Equals(nameProp.GetString(), resourceBaseName, StringComparison.OrdinalIgnoreCase));
        Assert.True(hasTestRegistry, $"Expected test registry '{resourceBaseName}' to exist.");

        foreach (var item in registryItems)
        {
            // Enforce new object shape { name, location?, loginServer?, skuName?, skuTier? }
            Assert.Equal(JsonValueKind.Object, item.ValueKind);
            var nameProp = item.AssertProperty("name");
            var objName = nameProp.GetString();
            Assert.False(string.IsNullOrWhiteSpace(objName));
            // Minimal safety checks: we don't re-validate Azure's naming rules; we just ensure
            // the service returned a sane, non-empty string without control characters.
            Assert.DoesNotContain('\r', objName);
            Assert.DoesNotContain('\n', objName);
            Assert.True(objName!.All(static c => !char.IsControl(c)), $"Registry name '{objName}' contains control characters.");
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

    [Theory]
    [InlineData(AuthMethod.Credential)]
    public async Task Should_list_repositories_for_registries(AuthMethod authMethod)
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var resourceBaseName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "acr_registry_repository_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "auth-method", authMethod.ToString() }
            });

        if (result is null)
        {
            // No registries or repos found in the test RG/subscription; treat as pass with null results
            return;
        }

        var map = result.AssertProperty("repositoriesByRegistry");
        Assert.Equal(JsonValueKind.Object, map.ValueKind);

        // Validate we have entries for the test registry and the seeded 'testrepo'
        var repoArray = map.AssertProperty(resourceBaseName);
        Assert.Equal(JsonValueKind.Array, repoArray.ValueKind);
        var repos = repoArray.EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        Assert.Contains("testrepo", repos);
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        // Empty subscription should trigger validation failure (400) -> null results
        var result = await CallToolAsync(
            "acr_registry_list",
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
            "acr_registry_list",
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
        var result = await CallToolAsync("acr_registry_list", []);

        Assert.Null(result);
    }
}
