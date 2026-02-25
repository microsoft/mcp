// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Core.LiveTests;

public class CommandTests(ITestOutputHelper output, LiveServerFixture liveServerFixture) : CommandTestsBase(output, liveServerFixture)
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
}
