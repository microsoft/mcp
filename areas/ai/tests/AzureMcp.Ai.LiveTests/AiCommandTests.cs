// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace AzureMcp.Ai.LiveTests
{
    [Trait("Area", "Ai")]
    [Trait("Category", "Live")]
    public class AiCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
        : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
    {
        // Static resource names for live testing
        // These match the actual deployed Azure OpenAI resources in the static test environment
        private const string OpenAiResourceName = "azmcp-test";
        private const string DefaultDeploymentName = "gpt-4o-mini";

        [Fact]
        public async Task Should_create_openai_completion()
        {
            var result = await CallToolAsync(
                "azmcp_ai_openai_completions_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "resource-name", OpenAiResourceName },
                    { "deployment-name", DefaultDeploymentName },
                    { "prompt-text", "What is Azure in one sentence?" },
                    { "max-tokens", 50 }
                });

            var completionText = result.AssertProperty("completionText");
            Assert.Equal(JsonValueKind.String, completionText.ValueKind);
            Assert.False(string.IsNullOrEmpty(completionText.GetString()));

            var usageInfo = result.AssertProperty("usageInfo");
            Assert.Equal(JsonValueKind.Object, usageInfo.ValueKind);
            
            var totalTokens = usageInfo.GetProperty("totalTokens");
            Assert.True(totalTokens.GetInt32() > 0);
        }

        [Fact]
        public async Task Should_create_openai_completion_with_custom_parameters()
        {
            var result = await CallToolAsync(
                "azmcp_ai_openai_completions_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "resource-name", OpenAiResourceName },
                    { "deployment-name", DefaultDeploymentName },
                    { "prompt-text", "Explain cloud computing briefly." },
                    { "max-tokens", 100 },
                    { "temperature", 0.7 }
                });

            var completionText = result.AssertProperty("completionText");
            Assert.Equal(JsonValueKind.String, completionText.ValueKind);
            Assert.False(string.IsNullOrEmpty(completionText.GetString()));

            var usageInfo = result.AssertProperty("usageInfo");
            Assert.Equal(JsonValueKind.Object, usageInfo.ValueKind);
            
            var inputTokens = usageInfo.GetProperty("inputTokens");
            Assert.True(inputTokens.GetInt32() > 0);
            
            var outputTokens = usageInfo.GetProperty("outputTokens");
            Assert.True(outputTokens.GetInt32() > 0);
            
            var totalTokens = usageInfo.GetProperty("totalTokens");
            Assert.True(totalTokens.GetInt32() > 0);
        }

        [Fact]
        public async Task Should_validate_required_parameters()
        {
            // Test with missing deployment name
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await CallToolAsync(
                    "azmcp_ai_openai_completions_create",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "resource-group", Settings.ResourceGroupName },
                        { "resource-name", OpenAiResourceName },
                        { "prompt-text", "Test prompt" }
                        // Missing deployment-name
                    }));
        }
    }
}
