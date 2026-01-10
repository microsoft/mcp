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
        }),
        // Sanitizes storageAccountTenantId in request bodies
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = Settings.TenantId,
          Value = "00000000-0000-0000-0000-000000000000",
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
                { "sync-group-name", Settings.ResourceBaseName }
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
                { "sync-group-name", Settings.ResourceBaseName },
                { "cloud-endpoint-name", Settings.ResourceBaseName }
            });

        var cloudEndpoint = result.AssertProperty("results");
        Assert.NotEqual(JsonValueKind.Null, cloudEndpoint.ValueKind);
    }

    [Fact(Skip = "QFE is in progress for protocol mismatch , causing deserialization issue")]
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
                { "sync-group-name", Settings.ResourceBaseName }
            });

        var serverEndpoints = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, serverEndpoints.ValueKind);
    }

    [Fact]
    public async Task Should_Crud_storage_sync_service()
    {
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

            var service = result.AssertProperty("result");
            Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
        }
        {
            var result = await CallToolAsync(
                "storagesync_service_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test" }
                });
            var service = result.AssertProperty("results");
            Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
        }
        {
            var result = await CallToolAsync(
                "storagesync_service_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test" },
                    { "incomingTrafficPolicy", $"AllowVirtualNetworksOnly" },
                    { "tags", "{\"Environment\":\"Test\"}" },
                    { "identityType", $"SystemAssigned" }
                });
            var service = result.AssertProperty("result");
            Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
        }
        {
            var result = await CallToolAsync(
                "storagesync_service_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test" }
                });
            var deleteResult = result.AssertProperty("message");
            Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
        }
    }

    [Fact]
    public async Task Should_Crud_sync_group()
    {
        // Create storage sync service
        {
            var result = await CallToolAsync(
                "storagesync_service_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test-service" },
                    { "location", "eastus" }
                });

            var service = result.AssertProperty("result");
            Assert.NotEqual(JsonValueKind.Null, service.ValueKind);
        }

        // Create sync group
        {
            var result = await CallToolAsync(
                "storagesync_syncgroup_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test-service" },
                    { "sync-group-name", $"{Settings.ResourceBaseName}-crud-test" }
                });

            var syncGroup = result.AssertProperty("result");
            Assert.NotEqual(JsonValueKind.Null, syncGroup.ValueKind);
        }

        // Get sync group
        {
            var result = await CallToolAsync(
                "storagesync_syncgroup_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test-service" },
                    { "sync-group-name", $"{Settings.ResourceBaseName}-crud-test" }
                });

            var syncGroup = result.AssertProperty("results");
            Assert.NotEqual(JsonValueKind.Null, syncGroup.ValueKind);
        }

        // Delete sync group
        {
            var result = await CallToolAsync(
                "storagesync_syncgroup_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test-service" },
                    { "sync-group-name", $"{Settings.ResourceBaseName}-crud-test" }
                });

            var deleteResult = result.AssertProperty("message");
            Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
        }

        // Delete storage sync service
        {
            var result = await CallToolAsync(
                "storagesync_service_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", $"{Settings.ResourceBaseName}-test-service" }
                });

            var deleteResult = result.AssertProperty("message");
            Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
        }
    }

    [Fact]
    public async Task Should_Crud_endpoint()
    {
        // Get existing cloud endpoint to retrieve storage account and file share details
        string storageAccountResourceId;
        string fileShareName;
        {
            var result = await CallToolAsync(
                "storagesync_cloudendpoint_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", Settings.ResourceBaseName },
                    { "sync-group-name", Settings.ResourceBaseName },
                    { "cloud-endpoint-name", Settings.ResourceBaseName }
                });

            var cloudEndpoint = result.AssertProperty("results");
            Assert.NotEqual(JsonValueKind.Null, cloudEndpoint.ValueKind);

            if (cloudEndpoint.ValueKind == JsonValueKind.Array)
            {
                var firstEndpoint = cloudEndpoint.EnumerateArray().First();
                storageAccountResourceId = firstEndpoint.GetProperty("storageAccountResourceId").GetString()!;
                fileShareName = firstEndpoint.GetProperty("azureFileShareName").GetString()!;
            }
            else
            {
                storageAccountResourceId = cloudEndpoint.GetProperty("storageAccountResourceId").GetString()!;
                fileShareName = cloudEndpoint.GetProperty("azureFileShareName").GetString()!;
            }
        }

        // Get server endpoints to save details before deletion
        List<(string name, string serverResourceId, string serverLocalPath)>? serverEndpointDetails = null;
        {
            var result = await CallToolAsync(
                "storagesync_serverendpoint_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", Settings.ResourceBaseName },
                    { "sync-group-name", Settings.ResourceBaseName }
                });

            var serverEndpoints = result.AssertProperty("results");
            if (serverEndpoints.ValueKind == JsonValueKind.Array)
            {
                serverEndpointDetails = serverEndpoints.EnumerateArray()
                    .Select(se => (
                        name: se.GetProperty("name").GetString()!,
                        serverResourceId: se.GetProperty("serverResourceId").GetString()!,
                        serverLocalPath: se.GetProperty("serverLocalPath").GetString()!
                    ))
                    .ToList();
            }
        }

        // Delete all server endpoints first (required before deleting cloud endpoint)
        if (serverEndpointDetails != null)
        {
            foreach (var endpoint in serverEndpointDetails)
            {
                var result = await CallToolAsync(
                    "storagesync_serverendpoint_delete",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "name", Settings.ResourceBaseName },
                        { "sync-group-name", Settings.ResourceBaseName },
                        { "server-endpoint-name", endpoint.name }
                    });

                var deleteResult = result.AssertProperty("message");
                Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
            }
        }

        // Delete existing cloud endpoint
        {
            var result = await CallToolAsync(
                "storagesync_cloudendpoint_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", Settings.ResourceBaseName },
                    { "sync-group-name", Settings.ResourceBaseName },
                    { "cloud-endpoint-name", Settings.ResourceBaseName }
                });

            var deleteResult = result.AssertProperty("message");
            Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
        }

        // Recreate cloud endpoint
        {
            var result = await CallToolAsync(
                "storagesync_cloudendpoint_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", Settings.ResourceBaseName },
                    { "sync-group-name", Settings.ResourceBaseName },
                    { "cloud-endpoint-name", Settings.ResourceBaseName },
                    { "storage-account-resource-id", storageAccountResourceId },
                    { "azure-file-share-name", fileShareName }
                });

            var cloudEndpoint = result.AssertProperty("result");
            Assert.NotEqual(JsonValueKind.Null, cloudEndpoint.ValueKind);
        }

        // Get cloud endpoint to verify it was recreated
        {
            var result = await CallToolAsync(
                "storagesync_cloudendpoint_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", Settings.ResourceBaseName },
                    { "sync-group-name", Settings.ResourceBaseName },
                    { "cloud-endpoint-name", Settings.ResourceBaseName }
                });

            var cloudEndpoint = result.AssertProperty("results");
            Assert.NotEqual(JsonValueKind.Null, cloudEndpoint.ValueKind);
        }

        // Recreate server endpoints to restore topology, or create a test endpoint if none existed
        if (serverEndpointDetails != null && serverEndpointDetails.Count > 0)
        {
            // Recreate existing endpoints
            foreach (var endpoint in serverEndpointDetails)
            {
                var result = await CallToolAsync(
                    "storagesync_serverendpoint_create",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "name", Settings.ResourceBaseName },
                        { "sync-group-name", Settings.ResourceBaseName },
                        { "server-endpoint-name", endpoint.name },
                        { "server-resource-id", endpoint.serverResourceId },
                        { "server-local-path", endpoint.serverLocalPath }
                    });

                var serverEndpoint = result.AssertProperty("result");
                Assert.NotEqual(JsonValueKind.Null, serverEndpoint.ValueKind);
            }
        }
        else
        {
            // No existing endpoints - create a test endpoint to validate creation
            // First, get registered servers to retrieve a valid server ID
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

            if (servers.GetArrayLength() > 0)
            {
                var firstServer = servers[0];
                var serverId = firstServer.GetProperty("id").GetString();

                var result = await CallToolAsync(
                    "storagesync_serverendpoint_create",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "name", Settings.ResourceBaseName },
                        { "sync-group-name", Settings.ResourceBaseName },
                        { "server-endpoint-name", $"{Settings.ResourceBaseName}-test" },
                        { "server-resource-id", serverId! },
                        { "server-local-path", $"D:\\{$"{Settings.ResourceBaseName}-test"}" }
                    });

                var serverEndpoint = result.AssertProperty("result");
                Assert.NotEqual(JsonValueKind.Null, serverEndpoint.ValueKind);
            }
            else
            {
                Output.WriteLine("Skipping server endpoint creation: No registered servers available");
            }
        }
    }

    [Fact]
    public async Task Should_trigger_cloud_endpoint_change_detection()
    {
        var result = await CallToolAsync(
            "storagesync_cloudendpoint_changedetection",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", Settings.ResourceBaseName },
                { "cloud-endpoint-name", Settings.ResourceBaseName },
                { "directory-path", "/" },
                { "change-detection-mode", "Recursive" },
                { "paths", new string[] { } }
            });

        var message = result.AssertProperty("message");
        Assert.NotEqual(JsonValueKind.Null, message.ValueKind);
    }

    [Fact]
    public async Task Should_update_server_endpoint()
    {
        // Get existing server endpoints
        var getResult = await CallToolAsync(
            "storagesync_serverendpoint_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", Settings.ResourceBaseName }
            });

        var serverEndpoints = getResult.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, serverEndpoints.ValueKind);

        if (serverEndpoints.GetArrayLength() != 0)
        {
            Assert.True(serverEndpoints.GetArrayLength() > 0, "No server endpoints exist to update. Run Should_Crud_endpoint test first to create endpoints.");

            // Use the first existing server endpoint
            var serverEndpointName = serverEndpoints[0].GetProperty("name").GetString()!;

            // Update the server endpoint
            var result = await CallToolAsync(
                "storagesync_serverendpoint_update",
                new()
                {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", Settings.ResourceBaseName },
                { "sync-group-name", Settings.ResourceBaseName },
                { "server-endpoint-name", serverEndpointName },
                { "cloud-tiering", true },
                { "volume-free-space-percent", 20 }
                });

            var serverEndpoint = result.AssertProperty("result");
            Assert.NotEqual(JsonValueKind.Null, serverEndpoint.ValueKind);
        }
        else
        {
            Output.WriteLine("Skipping test: No server endpoints available to update.");
        }
    }

    [Fact(Skip = "QFE is in progress for protocol mismatch , causing deserialization issue")]
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

    [Fact(Skip = "QFE is in progress for protocol mismatch , causing deserialization issue")]
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
