// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.LiveTests
{
    public class SignalRCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
        : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
    {
        [Fact]
        public async Task Should_list_signalr_runtimes_by_subscription_id()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_list",
                new() { { "subscription", Settings.SubscriptionId } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_list_signalr_runtimes_by_subscription_name()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_list",
                new() { { "subscription", Settings.SubscriptionName } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_list_signalr_runtimes_by_subscription_name_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_list",
                new() { { "subscription", Settings.SubscriptionName }, { "tenant", Settings.TenantId } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_list_signalr_runtimes_by_subscription_name_with_tenant_name()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_signalr_runtime_list",
                new() { { "subscription", Settings.SubscriptionName }, { "tenant", Settings.TenantName } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_show_signalr_runtime_details()
        {
            // First get the list of runtimes to find one to test with
            var getResult = await CallToolAsync(
                "azmcp_signalr_runtime_show",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName }
                });

            var runtime = getResult.AssertProperty("runtime");
            Assert.Equal(JsonValueKind.Object, runtime.ValueKind);

            // Verify essential properties exist
            runtime.AssertProperty("name");
            runtime.AssertProperty("location");
            runtime.AssertProperty("resourceGroupName");
            runtime.AssertProperty("skuTier");
            runtime.AssertProperty("provisioningState");
        }

        [Fact]
        public async Task Should_list_signalr_access_keys()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_key_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName }
                });

            var keys = result.AssertProperty("keys");
            Assert.Equal(JsonValueKind.Object, keys.ValueKind);

            // Verify essential key properties exist
            keys.AssertProperty("primaryKey");
            keys.AssertProperty("secondaryKey");
            keys.AssertProperty("primaryConnectionString");
            keys.AssertProperty("secondaryConnectionString");
        }

        [Fact]
        public async Task Should_list_signalr_access_keys_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_key_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName },
                    { "tenant", Settings.TenantId }
                });

            var keys = result.AssertProperty("keys");
            Assert.Equal(JsonValueKind.Object, keys.ValueKind);

            // Verify essential key properties exist
            keys.AssertProperty("primaryKey");
            keys.AssertProperty("secondaryKey");
        }

        [Fact]
        public async Task Should_list_signalr_network_rules()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_network-rule_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName }
                });

            var networkRules = result.AssertProperty("networkRules");
            Assert.Equal(JsonValueKind.Object, networkRules.ValueKind);

            // Verify network rules properties exist
            if (networkRules.TryGetProperty("publicNetwork", out var publicNetwork))
            {
                Assert.Equal(JsonValueKind.Object, publicNetwork.ValueKind);
            }

            if (networkRules.TryGetProperty("privateEndpoints", out var privateEndpoints))
            {
                Assert.Equal(JsonValueKind.Array, privateEndpoints.ValueKind);
            }
        }

        [Fact]
        public async Task Should_list_signalr_network_rules_with_subscription_name()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_network-rule_list",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName }
                });

            var networkRules = result.AssertProperty("networkRules");
            Assert.Equal(JsonValueKind.Object, networkRules.ValueKind);
        }

        [Fact]
        public async Task Should_show_signalr_identity_configuration()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_identity_show",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName }
                });

            var identity = result.AssertProperty("identity");
            Assert.Equal(JsonValueKind.Object, identity.ValueKind);

            // Verify identity properties exist
            identity.AssertProperty("type");

            // Optional properties that may exist based on identity configuration
            if (identity.TryGetProperty("principalId", out var principalId))
            {
                Assert.Equal(JsonValueKind.String, principalId.ValueKind);
            }

            if (identity.TryGetProperty("tenantId", out var tenantId))
            {
                Assert.Equal(JsonValueKind.String, tenantId.ValueKind);
            }

            if (identity.TryGetProperty("userAssignedIdentities", out var userAssignedIdentities))
            {
                // This should be null when system-assigned identity is used
                Assert.Equal(JsonValueKind.Null, userAssignedIdentities.ValueKind);
            }
        }

        [Fact]
        public async Task Should_show_signalr_identity_with_tenant_name()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_signalr_identity_show",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", Settings.ResourceBaseName },
                    { "tenant", Settings.TenantName }
                });

            var identity = result.AssertProperty("identity");
            Assert.Equal(JsonValueKind.Object, identity.ValueKind);
            identity.AssertProperty("type");
        }

        [Fact]
        public async Task Should_handle_invalid_signalr_name_gracefully()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_show",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr-name", "non-existent-signalr-service" }
                });

            // Should return runtime error response with error details
            Assert.True(result.HasValue);
            var errorDetails = result.Value;
            Assert.True(errorDetails.TryGetProperty("message", out _));
            Assert.True(errorDetails.TryGetProperty("type", out var typeProperty));
            Assert.Equal("Exception", typeProperty.GetString());
        }

        [Fact]
        public async Task Should_validate_required_parameters_for_show_command()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_show",
                new()
                {
                    { "subscription", Settings.SubscriptionId }
                    // Missing resource-group and signalr-name
                });

            // Should return validation error (no results)
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task Should_validate_required_parameters_for_key_list()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_key_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId }, { "resource-group", Settings.ResourceGroupName }
                    // Missing signalr-name
                });

            // Should return validation error (no results)
            Assert.False(result.HasValue);
        }
    }
}
