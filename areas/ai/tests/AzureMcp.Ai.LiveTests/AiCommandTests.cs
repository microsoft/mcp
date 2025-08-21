// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AzureMcp.Tests;
using AzureMcp.Tests.Client;
using AzureMcp.Tests.Client.Helpers;
using Xunit;

namespace AzureMcp.Ai.LiveTests
{
    [Trait("Area", "Ai")]
    [Trait("Category", "Live")]
    public class AiCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
        : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
    {
        [Fact]
        [Trait("Category", "LiveSkip")] // Skip until test infrastructure is ready
        public async Task Should_create_openai_completion()
        {
            // This test would require Azure OpenAI resource deployment
            // For now, we'll skip this until test infrastructure is ready
            var result = await CallToolAsync(
                "azmcp_ai_openai_completions_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "resource-name", "test-openai" },
                    { "deployment-name", "gpt-35-turbo" },
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
    }
}
