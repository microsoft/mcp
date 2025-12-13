// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.LiveTests;

public class StorageSyncCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public override List<BodyRegexSanitizer> BodyRegexSanitizers => [
        // Sanitizes all URLs to remove actual service names
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
          GroupForReplace = "host",
        })
    ];

    [Fact]
    public async Task Should_list_storage_sync_services()
    {
        var result = await CallToolAsync(
            "storagesync_service_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var services = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, services.ValueKind);
    }

    [Fact]
    public async Task Should_get_storage_sync_service()
    {
        var result = await CallToolAsync(
            "storagesync_service_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var service = result.AssertProperty("result");
        Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
    }

    [Fact]
    public async Task Should_list_sync_groups()
    {
        var result = await CallToolAsync(
            "storagesync_syncgroup_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var syncGroups = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, syncGroups.ValueKind);
    }

    [Fact]
    public async Task Should_list_cloud_endpoints()
    {
        var result = await CallToolAsync(
            "storagesync_cloudendpoint_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" }
            });

        var endpoints = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, endpoints.ValueKind);
    }

    [Fact]
    public async Task Should_list_registered_servers()
    {
        var result = await CallToolAsync(
            "storagesync_registeredserver_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var servers = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);
    }

    [Fact]
    public async Task Should_list_server_endpoints()
    {
        var result = await CallToolAsync(
            "storagesync_serverendpoint_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" }
            });

        var endpoints = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, endpoints.ValueKind);
    }
}
