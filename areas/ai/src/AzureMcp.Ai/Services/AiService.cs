// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.AI.OpenAI;
using Azure.ResourceManager.CognitiveServices;
using Azure.ResourceManager.CognitiveServices.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using AzureMcp.Ai.Models;
using OpenAI.Chat;

namespace AzureMcp.Ai.Services;

public class AiService(ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IAiService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<CompletionResult> CreateCompletionAsync(
        string resourceName,
        string deploymentName,
        string promptText,
        string subscription,
        string resourceGroup,
        int? maxTokens = null,
        double? temperature = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(resourceName, deploymentName, promptText, subscription, resourceGroup);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
        
        // Get the Cognitive Services account
        var cognitiveServicesAccounts = resourceGroupResource.Value.GetCognitiveServicesAccounts();
        var cognitiveServicesAccount = await cognitiveServicesAccounts.GetAsync(resourceName);
        
        // Get the endpoint and key
        var accountData = cognitiveServicesAccount.Value.Data;
        var endpoint = accountData.Properties.Endpoint;
        
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException($"Endpoint not found for resource '{resourceName}'");
        }

        // Get the access key
        var keys = await cognitiveServicesAccount.Value.GetKeysAsync();
        var key = keys.Value.Key1;

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException($"Access key not found for resource '{resourceName}'");
        }

        // Create Azure OpenAI client
        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        var chatClient = client.GetChatClient(deploymentName);

        // Set up completion options
        var chatOptions = new ChatCompletionOptions();
        
        if (maxTokens.HasValue)
        {
            chatOptions.MaxOutputTokenCount = maxTokens.Value;
        }
        
        if (temperature.HasValue)
        {
            chatOptions.Temperature = (float)temperature.Value;
        }

        // Create the completion request
        var messages = new List<ChatMessage>
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
}
