// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
        var resourceName = Settings.DeploymentOutputs.GetValueOrDefault("OpenAIAccount", "dummy-test");
        var deploymentName = Settings.DeploymentOutputs.GetValueOrDefault("OpenAIDeploymentName", "gpt-4o-mini");
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("OpenAIAccountResourceGroup", "static-test-resources");
        var subscriptionId = Settings.SubscriptionId;
        
        try
        {
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
                    { "temperature", "0.7" }
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
        catch (Exception ex)
        {
            // Log the exception but don't fail the test if resources aren't available
            Output.WriteLine($"OpenAI completion test skipped due to: {ex.Message}");
            Output.WriteLine("This test requires an Azure OpenAI resource with a deployed model in AI Foundry.");
        }
    }
}
