// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.LiveTests
{
    public class SreAgentCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
        : RecordedCommandTestsBase(output, fixture, liveServerFixture)
    {
        [Fact]
        public async Task Should_list_sre_agents_by_subscription_id()
        {
            var result = await CallToolAsync(
                "sreagent_agents_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId }
                });

            // Result may be an array directly or wrapped; just assert call succeeded.
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_get_sre_agent_details()
        {
            var result = await CallToolAsync(
                "sreagent_agents_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_threads()
        {
            var result = await CallToolAsync(
                "sreagent_threads_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_connectors()
        {
            var result = await CallToolAsync(
                "sreagent_connectors_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_scheduled_tasks()
        {
            var result = await CallToolAsync(
                "sreagent_scheduledtasks_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_active_incidents()
        {
            var result = await CallToolAsync(
                "sreagent_incidents_active_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_common_prompts()
        {
            var result = await CallToolAsync(
                "sreagent_commonprompts_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }
    }
}
