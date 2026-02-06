// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Core.LiveTests;

public class CommandTests(ITestOutputHelper output, TestProxyFixture testProxyFixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, testProxyFixture, liveServerFixture)
{
    [Fact]
    public async Task Should_list_groups_by_subscription()
    {
        var result = await CallToolAsync(
            "group_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var groupsArray = result.AssertProperty("groups");
        Assert.Equal(JsonValueKind.Array, groupsArray.ValueKind);
        Assert.NotEmpty(groupsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_subscriptions()
    {
        var result = await CallToolAsync(
            "subscription_list",
            new Dictionary<string, object?>());

        var subscriptionsArray = result.AssertProperty("subscriptions");
        Assert.Equal(JsonValueKind.Array, subscriptionsArray.ValueKind);
        Assert.NotEmpty(subscriptionsArray.EnumerateArray());
    }

    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        new BodyRegexSanitizer(new BodyRegexSanitizerBody
        {
            Regex = "resource[Gg]roups/([^?\\/\"]+)",
            GroupForReplace = "1",
            Value = "Sanitized",
        }),
        new BodyRegexSanitizer(new BodyRegexSanitizerBody
        {
            Regex = @"(?is)""tags""\s*:\s*{(.*?)}",
            GroupForReplace = "1",
            Value = "",
        }),
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..managedBy")),
    ];
}
