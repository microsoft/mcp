// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Models;

namespace Azure.Mcp.Tools.Foundry.Services;

public interface IFoundryService
{
    Task<List<ModelInformation>> ListModels(
        bool searchForFreePlayground = false,
        string publisherName = "",
        string licenseName = "",
        string modelName = "",
        int maxPages = 3,
        RetryPolicyOptions? retryPolicy = null
    );

    Task<List<Deployment>> ListDeployments(
        string endpoint,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null
    );

    Task<ModelDeploymentResult> DeployModel(string deploymentName,
        string modelName,
        string modelFormat,
        string azureAiServicesName,
        string resourceGroup,
        string subscriptionId,
        string? modelVersion = null,
        string? modelSource = null,
        string? skuName = null,
        int? skuCapacity = null,
        string? scaleType = null,
        int? scaleCapacity = null,
        RetryPolicyOptions? retryPolicy = null
    );

    Task<List<KnowledgeIndexInformation>> ListKnowledgeIndexes(
        string endpoint,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null
    );

    Task<KnowledgeIndexSchema> GetKnowledgeIndexSchema(
        string endpoint,
        string indexName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null
    );

    Task<CompletionResult> CreateCompletionAsync(
        string resourceName,
        string deploymentName,
        string promptText,
        string subscription,
        string resourceGroup,
        int? maxTokens = null,
        double? temperature = null,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<EmbeddingResult> CreateEmbeddingsAsync(
        string resourceName,
        string deploymentName,
        string inputText,
        string subscription,
        string resourceGroup,
        string? user = null,
        string encodingFormat = "float",
        int? dimensions = null,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<OpenAiModelsListResult> ListOpenAiModelsAsync(
        string resourceName,
        string subscription,
        string resourceGroup,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<PersistentAgent>> ListAgents(string endpoint, string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<AgentsConnectResult> ConnectAgent(
        string agentId,
        string query,
        string endpoint,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<AgentsQueryAndEvaluateResult> QueryAndEvaluateAgent(
        string agentId,
        string query,
        string endpoint,
        string azureOpenAIEndpoint,
        string azureOpenAIDeployment,
        string? tenantId = null,
        List<string>? evaluatorNames = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<AgentsEvaluateResult> EvaluateAgent(
        string evaluatorName,
        string query,
        string agentResponse,
        string azureOpenAIEndpoint,
        string azureOpenAIDeployment,
        string? toolDefinitions,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<ChatCompletionResult> CreateChatCompletionsAsync(
        string resourceName,
        string deploymentName,
        string subscription,
        string resourceGroup,
        List<object> messages,
        int? maxTokens = null,
        double? temperature = null,
        double? topP = null,
        double? frequencyPenalty = null,
        double? presencePenalty = null,
        string? stop = null,
        bool? stream = null,
        int? seed = null,
        string? user = null,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<AiResourceInformation>> ListAiResourcesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<AiResourceInformation> GetAiResourceAsync(
        string subscription,
        string resourceGroup,
        string resourceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
