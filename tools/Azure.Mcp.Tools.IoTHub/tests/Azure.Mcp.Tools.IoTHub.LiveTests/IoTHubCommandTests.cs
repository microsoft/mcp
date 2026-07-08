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
    public async Task Should_list_iot_hubs()
    {
        var result = await CallToolAsync("iothub_hub_get", new()
        {
            { "subscription", Settings.SubscriptionId },
            { "tenant", Settings.TenantId }
        });

        Assert.NotNull(result);
        var payload = result!.Value;

        var iotHubs = payload.AssertProperty("ioTHubs");
        Assert.Equal(JsonValueKind.Array, iotHubs.ValueKind);
        Assert.NotEmpty(iotHubs.EnumerateArray());

        var areResultsTruncated = payload.AssertProperty("areResultsTruncated");
        Assert.True(areResultsTruncated.ValueKind is JsonValueKind.True or JsonValueKind.False);
    }

    [Fact]
    public async Task Should_get_iot_hub_by_name()
    {
        var result = await CallToolAsync("iothub_hub_get", new()
        {
            { "name", Settings.ResourceBaseName },
            { "subscription", Settings.SubscriptionId },
            { "tenant", Settings.TenantId }
        });

        Assert.NotNull(result);
        var payload = result!.Value;

        var iotHubs = payload.AssertProperty("ioTHubs");
        Assert.Equal(JsonValueKind.Array, iotHubs.ValueKind);
        Assert.Equal(1, iotHubs.GetArrayLength());

        var iotHub = iotHubs.EnumerateArray().First();
        Assert.Equal(JsonValueKind.Object, iotHub.ValueKind);
        Assert.Equal(JsonValueKind.String, iotHub.GetProperty("name").ValueKind);

        var areResultsTruncated = payload.AssertProperty("areResultsTruncated");
        Assert.True(areResultsTruncated.ValueKind is JsonValueKind.True or JsonValueKind.False);
    }
}
