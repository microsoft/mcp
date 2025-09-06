// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AzureManagedLustre.LiveTests
{
    public class AzureManagedLustreCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
        : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
    {
        [Fact]
        public async Task Should_list_filesystems_by_subscription()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId }
                });

            var fileSystems = result.AssertProperty("fileSystems");
            Assert.Equal(JsonValueKind.Array, fileSystems.ValueKind);
        }

        [Fact]
        public async Task Should_calculate_required_subnet_size()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_subnetsize_ask",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "sku", "AMLFS-Durable-Premium-40" },
                    { "size", 480 }
                });

            var ips = result.AssertProperty("numberOfRequiredIPs");
            Assert.Equal(JsonValueKind.Number, ips.ValueKind);
        }

        [Fact]
        public async Task Should_check_subnet_size_and_succeed()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_subnetsize_validate",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "sku", "AMLFS-Durable-Premium-40" },
                    { "size", 480 },
                    { "location", Environment.GetEnvironmentVariable("LOCATION") },
                    { "subnet-id", Environment.GetEnvironmentVariable("AMLFS_SUBNET_ID") }
                });

            var valid = result.AssertProperty("valid");
            Assert.Equal(JsonValueKind.True, valid.ValueKind);
            Assert.True(valid.GetBoolean());
        }

        [Fact]
        public async Task Should_check_subnet_size_and_fail()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_subnetsize_validate",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "sku", "AMLFS-Durable-Premium-40" },
                    { "size", 1008 },
                    { "location", Environment.GetEnvironmentVariable("LOCATION") },
                    { "subnet-id", Environment.GetEnvironmentVariable("AMLFS_SUBNET_SMALL_ID") }
                });

            var valid = result.AssertProperty("valid");
            Assert.Equal(JsonValueKind.False, valid.ValueKind);
            Assert.False(valid.GetBoolean());
        }
    }
}
