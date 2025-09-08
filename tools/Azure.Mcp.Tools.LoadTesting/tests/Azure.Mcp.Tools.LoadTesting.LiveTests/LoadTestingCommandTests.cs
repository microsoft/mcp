// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.LiveTests;

public class LoadTestingCommandTests : CommandTestsBase
{
    private const string TestResourceName = "TestResourceName";
    private const string TestRunId = "TestRunId";
    public LoadTestingCommandTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async Task Should_list_loadtests()
    {
        // Arrange
        var result = await CallToolAsync(
            "azmcp_loadtesting_testresource_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "resource-group", Settings.ResourceGroupName }
            });

        // Assert
        var items = result.AssertProperty("LoadTest");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.NotEmpty(items.EnumerateArray());
        foreach (var item in items.EnumerateArray())
        {
            Assert.NotNull(item.GetProperty("Id").GetString());
        }
    }
}
