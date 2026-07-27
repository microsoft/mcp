// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.LiveTests;

public class IoTHubCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_get_iot_hub_by_name_and_resource_group()
    {
        var result = await CallToolAsync("iothub_hub_get", new()
        {
            { "hub-name", Settings.ResourceBaseName },
            { "resource-group", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "tenant", Settings.TenantId }
        });

        Assert.NotNull(result);
        var payload = result!.Value;

        var iotHub = payload.AssertProperty("ioTHub");
        Assert.Equal(JsonValueKind.Object, iotHub.ValueKind);
        Assert.Equal(JsonValueKind.String, iotHub.GetProperty("name").ValueKind);

        var areResultsTruncated = payload.AssertProperty("areResultsTruncated");
        Assert.True(areResultsTruncated.ValueKind is JsonValueKind.True or JsonValueKind.False);
    }
}
