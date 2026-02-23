// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.LiveTests;

public class AzureMigrateCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..displayName")
        {
            Value = "Sanitized"
        })
    ];

    public override List<string> DisabledDefaultSanitizers =>
    [
        ..base.DisabledDefaultSanitizers,
        "AZSDK2003"
    ];

    [Fact]
    public async Task Should_check_platform_landing_zone_exists()
    {
        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "check" }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.True(
            messageText.Contains("exists", StringComparison.OrdinalIgnoreCase) ||
            messageText.Contains("No Platform Landing zone found", StringComparison.OrdinalIgnoreCase),
            "Expected check result message");
    }

    [Fact]
    public async Task Should_update_platform_landing_zone_parameters()
    {
        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "update" },
                { "region-type", "single" },
                { "firewall-type", "azurefirewall" },
                { "network-architecture", "hubspoke" },
                { "regions", "southeastasia" },
                { "environment-name", "prod" },
                { "version-control-system", "local" },
                { "organization-name", "contoso" },
                { "identity-subscription-id", Settings.SubscriptionId },
                { "management-subscription-id", Settings.SubscriptionId },
                { "connectivity-subscription-id", Settings.SubscriptionId }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.Contains("Parameters updated successfully", messageText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_parameter_status()
    {
        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "status" }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.NotEmpty(messageText);
    }

    [Fact]
    public async Task Should_handle_invalid_action()
    {
        try
        {
            await CallToolAsync(
                "azuremigrate_platformlandingzone_request",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "migrate-project-name", Settings.ResourceBaseName },
                    { "action", "invalidaction" }
                });

            Assert.Fail("Expected an exception for invalid action");
        }
        catch (Exception ex)
        {
            Assert.Contains("Invalid action", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
