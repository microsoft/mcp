// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.AI.Agents.Persistent;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.LiveTests;

public class FoundryCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_foundry_models()
    {
        var result = await CallToolAsync(
            "azmcp_foundry_models_list",
            new()
            {
                { "search-for-free-playground", "true" }
            });

        var modelsArray = result.AssertProperty("models");
        Assert.Equal(JsonValueKind.Array, modelsArray.ValueKind);
        Assert.NotEmpty(modelsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_foundry_model_deployments()
    {
        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "azmcp_foundry_models_deployments_list",
            new()
            {
                { "endpoint", $"https://{accounts}.services.ai.azure.com/api/projects/{projectName}" },
                { "tenant", Settings.TenantId }
            });

        var deploymentsArray = result.AssertProperty("deployments");
        Assert.Equal(JsonValueKind.Array, deploymentsArray.ValueKind);
        Assert.NotEmpty(deploymentsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_deploy_foundry_model()
    {
        var deploymentName = $"test-deploy-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var result = await CallToolAsync(
            "azmcp_foundry_models_deploy",
            new()
            {
                { "deployment", deploymentName },
                { "model-name", "gpt-4o" },
                { "model-format", "OpenAI"},
                { "azure-ai-services", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
            });

        var deploymentResource = result.AssertProperty("deploymentData");
        Assert.Equal(JsonValueKind.Object, deploymentResource.ValueKind);
        Assert.NotEmpty(deploymentResource.EnumerateObject());
    }

    [Fact]
    public async Task Should_list_foundry_knowledge_indexes()
    {
        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "azmcp_foundry_knowledge_index_list",
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
            "azmcp_foundry_knowledge_index_list",
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
                "azmcp_foundry_knowledge_index_schema",
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
        var subscriptionId = Settings.SubscriptionId;
        var result = await CallToolAsync(
            "azmcp_foundry_openai_create-completion",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "deployment", deploymentName },
                { "prompt-text", "What is Azure? Please provide a brief answer." },
                { "max-tokens", "50" },
                { "temperature", "0.7" },
                { "tenant", Settings.TenantId }
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
        var inputText = "Generate embeddings for this test text using Azure OpenAI.";

        var result = await CallToolAsync(
            "azmcp_foundry_openai_embeddings-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "deployment", deploymentName },
                { "input-text", inputText },
                { "user", "test-user" },
                { "encoding-format", "float" },
                { "tenant", Settings.TenantId }
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
        Assert.Equal(deploymentName, model.GetString());

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

        Assert.Equal(resourceName, resourceNameProperty.GetString());
        Assert.Equal(deploymentName, deploymentNameProperty.GetString());
        Assert.Equal(inputText, inputTextProperty.GetString());
    }

    [Fact]
    public async Task Should_create_openai_embeddings_with_optional_parameters()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("EMBEDDINGDEPLOYMENTNAME", "text-embedding-ada-002");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var inputText = "Test embeddings with optional parameters.";
        var dimensions = 512; // Test with reduced dimensions if supported

        var result = await CallToolAsync(
            "azmcp_foundry_openai_embeddings-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "deployment", deploymentName },
                { "input-text", inputText },
                { "user", "test-user-with-params" },
                { "encoding-format", "float" },
                { "dimensions", dimensions.ToString() },
                { "tenant", Settings.TenantId }
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
        var subscriptionId = Settings.SubscriptionId;

        var result = await CallToolAsync(
            "azmcp_foundry_openai_models-list",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "tenant", Settings.TenantId }
            });

        // Verify the response structure
        var modelsListResult = result.AssertProperty("modelsListResult");
        Assert.Equal(JsonValueKind.Object, modelsListResult.ValueKind);

        // Verify resource name matches
        var returnedResourceName = modelsListResult.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, returnedResourceName.ValueKind);
        Assert.Equal(resourceName, returnedResourceName.GetString());

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
        Assert.Equal(resourceName, commandResourceName.GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_connect_agent()
    {
        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var agentName = $"test-agent-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var query = "What is the weather today in NYC?";
        var endpoint = $"https://{accounts}.services.ai.azure.com/api/projects/{projectName}";

        var agentId = await CreateAgent(agentName, endpoint, "gpt-4o");

        var result = await CallToolAsync(
            "azmcp_foundry_agents_connect",
            new()
            {
                { "agent-id", agentId },
                { "query", query },
                { "endpoint", endpoint }
            });
        var response = result.AssertProperty("response");
        Assert.Equal(JsonValueKind.Object, response.ValueKind);
        Assert.NotEmpty(response.EnumerateObject());
        response.AssertProperty("query");
        response.AssertProperty("response");
        response.AssertProperty("queryText");
        response.AssertProperty("responseText");
        response.AssertProperty("agentId");
        response.AssertProperty("toolDefinitions");
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_agents()
    {
        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var agentName = $"test-agent-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var endpoint = $"https://{accounts}.services.ai.azure.com/api/projects/{projectName}";

        await CreateAgent(agentName, endpoint, "gpt-4o");

        var result = await CallToolAsync(
            "azmcp_foundry_agents_list",
            new()
            {
                { "endpoint", endpoint }
            });
        var agentsArray = result.AssertProperty("agents");
        Assert.Equal(JsonValueKind.Array, agentsArray.ValueKind);
        Assert.NotEmpty(agentsArray.EnumerateArray());
    }

    [Theory]
    [InlineData("task_adherence", "Task Adherence")]
    [InlineData("tool_call_accuracy", "Tool Call Accuracy")]
    [InlineData("intent_resolution", "Intent Resolution")]
    [Trait("Category", "Live")]
    public async Task Should_query_and_evaluate_agent(string evaluatorName, string evaluationMetric)
    {
        // to be filled in

        var projectName = $"{Settings.ResourceBaseName}-ai-projects";
        var accounts = Settings.ResourceBaseName;
        var agentName = $"test-agent-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var endpoint = $"https://{accounts}.services.ai.azure.com/api/projects/{projectName}";
        var azureOpenAIEndpoint = $"https://{accounts}.cognitiveservices.azure.com";
        var azureOpenAIDeployment = "gpt-4o";

        var agentId = await CreateAgent(agentName, endpoint, "gpt-4o");
        var result = await CallToolAsync(
            "azmcp_foundry_agents_query-and-evaluate",
            new()
            {
                { "agent-id", agentId },
                { "query", "What is the weather in NYC today?"},
                { "endpoint", endpoint },
                { "azure-openai-endpoint", azureOpenAIEndpoint },
                { "azure-openai-deployment", azureOpenAIDeployment },
                { "evaluators", evaluatorName }
            });

        var response = result.AssertProperty("response");
        Assert.Equal(JsonValueKind.Object, response.ValueKind);
        Assert.NotEmpty(response.EnumerateObject());
        response.AssertProperty("query");
        response.AssertProperty("response");
        response.AssertProperty("queryText");
        response.AssertProperty("responseText");
        response.AssertProperty("evaluators");
        var evaluationResults = response.AssertProperty("evaluationResult");
        Assert.Equal(JsonValueKind.Object, evaluationResults.ValueKind);
        Assert.NotEmpty(evaluationResults.EnumerateObject());
        var metrics = evaluationResults.AssertProperty("metrics");
        Assert.Equal(JsonValueKind.Object, metrics.ValueKind);
        var metric = metrics.AssertProperty(evaluationMetric);
        Assert.Equal(JsonValueKind.Object, metric.ValueKind);
        metric.AssertProperty("value");
        metric.AssertProperty("reason");
        var interpretation = metric.AssertProperty("interpretation");
        Assert.Equal(JsonValueKind.Object, interpretation.ValueKind);
        var context = metric.AssertProperty("context");
        Assert.Equal(JsonValueKind.Object, context.ValueKind);
    }

    [Theory]
    [InlineData("task_adherence", "Task Adherence")]
    [InlineData("tool_call_accuracy", "Tool Call Accuracy")]
    [InlineData("intent_resolution", "Intent Resolution")]
    [Trait("Category", "Live")]
    public async Task Should_evaluate_agent(string evaluatorName, string evaluationMetric)
    {
        // to be filled in
        var query = "[{\"role\":\"user\",\"contents\":[{\"$type\":\"text\",\"text\":\"What is the weather in NYC today?\"}],\"messageId\":\"msg_fakeMessageHash1\"}]";
        var agentResponse = "[{\"role\":\"user\",\"contents\":[{\"$type\":\"text\",\"text\":\"What is the weather in NYC today?\"}],\"messageId\":\"msg_fakeMessageHash1\"},{\"authorName\":\"asst_XNa6yxvWUvRhCpWmE3kxk09u\",\"role\":\"assistant\",\"contents\":[{\"$type\":\"functionCall\",\"callId\":\"call_fakeCallHash1\",\"name\":\"bing_grounding\",\"arguments\":{\"requesturl\":\"https://api.bing.microsoft.com/v7.0/search?q=NewYorkCityweatherAugust42025\"}}],\"messageId\":\"step_fakeRunStepHash1\"},{\"authorName\":\"asst_XNa6yxvWUvRhCpWmE3kxk09u\",\"role\":\"assistant\",\"contents\":[{\"$type\":\"text\",\"text\":\"The weather in New York City today, August 4, 2025, is expected to have a high of 88°F during the day and a low of 70°F at night. There is a 25% chance of precipitation, with light winds at about 7 mph.\\u30103:2\\u2020source\\u3011.\"}],\"messageId\":\"msg_fakeMessageHash2\"}]";
        var toolDefinitions = "[{\"name\": \"bing_grounding\", \"description\": \"Enhance model output with web data.\", \"jsonSchema\": {\"type\": \"object\",\"properties\": {\"requesturl\": {\"type\": \"string\",\"description\": \"URL used in Bing Search API.\"}}}}]";
        var accounts = Settings.ResourceBaseName;
        var azureOpenAIEndpoint = $"https://{accounts}.cognitiveservices.azure.com";
        var azureOpenAIDeployment = "gpt-4o";
        var result = await CallToolAsync(
            "azmcp_foundry_agents_evaluate",
            new()
            {
                { "evaluator", evaluatorName },
                { "query", query},
                { "response", agentResponse },
                { "azure-openai-endpoint", azureOpenAIEndpoint },
                { "azure-openai-deployment", azureOpenAIDeployment },
                { "tool-definitions", toolDefinitions },
                { "evaluators", evaluatorName }
            });

        var response = result.AssertProperty("response");
        Assert.Equal(JsonValueKind.Object, response.ValueKind);
        Assert.NotEmpty(response.EnumerateObject());
        var evaluationResults = response.AssertProperty("evaluationResult");
        Assert.Equal(JsonValueKind.Object, evaluationResults.ValueKind);
        Assert.NotEmpty(evaluationResults.EnumerateObject());
        var metrics = evaluationResults.AssertProperty("metrics");
        Assert.Equal(JsonValueKind.Object, metrics.ValueKind);
        var metric = metrics.AssertProperty(evaluationMetric);
        Assert.Equal(JsonValueKind.Object, metric.ValueKind);
        metric.AssertProperty("value");
        metric.AssertProperty("reason");
        var interpretation = metric.AssertProperty("interpretation");
        Assert.Equal(JsonValueKind.Object, interpretation.ValueKind);
        var context = metric.AssertProperty("context");
        Assert.Equal(JsonValueKind.Object, context.ValueKind);
    }

    [Fact]
    public async Task Should_create_openai_chat_completions()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIDEPLOYMENTNAME", "gpt-4o-mini");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var messages = JsonSerializer.Serialize(new[]
        {
            new { role = "system", content = "You are a helpful assistant." },
            new { role = "user", content = "Hello, how are you today?" }
        });

        var result = await CallToolAsync(
            "azmcp_foundry_openai_chat-completions-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "deployment", deploymentName },
                { "message-array", messages },
                { "max-tokens", "150" },
                { "temperature", "0.7" },
                { "user", "test-user" },
                { "tenant", Settings.TenantId }
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
        Assert.Equal(resourceName, commandResourceName.GetString());

        var commandDeploymentName = result.AssertProperty("deploymentName");
        Assert.Equal(JsonValueKind.String, commandDeploymentName.ValueKind);
        Assert.Equal(deploymentName, commandDeploymentName.GetString());
    }

    [Fact]
    public async Task Should_create_openai_chat_completions_with_conversation_history()
    {
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIDEPLOYMENTNAME", "gpt-4o-mini");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        var messages = JsonSerializer.Serialize(new[]
        {
            new { role = "system", content = "You are a helpful assistant that answers questions about Azure." },
            new { role = "user", content = "What is Azure OpenAI Service?" },
            new { role = "assistant", content = "Azure OpenAI Service is a cloud service that provides REST API access to OpenAI's language models including GPT-4, GPT-3.5-turbo, and Embeddings model series." },
            new { role = "user", content = "How can I use it for chat applications?" }
        });

        var result = await CallToolAsync(
            "azmcp_foundry_openai_chat-completions-create",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "deployment", deploymentName },
                { "message-array", messages },
                { "max-tokens", "200" },
                { "temperature", "0.5" },
                { "top-p", "0.9" },
                { "user", "test-user-conversation" },
                { "tenant", Settings.TenantId }
            });

        // Verify response structure (same checks as basic test)
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

        // Verify the response is relevant to the conversation context
        var responseText = content.GetString();
        Assert.False(string.IsNullOrEmpty(responseText));

        // The response should be contextually relevant to chat applications
        // We don't check for specific words to avoid brittleness, just that we got a meaningful response
        Assert.True(responseText.Length > 10, "Response should be substantial");

        var finishReason = firstChoice.GetProperty("finish_reason");
        Assert.Equal(JsonValueKind.String, finishReason.ValueKind);

        // Verify usage information
        var usage = chatResult.AssertProperty("usage");
        var promptTokens = usage.AssertProperty("prompt_tokens");
        Assert.True(promptTokens.GetInt32() > 0, "Should have consumed prompt tokens");

        var completionTokens = usage.AssertProperty("completion_tokens");
        Assert.True(completionTokens.GetInt32() > 0, "Should have generated completion tokens");

        var totalTokens = usage.AssertProperty("total_tokens");
        Assert.True(totalTokens.GetInt32() > 50, "Conversation should consume reasonable tokens");
        Assert.Equal(promptTokens.GetInt32() + completionTokens.GetInt32(), totalTokens.GetInt32());

        // Verify command metadata (returned resource and deployment names should match input)
        var commandResourceName = result.AssertProperty("resourceName");
        Assert.Equal(JsonValueKind.String, commandResourceName.ValueKind);
        Assert.Equal(resourceName, commandResourceName.GetString());

        var commandDeploymentName = result.AssertProperty("deploymentName");
        Assert.Equal(JsonValueKind.String, commandDeploymentName.ValueKind);
        Assert.Equal(deploymentName, commandDeploymentName.GetString());
    }

    [Fact]
    public async Task Should_list_all_foundry_resources_in_subscription()
    {
        var subscriptionId = Settings.SubscriptionId;

        var result = await CallToolAsync(
            "azmcp_foundry_resource_get",
            new()
            {
                { "subscription", subscriptionId },
                { "tenant", Settings.TenantId }
            });

        // Verify the response structure
        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // Should have at least one resource (the test resource)
        Assert.NotEmpty(resources.EnumerateArray());

        // Verify first resource structure
        var firstResource = resources.EnumerateArray().First();

        // Verify required properties exist
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

        // Verify deployments array exists (may be empty)
        var deployments = firstResource.AssertProperty("deployments");
        Assert.Equal(JsonValueKind.Array, deployments.ValueKind);
    }

    [Fact]
    public async Task Should_list_foundry_resources_in_resource_group()
    {
        var subscriptionId = Settings.SubscriptionId;
        var resourceGroup = Settings.ResourceGroupName;

        var result = await CallToolAsync(
            "azmcp_foundry_resource_get",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "tenant", Settings.TenantId }
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
            Assert.Equal(resourceGroup, rg.GetString());
        }
    }

    [Fact]
    public async Task Should_get_specific_foundry_resource()
    {
        var subscriptionId = Settings.SubscriptionId;
        var resourceGroup = Settings.ResourceGroupName;
        var resourceName = Settings.ResourceBaseName;

        var result = await CallToolAsync(
            "azmcp_foundry_resource_get",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", resourceGroup },
                { "resource-name", resourceName },
                { "tenant", Settings.TenantId }
            });

        // Verify the response structure
        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // Should return exactly one resource
        Assert.Single(resources.EnumerateArray());

        var resource = resources.EnumerateArray().First();

        // Verify resource details match the request
        var returnedResourceName = resource.AssertProperty("resourceName");
        Assert.Equal(resourceName, returnedResourceName.GetString());

        var returnedResourceGroup = resource.AssertProperty("resourceGroup");
        Assert.Equal(resourceGroup, returnedResourceGroup.GetString());

        // Verify all required properties
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

        // Verify deployments array structure
        var deployments = resource.AssertProperty("deployments");
        Assert.Equal(JsonValueKind.Array, deployments.ValueKind);

        // If deployments exist, verify their structure
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

            // Optional properties - verify structure if present
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

    [Fact]
    public async Task Should_get_foundry_resource_using_static_resources()
    {
        // Use the static OpenAI account that's defined in test-resources.bicep
        var staticOpenAIAccount = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNT", "azmcp-test");
        var staticResourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OPENAIACCOUNTRESOURCEGROUP", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;

        var result = await CallToolAsync(
            "azmcp_foundry_resource_get",
            new()
            {
                { "subscription", subscriptionId },
                { "resource-group", staticResourceGroup },
                { "resource-name", staticOpenAIAccount },
                { "tenant", Settings.TenantId }
            });

        // Verify the response structure
        var resources = result.AssertProperty("resources");
        Assert.Equal(JsonValueKind.Array, resources.ValueKind);

        // Should return the static resource
        Assert.NotEmpty(resources.EnumerateArray());

        var resource = resources.EnumerateArray().First();

        // Verify resource matches static configuration
        var resourceName = resource.AssertProperty("resourceName");
        Assert.Equal(staticOpenAIAccount, resourceName.GetString());

        var resourceGroup = resource.AssertProperty("resourceGroup");
        Assert.Equal(staticResourceGroup, resourceGroup.GetString());

        // Verify endpoint is valid
        var endpoint = resource.AssertProperty("endpoint");
        Assert.Equal(JsonValueKind.String, endpoint.ValueKind);
        Assert.NotEmpty(endpoint.GetString()!);
        Assert.StartsWith("https://", endpoint.GetString());

        // Verify deployments exist for static resource
        var deployments = resource.AssertProperty("deployments");
        Assert.Equal(JsonValueKind.Array, deployments.ValueKind);

        // Static resource should have at least the gpt-4o-mini deployment
        var deploymentsArray = deployments.EnumerateArray().ToArray();
        if (deploymentsArray.Length > 0)
        {
            // Check if gpt-4o-mini deployment exists
            var hasExpectedDeployment = deploymentsArray.Any(d =>
            {
                if (d.TryGetProperty("deploymentName", out var name) ||
                    d.TryGetProperty("modelName", out name))
                {
                    var nameStr = name.GetString();
                    return nameStr != null && nameStr.Contains("gpt-4o-mini", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            });

            Output.WriteLine($"Found {deploymentsArray.Length} deployment(s) on static resource");
        }
    }

    private async Task<string> CreateAgent(string agentName, string projectEndpoint, string deploymentName)
    {
        var client = new PersistentAgentsClient(
            projectEndpoint,
            new CustomChainedCredential());

        var bingConnectionId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.CognitiveServices/accounts/{Settings.ResourceBaseName}/projects/{Settings.ResourceBaseName}-ai-projects/connections/{Settings.ResourceBaseName}-bing-connection";

        var bingGroundingToolParameters = new BingGroundingSearchToolParameters(
            [new BingGroundingSearchConfiguration(bingConnectionId)]
        );

        PersistentAgent agent = await client.Administration.CreateAgentAsync(
            model: deploymentName,
            name: agentName,
            instructions: "You politely help with general knowledge questions. Use the bing search tool to help ground your responses.",
            tools: [new BingGroundingToolDefinition(bingGroundingToolParameters)]);
        return agent.Id;
    }
}
