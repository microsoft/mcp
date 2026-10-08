// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests;

public class IoTHubCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Endpoint names and metric dimension names must remain distinct during playback.
    public override List<string> DisabledDefaultSanitizers => [.. base.DisabledDefaultSanitizers, "AZSDK3493"];

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new(new()
        {
            Regex = @"(?i)(/subscriptions/|""subscriptionId""\s*:\s*"")(?<subscription>[0-9a-f-]{36})",
            GroupForReplace = "subscription",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new(new()
        {
            Regex = @"(?i)(/resourceGroups/|resource group ')(?<group>[^/'""?]+)",
            GroupForReplace = "group",
            Value = "Sanitized"
        })
    ];

    // Routing evidence does not use subscription/resource tags or subscription display names.
    public override List<BodyRegexSanitizer> BodyRegexSanitizers { get; } =
    [
        new(new()
        {
            Regex = @"""tags""\s*:\s*\{(?:[^""{}]|""(?:\\.|[^""\\])*"")*\}",
            Value = "\"tags\": {}"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers { get; } =
    [
        new(new("$..displayName") { Value = "Sanitized" }),
        new(new("$..resourcegroup") { Value = "Sanitized" })
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        new(new("x-ms-operation-identifier") { Value = "Sanitized" })
    ];

    [Fact]
    public async Task IoTHubDevice_ListDevices()
    {
        // ResourceBaseName equals the deployed hub name and is available in both Record and
        // Playback (sanitized to match the recording). The IOTHUB_NAME env var is only set
        // during Record, so relying on it would break playback in CI.
        var hubName = Settings.ResourceBaseName;

        await CallToolAsync("iothub_device_list", new()
        {
            { "hub-name", hubName },
            { "resource-group", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId }
        });

        await CallToolAsync("iothub_device_list", new()
        {
            { "hub-name", hubName },
            { "resource-group", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "max-count", 2 }
        });
    }

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

        var areResultsTruncated =
            payload.AssertProperty("areResultsTruncated");

        Assert.True(
            areResultsTruncated.ValueKind is
            JsonValueKind.True or JsonValueKind.False);
    }

    [Fact]
    public async Task Should_get_iot_hub_routing_endpoint_health()
    {
        var result = await CallToolAsync("iothub_routing_endpoint-health", new()
        {
            { "hub-name", Settings.ResourceBaseName },
            { "resource-group", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "tenant", Settings.TenantId }
        });

        Assert.NotNull(result);
        var endpoints = result.Value.AssertProperty("endpoints");
        Assert.Equal(JsonValueKind.Array, endpoints.ValueKind);
        Assert.Contains(endpoints.EnumerateArray(), endpoint => endpoint.GetProperty("endpointName").GetString() == "sbqueue-endpoint");
        Assert.Contains(endpoints.EnumerateArray(), endpoint => endpoint.GetProperty("endpointName").GetString() == "sbtopic-endpoint");
        foreach (var endpoint in endpoints.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.String, endpoint.GetProperty("endpointId").ValueKind);
            Assert.Equal(JsonValueKind.String, endpoint.GetProperty("endpointName").ValueKind);
            Assert.Equal(JsonValueKind.String, endpoint.GetProperty("healthStatus").ValueKind);
        }
    }

    [Fact]
    public async Task Should_get_iot_hub_routing_endpoint_diagnostics()
    {
        var endTime = RegisterOrRetrieveVariable("RoutingEndTime", DateTimeOffset.UtcNow.ToString("O"));
        var startTime = RegisterOrRetrieveVariable("RoutingStartTime", DateTimeOffset.Parse(endTime).AddHours(-6).ToString("O"));
        var result = await CallToolAsync("iothub_routing_endpoint-diagnostics", new()
        {
            { "hub-name", Settings.ResourceBaseName },
            { "resource-group", Settings.ResourceGroupName },
            { "subscription", Settings.SubscriptionId },
            { "tenant", Settings.TenantId },
            { "start-time", startTime },
            { "end-time", endTime },
            { "interval", "PT1H" }
        });

        Assert.NotNull(result);
        var observationWindow = result.Value.AssertProperty("observationWindow");
        Assert.Equal(JsonValueKind.Object, observationWindow.ValueKind);
        var endpoints = result.Value.AssertProperty("endpoints");
        Assert.Equal(JsonValueKind.Array, endpoints.ValueKind);
        var queue = Assert.Single(endpoints.EnumerateArray(), endpoint => endpoint.GetProperty("name").GetString() == "sbqueue-endpoint");
        var topic = Assert.Single(endpoints.EnumerateArray(), endpoint => endpoint.GetProperty("name").GetString() == "sbtopic-endpoint");
        Assert.Equal("exists", queue.GetProperty("target").GetProperty("existenceStatus").GetString());
        Assert.Equal("exists", topic.GetProperty("target").GetProperty("existenceStatus").GetString());
        Assert.Equal(
            queue.GetProperty("target").GetProperty("resourceId").GetString(),
            topic.GetProperty("target").GetProperty("resourceId").GetString());
        Assert.Equal("queried", queue.GetProperty("targetEmitted").GetProperty("queryStatus").GetString());
        Assert.Equal(queue.GetProperty("targetEmitted").GetRawText(), topic.GetProperty("targetEmitted").GetRawText());
        foreach (var endpoint in endpoints.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.String, endpoint.GetProperty("name").ValueKind);
            Assert.Equal(JsonValueKind.String, endpoint.GetProperty("endpointType").ValueKind);
            Assert.Equal(JsonValueKind.Object, endpoint.GetProperty("hubEmitted").ValueKind);
            Assert.Equal(JsonValueKind.Object, endpoint.GetProperty("targetEmitted").ValueKind);
            Assert.False(endpoint.TryGetProperty("health", out _));
        }
    }
}
