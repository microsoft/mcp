// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Security;
using System.Text.Json.Nodes;
using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.CognitiveServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using OpenAI.Chat;

namespace Azure.Mcp.Tools.FoundryExtensions.Services;

public class FoundryExtensionsService(
    IHttpClientFactory httpClientFactory,
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<FoundryExtensionsService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IFoundryExtensionsService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<FoundryExtensionsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private ArmEnvironment GetArmEnvironment() =>
        TenantService.CloudConfiguration.ArmEnvironment;

    /// <summary>
    /// Validates that the endpoint value satisfies the pattern of a Foundry project endpoint.
    /// </summary>
    internal void ValidateProjectEndpoint(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

        try
        {
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, "foundry", GetArmEnvironment());
        }
        catch (SecurityException ex)
        {
            throw new ArgumentException($"Invalid Foundry project endpoint: '{TruncateForLogging(endpoint)}'",
                nameof(endpoint), ex);
        }
    }

    /// <summary>
    /// Validates that the endpoint value satisfies the pattern of an Azure OpenAI endpoint.
    /// </summary>
    internal void ValidateAzureOpenAiEndpoint(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

        try
        {
            EndpointValidator.ValidateAzureServiceEndpoint(endpoint, "azure-openai", GetArmEnvironment());

            // Azure OpenAI-specific structural checks beyond domain validation
            var parsedUri = new Uri(endpoint);

            // Azure OpenAI endpoints should not contain path segments
            var paths = parsedUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length != 0)
            {
                throw new ArgumentException("Azure OpenAI endpoint should not contain path segments");
            }

            // Validate the resource name portion of the host
            string[] knownSuffixes = [".openai.azure.com", ".cognitiveservices.azure.com",
                ".openai.azure.cn", ".cognitiveservices.azure.cn",
                ".openai.azure.us", ".cognitiveservices.azure.us",
                ".openai.azure.de", ".cognitiveservices.azure.de"];
            var host = parsedUri.Host;
            var matchedSuffix = knownSuffixes.FirstOrDefault(suffix => host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
            if (matchedSuffix != null)
            {
                var resourceName = host[..^matchedSuffix.Length];
                if (resourceName.Length < 2 || resourceName.Length > 64)
                {
                    throw new ArgumentException("Azure OpenAI resource name must be between 2 and 64 characters");
                }

                if (resourceName.StartsWith('-') || resourceName.EndsWith('-'))
                {
                    throw new ArgumentException("Azure OpenAI resource name cannot start or end with a hyphen");
                }

                if (!resourceName.All(c => char.IsLetterOrDigit(c) || c == '-'))
                {
                    throw new ArgumentException("Azure OpenAI resource name must contain only alphanumeric characters and hyphens");
                }
            }
        }
        catch (SecurityException ex)
        {
            throw new ArgumentException($"Invalid Azure OpenAI endpoint: '{TruncateForLogging(endpoint)}'",
                nameof(endpoint), ex);
        }
        catch (ArgumentException ex) when (!ex.Message.StartsWith("Invalid Azure OpenAI endpoint", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Invalid Azure OpenAI endpoint: '{TruncateForLogging(endpoint)}'",
                nameof(endpoint), ex);
        }
    }

    private static string TruncateForLogging(string value)
    {
        const int maxLength = 100;
        return value.Length > maxLength ? value[..maxLength] + "..." : value;
    }

    public async Task<List<KnowledgeIndexInformation>> ListKnowledgeIndexes(
        string endpoint,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));
        ValidateProjectEndpoint(endpoint);

        var projectClient = await CreateAIProjectClientWithAuth(endpoint, tenantId, cancellationToken);
        var indexesClient = projectClient.GetIndexesClient();

        var indexes = new List<KnowledgeIndexInformation>();
        await foreach (var index in indexesClient.GetIndicesAsync(cancellationToken))
        {
            // Determine the type based on the actual type of the index
            string indexType = index switch
            {
                AzureAISearchIndex => "AzureAISearchIndex",
                ManagedAzureAISearchIndex => "ManagedAzureAISearchIndex",
                CosmosDBIndex => "CosmosDBIndex",
                _ => index.GetType().Name
            };

            indexes.Add(new()
            {
                Type = indexType,
                Id = index.Id,
                Name = index.Name,
                Version = index.Version,
                Description = index.Description,
                Tags = index.Tags?.ToDictionary()
            });
        }

        return indexes;
    }

    public async Task<KnowledgeIndexSchema> GetKnowledgeIndexSchema(
        string endpoint,
        string indexName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(endpoint), endpoint),
            (nameof(indexName), indexName));
        ValidateProjectEndpoint(endpoint);

        var projectClient = await CreateAIProjectClientWithAuth(endpoint, tenantId, cancellationToken);
        var indexesClient = projectClient.GetIndexesClient();

        // Find the index by name using async enumerable
        var index = await indexesClient.GetIndicesAsync(cancellationToken: cancellationToken)
            .Where(i => string.Equals(i.Name, indexName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (index == null)
        {
            throw new Exception($"Knowledge index '{indexName}' not found.");
        }

        // Map the SDK index to our AOT-safe schema type
        string indexType = index switch
        {
            AzureAISearchIndex => "AzureAISearchIndex",
            ManagedAzureAISearchIndex => "ManagedAzureAISearchIndex",
            CosmosDBIndex => "CosmosDBIndex",
            _ => index.GetType().Name
        };

        return new()
        {
            Type = indexType,
            Id = index.Id,
            Name = index.Name,
            Version = index.Version,
            Description = index.Description,
            Tags = index.Tags?.ToDictionary()
        };
    }

    public async Task<CompletionResult> CreateCompletionAsync(
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(resourceName), resourceName),
            (nameof(deploymentName), deploymentName),
            (nameof(promptText), promptText),
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken: cancellationToken);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName, cancellationToken: cancellationToken);

        // Get the endpoint
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client with flexible authentication
        AzureOpenAIClient client = await CreateOpenAIClientWithAuth(endpoint, resourceName, cognitiveServicesAccount.Value, authMethod, tenant, cancellationToken);

        var chatClient = client.GetChatClient(deploymentName);

        // Set up completion options
        var chatOptions = new ChatCompletionOptions();

        // Set max tokens with a default value if not provided
        var effectiveMaxTokens = maxTokens ?? 1000;
        if (effectiveMaxTokens <= 0)
        {
            effectiveMaxTokens = 1000; // Ensure we always have a positive value
        }
        chatOptions.MaxOutputTokenCount = effectiveMaxTokens;

        if (temperature.HasValue)
        {
            chatOptions.Temperature = (float)temperature.Value;
        }

        // Create the completion request
        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new UserChatMessage(promptText)
        };

        var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken: cancellationToken);
        var result = completion.Value;
        var completionText = result.Content[0].Text;

        var usageInfo = new CompletionUsageInfo(
            result.Usage.InputTokenCount,
            result.Usage.OutputTokenCount,
            result.Usage.TotalTokenCount);

        return new(completionText, usageInfo);
    }

    public async Task<EmbeddingResult> CreateEmbeddingsAsync(
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(resourceName), resourceName),
            (nameof(deploymentName), deploymentName),
            (nameof(inputText), inputText),
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken: cancellationToken);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName, cancellationToken: cancellationToken);

        // Get the endpoint
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client with flexible authentication
        AzureOpenAIClient client = await CreateOpenAIClientWithAuth(endpoint, resourceName, cognitiveServicesAccount.Value, authMethod, tenant, cancellationToken);

        var embeddingClient = client.GetEmbeddingClient(deploymentName);

        // Create the embedding request
        var embedding = await embeddingClient.GenerateEmbeddingAsync(inputText, cancellationToken: cancellationToken);
        var result = embedding.Value;

        var embeddingData = new List<EmbeddingData>
        {
            new("embedding", 0, result.ToFloats().ToArray())
        };

        // Note: Usage information might not be available in the current SDK version
        // Using placeholder values for now
        var splitInput = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var usageInfo = new EmbeddingUsageInfo(
            splitInput.Length, // Approximate token count
            splitInput.Length);

        return new("list", embeddingData, deploymentName, usageInfo);
    }

    public async Task<OpenAiModelsListResult> ListOpenAiModelsAsync(
        string resourceName,
        string subscription,
        string resourceGroup,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(resourceName), resourceName), (nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken: cancellationToken);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName, cancellationToken: cancellationToken);

        // Get all deployments for this account
        var deployments = cognitiveServicesAccount.Value.GetCognitiveServicesAccountDeployments();
        var allDeployments = new List<OpenAiModelDeployment>();

        await foreach (var deployment in deployments.GetAllAsync(cancellationToken: cancellationToken))
        {
            var deploymentData = deployment.Data;
            var properties = deploymentData.Properties;

            // Determine model capabilities based on model name
            var capabilities = DetermineModelCapabilities(properties.Model?.Name);

            var modelDeployment = new OpenAiModelDeployment(
                DeploymentName: deploymentData.Name,
                ModelName: properties.Model?.Name ?? "Unknown",
                ModelVersion: properties.Model?.Version,
                ScaleType: properties.ScaleSettings?.ScaleType?.ToString(),
                Capacity: properties.ScaleSettings?.Capacity,
                ProvisioningState: deploymentData.Properties.ProvisioningState?.ToString(),
                CreatedAt: null, // This information may not be available in the current API
                UpdatedAt: null, // This information may not be available in the current API
                Capabilities: capabilities
            );

            allDeployments.Add(modelDeployment);
        }

        return new(allDeployments, resourceName);
    }

    private static OpenAiModelCapabilities DetermineModelCapabilities(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
        {
            return new(false, false, false, false);
        }

        var modelNameLower = modelName.ToLowerInvariant();

        // Determine capabilities based on model name patterns
        var isEmbeddingModel = modelNameLower.Contains("embedding") || modelNameLower.Contains("ada");
        var isCompletionModel = modelNameLower.Contains("gpt") || modelNameLower.Contains("davinci") || modelNameLower.Contains("curie") || modelNameLower.Contains("babbage");
        var isChatModel = modelNameLower.Contains("gpt-3.5") || modelNameLower.Contains("gpt-4") || modelNameLower.Contains("gpt-35");
        var supportsFineTuning = modelNameLower.Contains("gpt-3.5") || modelNameLower.Contains("gpt-35") || modelNameLower.Contains("davinci");

        return new(
            Completions: isCompletionModel,
            Embeddings: isEmbeddingModel,
            ChatCompletions: isChatModel,
            FineTuning: supportsFineTuning);
    }

    public async Task<ChatCompletionResult> CreateChatCompletionsAsync(
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(resourceName), resourceName),
            (nameof(deploymentName), deploymentName),
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup));

        if (messages == null || messages.Count == 0)
        {
            throw new ArgumentException("Messages array cannot be null or empty", nameof(messages));
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken: cancellationToken);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName, cancellationToken: cancellationToken);

        // Get the endpoint
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client with flexible authentication
        AzureOpenAIClient client = await CreateOpenAIClientWithAuth(endpoint, resourceName, cognitiveServicesAccount.Value, authMethod, tenant, cancellationToken);

        var chatClient = client.GetChatClient(deploymentName);

        // Convert messages to ChatMessage objects
        var chatMessages = new List<OpenAI.Chat.ChatMessage>();
        foreach (var message in messages)
        {
            if (message is JsonObject jsonMessage)
            {
                var role = jsonMessage["role"]?.ToString();
                var content = jsonMessage["content"]?.ToString();

                if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(content))
                {
                    throw new ArgumentException("Each message must have 'role' and 'content' properties");
                }

                OpenAI.Chat.ChatMessage chatMessage = role.ToLowerInvariant() switch
                {
                    "system" => OpenAI.Chat.ChatMessage.CreateSystemMessage(content),
                    "user" => OpenAI.Chat.ChatMessage.CreateUserMessage(content),
                    "assistant" => OpenAI.Chat.ChatMessage.CreateAssistantMessage(content),
                    _ => throw new ArgumentException($"Invalid message role: {role}")
                };

                chatMessages.Add(chatMessage);
            }
            else
            {
                throw new ArgumentException("Messages must be valid JSON objects with 'role' and 'content' properties");
            }
        }

        // Create chat completion options
        var options = new ChatCompletionOptions();
        if (maxTokens.HasValue)
            options.MaxOutputTokenCount = maxTokens.Value;
        if (temperature.HasValue)
            options.Temperature = (float)temperature.Value;
        if (topP.HasValue)
            options.TopP = (float)topP.Value;
        if (frequencyPenalty.HasValue)
            options.FrequencyPenalty = (float)frequencyPenalty.Value;
        if (presencePenalty.HasValue)
            options.PresencePenalty = (float)presencePenalty.Value;
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        if (seed.HasValue)
            options.Seed = seed.Value;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        if (!string.IsNullOrEmpty(user))
            options.EndUserId = user;

        // Handle stop sequences
        if (!string.IsNullOrEmpty(stop))
        {
            var stopSequences = stop.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .ToArray();
            foreach (var stopSequence in stopSequences)
            {
                options.StopSequences.Add(stopSequence);
            }
        }

        // Create the chat completion
        var response = await chatClient.CompleteChatAsync(chatMessages, options, cancellationToken: cancellationToken);
        var result = response.Value;

        // Convert response to our model
        var choices = new List<ChatCompletionChoice>();
        for (int i = 0; i < result.Content.Count; i++)
        {
            var contentPart = result.Content[i];
            var chatCompletionMessage = new ChatCompletionMessage(
                Role: "assistant",
                Content: contentPart.Text,
                Name: null,
                FunctionCall: null,
                ToolCalls: null
            );

            choices.Add(new(
                Index: i,
                Message: chatCompletionMessage,
                FinishReason: result.FinishReason.ToString(),
                LogProbs: null));
        }

        // Create usage information
        var usage = new ChatCompletionUsageInfo(
            PromptTokens: result.Usage?.InputTokenCount ?? 0,
            CompletionTokens: result.Usage?.OutputTokenCount ?? 0,
            TotalTokens: result.Usage?.TotalTokenCount ?? 0
        );

        return new(
            Id: result.Id ?? Guid.NewGuid().ToString(),
            Object: "chat.completion",
            Created: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model: deploymentName,
            Choices: choices,
            Usage: usage,
            SystemFingerprint: result.SystemFingerprint
        );
    }

    private async Task<AzureOpenAIClient> CreateOpenAIClientWithAuth(
        string endpoint,
        string resourceName,
        CognitiveServicesAccountResource cognitiveServicesAccount,
        AuthMethod authMethod,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        // Configure AzureOpenAIClientOptions with HttpClient transport for test proxy support
        var httpClient = _httpClientFactory.CreateClient();
        var clientOptions = new AzureOpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        switch (authMethod)
        {
            case AuthMethod.Key:
                // Get the access key
                var keys = await cognitiveServicesAccount.GetKeysAsync(cancellationToken);
                var key = keys.Value.Key1;

                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException($"Access key not found for resource '{resourceName}'");
                }

                return new(new(endpoint), new AzureKeyCredential(key), clientOptions);

            case AuthMethod.Credential:
            default:
                var credential = await GetCredential(tenant, cancellationToken);
                return new(new(endpoint), credential, clientOptions);
        }
    }

    private async Task<AIProjectClient> CreateAIProjectClientWithAuth(
        string endpoint,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var credential = await GetCredential(tenant, cancellationToken);
        var transport = CreateTransport();

        var clientOptions = new AIProjectClientOptions
        {
            Transport = transport
        };

        return new(new(endpoint), credential, clientOptions);
    }

    private HttpClientTransport CreateTransport() => new(_httpClientFactory.CreateClient());

    public async Task<List<AiResourceInformation>> ListAiResourcesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        ArmClient armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        var resources = new List<AiResourceInformation>();

        // Get Cognitive Services accounts
        if (string.IsNullOrEmpty(resourceGroup))
        {
            // List all AI resources in the subscription
            await foreach (var account in subscriptionResource.GetCognitiveServicesAccountsAsync(cancellationToken: cancellationToken))
            {
                var resourceInfo = await BuildResourceInformation(account, subscriptionResource.Data.DisplayName, cancellationToken);
                resources.Add(resourceInfo);
            }
        }
        else
        {
            // List AI resources in specific resource group - use resource group scope for better performance
            var resourceGroupResource = await subscriptionResource.GetResourceGroups().GetAsync(resourceGroup, cancellationToken: cancellationToken);
            await foreach (var account in resourceGroupResource.Value.GetCognitiveServicesAccounts().GetAllAsync(cancellationToken: cancellationToken))
            {
                var resourceInfo = await BuildResourceInformation(account, subscriptionResource.Data.DisplayName, cancellationToken);
                resources.Add(resourceInfo);
                if (account.Data.Id.ResourceGroupName?.Equals(resourceGroup, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var retrieved = await BuildResourceInformation(account, subscriptionResource.Data.DisplayName, cancellationToken);
                    resources.Add(retrieved);
                }
            }
        }

        return resources;
    }

    public async Task<AiResourceInformation> GetAiResourceAsync(
        string subscription,
        string resourceGroup,
        string resourceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(resourceName), resourceName));

        ArmClient armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var rgResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken: cancellationToken);

        if (rgResource?.Value == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
        }

        var account = await rgResource.Value.GetCognitiveServicesAccountAsync(resourceName, cancellationToken: cancellationToken);
        if (account?.Value == null)
        {
            throw new Exception($"AI resource '{resourceName}' not found in resource group '{resourceGroup}'");
        }

        return await BuildResourceInformation(account.Value, subscriptionResource.Data.DisplayName, cancellationToken);
    }

    private async Task<AiResourceInformation> BuildResourceInformation(
        CognitiveServicesAccountResource account,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        var resourceInfo = new AiResourceInformation
        {
            ResourceName = account.Data.Name,
            ResourceGroup = account.Data.Id.ResourceGroupName,
            SubscriptionName = subscriptionName,
            Location = account.Data.Location.Name,
            Endpoint = account.Data.Properties?.Endpoint,
            Kind = account.Data.Kind,
            SkuName = account.Data.Sku?.Name,
            Deployments = []
        };

        // Get deployments for this resource
        try
        {
            await foreach (var deployment in account.GetCognitiveServicesAccountDeployments().WithCancellation(cancellationToken))
            {
                resourceInfo.Deployments.Add(new()
                {
                    DeploymentName = deployment.Data.Name,
                    ModelName = deployment.Data.Properties?.Model?.Name,
                    ModelVersion = deployment.Data.Properties?.Model?.Version,
                    ModelFormat = deployment.Data.Properties?.Model?.Format,
                    SkuName = deployment.Data.Sku?.Name,
                    SkuCapacity = deployment.Data.Sku?.Capacity,
                    ScaleType = deployment.Data.Properties?.ScaleSettings?.ScaleType.ToString(),
                    ProvisioningState = deployment.Data.Properties?.ProvisioningState.ToString()
                });
            }
        }
        catch
        {
            // If we can't list deployments, continue with empty list
            resourceInfo.Deployments = [];
        }

        return resourceInfo;
    }
}
