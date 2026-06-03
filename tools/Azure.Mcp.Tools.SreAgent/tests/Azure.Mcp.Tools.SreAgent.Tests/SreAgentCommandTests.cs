// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests
{
    public class SreAgentCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
        : RecordedCommandTestsBase(output, fixture, liveServerFixture)
    {
        // Disable body comparison: SRE Agent data-plane responses contain dynamic fields
        // (timestamps, generated IDs, system state) that would cause spurious playback mismatches.
        public override CustomDefaultMatcher? TestMatcher => new()
        {
            ExcludedHeaders = "Authorization,Content-Type",
            CompareBodies = false
        };

        // Sanitize SRE Agent data-plane hostname in response bodies so recordings don't
        // contain the real resource name (e.g. "mcpfb80ce3a--e5d0b29a.00632926.eastus2.azuresre.ai").
        // Also sanitize tenant IDs and Owners fields (resource creator alias) from response bodies.
        public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
        [
            new BodyRegexSanitizer(new BodyRegexSanitizerBody
            {
                Regex = @"(?<=https://)(?<host>[^/""\s]+\.azuresre\.ai)",
                GroupForReplace = "host",
                Value = "sanitized.eastus2.azuresre.ai"
            }),
            new BodyRegexSanitizer(new BodyRegexSanitizerBody
            {
                Regex = @"(?<=/tenants/)(?<tenantId>[0-9a-fA-F-]{36})",
                GroupForReplace = "tenantId",
                Value = "00000000-0000-0000-0000-000000000000"
            }),
            new BodyRegexSanitizer(new BodyRegexSanitizerBody
            {
                Regex = @"(?<=""Owners""\s*:\s*"")(?<owner>[^""]+)",
                GroupForReplace = "owner",
                Value = "sanitized"
            })
        ];

        // Sanitize SRE Agent data-plane hostname in request/response URIs.
        public override List<UriRegexSanitizer> UriRegexSanitizers =>
        [
            new UriRegexSanitizer(new UriRegexSanitizerBody
            {
                Regex = @"(?<=https://)(?<host>[^/]+\.azuresre\.ai)",
                GroupForReplace = "host",
                Value = "sanitized.eastus2.azuresre.ai"
            })
        ];

        // Sanitize x-ms-operation-identifier response header which contains real tenant ID and object ID.
        public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
        [
            new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
            {
                Value = "sanitized"
            })
        ];

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

        [Fact]
        public async Task Should_list_agent_tools()
        {
            var result = await CallToolAsync(
                "sreagent_agents_tools_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_skills()
        {
            var result = await CallToolAsync(
                "sreagent_skills_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_hooks()
        {
            var result = await CallToolAsync(
                "sreagent_hooks_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_incident_plans()
        {
            var result = await CallToolAsync(
                "sreagent_incidents_plans_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_list_memories()
        {
            var result = await CallToolAsync(
                "sreagent_docs_memories_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_create_get_and_delete_common_prompt()
        {
            const string promptName = "live-test-prompt";

            // Create
            var createResult = await CallToolAsync(
                "sreagent_commonprompts_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", promptName },
                    { "content", "You are a helpful SRE assistant." }
                });
            Assert.NotNull(createResult);

            // Get
            var getResult = await CallToolAsync(
                "sreagent_commonprompts_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", promptName }
                });
            Assert.NotNull(getResult);

            // Delete
            var deleteResult = await CallToolAsync(
                "sreagent_commonprompts_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", promptName },
                    { "confirm", true }
                });
            Assert.NotNull(deleteResult);
        }

        [Fact]
        public async Task Should_add_search_and_delete_memory()
        {
            const string memoryName = "live-test-memory.md";

            // Add
            var addResult = await CallToolAsync(
                "sreagent_docs_memories_add",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", memoryName },
                    { "content", "# Live Test Memory\nThis document is used by automated live tests." }
                });
            Assert.NotNull(addResult);

            // Search (best-effort; indexing may be asynchronous)
            var searchResult = await CallToolAsync(
                "sreagent_docs_memories_search",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "query", "live test memory" }
                });
            Assert.NotNull(searchResult);

            // Delete
            var deleteResult = await CallToolAsync(
                "sreagent_docs_memories_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", memoryName },
                    { "confirm", true }
                });
            Assert.NotNull(deleteResult);
        }

        [Fact]
        public async Task Should_reindex_memories()
        {
            var result = await CallToolAsync(
                "sreagent_docs_memories_reindex",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName }
                });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_create_get_and_delete_mcp_connector()
        {
            const string connectorName = "live-test-mcp-connector";

            // Create (HTTP type with a placeholder endpoint)
            var createResult = await CallToolAsync(
                "sreagent_connectors_create_mcp",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", connectorName },
                    { "type", "http" },
                    { "endpoint", "https://example.com/mcp" }
                });
            Assert.NotNull(createResult);

            // Get
            var getResult = await CallToolAsync(
                "sreagent_connectors_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", connectorName }
                });
            Assert.NotNull(getResult);

            // Delete
            var deleteResult = await CallToolAsync(
                "sreagent_connectors_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", connectorName },
                    { "confirm", true }
                });
            Assert.NotNull(deleteResult);
        }

        [Fact]
        public async Task Should_create_and_delete_skill()
        {
            const string skillName = "live-test-skill";

            // Create
            var createResult = await CallToolAsync(
                "sreagent_skills_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", skillName },
                    { "content", "## Restart Service\nRestart the given service using `systemctl restart <service>`." },
                    { "description", "Runbook for restarting a service" }
                });
            Assert.NotNull(createResult);

            // Delete
            var deleteResult = await CallToolAsync(
                "sreagent_skills_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "agent", Settings.ResourceBaseName },
                    { "name", skillName },
                    { "confirm", true }
                });
            Assert.NotNull(deleteResult);
        }

    }
}
