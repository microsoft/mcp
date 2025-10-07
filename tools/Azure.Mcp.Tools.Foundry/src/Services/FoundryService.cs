// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks.Dataflow;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Services.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.CognitiveServices;
using Azure.ResourceManager.CognitiveServices.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using OpenAI.Chat;
using OpenAI.Embeddings;

#pragma warning disable AIEVAL001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Azure.Mcp.Tools.Foundry.Services;

public class FoundryService(
    IHttpClientService httpClientService,
    ISubscriptionService subscriptionService,
    ITenantService tenantService) : BaseAzureResourceService(subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService)), tenantService ?? throw new ArgumentNullException(nameof(tenantService))), IFoundryService
{
    private static readonly Dictionary<string, Func<IEvaluator>> AgentEvaluatorDictionary = new()
    {
        { "intent_resolution", () => new IntentResolutionEvaluator()},
        { "tool_call_accuracy", () => new ToolCallAccuracyEvaluator()},
        { "task_adherence", () => new TaskAdherenceEvaluator()},
    };

    private static readonly Dictionary<string, Func<IEnumerable<AITool>, EvaluationContext>> AgentEvaluatorContextDictionary = new()
    {
        { "intent_resolution", toolDefinitions => new IntentResolutionEvaluatorContext(toolDefinitions)},
        { "tool_call_accuracy", toolDefinitions => new ToolCallAccuracyEvaluatorContext(toolDefinitions)},
        { "task_adherence", toolDefinitions => new TaskAdherenceEvaluatorContext(toolDefinitions)},
    };

    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<List<ModelInformation>> ListModels(
        bool searchForFreePlayground = false,
        string publisherName = "",
        string licenseName = "",
        string modelName = "",
        int maxPages = 3,
        RetryPolicyOptions? retryPolicy = null)
    {
        string url = "https://api.catalog.azureml.ms/asset-gallery/v1.0/models";
        var request = new ModelCatalogRequest { Filters = [new ModelCatalogFilter("labels", ["latest"], "eq")] };

        if (searchForFreePlayground)
        {
            request.Filters.Add(new ModelCatalogFilter("freePlayground", ["true"], "eq"));
        }

        if (!string.IsNullOrEmpty(publisherName))
        {
            request.Filters.Add(new ModelCatalogFilter("publisher", [publisherName], "contains"));
        }

        if (!string.IsNullOrEmpty(licenseName))
        {
            request.Filters.Add(new ModelCatalogFilter("license", [licenseName], "contains"));
        }

        if (!string.IsNullOrEmpty(modelName))
        {
            request.Filters.Add(new ModelCatalogFilter("name", [modelName], "eq"));
        }

        var modelsList = new List<ModelInformation>();
        int pageCount = 0;

        try
        {
            while (pageCount < maxPages)
            {
                pageCount++;
                try
                {
                    var content = new StringContent(
                        JsonSerializer.Serialize(request, FoundryJsonContext.Default.ModelCatalogRequest),
                        Encoding.UTF8,
                        "application/json");

                    var httpResponse = await _httpClientService.DefaultClient.PostAsync(url, content);
                    httpResponse.EnsureSuccessStatusCode();

                    var responseText = await httpResponse.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize(responseText,
                        FoundryJsonContext.Default.ModelCatalogResponse);
                    if (response == null || response.Summaries.Count == 0)
                    {
                        break;
                    }

                    foreach (var summary in response.Summaries)
                    {
                        try
                        {
                            summary.DeploymentInformation.IsFreePlayground = summary.PlaygroundLimits != null;
                            if (!string.IsNullOrEmpty(summary.Publisher) &&
                                summary.Publisher.Equals("openai", StringComparison.OrdinalIgnoreCase))
                            {
                                summary.DeploymentInformation.IsOpenAI = true;
                            }
                            else
                            {
                                if (summary.AzureOffers != null)
                                {
                                    summary.DeploymentInformation.IsServerlessEndpoint =
                                        summary.AzureOffers.Contains("standard-paygo");

                                    summary.DeploymentInformation.IsManagedCompute =
                                        summary.AzureOffers.Contains("VM") ||
                                        summary.AzureOffers.Contains("VM-withSurcharge");
                                }
                            }

                            modelsList.Add(summary);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if (string.IsNullOrEmpty(response.ContinuationToken))
                    {
                        break;
                    }

                    request.ContinuationToken = response.ContinuationToken;
                }
                catch (HttpRequestException)
                {
                    break;
                }
                catch (JsonException)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving models from model catalog: {e.Message}");
        }

        return modelsList;
    }

    public async Task<List<Deployment>> ListDeployments(string endpoint, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint);

        try
        {
            var credential = await GetCredential(tenantId);
            var deploymentsClient = new AIProjectClient(new Uri(endpoint), credential).GetDeploymentsClient();

            var deployments = new List<Deployment>();
            await foreach (var deployment in deploymentsClient.GetDeploymentsAsync())
            {
                deployments.Add(deployment);
            }

            return deployments;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list deployments: {ex.Message}", ex);
        }
    }

    public async Task<ModelDeploymentResult> DeployModel(string deploymentName, string modelName, string modelFormat,
        string azureAiServicesName, string resourceGroup, string subscriptionId, string? modelVersion = null, string? modelSource = null,
        string? skuName = null, int? skuCapacity = null, string? scaleType = null, int? scaleCapacity = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(deploymentName, modelName, modelFormat, azureAiServicesName, resourceGroup, subscriptionId);

        try
        {
            // Create ArmClient for deployments
            ArmClient armClient = await CreateArmClientWithApiVersionAsync("Microsoft.CognitiveServices/accounts/deployments", "2025-06-01", null, retryPolicy);

            // Retrieve the Cognitive Services account
            var cognitiveServicesAccount = await GetGenericResourceAsync(
                armClient,
                new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.CognitiveServices/accounts/{azureAiServicesName}"));

            // Prepare data for the deployment
            ResourceIdentifier deploymentId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.CognitiveServices/accounts/{azureAiServicesName}/deployments/{deploymentName}");
            var deploymentData = new Models.CognitiveServicesAccountDeploymentData
            {
                Properties = new Models.CognitiveServicesAccountDeploymentProperties
                {
                    Model = new Models.CognitiveServicesAccountDeploymentModel
                    {
                        Format = modelFormat,
                        Name = modelName,
                        Version = modelVersion,
                        Source = string.IsNullOrEmpty(modelSource) ? null : modelSource
                    },
                    ScaleSettings = string.IsNullOrEmpty(scaleType) ? null : new Models.CognitiveServicesAccountDeploymentScaleSettings
                    {
                        ScaleType = scaleType,
                        Capacity = scaleCapacity
                    }
                },
                Sku = string.IsNullOrEmpty(skuName) ? null : new Models.CognitiveServicesSku
                {
                    Capacity = skuCapacity
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync<Models.CognitiveServicesAccountDeploymentData>(
                armClient,
                deploymentId,
                cognitiveServicesAccount.Data.Location,
                deploymentData,
                FoundryJsonContext.Default.CognitiveServicesAccountDeploymentData);
            if (!result.HasData)
            {
                return new ModelDeploymentResult
                {
                    HasData = false
                };
            }
            else
            {
                return new ModelDeploymentResult
                {
                    HasData = true,
                    Id = result.Data.Id.ToString(),
                    Name = result.Data.Name,
                    Type = result.Data.ResourceType.ToString(),
                    Sku = result.Data.Sku,
                    Tags = result.Data.Tags,
                    Properties = result.Data.Properties?.ToObjectFromJson(FoundryJsonContext.Default.IDictionaryStringObject)
                };
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to deploy model: {ex.Message}", ex);
        }
    }

    public async Task<List<KnowledgeIndexInformation>> ListKnowledgeIndexes(string endpoint, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint);

        try
        {
            var credential = await GetCredential(tenantId);
            var indexesClient = new AIProjectClient(new Uri(endpoint), credential).GetIndexesClient();

            var indexes = new List<KnowledgeIndexInformation>();
            await foreach (var index in indexesClient.GetIndicesAsync())
            {
                // Determine the type based on the actual type of the index
                string indexType = index switch
                {
                    AzureAISearchIndex => "AzureAISearchIndex",
                    ManagedAzureAISearchIndex => "ManagedAzureAISearchIndex",
                    CosmosDBIndex => "CosmosDBIndex",
                    _ => index.GetType().Name
                };

                var knowledgeIndex = new KnowledgeIndexInformation
                {
                    Type = indexType,
                    Id = index.Id,
                    Name = index.Name,
                    Version = index.Version,
                    Description = index.Description,
                    Tags = index.Tags?.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value) ?? null
                };

                indexes.Add(knowledgeIndex);
            }

            return indexes;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list knowledge indexes: {ex.Message}", ex);
        }
    }

    public async Task<KnowledgeIndexSchema> GetKnowledgeIndexSchema(string endpoint, string indexName, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint, indexName);

        try
        {
            var credential = await GetCredential(tenantId);
            var indexesClient = new AIProjectClient(new Uri(endpoint), credential).GetIndexesClient();

            // Find the index by name using async enumerable
            var index = await indexesClient.GetIndicesAsync()
                .Where(i => string.Equals(i.Name, indexName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();

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

            return new KnowledgeIndexSchema
            {
                Type = indexType,
                Id = index.Id,
                Name = index.Name,
                Version = index.Version,
                Description = index.Description,
                Tags = index.Tags?.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value)
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get knowledge index schema: {ex.Message}", ex);
        }
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(resourceName, deploymentName, promptText, subscription, resourceGroup);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName);

        // Get the endpoint
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client with flexible authentication
        AzureOpenAIClient client = await CreateOpenAIClientWithAuth(endpoint, resourceName, cognitiveServicesAccount.Value, authMethod, tenant);

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

        var completion = await chatClient.CompleteChatAsync(messages, chatOptions);

        var result = completion.Value;
        var completionText = result.Content[0].Text;

        var usageInfo = new CompletionUsageInfo(
            result.Usage.InputTokenCount,
            result.Usage.OutputTokenCount,
            result.Usage.TotalTokenCount);

        return new CompletionResult(completionText, usageInfo);
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(resourceName, deploymentName, inputText, subscription, resourceGroup);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName);

        // Get the endpoint
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client with flexible authentication
        AzureOpenAIClient client = await CreateOpenAIClientWithAuth(endpoint, resourceName, cognitiveServicesAccount.Value, authMethod, tenant);

        var embeddingClient = client.GetEmbeddingClient(deploymentName);

        // Create the embedding request
        var embedding = await embeddingClient.GenerateEmbeddingAsync(inputText);

        var result = embedding.Value;

        var embeddingData = new List<EmbeddingData>
        {
            new EmbeddingData(
                "embedding",
                0,
                result.ToFloats().ToArray())
        };

        // Note: Usage information might not be available in the current SDK version
        // Using placeholder values for now
        var splitInput = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var usageInfo = new EmbeddingUsageInfo(
            splitInput.Length, // Approximate token count
            splitInput.Length);

        return new EmbeddingResult(
            "list",
            embeddingData,
            deploymentName,
            usageInfo);
    }

    public async Task<OpenAiModelsListResult> ListOpenAiModelsAsync(
        string resourceName,
        string subscription,
        string resourceGroup,
        string? tenant = null,
        AuthMethod authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(resourceName, subscription, resourceGroup);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName);

        // Get all deployments for this account
        var deployments = cognitiveServicesAccount.Value.GetCognitiveServicesAccountDeployments();
        var allDeployments = new List<OpenAiModelDeployment>();

        await foreach (var deployment in deployments.GetAllAsync())
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

        return new OpenAiModelsListResult(allDeployments, resourceName);
    }

    private static OpenAiModelCapabilities DetermineModelCapabilities(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
        {
            return new OpenAiModelCapabilities(false, false, false, false);
        }

        var modelNameLower = modelName.ToLowerInvariant();

        // Determine capabilities based on model name patterns
        var isEmbeddingModel = modelNameLower.Contains("embedding") || modelNameLower.Contains("ada");
        var isCompletionModel = modelNameLower.Contains("gpt") || modelNameLower.Contains("davinci") || modelNameLower.Contains("curie") || modelNameLower.Contains("babbage");
        var isChatModel = modelNameLower.Contains("gpt-3.5") || modelNameLower.Contains("gpt-4") || modelNameLower.Contains("gpt-35");
        var supportsFineTuning = modelNameLower.Contains("gpt-3.5") || modelNameLower.Contains("gpt-35") || modelNameLower.Contains("davinci");

        return new OpenAiModelCapabilities(
            Completions: isCompletionModel,
            Embeddings: isEmbeddingModel,
            ChatCompletions: isChatModel,
            FineTuning: supportsFineTuning
        );
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
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(resourceName, deploymentName, subscription, resourceGroup);

        if (messages == null || messages.Count == 0)
        {
            throw new ArgumentException("Messages array cannot be null or empty", nameof(messages));
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName);

        // Get the endpoint
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client with flexible authentication
        AzureOpenAIClient client = await CreateOpenAIClientWithAuth(endpoint, resourceName, cognitiveServicesAccount.Value, authMethod, tenant);

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
        var response = await chatClient.CompleteChatAsync(chatMessages, options);
        var result = response.Value;

        // Convert response to our model
        var choices = new List<ChatCompletionChoice>();
        for (int i = 0; i < result.Content.Count; i++)
        {
            var contentPart = result.Content[i];
            var message = new ChatCompletionMessage(
                Role: "assistant",
                Content: contentPart.Text,
                Name: null,
                FunctionCall: null,
                ToolCalls: null
            );

            var choice = new ChatCompletionChoice(
                Index: i,
                Message: message,
                FinishReason: result.FinishReason.ToString(),
                LogProbs: null
            );

            choices.Add(choice);
        }

        // Create usage information
        var usage = new ChatCompletionUsageInfo(
            PromptTokens: result.Usage?.InputTokenCount ?? 0,
            CompletionTokens: result.Usage?.OutputTokenCount ?? 0,
            TotalTokens: result.Usage?.TotalTokenCount ?? 0
        );

        return new ChatCompletionResult(
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
        string? tenant = null)
    {
        AzureOpenAIClient client;

        switch (authMethod)
        {
            case AuthMethod.Key:
                // Get the access key
                var keys = await cognitiveServicesAccount.GetKeysAsync();
                var key = keys.Value.Key1;

                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException($"Access key not found for resource '{resourceName}'");
                }

                client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
                break;

            case AuthMethod.Credential:
            default:
                var credential = await GetCredential(tenant);
                client = new AzureOpenAIClient(new Uri(endpoint), credential);
                break;
        }
        return client;
    }

    public async Task<List<PersistentAgent>> ListAgents(string endpoint, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint);

        try
        {
            var credential = await GetCredential(tenantId);
            var agentsClient = new AIProjectClient(new Uri(endpoint), credential).GetPersistentAgentsClient();

            var agents = new List<PersistentAgent>();
            await foreach (var agent in agentsClient.Administration.GetAgentsAsync())
            {
                if (agent != null)
                {
                    agents.Add(agent);
                }
            }

            return agents;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list agents: {ex.Message}", ex);
        }
    }

    public async Task<AgentsConnectResult> ConnectAgent(
        string agentId,
        string query,
        string endpoint,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        try
        {
            ValidateRequiredParameters(agentId, query, endpoint);

            var credential = await GetCredential(tenantId);
            var agentsClient = new AIProjectClient(new Uri(endpoint), credential).GetPersistentAgentsClient();

            var thread = await agentsClient.Threads.CreateThreadAsync();
            var threadId = thread.Value.Id;

            await agentsClient.Messages.CreateMessageAsync(threadId, MessageRole.User, query);

            var run = await agentsClient.Runs.CreateRunAsync(threadId, agentId);
            var runId = run.Value.Id;

            while (run.Value.Status == RunStatus.Queued || run.Value.Status == RunStatus.InProgress || run.Value.Status == RunStatus.RequiresAction)
            {
                await Task.Delay(1);
                run = await agentsClient.Runs.GetRunAsync(threadId, runId);
            }

            if (run.Value.Status == RunStatus.Failed)
            {
                throw new Exception($"Agent run failed: {run.Value.LastError}");
            }

            var (textResponse, citations) = buildResponseAndCitations(agentsClient, threadId);
            var messages = await agentsClient.Messages.GetMessagesAsync(threadId, order: ListSortOrder.Ascending).ToListAsync();
            var runSteps = await agentsClient.Runs.GetRunStepsAsync(threadId, runId, order: ListSortOrder.Ascending).ToListAsync();

            List<PersistentThreadMessage> requestMessages = [];
            List<PersistentThreadMessage> responseMessages = [];

            foreach (var message in messages)
            {
                if (message.Role == MessageRole.User)
                {
                    requestMessages.Add(message);
                }
                else
                {
                    responseMessages.Add(message);
                }
            }

            var convertedRequestMessages = ConvertMessages(requestMessages).ToList();
            convertedRequestMessages.Prepend(new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, run.Value.Instructions));

            // full list of messages converted to Microsoft.Extensions.AI.ChatMessage for evaluation
            var convertedResponse = ConvertMessages(messages)
                .Concat(ConvertSteps(runSteps))
                .OrderBy(o => o.RawRepresentation switch
                {
                    PersistentThreadMessage m => m.CreatedAt,
                    RunStep s => s.CreatedAt,
                    _ => default,
                })
                .ThenBy(o => o.Contents!.OfType<FunctionCallContent>().Any() ? 0 : o.Contents!.OfType<FunctionResultContent>().Any() ? 1 : 2)
                .ToList();

            var toolDefinitions = ConvertTools(run.Value.Tools).ToList();

            return new AgentsConnectResult
            {
                AgentId = agentId,
                ThreadId = threadId,
                RunId = runId,
                ResponseText = textResponse.ToString().Trim(),
                QueryText = query,
                Response = convertedResponse,
                Query = convertedRequestMessages,
                ToolDefinitions = JsonSerializer.Serialize(toolDefinitions, (JsonTypeInfo<List<ToolDefinitionAIFunction>>)AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(List<ToolDefinitionAIFunction>))),
                Citations = citations
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to connect to agent: {ex.Message}", ex);
        }
    }

    public async Task<AgentsQueryAndEvaluateResult> QueryAndEvaluateAgent(string agentId, string query, string endpoint, string azureOpenAIEndpoint, string azureOpenAIDeployment, string? tenant = null, List<string>? evaluatorNames = null, RetryPolicyOptions? retryPolicy = null)
    {
        try
        {
            ValidateRequiredParameters(agentId, query, endpoint);

            var connectAgentResult = await ConnectAgent(agentId, query, endpoint, tenant, retryPolicy);

            var credential = await GetCredential(tenant);

            List<IEvaluator> evaluators = [];
            List<EvaluationContext> evaluationContexts = [];
            var toolDefinitions = connectAgentResult.ToolDefinitions;
            var loadedToolDefinitions = ConvertToolDefinitionsFromString(toolDefinitions);

            if (evaluatorNames == null || evaluatorNames.Count == 0)
            {
                evaluatorNames = AgentEvaluatorDictionary.Keys.ToList();
            }

            foreach (var name in evaluatorNames)
            {
                var evaluatorName = name.ToLowerInvariant();
                if (AgentEvaluatorDictionary.TryGetValue(evaluatorName, out var createEvaluator))
                {
                    var evaluator = createEvaluator();
                    evaluators.Add(evaluator);
                    evaluationContexts.Add(AgentEvaluatorContextDictionary[evaluatorName](loadedToolDefinitions ?? []));
                }
            }
            var compositeEvaluator = new CompositeEvaluator(evaluators);

            var azureOpenAIChatClient = GetAzureOpenAIChatClient(azureOpenAIEndpoint, azureOpenAIDeployment, credential);

            var evaluationResult = await compositeEvaluator.EvaluateAsync(
                connectAgentResult.Query ?? [],
                new ChatResponse(connectAgentResult.Response ?? []),
                new ChatConfiguration(azureOpenAIChatClient),
                evaluationContexts);

            return new AgentsQueryAndEvaluateResult
            {
                AgentId = agentId,
                ThreadId = connectAgentResult.ThreadId,
                RunId = connectAgentResult.RunId,
                ResponseText = connectAgentResult.ResponseText,
                QueryText = connectAgentResult.QueryText,
                Response = connectAgentResult.Response,
                Query = connectAgentResult.Query,
                ToolDefinitions = connectAgentResult.ToolDefinitions,
                Citations = connectAgentResult.Citations,
                Evaluators = evaluatorNames,
                EvaluationResult = evaluationResult
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to connect to agent and evaluate its response: {ex.Message}", ex);
        }
    }

    public async Task<AgentsEvaluateResult> EvaluateAgent(string evaluatorName, string query, string agentResponse, string azureOpenAIEndpoint, string azureOpenAIDeployment, string? toolDefinitions, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(evaluatorName, query, agentResponse);
        try
        {
            if (!AgentEvaluatorDictionary.ContainsKey(evaluatorName.ToLowerInvariant()))
            {
                throw new Exception($"Unknown evaluator {evaluatorName}. Supported evaluators are: {string.Join(", ", AgentEvaluatorDictionary.Keys)}");
            }

            var loadedQuery = JsonSerializer.Deserialize(query, (JsonTypeInfo<List<Microsoft.Extensions.AI.ChatMessage>>)AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(List<Microsoft.Extensions.AI.ChatMessage>)));
            var loadedAgentResponse = JsonSerializer.Deserialize(agentResponse, (JsonTypeInfo<List<Microsoft.Extensions.AI.ChatMessage>>)AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(List<Microsoft.Extensions.AI.ChatMessage>)));
            var loadedToolDefinitions = ConvertToolDefinitionsFromString(toolDefinitions);

            var evaluator = AgentEvaluatorDictionary[evaluatorName.ToLowerInvariant()]();
            List<EvaluationContext> evaluationContext = [];
            evaluationContext.Add(AgentEvaluatorContextDictionary[evaluatorName.ToLowerInvariant()](loadedToolDefinitions!));

            var credential = await GetCredential(tenantId);

            var azureOpenAIChatClient = GetAzureOpenAIChatClient(azureOpenAIEndpoint, azureOpenAIDeployment, credential);

            var result = await evaluator.EvaluateAsync(
                loadedQuery ?? [],
                new ChatResponse(loadedAgentResponse),
                new ChatConfiguration(azureOpenAIChatClient),
                evaluationContext);

            return new AgentsEvaluateResult
            {
                Evaluator = evaluatorName,
                EvaluationResult = result
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to evaluate agent response: {ex.Message}", ex);
        }
    }

    private List<ToolDefinitionAIFunction> ConvertToolDefinitionsFromString(string? toolDefinitions)
    {
        if (string.IsNullOrEmpty(toolDefinitions))
        {
            return [];
        }

        List<ToolDefinitionAIFunction> functionDefinitions = [];
        using JsonDocument toolDefinitionsAsJson = JsonDocument.Parse(toolDefinitions, new() { AllowTrailingCommas = true });
        foreach (JsonElement jsonElement in toolDefinitionsAsJson.RootElement.EnumerateArray())
        {
            functionDefinitions.Add(ToolDefinitionAIFunction.DeserializeToolDefinition(jsonElement));
        }
        return functionDefinitions;
    }

    private static IEnumerable<Microsoft.Extensions.AI.ChatMessage> ConvertMessages(IEnumerable<PersistentThreadMessage> messages)
    {
        foreach (PersistentThreadMessage message in messages)
        {
            Microsoft.Extensions.AI.ChatMessage result = new()
            {
                AuthorName = message.AssistantId,
                MessageId = message.Id,
                RawRepresentation = message,
                Role = message.Role == MessageRole.Agent ? ChatRole.Assistant : ChatRole.User,
            };

            foreach (var messageContent in message.ContentItems)
            {
                AIContent content = messageContent switch
                {
                    MessageTextContent mtc => new TextContent(mtc.Text),
                    _ => new AIContent(),
                };
                content.RawRepresentation = messageContent;
                result.Contents.Add(content);
            }

            yield return result;
        }
    }

    private static IEnumerable<Microsoft.Extensions.AI.ChatMessage> ConvertSteps(IEnumerable<RunStep> steps)
    {
        foreach (RunStep step in steps)
        {
            if (step.StepDetails is RunStepToolCallDetails { ToolCalls: not null } details)
            {
                foreach (RunStepToolCall toolCall in details.ToolCalls)
                {
                    Microsoft.Extensions.AI.ChatMessage CreateRequestMessage(string name, Dictionary<string, object?> arguments) =>
                        new(ChatRole.Assistant, [new FunctionCallContent(toolCall.Id, name, arguments)])
                        {
                            AuthorName = step.AssistantId,
                            MessageId = step.Id,
                            RawRepresentation = step,
                        };

                    Microsoft.Extensions.AI.ChatMessage CreateResponseMessage(object result) =>
                        new(ChatRole.Tool, [new FunctionResultContent(toolCall.Id, result)])
                        {
                            AuthorName = step.AssistantId,
                            MessageId = step.Id,
                            RawRepresentation = step,
                        };

                    switch (toolCall)
                    {
                        case RunStepFunctionToolCall function:
                            yield return CreateRequestMessage(function.Name, Parse(function.Arguments) ?? []);
                            // TODO: output doesn't appear to be available in the API

                            static Dictionary<string, object?>? Parse(string arguments)
                            {
                                try
                                { return JsonSerializer.Deserialize(arguments, DictionaryTypeInfo); }
                                catch { return null; }
                            }
                            break;

                        case RunStepCodeInterpreterToolCall code:
                            yield return CreateRequestMessage("code_interpreter", new() { ["input"] = code.Input });
                            yield return CreateResponseMessage(string.Concat(code.Outputs.OfType<RunStepCodeInterpreterLogOutput>().Select(o => o.Logs)));
                            break;

                        case RunStepBingGroundingToolCall bing:
                            yield return CreateRequestMessage("bing_grounding", new() { ["requesturl"] = bing.BingGrounding["requesturl"] });
                            break;

                        case RunStepFileSearchToolCall fileSearch:
                            yield return CreateRequestMessage("file_search", new()
                            {
                                ["ranking_options"] = JsonSerializer.SerializeToElement(new()
                                {
                                    ["ranker"] = fileSearch.FileSearch.RankingOptions.Ranker,
                                    ["score_threshold"] = fileSearch.FileSearch.RankingOptions.ScoreThreshold,
                                }, DictionaryTypeInfo)
                            });
                            yield return CreateResponseMessage(fileSearch.FileSearch.Results.Select(r => new
                            {
                                file_id = r.FileId,
                                file_name = r.FileName,
                                score = r.Score,
                                content = r.Content,
                            }).ToList());
                            break;

                        case RunStepAzureAISearchToolCall aiSearch:
                            yield return CreateRequestMessage("azure_ai_search", new() { ["input"] = aiSearch.AzureAISearch["input"] });
                            yield return CreateResponseMessage(new Dictionary<string, object?>
                            {
                                ["output"] = aiSearch.AzureAISearch["output"]
                            });
                            break;

                        case RunStepMicrosoftFabricToolCall fabric:
                            yield return CreateRequestMessage("fabric_dataagent", new() { ["input"] = fabric.MicrosoftFabric["input"] });
                            yield return CreateResponseMessage(fabric.MicrosoftFabric["output"]);
                            break;
                    }
                }
            }
        }
    }

    private static IEnumerable<ToolDefinitionAIFunction> ConvertTools(IEnumerable<ToolDefinition> tools)
    {
        foreach (var tool in tools)
        {
            switch (tool)
            {
                case FunctionToolDefinition functionToolDefinition:
                    yield return new ToolDefinitionAIFunction(functionToolDefinition.Name,
                        functionToolDefinition.Description,
                        DeserializeToElement(functionToolDefinition.Parameters));
                    break;
                case CodeInterpreterToolDefinition:
                    JsonObject codeInterpreterSchema = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["input"] = new JsonObject { ["type"] = "string", ["description"] = "Generated code to be executed." },
                        }
                    };
                    yield return new ToolDefinitionAIFunction(
                        "code_interpreter",
                        "Use code interpreter to read and interpret information from datasets, "
                        + "generate code, and create graphs and charts using your data. Supports "
                        + "up to 20 files.",
                        JsonSerializer.SerializeToElement(codeInterpreterSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject))));
                    break;
                case BingGroundingToolDefinition:
                    var bingGroundingSchema = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["requesturl"] = new JsonObject
                            {
                                ["type"] = "string",
                                ["description"] = "URL used in Bing Search API."
                            }
                        }
                    };
                    yield return new ToolDefinitionAIFunction(
                        "bing_grounding",
                        "Enhance model output with web data.",
                        JsonSerializer.SerializeToElement(bingGroundingSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject))));
                    break;
                case FileSearchToolDefinition:
                    JsonObject fileSearchSchema = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["ranking_options"] = new JsonObject
                            {
                                ["type"] = "object",
                                ["properties"] = new JsonObject
                                {
                                    ["ranker"] = new JsonObject { ["type"] = "string", ["description"] = "Ranking algorithm to use" },
                                    ["score_threshold"] = new JsonObject { ["type"] = "number", ["description"] = "Minimum score threshold for search results." },
                                },
                                ["description"] = "Ranking options for search results."
                            }
                        }
                    };
                    yield return new ToolDefinitionAIFunction(
                        "file_search",
                        "Search for data across uploaded files.",
                        JsonSerializer.SerializeToElement(fileSearchSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject))));
                    break;
                case AzureAISearchToolDefinition:
                    JsonObject azureAISearchSchema = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["input"] = new JsonObject { ["type"] = "string", ["description"] = "Search terms to use." },
                        }
                    };
                    yield return new ToolDefinitionAIFunction(
                        "azure_ai_search",
                        "Search an Azure AI Search index for relevant data.",
                        JsonSerializer.SerializeToElement(azureAISearchSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject))));
                    break;
                case MicrosoftFabricToolDefinition:
                    JsonObject microsoftFabricSchema = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["input"] = new JsonObject { ["type"] = "string", ["description"] = "Search terms to use." },
                        }
                    };
                    yield return new ToolDefinitionAIFunction(
                        "microsoft_fabric",
                        "Connect to Microsoft Fabric data agents to retrieve data across different data sources.",
                        JsonSerializer.SerializeToElement(microsoftFabricSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject))));
                    break;
            }
        }
    }

    private static (string messageContents, List<string> citations) buildResponseAndCitations(PersistentAgentsClient agentClient, string threadId)
    {
        var responseMessage = agentClient.Messages.GetMessagesAsync(threadId).FirstOrDefaultAsync(msg => msg.Role == MessageRole.Agent).Result;

        var result = new StringBuilder();
        var citations = new List<string>();

        if (responseMessage != null)
        {
            foreach (var messageContent in responseMessage.ContentItems)
            {
                if (messageContent is MessageTextContent messageTextContent)
                {
                    result.AppendLine(messageTextContent.Text);
                    foreach (var messageTextAnnotation in messageTextContent.Annotations)
                    {
                        if (messageTextAnnotation is MessageTextUriCitationAnnotation messageTextUriCitationAnnotation)
                        {
                            var citation = $"[{messageTextUriCitationAnnotation.UriCitation.Title}]({messageTextUriCitationAnnotation.UriCitation.Uri})";
                            if (!citations.Contains(citation))
                            {
                                citations.Add(citation);
                            }
                        }
                    }
                }
            }
        }

        if (citations.Count > 0)
        {
            result.AppendLine("\n\n## Sources");
            foreach (var citation in citations)
            {
                result.AppendLine($"- {citation}");
            }
        }
        return (result.ToString().Trim(), citations);
    }

    private static IChatClient GetAzureOpenAIChatClient(string azureOpenAIEndpoint, string azureOpenAIDeployment, TokenCredential credential)
    {
        var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        switch (azureOpenAIKey)
        {
            case null:
                return new AzureOpenAIClient(
                new Uri(azureOpenAIEndpoint),
                credential).GetChatClient(azureOpenAIDeployment).AsIChatClient();
            default:
                return new AzureOpenAIClient(
                new Uri(azureOpenAIEndpoint),
                new ApiKeyCredential(azureOpenAIKey)).GetChatClient(azureOpenAIDeployment).AsIChatClient();
        }
    }

    private static JsonTypeInfo<Dictionary<string, object?>> DictionaryTypeInfo { get; } =
        (JsonTypeInfo<Dictionary<string, object?>>)AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(Dictionary<string, object?>));

    private static JsonElement DeserializeToElement(BinaryData data) =>
        (JsonElement)JsonSerializer.Deserialize(data.ToMemory().Span, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonElement)))!;

    public sealed class ToolDefinitionAIFunction(string name, string description, JsonElement? schema = null) : AIFunctionDeclaration
    {
        public override string Name => name;
        public override string Description => description;
        public override JsonElement JsonSchema => schema ?? base.JsonSchema;

        public static ToolDefinitionAIFunction DeserializeToolDefinition(JsonElement json)
        {
            string name = json.GetProperty("name").GetString() ?? "";
            string description = json.GetProperty("description").GetString() ?? "";
            JsonElement? schema = json.TryGetProperty("jsonSchema", out JsonElement schemaElement) ? schemaElement.Clone() : null;
            return new ToolDefinitionAIFunction(name, description, schema);
        }

        public string SerializeToolDefinition()
        {
            MemoryStream bytes = new();
            using (Utf8JsonWriter writer = new(bytes))
            {
                writer.WriteStartObject();
                writer.WriteString("name", Name);
                writer.WriteString("description", Description);
                writer.WritePropertyName("jsonSchema");
                JsonSchema.WriteTo(writer);
                writer.WriteEndObject();
            }
            return Encoding.UTF8.GetString(bytes.GetBuffer(), 0, (int)bytes.Length);
        }
    }
}
