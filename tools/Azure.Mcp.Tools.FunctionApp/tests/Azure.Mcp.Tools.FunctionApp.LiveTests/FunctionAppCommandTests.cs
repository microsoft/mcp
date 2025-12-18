// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.LiveTests;

public sealed class FunctionAppCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.customDomainVerificationId")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.inboundIpAddress")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.possibleInboundIpAddresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.inboundIpv6Address")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.possibleInboundIpv6Addresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.ftpsHostName")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.outboundIpAddresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.possibleOutboundIpAddresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.outboundIpv6Addresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.possibleOutboundIpv6Addresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..properties.homeStamp")),
    ];

    [Fact]
    public async Task Should_list_function_apps_by_subscription()
    {
        var result = await CallToolAsync(
            "functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var functionApps = result.AssertProperty("functionApps");
        Assert.Equal(JsonValueKind.Array, functionApps.ValueKind);

        Assert.True(functionApps.GetArrayLength() >= 2, "Expected at least two Function Apps in the test environment");

        foreach (var functionApp in functionApps.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, functionApp.ValueKind);

            var nameProperty = functionApp.AssertProperty("name");
            Assert.False(string.IsNullOrEmpty(nameProperty.GetString()));

            var rgProperty = functionApp.AssertProperty("resourceGroupName");
            Assert.False(string.IsNullOrEmpty(rgProperty.GetString()));

            var aspProperty = functionApp.AssertProperty("appServicePlanName");
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
            "functionapp_get",
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
            "functionapp_get",
            new()
            {
                { "subscription", "invalid-subscription" }
            });

        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_subscription_parameter()
    {
        var result = await CallToolAsync("functionapp_get", []);

        Assert.False(result.HasValue);
    }

    [Fact]
    public async Task Should_get_specific_function_app()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        // List to obtain a real function app and its resource group
        var listResult = await CallToolAsync(
            "functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName }
            });

        var functionApps = listResult.AssertProperty("functionApps");
        Assert.True(functionApps.GetArrayLength() > 0, "Expected at least one Function App for get command test");

        var first = functionApps.EnumerateArray().First();
        var name = RegisterOrRetrieveVariable("functionAppName", first.AssertProperty("name").GetString()!);
        if (TestMode == Tests.Helpers.TestMode.Playback)
        {
            name = string.Concat("Sanitized", name.AsSpan(name.IndexOf('-')));
        }
        var resourceGroup = first.AssertProperty("resourceGroupName").GetString();

        var getResult = await CallToolAsync(
            "functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroup },
                { "function-app", name }
            });

        functionApps = getResult.AssertProperty("functionApps");
        Assert.Equal(JsonValueKind.Array, functionApps.ValueKind);
        Assert.Single(functionApps.EnumerateArray());

        var functionApp = functionApps.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, functionApp.ValueKind);

        Assert.Equal(TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : name, functionApp.AssertProperty("name").GetString());
        Assert.Equal(resourceGroup, functionApp.AssertProperty("resourceGroupName").GetString());
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
            "functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "nonexistent-rg" },
                { "function-app", "nonexistent-functionapp" }
            });

        Assert.True(result.HasValue);
        var errorDetails = result.Value;
        errorDetails.AssertProperty("message");
        var typeProperty = errorDetails.AssertProperty("type");
        Assert.Equal("Exception", typeProperty.GetString());
    }

    [Fact]
    public async Task Should_validate_required_parameters_for_get_command()
    {
        // Missing resource-group
        var missingRg = await CallToolAsync(
            "functionapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "function-app", "name-test" }
            });
        Assert.False(missingRg.HasValue);

        // Missing subscription
        var missingSub = await CallToolAsync(
            "functionapp_get",
            new()
            {
                { "resource-group", "rg-test" },
                { "function-app", "name-test" }
            });
        Assert.False(missingSub.HasValue);
    }
}
