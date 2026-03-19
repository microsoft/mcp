// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.LiveTests;

public class FoundryExtensionsCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Sanitize subscription IDs in URIs to allow playback to work
    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        // Subscription ID with trailing slash (e.g. /subscriptions/<id>/resourceGroups/...)
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/subscriptions/(?<sub>[^/?]+)/",
            GroupForReplace = "sub",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        // Subscription ID without trailing path (e.g. /subscriptions/<id>?api-version=...)
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/subscriptions/(?<sub>[^/?]+)(?=[?])",
            GroupForReplace = "sub",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/resourcegroups/(?<rg>[^/?]+)",
            GroupForReplace = "rg",
            Value = "Sanitized"
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/resourceGroups/(?<rg>[^/?]+)",
            GroupForReplace = "rg",
            Value = "Sanitized"
        }),
        // Cognitive Services account name in ARM paths
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/accounts/(?<acct>[^/?]+)",
            GroupForReplace = "acct",
            Value = "Sanitized"
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/projects/(?<project>[^/?]+)",
            GroupForReplace = "project",
            Value = "Sanitized-ai-projects"
        })
    ];

    // Disable automatic additions so we can control sanitizer ORDER manually.
    // ResourceGroupName must be sanitized BEFORE ResourceBaseName, otherwise
    // "vigera-mcpdb643c27" becomes "vigera-Sanitized" instead of "Sanitized".
    public override bool EnableDefaultSanitizerAdditions => false;

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        // ResourceGroupName FIRST (e.g. "vigera-mcpdb643c27" → "Sanitized")
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = System.Text.RegularExpressions.Regex.Escape(Settings.ResourceGroupName),
            Value = "Sanitized",
        }),
        // Then ResourceBaseName (e.g. "mcpdb643c27" → "Sanitized")
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = Settings.ResourceBaseName,
            Value = "Sanitized",
        }),
        // Then SubscriptionId
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = Settings.SubscriptionId,
            Value = "00000000-0000-0000-0000-000000000000",
        }),
    ];

    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        .. base.BodyRegexSanitizers,
        // Sanitize resource group names with usernames in connection IDs
        new BodyRegexSanitizer(new BodyRegexSanitizerBody
        {
            Regex = @"SSS3PT_[^/\""]+",
            Value = "Sanitized"
        })
    ];

    [Fact]
    public async Task Should_list_foundry_knowledge_indexes()
    {
        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "foundryextensions_knowledge_index_list",
            new()
            {
                { "endpoint", $"https://{accounts}.services.ai.azure.com/api/projects/{projectName}" },
                { "tenant", Settings.TenantId }
            });

        // The command may return null if no indexes exist, or an array if indexes are found
        if (result.HasValue && result.Value.TryGetProperty("indexes", out var indexesArray))
        {
            Assert.Equal(JsonValueKind.Array, indexesArray.ValueKind);
        }
        // If no "indexes" property or result is null, the command succeeded with no content
    }

    [Fact]
    public async Task Should_get_foundry_knowledge_index_schema()
    {
        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var endpoint = $"https://{accounts}.services.ai.azure.com/api/projects/{projectName}";

        // First get list of indexes to find one to test with
        var listResult = await CallToolAsync(
            "foundryextensions_knowledge_index_list",
            new()
            {
                { "endpoint", endpoint },
                { "tenant", Settings.TenantId }
            });

        // Check if we have indexes to test with
        if (listResult.HasValue && listResult.Value.TryGetProperty("indexes", out var indexesArray) && indexesArray.GetArrayLength() > 0)
        {
            var firstIndex = indexesArray[0];
            var indexName = firstIndex.GetProperty("name").GetString();

            var result = await CallToolAsync(
                "foundryextensions_knowledge_index_schema",
                new()
                {
                    { "endpoint", endpoint },
                    { "index", indexName! },
                    { "tenant", Settings.TenantId }
                });

            var schema = result.AssertProperty("schema");
            Assert.Equal(JsonValueKind.Object, schema.ValueKind);
        }
        else
        {
            // Skip test if no indexes are available
            Output.WriteLine("Skipping knowledge index schema test - no indexes available for testing");
        }
    }

    [Fact]
    public async Task Should_create_openai_completion()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIDEPLOYMENTNAME", "gpt-4o-mini");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var tenantId = Settings.TenantId;

        // Register variables for playback mode
        RegisterVariable("resourceName", resourceName);
        RegisterVariable("deploymentName", deploymentName);
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("tenantId", tenantId);

        var subscriptionId = Settings.SubscriptionId;
        var result = await CallToolAsync(
            "foundryextensions_openai_create-completion",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "deployment", TestVariables["deploymentName"] },
                { "prompt-text", "What is Azure? Please provide a brief answer." },
                { "max-tokens", "50" },
                { "temperature", "0.7" },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var completionText = result.AssertProperty("completionText");
        Assert.Equal(JsonValueKind.String, completionText.ValueKind);
        Assert.NotEmpty(completionText.GetString()!);

        var usageInfo = result.AssertProperty("usageInfo");
        Assert.Equal(JsonValueKind.Object, usageInfo.ValueKind);

        // Verify usage info contains expected properties
        var promptTokens = usageInfo.AssertProperty("promptTokens");
        var completionTokens = usageInfo.AssertProperty("completionTokens");
        var totalTokens = usageInfo.AssertProperty("totalTokens");

        Assert.Equal(JsonValueKind.Number, promptTokens.ValueKind);
        Assert.Equal(JsonValueKind.Number, completionTokens.ValueKind);
        Assert.Equal(JsonValueKind.Number, totalTokens.ValueKind);

        // Verify total tokens = prompt + completion
        Assert.Equal(
            promptTokens.GetInt32() + completionTokens.GetInt32(),
            totalTokens.GetInt32()
        );
    }

    [Fact]
    public async Task Should_create_openai_embeddings()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("EMBEDDINGDEPLOYMENTNAME", "text-embedding-ada-002");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var tenantId = Settings.TenantId;
        var inputText = "Generate embeddings for this test text using Azure OpenAI.";

        RegisterVariable("resourceName", resourceName);
        RegisterVariable("deploymentName", deploymentName);
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("subscriptionId", subscriptionId);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_openai_embeddings-create",
            new()
            {
                { "subscription", TestVariables["subscriptionId"] },
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "deployment", TestVariables["deploymentName"] },
                { "input-text", inputText },
                { "user", "test-user" },
                { "encoding-format", "float" },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var embeddingResult = result.AssertProperty("embeddingResult");
        Assert.Equal(JsonValueKind.Object, embeddingResult.ValueKind);

        // Verify embedding result properties
        var objectType = embeddingResult.AssertProperty("object");
        Assert.Equal(JsonValueKind.String, objectType.ValueKind);
        Assert.Equal("list", objectType.GetString());

        var data = embeddingResult.AssertProperty("data");
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.NotEmpty(data.EnumerateArray());

        // Verify first embedding data element
        var firstEmbedding = data.EnumerateArray().First();
        var embeddingObject = firstEmbedding.GetProperty("object");
        Assert.Equal("embedding", embeddingObject.GetString());

        var embeddingVector = firstEmbedding.GetProperty("embedding");
        Assert.Equal(JsonValueKind.Array, embeddingVector.ValueKind);

        // Verify embedding vector contains float values and has reasonable dimensions
        var vectorArray = embeddingVector.EnumerateArray().ToArray();
        Assert.True(vectorArray.Length > 0, "Embedding vector should not be empty");
        Assert.True(vectorArray.Length >= 1536, $"Embedding vector should have at least 1536 dimensions, got {vectorArray.Length}"); // Ada-002 has 1536 dimensions

        // Verify all values are valid numbers
        foreach (var value in vectorArray)
        {
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            var floatValue = value.GetSingle();
            Assert.True(!float.IsNaN(floatValue), "Embedding values should not be NaN");
            Assert.True(!float.IsInfinity(floatValue), "Embedding values should not be infinity");
        }

        // Verify model name in response
        var model = embeddingResult.AssertProperty("model");
        Assert.Equal(JsonValueKind.String, model.ValueKind);
        Assert.Equal(TestVariables["deploymentName"], model.GetString());

        // Verify usage information
        var usage = embeddingResult.AssertProperty("usage");
        Assert.Equal(JsonValueKind.Object, usage.ValueKind);

        var promptTokens = usage.AssertProperty("prompt_tokens");
        var totalTokens = usage.AssertProperty("total_tokens");

        Assert.Equal(JsonValueKind.Number, promptTokens.ValueKind);
        Assert.Equal(JsonValueKind.Number, totalTokens.ValueKind);

        // For embeddings, prompt tokens should equal total tokens (no completion tokens)
        Assert.Equal(promptTokens.GetInt32(), totalTokens.GetInt32());
        Assert.True(promptTokens.GetInt32() > 0, "Should have used some tokens");

        // Verify metadata properties are present
        var resourceNameProperty = result.AssertProperty("resourceName");
        var deploymentNameProperty = result.AssertProperty("deploymentName");
        var inputTextProperty = result.AssertProperty("inputText");

        Assert.Equal(JsonValueKind.String, resourceNameProperty.ValueKind);
        Assert.Equal(JsonValueKind.String, deploymentNameProperty.ValueKind);
        Assert.Equal(JsonValueKind.String, inputTextProperty.ValueKind);

        Assert.Equal(TestVariables["resourceName"], resourceNameProperty.GetString());
        Assert.Equal(TestVariables["deploymentName"], deploymentNameProperty.GetString());
        Assert.Equal(inputText, inputTextProperty.GetString());
    }

    [Fact]
    public async Task Should_create_openai_embeddings_with_optional_parameters()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("EMBEDDINGDEPLOYMENTNAME", "text-embedding-ada-002");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var tenantId = Settings.TenantId;
        var inputText = "Test embeddings with optional parameters.";
        var dimensions = 512; // Test with reduced dimensions if supported

        RegisterVariable("resourceName", resourceName);
        RegisterVariable("deploymentName", deploymentName);
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_openai_embeddings-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "deployment", TestVariables["deploymentName"] },
                { "input-text", inputText },
                { "user", "test-user-with-params" },
                { "encoding-format", "float" },
                { "dimensions", dimensions.ToString() },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure (same as basic test)
        var embeddingResult = result.AssertProperty("embeddingResult");
        var data = embeddingResult.AssertProperty("data");
        var firstEmbedding = data.EnumerateArray().First();
        var embeddingVector = firstEmbedding.GetProperty("embedding");

        // Verify embedding vector dimensions match requested dimensions (if model supports it)
        var vectorArray = embeddingVector.EnumerateArray().ToArray();
        Assert.True(vectorArray.Length > 0, "Embedding vector should not be empty");

        // Note: Some models may not support custom dimensions and will return default size
        // So we just verify we got a reasonable response, not necessarily the exact dimensions requested
        Assert.True(vectorArray.Length >= 512, $"Embedding vector should have reasonable dimensions, got {vectorArray.Length}");

        // Verify all values are valid numbers
        foreach (var value in vectorArray)
        {
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            var floatValue = value.GetSingle();
            Assert.True(!float.IsNaN(floatValue), "Embedding values should not be NaN");
            Assert.True(!float.IsInfinity(floatValue), "Embedding values should not be infinity");
        }

        // Verify usage information shows token consumption
        var usage = embeddingResult.AssertProperty("usage");
        var totalTokens = usage.AssertProperty("total_tokens");
        Assert.True(totalTokens.GetInt32() > 0, "Should have consumed tokens");
    }

    [Fact]
    public async Task Should_list_openai_models()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var tenantId = Settings.TenantId;

        // Register variables for recording sanitization
        RegisterVariable("resourceName", resourceName);
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_openai_models-list",
            new()
            {
                { "subscription", Settings.SubscriptionId },  // Don't register - test proxy auto-sanitizes GUIDs in URLs
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var modelsListResult = result.AssertProperty("modelsListResult");
        Assert.Equal(JsonValueKind.Object, modelsListResult.ValueKind);

        // Verify resource name matches
        var returnedResourceName = modelsListResult.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, returnedResourceName.ValueKind);
        Assert.Equal(TestVariables["resourceName"], returnedResourceName.GetString());

        // Verify models array exists (may be empty if no models deployed)
        var models = modelsListResult.AssertProperty("models");
        Assert.Equal(JsonValueKind.Array, models.ValueKind);

        // If models exist, verify their structure
        var modelArray = models.EnumerateArray().ToArray();
        if (modelArray.Length > 0)
        {
            foreach (var model in modelArray)
            {
                // Verify required properties exist
                var deploymentName = model.GetProperty("deploymentName");
                var modelName = model.GetProperty("modelName");

                Assert.Equal(JsonValueKind.String, deploymentName.ValueKind);
                Assert.Equal(JsonValueKind.String, modelName.ValueKind);
                Assert.NotEmpty(deploymentName.GetString()!);
                Assert.NotEmpty(modelName.GetString()!);

                // Verify modelVersion if present
                if (model.TryGetProperty("modelVersion", out var modelVersion))
                {
                    Assert.Equal(JsonValueKind.String, modelVersion.ValueKind);
                    Assert.NotEmpty(modelVersion.GetString()!);
                }

                // Verify capabilities structure if present
                if (model.TryGetProperty("capabilities", out var capabilities))
                {
                    Assert.Equal(JsonValueKind.Object, capabilities.ValueKind);

                    // Check boolean capability properties (only validate the ones that are present)
                    if (capabilities.TryGetProperty("completions", out var completions))
                    {
                        Assert.True(completions.ValueKind == JsonValueKind.True || completions.ValueKind == JsonValueKind.False,
                            "completions should be a boolean value");
                    }

                    if (capabilities.TryGetProperty("embeddings", out var embeddings))
                    {
                        Assert.True(embeddings.ValueKind == JsonValueKind.True || embeddings.ValueKind == JsonValueKind.False,
                            "embeddings should be a boolean value");
                    }

                    if (capabilities.TryGetProperty("chatCompletions", out var chatCompletions))
                    {
                        Assert.True(chatCompletions.ValueKind == JsonValueKind.True || chatCompletions.ValueKind == JsonValueKind.False,
                            "chatCompletions should be a boolean value");
                    }

                    if (capabilities.TryGetProperty("fineTuning", out var fineTuning))
                    {
                        Assert.True(fineTuning.ValueKind == JsonValueKind.True || fineTuning.ValueKind == JsonValueKind.False,
                            "fineTuning should be a boolean value");
                    }
                }

                // Verify provisioningState if present
                if (model.TryGetProperty("provisioningState", out var provisioningState))
                {
                    Assert.Equal(JsonValueKind.String, provisioningState.ValueKind);
                    Assert.NotEmpty(provisioningState.GetString()!);
                }

                // Verify optional capacity property if present
                if (model.TryGetProperty("capacity", out var capacity))
                {
                    Assert.Equal(JsonValueKind.Number, capacity.ValueKind);
                    Assert.True(capacity.GetInt32() > 0);
                }
            }
        }

        // Verify command metadata (returned resource name should match input)
        var commandResourceName = result.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, commandResourceName.ValueKind);
        Assert.Equal(TestVariables["resourceName"], commandResourceName.GetString());
    }

    [Fact]
    public async Task Should_create_openai_chat_completions()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIDEPLOYMENTNAME", "gpt-4o-mini");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var tenantId = Settings.TenantId;
        var messages = JsonSerializer.Serialize(new[]
        {
            new { role = "system", content = "You are a helpful assistant." },
            new { role = "user", content = "Hello, how are you today?" }
        });

        RegisterVariable("resourceName", resourceName);
        RegisterVariable("deploymentName", deploymentName);
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_openai_chat-completions-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "deployment", TestVariables["deploymentName"] },
                { "message-array", messages },
                { "max-tokens", "150" },
                { "temperature", "0.7" },
                { "user", "test-user" },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var chatResult = result.AssertProperty("result");
        Assert.Equal(JsonValueKind.Object, chatResult.ValueKind);

        // Verify chat completion result properties
        var id = chatResult.AssertProperty("id");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.False(string.IsNullOrEmpty(id.GetString()));

        var objectType = chatResult.AssertProperty("object");
        Assert.Equal(JsonValueKind.String, objectType.ValueKind);
        Assert.Equal("chat.completion", objectType.GetString());

        var model = chatResult.AssertProperty("model");
        Assert.Equal(JsonValueKind.String, model.ValueKind);
        Assert.Equal(deploymentName, model.GetString());

        var choices = chatResult.AssertProperty("choices");
        Assert.Equal(JsonValueKind.Array, choices.ValueKind);
        Assert.NotEmpty(choices.EnumerateArray());

        // Verify first choice
        var firstChoice = choices.EnumerateArray().First();

        var message = firstChoice.GetProperty("message");
        var role = message.GetProperty("role");
        Assert.Equal("assistant", role.GetString());

        var content = message.GetProperty("content");
        Assert.Equal(JsonValueKind.String, content.ValueKind);
        Assert.False(string.IsNullOrEmpty(content.GetString()));

        var finishReason = firstChoice.GetProperty("finish_reason");
        Assert.Equal(JsonValueKind.String, finishReason.ValueKind);

        // Verify usage information
        var usage = chatResult.AssertProperty("usage");
        var promptTokens = usage.AssertProperty("prompt_tokens");
        Assert.True(promptTokens.GetInt32() > 0, "Should have consumed prompt tokens");

        var completionTokens = usage.AssertProperty("completion_tokens");
        Assert.True(completionTokens.GetInt32() > 0, "Should have generated completion tokens");

        var totalTokens = usage.AssertProperty("total_tokens");
        Assert.True(totalTokens.GetInt32() > 0, "Should have total token usage");
        Assert.Equal(promptTokens.GetInt32() + completionTokens.GetInt32(), totalTokens.GetInt32());

        // Verify command metadata (returned resource and deployment names should match input)
        var commandResourceName = result.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, commandResourceName.ValueKind);
        Assert.Equal(TestVariables["resourceName"], commandResourceName.GetString());

        var commandDeploymentName = result.AssertProperty("deploymentName");
        Assert.Equal(JsonValueKind.String, commandDeploymentName.ValueKind);
        Assert.Equal(TestVariables["deploymentName"], commandDeploymentName.GetString());
    }

    [Fact]
    public async Task Should_create_openai_chat_completions_with_conversation_history()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIDEPLOYMENTNAME", "gpt-4o-mini");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var tenantId = Settings.TenantId;
        var messages = JsonSerializer.Serialize(new[]
        {
            new { role = "system", content = "You are a helpful assistant that answers questions about Azure." },
            new { role = "user", content = "What is Azure OpenAI Service?" },
            new { role = "assistant", content = "Azure OpenAI Service is a cloud service that provides REST API access to OpenAI's language models including GPT-4, GPT-3.5-turbo, and Embeddings model series." },
            new { role = "user", content = "How can I use it for chat applications?" }
        });

        RegisterVariable("resourceName", resourceName);
        RegisterVariable("deploymentName", deploymentName);
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_openai_chat-completions-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "deployment", TestVariables["deploymentName"] },
                { "message-array", messages },
                { "max-tokens", "200" },
                { "temperature", "0.5" },
                { "top-p", "0.9" },
                { "user", "test-user-conversation" },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify response structure
        var chatResult = result.AssertProperty("result");
        Assert.Equal(JsonValueKind.Object, chatResult.ValueKind);

        var id = chatResult.AssertProperty("id");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.False(string.IsNullOrEmpty(id.GetString()));

        var objectType = chatResult.AssertProperty("object");
        Assert.Equal(JsonValueKind.String, objectType.ValueKind);
        Assert.Equal("chat.completion", objectType.GetString());

        var model = chatResult.AssertProperty("model");
        Assert.Equal(JsonValueKind.String, model.ValueKind);
        Assert.Equal(deploymentName, model.GetString());

        var choices = chatResult.AssertProperty("choices");
        Assert.Equal(JsonValueKind.Array, choices.ValueKind);
        Assert.NotEmpty(choices.EnumerateArray());

        var firstChoice = choices.EnumerateArray().First();
        var message = firstChoice.GetProperty("message");
        var role = message.GetProperty("role");
        Assert.Equal("assistant", role.GetString());

        var content = message.GetProperty("content");
        Assert.Equal(JsonValueKind.String, content.ValueKind);

        var responseText = content.GetString();
        Assert.False(string.IsNullOrEmpty(responseText));
        Assert.True(responseText.Length > 10, "Response should be substantial");

        var finishReason = firstChoice.GetProperty("finish_reason");
        Assert.Equal(JsonValueKind.String, finishReason.ValueKind);

        var usage = chatResult.AssertProperty("usage");
        var promptTokens = usage.AssertProperty("prompt_tokens");
        Assert.True(promptTokens.GetInt32() > 0, "Should have consumed prompt tokens");

        var completionTokens = usage.AssertProperty("completion_tokens");
        Assert.True(completionTokens.GetInt32() > 0, "Should have generated completion tokens");

        var totalTokens = usage.AssertProperty("total_tokens");
        Assert.True(totalTokens.GetInt32() > 50, "Conversation should consume reasonable tokens");
        Assert.Equal(promptTokens.GetInt32() + completionTokens.GetInt32(), totalTokens.GetInt32());

        var commandResourceName = result.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, commandResourceName.ValueKind);
        Assert.Equal(TestVariables["resourceName"], commandResourceName.GetString());

        var commandDeploymentName = result.AssertProperty("deploymentName");
        Assert.Equal(JsonValueKind.String, commandDeploymentName.ValueKind);
        Assert.Equal(TestVariables["deploymentName"], commandDeploymentName.GetString());
    }

    [Fact]
    public async Task Should_list_all_foundry_resources_in_subscription()
    {
        var subscriptionId = Settings.SubscriptionId;
        var tenantId = Settings.TenantId;

        // Register variables for recording sanitization
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_resource_get",
            new()
            {
                { "subscription", subscriptionId },  // Don't register - test proxy auto-sanitizes GUIDs
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // Should have at least one resource (the test resource)
        Assert.NotEmpty(resources.EnumerateArray());

        // Verify first resource structure
        var firstResource = resources.EnumerateArray().First();

        var resourceName = firstResource.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, resourceName.ValueKind);
        Assert.NotEmpty(resourceName.GetString()!);

        var resourceGroup = firstResource.AssertProperty("resourceGroup");
        Assert.Equal(JsonValueKind.String, resourceGroup.ValueKind);
        Assert.NotEmpty(resourceGroup.GetString()!);

        var subscriptionName = firstResource.AssertProperty("subscriptionName");
        Assert.Equal(JsonValueKind.String, subscriptionName.ValueKind);
        Assert.NotEmpty(subscriptionName.GetString()!);

        var location = firstResource.AssertProperty("location");
        Assert.Equal(JsonValueKind.String, location.ValueKind);
        Assert.NotEmpty(location.GetString()!);

        var endpoint = firstResource.AssertProperty("endpoint");
        Assert.Equal(JsonValueKind.String, endpoint.ValueKind);
        Assert.NotEmpty(endpoint.GetString()!);

        var kind = firstResource.AssertProperty("kind");
        Assert.Equal(JsonValueKind.String, kind.ValueKind);
        Assert.NotEmpty(kind.GetString()!);

        var skuName = firstResource.AssertProperty("skuName");
        Assert.Equal(JsonValueKind.String, skuName.ValueKind);
        Assert.NotEmpty(skuName.GetString()!);

        var deployments = firstResource.AssertProperty("deployments");
        Assert.Equal(JsonValueKind.Array, deployments.ValueKind);
    }

    [Fact]
    public async Task Should_list_foundry_resources_in_resource_group()
    {
        var subscriptionId = Settings.SubscriptionId;
        var resourceGroup = Settings.ResourceGroupName;
        var tenantId = Settings.TenantId;

        // Register variables for recording sanitization
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_resource_get",
            new()
            {
                { "subscription", subscriptionId },  // Don't register - test proxy auto-sanitizes GUIDs
                { "resource-group", TestVariables["resourceGroup"] },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // Should have at least one resource in this resource group
        Assert.NotEmpty(resources.EnumerateArray());

        // Verify all resources are in the specified resource group
        foreach (var resource in resources.EnumerateArray())
        {
            var rg = resource.GetProperty("resourceGroup");
            var rgValue = rg.GetString();
            // In playback mode, resource group may be sanitized
            Assert.True(
                rgValue == TestVariables["resourceGroup"] || rgValue?.Contains("Sanitized") == true,
                $"Expected resource group '{TestVariables["resourceGroup"]}' or sanitized variant, got '{rgValue}'");
        }
    }

    [Fact]
    public async Task Should_get_specific_foundry_resource()
    {
        var subscriptionId = Settings.SubscriptionId;
        var resourceGroup = Settings.ResourceGroupName;
        var resourceName = Settings.ResourceBaseName;
        var tenantId = Settings.TenantId;

        // Register variables for recording sanitization
        RegisterVariable("resourceGroup", resourceGroup);
        RegisterVariable("resourceName", resourceName);
        RegisterVariable("tenantId", tenantId);

        var result = await CallToolAsync(
            "foundryextensions_resource_get",
            new()
            {
                { "subscription", subscriptionId },  // Don't register - test proxy auto-sanitizes GUIDs
                { "resource-group", TestVariables["resourceGroup"] },
                { "resource-name", TestVariables["resourceName"] },
                { "tenant", TestVariables["tenantId"] }
            });

        // Verify the response structure
        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // Should return exactly one resource
        Assert.Single(resources.EnumerateArray());

        var resource = resources.EnumerateArray().First();

        // Verify resource details match the request
        var returnedResourceName = resource.AssertProperty("resourceName");
        Assert.Equal(TestMode == TestMode.Playback ? "Sanitized" : TestVariables["resourceName"], returnedResourceName.GetString());

        var returnedResourceGroup = resource.AssertProperty("resourceGroup");
        Assert.Equal(TestMode == TestMode.Playback ? "Sanitized" : TestVariables["resourceGroup"], returnedResourceGroup.GetString());

        var subscriptionName = resource.AssertProperty("subscriptionName");
        Assert.Equal(JsonValueKind.String, subscriptionName.ValueKind);
        Assert.NotEmpty(subscriptionName.GetString()!);

        var location = resource.AssertProperty("location");
        Assert.Equal(JsonValueKind.String, location.ValueKind);
        Assert.NotEmpty(location.GetString()!);

        var endpoint = resource.AssertProperty("endpoint");
        Assert.Equal(JsonValueKind.String, endpoint.ValueKind);
        Assert.NotEmpty(endpoint.GetString()!);
        Assert.StartsWith("https://", endpoint.GetString());

        var kind = resource.AssertProperty("kind");
        Assert.Equal(JsonValueKind.String, kind.ValueKind);
        Assert.Contains(kind.GetString(), new[] { "OpenAI", "AIServices", "CognitiveServices" });

        var skuName = resource.AssertProperty("skuName");
        Assert.Equal(JsonValueKind.String, skuName.ValueKind);
        Assert.NotEmpty(skuName.GetString()!);

        var deployments = resource.AssertProperty("deployments");
        Assert.Equal(JsonValueKind.Array, deployments.ValueKind);

        var deploymentsArray = deployments.EnumerateArray().ToArray();
        if (deploymentsArray.Length > 0)
        {
            var firstDeployment = deploymentsArray[0];

            var deploymentName = firstDeployment.AssertProperty("deploymentName");
            Assert.Equal(JsonValueKind.String, deploymentName.ValueKind);
            Assert.NotEmpty(deploymentName.GetString()!);

            var modelName = firstDeployment.AssertProperty("modelName");
            Assert.Equal(JsonValueKind.String, modelName.ValueKind);
            Assert.NotEmpty(modelName.GetString()!);

            if (firstDeployment.TryGetProperty("modelVersion", out var modelVersion))
            {
                Assert.Equal(JsonValueKind.String, modelVersion.ValueKind);
            }

            if (firstDeployment.TryGetProperty("modelFormat", out var modelFormat))
            {
                Assert.Equal(JsonValueKind.String, modelFormat.ValueKind);
            }

            if (firstDeployment.TryGetProperty("skuName", out var deploymentSkuName))
            {
                Assert.Equal(JsonValueKind.String, deploymentSkuName.ValueKind);
            }

            if (firstDeployment.TryGetProperty("skuCapacity", out var skuCapacity))
            {
                Assert.Equal(JsonValueKind.Number, skuCapacity.ValueKind);
                Assert.True(skuCapacity.GetInt32() > 0);
            }

            if (firstDeployment.TryGetProperty("provisioningState", out var provisioningState))
            {
                Assert.Equal(JsonValueKind.String, provisioningState.ValueKind);
            }
        }
    }

}
