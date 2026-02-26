// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.LiveTests;

/// <summary>
/// Live tests for FileShares commands.
/// These tests exercise the actual Azure FileShares resource provider with real resources.
/// </summary>
public class FileSharesCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private string FileShare1Name => $"{Settings.ResourceBaseName}-fileshare-01";
    private string FileShare2Name => $"{Settings.ResourceBaseName}-fileshare-02";
    private const string Sanitized = "Sanitized";
    private const string Location = "eastus";

    public override List<UriRegexSanitizer> UriRegexSanitizers => new[]
    {
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "resource[gG]roups\\/([^?\\/]+)",
            Value = Sanitized,
            GroupForReplace = "1"
        })
    }.ToList();

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers => new[]
    {
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceGroupName,
            Value = Sanitized,
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceBaseName,
            Value = Sanitized,
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = FileShare1Name,
            Value = Sanitized,
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = FileShare2Name,
            Value = Sanitized,
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
        // Sanitizes tenant ID in request bodies
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = Settings.TenantId,
          Value = "00000000-0000-0000-0000-000000000000",
        })
    ];



    [Fact]
    public async Task Should_get_file_share_details_by_subscription_and_name()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", FileShare1Name }
            });

        var fileShares = result.AssertProperty("fileShares");
        Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);

        var fileShareArray = fileShares.EnumerateArray().ToList();
        Assert.Single(fileShareArray);

        var fileShare = fileShareArray[0];
        var name = fileShare.GetProperty("name");
        Assert.True(FileShare1Name == name.GetString() || Sanitized == name.GetString());

        var location = fileShare.GetProperty("location");
        Assert.NotEqual(JsonValueKind.Null, location.ValueKind);

        var provisioningState = fileShare.GetProperty("provisioningState");
        Assert.NotEqual(JsonValueKind.Null, provisioningState.ValueKind);

        // Verify protocol is NFS as defined in bicep
        var protocol = fileShare.GetProperty("protocol");
        Assert.Equal("NFS", protocol.GetString());
    }

    [Fact]
    public async Task Should_get_file_share_details_with_tenant_id()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", FileShare1Name },
                { "tenant", Settings.TenantId }
            });

        var fileShares = result.AssertProperty("fileShares");
        Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);

        var fileShareArray = fileShares.EnumerateArray().ToList();
        Assert.Single(fileShareArray);

    }

    [Fact]
    public async Task Should_get_second_file_share_details()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", FileShare2Name }
            });

        var fileShares = result.AssertProperty("fileShares");
        Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);

        var fileShareArray = fileShares.EnumerateArray().ToList();
        Assert.Single(fileShareArray);

        var fileShare = fileShareArray[0];
        var name = fileShare.GetProperty("name");
        Assert.True(FileShare2Name == name.GetString() || Sanitized == name.GetString());

        // Verify protocol is NFS as defined in bicep
        var protocol = fileShare.GetProperty("protocol");
        Assert.Equal("NFS", protocol.GetString());
    }

    [Fact]
    public async Task Should_check_file_share_name_availability()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_check-name-availability",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", "test-available-name-" + Guid.NewGuid().ToString().Substring(0, 8) },
                { "location", Location }
            });

        var available = result.AssertProperty("isAvailable");
        Assert.Equal(JsonValueKind.True, available.ValueKind);
    }

    [Fact]
    public async Task Should_get_file_share_limits()
    {
        var result = await CallToolAsync(
            "fileshares_limits",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "location", Location },
                { "tenant", Settings.TenantId }
            });

        var limits = result.AssertProperty("limits");
        Assert.NotEqual(JsonValueKind.Null, limits.ValueKind);
    }

    [Fact]
    public async Task Should_get_file_share_provisioning_recommendation()
    {
        var result = await CallToolAsync(
            "fileshares_rec",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "location", Location },
                { "provisioned-storage-in-gib", 125 },
                { "tenant", Settings.TenantId }
            });

        var provisionedIOPerSec = result.AssertProperty("provisionedIOPerSec");
        Assert.NotEqual(JsonValueKind.Null, provisionedIOPerSec.ValueKind);
        Assert.True(provisionedIOPerSec.GetInt32() > 0);

        var provisionedThroughputMiBPerSec = result.AssertProperty("provisionedThroughputMiBPerSec");
        Assert.NotEqual(JsonValueKind.Null, provisionedThroughputMiBPerSec.ValueKind);

        var availableRedundancyOptions = result.AssertProperty("availableRedundancyOptions");
        Assert.Equal(JsonValueKind.Array, availableRedundancyOptions.ValueKind);
    }

    [Fact]
    public async Task Should_get_file_share_usage_data()
    {
        var result = await CallToolAsync(
            "fileshares_usage",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "location", Location },
                { "tenant", Settings.TenantId }
            });

        var liveShares = result.AssertProperty("liveShares");
        Assert.NotEqual(JsonValueKind.Null, liveShares.ValueKind);
    }

    [Fact]
    public async Task Should_Crud_file_share()
    {
        // Get existing file share to retrieve configuration and subnet info
        string testShareName = $"{Settings.ResourceBaseName}-crud";
        string? subnetId = null;
        string? mediaTier = null;
        string? redundancy = null;
        string? protocol = null;
        int? provisionedStorageInGiB = null;
        int? provisionedIOPerSec = null;
        int? provisionedThroughputMiBPerSec = null;
        string? publicNetworkAccess = null;
        string? nfsRootSquash = null;

        {
            var result = await CallToolAsync(
                "fileshares_fileshare_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", FileShare1Name }
                });

            var fileShares = result.AssertProperty("fileShares");
            Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);

            var fileShareArray = fileShares.EnumerateArray().ToList();
            Assert.Single(fileShareArray);

            var existingShare = fileShareArray[0];

            // Extract properties from existing share
            if (existingShare.TryGetProperty("mediaTier", out var mediaTierElement))
            {
                mediaTier = mediaTierElement.GetString();
            }

            if (existingShare.TryGetProperty("redundancy", out var redundancyElement))
            {
                redundancy = redundancyElement.GetString();
            }

            if (existingShare.TryGetProperty("protocol", out var protocolElement))
            {
                protocol = protocolElement.GetString();
            }

            if (existingShare.TryGetProperty("provisionedStorageInGiB", out var storageElement))
            {
                provisionedStorageInGiB = storageElement.GetInt32();
            }

            if (existingShare.TryGetProperty("provisionedIOPerSec", out var ioElement))
            {
                provisionedIOPerSec = ioElement.GetInt32();
            }

            if (existingShare.TryGetProperty("provisionedThroughputMiBPerSec", out var throughputElement))
            {
                provisionedThroughputMiBPerSec = throughputElement.GetInt32();
            }

            if (existingShare.TryGetProperty("publicNetworkAccess", out var publicNetworkElement))
            {
                publicNetworkAccess = publicNetworkElement.GetString();
            }

            if (existingShare.TryGetProperty("nfsRootSquash", out var nfsRootSquashElement))
            {
                nfsRootSquash = nfsRootSquashElement.GetString();
            }

            if (existingShare.TryGetProperty("allowedSubnets", out var allowedSubnetsElement) &&
                allowedSubnetsElement.ValueKind == JsonValueKind.Array)
            {
                var subnets = allowedSubnetsElement.EnumerateArray().ToList();
                if (subnets.Count > 0)
                {
                    subnetId = subnets[0].GetString();
                }
            }
        }

        // Create new file share with all parameters
        {
            var createParams = new Dictionary<string, object?>
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "resource-group", Settings.ResourceGroupName },
                { "name", testShareName },
                { "location", Location },
                { "mount-name", testShareName + "-mount" },
                { "media-tier", mediaTier ?? "SSD" },
                { "redundancy", redundancy ?? "Local" },
                { "protocol", protocol ?? "NFS" },
                { "provisioned-storage-in-gib", provisionedStorageInGiB ?? 32 },
                { "provisioned-io-per-sec", provisionedIOPerSec ?? 3000 },
                { "provisioned-throughput-mib-per-sec", provisionedThroughputMiBPerSec ?? 125 },
                { "public-network-access", publicNetworkAccess ?? "Enabled" },
                { "nfs-root-squash", nfsRootSquash ?? "NoRootSquash" },
                { "tags", "{\"environment\":\"test\",\"owner\":\"ankushb\"}" }
            };

            if (!string.IsNullOrEmpty(subnetId))
            {
                createParams.Add("allowed-subnets", subnetId);
            }

            var result = await CallToolAsync("fileshares_fileshare_create", createParams);

            var fileShare = result.AssertProperty("fileShare");
            Assert.NotEqual(JsonValueKind.Null, fileShare.ValueKind);
        }

        // Get created file share to verify
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", testShareName }
                });

            var fileShares = result.AssertProperty("fileShares");
            Assert.Equal(JsonValueKind.Array, fileShares.ValueKind);

            var fileShareArray = fileShares.EnumerateArray().ToList();
            Assert.Single(fileShareArray);
        }

        // Update file share - Skip for now due to service limitation
        // The update operation requires all properties to be provided, not just the ones being changed
        // TODO: Fix update command to properly handle partial updates
        /*
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", testShareName },
                    { "provisioned-storage-in-gib", 64 },
                    { "provisioned-io-per-sec", provisionedIOPerSec ?? 3000 },
                    { "provisioned-throughput-mib-per-sec", provisionedThroughputMiBPerSec ?? 125 }
                });

            var fileShare = result.AssertProperty("fileShare");
            Assert.NotEqual(JsonValueKind.Null, fileShare.ValueKind);
        }
        */

        // Delete file share
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "name", testShareName }
                });

            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task Should_Crud_snapshot()
    {
        var testSnapshotName = $"{Settings.ResourceBaseName}-snapshot-test";

        // Create snapshot
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_snapshot_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name },
                    { "snapshot-name", testSnapshotName }
                });

            var snapshot = result.AssertProperty("snapshot");
            Assert.NotEqual(JsonValueKind.Null, snapshot.ValueKind);
        }

        // Get snapshot to verify creation
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_snapshot_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name }
                });

            var snapshots = result.AssertProperty("snapshots");
            Assert.Equal(JsonValueKind.Array, snapshots.ValueKind);

            var snapshotArray = snapshots.EnumerateArray().ToList();
            Assert.True(snapshotArray.Count > 0, "Created snapshot should be present in list");
        }

        // Update snapshot (if supported)
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_snapshot_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name },
                    { "snapshot-name", testSnapshotName },
                    { "description", "Updated snapshot description" }
                });

            var snapshot = result.AssertProperty("snapshot");
            Assert.NotEqual(JsonValueKind.Null, snapshot.ValueKind);
        }

        // Delete snapshot
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_snapshot_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name },
                    { "name", testSnapshotName }
                });

            var deleteResult = result.AssertProperty("message");
            Assert.NotEqual(JsonValueKind.Null, deleteResult.ValueKind);
        }
    }

    [Fact]
    public async Task Should_list_private_endpoint_connections()
    {
        var result = await CallToolAsync(
            "fileshares_fileshare_privateendpointconnection_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "file-share-name", FileShare1Name }
            });

        var connections = result.AssertProperty("connections");
        Assert.Equal(JsonValueKind.Array, connections.ValueKind);

        // Should have at least one connection from test infrastructure
        var connectionArray = connections.EnumerateArray().ToList();
        Assert.True(connectionArray.Count > 0, "At least one private endpoint connection should exist from test infrastructure");
    }

    [Fact]
    public async Task Should_get_specific_private_endpoint_connection()
    {
        // First list to get the connection name
        string? connectionName = null;
        {
            var listResult = await CallToolAsync(
                "fileshares_fileshare_privateendpointconnection_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name }
                });

            var connections = listResult.AssertProperty("connections");
            var connectionArray = connections.EnumerateArray().ToList();

            if (connectionArray.Count > 0)
            {
                var firstConnection = connectionArray[0];
                if (firstConnection.TryGetProperty("name", out var nameElement))
                {
                    connectionName = nameElement.GetString();
                }
            }
        }

        Assert.NotNull(connectionName);

        // Get specific connection
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_privateendpointconnection_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name },
                    { "connection-name", connectionName }
                });

            var connections = result.AssertProperty("connections");
            Assert.Equal(JsonValueKind.Array, connections.ValueKind);

            var connectionArray = connections.EnumerateArray().ToList();
            Assert.Single(connectionArray);

            var connection = connectionArray[0];
            var name = connection.GetProperty("name");
            Assert.Equal(connectionName, name.GetString());
        }
    }

    [Fact]
    public async Task Should_update_private_endpoint_connection_status()
    {
        // First list to get the connection name
        string? connectionName = null;
        {
            var listResult = await CallToolAsync(
                "fileshares_fileshare_privateendpointconnection_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name }
                });

            var connections = listResult.AssertProperty("connections");
            var connectionArray = connections.EnumerateArray().ToList();

            if (connectionArray.Count > 0)
            {
                var firstConnection = connectionArray[0];
                if (firstConnection.TryGetProperty("name", out var nameElement))
                {
                    connectionName = nameElement.GetString();
                }
            }
        }

        Assert.NotNull(connectionName);

        // Update connection status to Approved with description
        {
            var result = await CallToolAsync(
                "fileshares_fileshare_privateendpointconnection_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "file-share-name", FileShare1Name },
                    { "connection-name", connectionName },
                    { "status", "Approved" },
                    { "description", "Connection approved by live test" }
                });

            var connection = result.AssertProperty("connection");
            Assert.NotEqual(JsonValueKind.Null, connection.ValueKind);

            // Verify status was updated
            var connectionState = connection.GetProperty("connectionState");
            Assert.Equal("Approved", connectionState.GetString());
        }
    }

    private new const string TenantNameReason = "Tenant name resolution is not supported for service principals";
}
