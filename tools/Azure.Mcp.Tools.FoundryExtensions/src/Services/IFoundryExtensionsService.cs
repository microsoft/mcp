// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.AI.Projects;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FoundryExtensions.Models;

namespace Azure.Mcp.Tools.FoundryExtensions.Services;

public interface IFoundryExtensionsService
{
    Task<List<KnowledgeIndexInformation>> ListKnowledgeIndexes(
        string endpoint,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<KnowledgeIndexSchema> GetKnowledgeIndexSchema(
        string endpoint,
        string indexName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default
    );

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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default
    );

    Task<OpenAiModelsListResult> ListOpenAiModelsAsync(
        string resourceName,
        string subscription,
        string resourceGroup,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default
    );

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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default
    );

    Task<List<AiResourceInformation>> ListAiResourcesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default
    );

    Task<AiResourceInformation> GetAiResourceAsync(
        string subscription,
        string resourceGroup,
        string resourceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default
    );
}
