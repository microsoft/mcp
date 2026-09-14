// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests;

public class AzureMigrateCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new(new("$..displayName")
        {
            Value = "Sanitized"
        })
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        // The CLI user agent embeds the host OS and framework moniker, which differ between the machine that
        // produced the recording and the CI agents that replay it. Normalize it so matching is platform independent.
        new(new("User-Agent")
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
    public async Task Should_list_platform_landing_zones()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("azuremigrate_platformlandingzone_request"))
        {
            return;
        }

        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "list" }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.True(
            messageText.Contains("Platform Landing Zones under migrate project", StringComparison.OrdinalIgnoreCase) ||
            messageText.Contains("No Platform Landing Zone exists under migrate project", StringComparison.OrdinalIgnoreCase),
            "Expected list result message");
    }

    [Fact]
    public async Task Should_get_platform_landing_zone()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("azuremigrate_platformlandingzone_request"))
        {
            return;
        }

        var result = await CallToolAsync(
            "azuremigrate_platformlandingzone_request",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "migrate-project-name", Settings.ResourceBaseName },
                { "action", "get" }
            });

        var message = result.AssertProperty("message");
        Assert.Equal(JsonValueKind.String, message.ValueKind);
        var messageText = message.GetString();
        Assert.NotNull(messageText);
        Assert.True(
            messageText.Contains("Generation status", StringComparison.OrdinalIgnoreCase) ||
            messageText.Contains("No Platform Landing Zone exists under migrate project", StringComparison.OrdinalIgnoreCase),
            "Expected get result message");
    }

    [Fact]
    public async Task Should_handle_invalid_action()
    {
        if (await AssertLocalToolIsUnavailableInHttpMode("azuremigrate_platformlandingzone_request"))
        {
            return;
        }

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
