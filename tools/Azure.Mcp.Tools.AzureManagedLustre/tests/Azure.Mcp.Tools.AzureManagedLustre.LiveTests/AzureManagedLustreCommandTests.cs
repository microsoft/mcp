// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.AzureManagedLustre.LiveTests
{
    public class AzureManagedLustreCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
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
                "azmcp_azuremanagedlustre_filesystem_required-subnet-size",
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
        public async Task Should_get_sku_info()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_sku_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId }
                });

            var skus = result.AssertProperty("skus");
            Assert.Equal(JsonValueKind.Array, skus.ValueKind);
        }

        [Fact]
        public async Task Should_get_sku_info_zonal_support()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_sku_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "location", "westeurope" }
                });

            var skus = result.AssertProperty("skus");
            foreach (var sku in skus.EnumerateArray())
            {
                Assert.True(sku.TryGetProperty("supportsZones", out var supportsZones), "Property 'supportsZones' is missing.");
                Assert.True(supportsZones.GetBoolean(), "'supportsZones' must be true.");
            }
        }

        [Fact]
        public async Task Should_get_sku_info_no_zonal_support()
        {
            var result = await CallToolAsync(
                "azmcp_azuremanagedlustre_filesystem_sku_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "location", "westus" }
                });

            var skus = result.AssertProperty("skus");
            Assert.Equal(JsonValueKind.Array, skus.ValueKind);
            foreach (var sku in skus.EnumerateArray())
            {
                Assert.True(sku.TryGetProperty("supportsZones", out var supportsZones), "Property 'supportsZones' is missing.");
                Assert.False(supportsZones.GetBoolean(), "'supportsZones' must be false.");
            }
        }

    }
}
