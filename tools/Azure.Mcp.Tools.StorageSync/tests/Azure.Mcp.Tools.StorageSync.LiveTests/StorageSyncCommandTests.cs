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
    public override List<UriRegexSanitizer> UriRegexSanitizers => new[]
    {
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "resource[gG]roups\\/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        })
    }.ToList();

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers => new[]
    {
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceGroupName,
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceBaseName,
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.SubscriptionId,
            Value = "00000000-0000-0000-0000-000000000000",
        })
    }.ToList();

    public override List<BodyRegexSanitizer> BodyRegexSanitizers => [
        // Sanitizes all URLs to remove actual service names
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
          GroupForReplace = "host",
        })
    ];

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

        var service = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
    }

    [Fact]
    public async Task Should_get_sync_group()
    {
        var result = await CallToolAsync(
            "storagesync_syncgroup_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" }
            });

        var syncGroup = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, syncGroup.ValueKind);
    }

    [Fact]
    public async Task Should_get_cloud_endpoint()
    {
        var result = await CallToolAsync(
            "storagesync_cloudendpoint_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "cloud-endpoint-name", $"{Settings.ResourceBaseName}-ce" }
            });

        var cloudEndpoint = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, cloudEndpoint.ValueKind);
    }

    [Fact]
    public async Task Should_get_registered_servers()
    {
        var result = await CallToolAsync(
            "storagesync_registeredserver_get",
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
    public async Task Should_get_server_endpoints()
    {
        var result = await CallToolAsync(
            "storagesync_serverendpoint_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" }
            });

        var serverEndpoints = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, serverEndpoints.ValueKind);
    }

    [Fact]
    public async Task Should_create_storage_sync_service()
    {
        var result = await CallToolAsync(
            "storagesync_service_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", $"{Settings.ResourceBaseName}-test" },
                { "location", "eastus" }
            });

        var service = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
    }

    [Fact]
    public async Task Should_update_storage_sync_service()
    {
        var result = await CallToolAsync(
            "storagesync_service_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "tags", "{\"Environment\":\"Test\"}" }
            });

        var service = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
    }

    [Fact]
    public async Task Should_delete_storage_sync_service()
    {
        var result = await CallToolAsync(
            "storagesync_service_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", $"{Settings.ResourceBaseName}-test" }
            });

        var deleteResult = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
    }

    [Fact]
    public async Task Should_create_sync_group()
    {
        var result = await CallToolAsync(
            "storagesync_syncgroup_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg-test" }
            });

        var syncGroup = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, syncGroup.ValueKind);
    }

    [Fact]
    public async Task Should_delete_sync_group()
    {
        var result = await CallToolAsync(
            "storagesync_syncgroup_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg-test" }
            });

        var deleteResult = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
    }

    [Fact]
    public async Task Should_create_cloud_endpoint()
    {
        var result = await CallToolAsync(
            "storagesync_cloudendpoint_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "cloud-endpoint-name", $"{Settings.ResourceBaseName}-ce-test" },
                { "storage-account-resource-id", $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{Settings.ResourceBaseName}sa" },
                { "azure-file-share-name", "testshare" }
            });

        var cloudEndpoint = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, cloudEndpoint.ValueKind);
    }

    [Fact]
    public async Task Should_delete_cloud_endpoint()
    {
        var result = await CallToolAsync(
            "storagesync_cloudendpoint_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "cloud-endpoint-name", $"{Settings.ResourceBaseName}-ce-test" }
            });

        var deleteResult = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
    }

    [Fact]
    public async Task Should_trigger_cloud_endpoint_change_detection()
    {
        var result = await CallToolAsync(
            "storagesync_cloudendpoint_triggerchangedetection",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "cloud-endpoint-name", $"{Settings.ResourceBaseName}-ce" }
            });

        var triggerResult = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, triggerResult.ValueKind);
    }

    [Fact]
    public async Task Should_create_server_endpoint()
    {
        // First, get the registered servers to retrieve a valid server ID
        var serversResult = await CallToolAsync(
            "storagesync_registeredserver_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var servers = serversResult.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);

        // Skip test if no registered servers exist
        if (servers.GetArrayLength() == 0)
        {
            Output.WriteLine("Skipping test: No registered servers available");
            return;
        }

        var firstServer = servers[0];
        var serverId = firstServer.GetProperty("id").GetString();

        var result = await CallToolAsync(
            "storagesync_serverendpoint_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "server-endpoint-name", $"{Settings.ResourceBaseName}-se-test" },
                { "server-resource-id", serverId },
                { "server-local-path", "D:\\\\SyncFolder" }
            });

        var serverEndpoint = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, serverEndpoint.ValueKind);
    }

    [Fact]
    public async Task Should_update_server_endpoint()
    {
        var result = await CallToolAsync(
            "storagesync_serverendpoint_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "server-endpoint-name", $"{Settings.ResourceBaseName}-se" },
                { "cloud-tiering", "on" },
                { "volume-free-space-percent", "20" }
            });

        var serverEndpoint = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, serverEndpoint.ValueKind);
    }

    [Fact]
    public async Task Should_delete_server_endpoint()
    {
        var result = await CallToolAsync(
            "storagesync_serverendpoint_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", $"{Settings.ResourceBaseName}-sg" },
                { "server-endpoint-name", $"{Settings.ResourceBaseName}-se-test" }
            });

        var deleteResult = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
    }

    [Fact]
    public async Task Should_update_registered_server()
    {
        // First, get the registered servers to retrieve a valid server ID
        var serversResult = await CallToolAsync(
            "storagesync_registeredserver_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var servers = serversResult.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);

        // Skip test if no registered servers exist
        if (servers.GetArrayLength() == 0)
        {
            Output.WriteLine("Skipping test: No registered servers available");
            return;
        }

        var firstServer = servers[0];
        var serverId = firstServer.GetProperty("serverId").GetString() ??
                       firstServer.GetProperty("name").GetString();

        var result = await CallToolAsync(
            "storagesync_registeredserver_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "server-id", serverId },
                { "friendly-name", "UpdatedServerName" }
            });

        var server = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, server.ValueKind);
    }

    [Fact]
    public async Task Should_unregister_registered_server()
    {
        // First, get the registered servers to retrieve a valid server ID
        var serversResult = await CallToolAsync(
            "storagesync_registeredserver_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName }
            });

        var servers = serversResult.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, servers.ValueKind);

        // Skip test if no registered servers exist
        if (servers.GetArrayLength() == 0)
        {
            Output.WriteLine("Skipping test: No registered servers available");
            return;
        }

        // Use the last server for unregister to avoid breaking other tests
        var lastServer = servers[servers.GetArrayLength() - 1];
        var serverId = lastServer.GetProperty("serverId").GetString() ??
                       lastServer.GetProperty("name").GetString();

        var result = await CallToolAsync(
            "storagesync_registeredserver_unregister",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "server-id", serverId }
            });

        var unregisterResult = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, unregisterResult.ValueKind);
    }
}
