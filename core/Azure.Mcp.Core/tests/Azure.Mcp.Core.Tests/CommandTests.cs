// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Group;
using Azure.Mcp.Core.Areas.Subscription;
using Azure.Mcp.Tests.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Core.Tests;

public class CommandTests(ITestOutputHelper output, TestProxyFixture testProxyFixture)
    : RecordedAzureToolTestsBase<GroupSetup>(output, testProxyFixture)
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        var subscription = new SubscriptionSetup();
        services.AddSingleton<IAreaSetup>(subscription);
        subscription.ConfigureServices(services);
    }

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
