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
