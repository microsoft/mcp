// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.LiveTests
{
    public class SignalRCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
    {
        [Fact]
        public async Task Should_get_signalr_runtimes_by_subscription_id()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new() { { "subscription", Settings.SubscriptionId } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            foreach (var runtime in runtimes.EnumerateArray())
            {
                Assert.Equal(JsonValueKind.Object, runtime.ValueKind);

                // Verify required properties exist
                var nameProperty = runtime.AssertProperty("name");
                Assert.False(string.IsNullOrEmpty(nameProperty.GetString()));
                var kindProperty = runtime.AssertProperty("kind");
                Assert.Equal("SignalR", kindProperty.GetString(), ignoreCase: true);
            }
        }

        [Fact]
        public async Task Should_handle_empty_subscription_gracefully()
        {
            // Empty subscription should trigger validation failure (400) -> null results
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new() { { "subscription", "" } });

            Assert.Null(result);
        }

        [Fact]
        public async Task Should_handle_invalid_subscription_gracefully()
        {
            // Invalid identifier should reach execution and return structured error details (HasValue)
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new()
                {
                    { "subscription", "invalid-subscription" }
                });

            Assert.NotNull(result);
            var message = result.AssertProperty("message");
            Assert.Contains("invalid-subscription", message.GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Should_get_signalr_runtimes_by_subscription_name()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new() { { "subscription", Settings.SubscriptionName } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_get_signalr_runtimes_by_subscription_name_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new() { { "subscription", Settings.SubscriptionName }, { "tenant", Settings.TenantId } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_get_signalr_runtimes_by_subscription_name_with_tenant_name()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new() { { "subscription", Settings.SubscriptionName }, { "tenant", Settings.TenantName } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_get_signalr_runtimes_by_subscription_with_resource_group()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new() { { "subscription", Settings.SubscriptionName }, { "resource-group", Settings.ResourceGroupName } });

            var runtimes = result.AssertProperty("runtimes");
            Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
            // Note: Array might be empty if no SignalR runtimes exist in subscription
        }

        [Fact]
        public async Task Should_get_signalr_runtime_detail()
        {
            var getResult = await CallToolAsync(
                "azmcp_signalr_runtime_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "signalr", Settings.ResourceBaseName }
                });

            var runtimes = getResult.AssertProperty("runtimes");
            var runtime = runtimes[0];
            Assert.Equal(JsonValueKind.Object, runtime.ValueKind);

            // Verify essential properties exist
            var nameProperty = runtime.AssertProperty("name");
            Assert.Equal(Settings.ResourceBaseName, nameProperty.GetString());

            var kindProperty = runtime.AssertProperty("kind");
            Assert.Equal("SignalR", kindProperty.GetString(), ignoreCase: true);
        }
    }
}
