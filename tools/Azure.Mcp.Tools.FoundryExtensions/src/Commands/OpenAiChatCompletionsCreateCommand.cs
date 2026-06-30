// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

[CommandMetadata(
    Id = "d4e5f6a7-4567-89ab-def0-123456789012",
    Name = "chat-completions-create",
    Title = "Create OpenAI Chat Completions",
    Description = """
        Create chat completions using Azure OpenAI in Microsoft Foundry. Send messages to Azure OpenAI chat models deployed
        in your Microsoft Foundry resource and receive AI-generated conversational responses. Supports multi-turn conversations
        with message history, system instructions, and response customization. Use this when you need to create chat
        completions, have AI conversations, get conversational responses, or build interactive dialogues with Azure OpenAI.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class OpenAiChatCompletionsCreateCommand(IFoundryExtensionsService foundryExtensionsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<OpenAiChatCompletionsCreateOptions, OpenAiChatCompletionsCreateCommand.OpenAiChatCompletionsCreateCommandResult>(subscriptionResolver)
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OpenAiChatCompletionsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Parse the message array
            var messages = new List<object>();
            if (!string.IsNullOrEmpty(options.MessageArray))
            {
                using var jsonDocument = JsonDocument.Parse(options.MessageArray);
                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    // Convert JsonElement to JsonObject for proper type matching in service
                    var jsonNode = JsonNode.Parse(element.GetRawText());
                    if (jsonNode is JsonObject jsonObj)
                    {
                        messages.Add(jsonObj);
                    }
                }
            }

            var result = await _foundryExtensionsService.CreateChatCompletionsAsync(
                options.ResourceName,
                options.Deployment,
                options.Subscription!,
                options.ResourceGroup,
                messages,
                options.MaxTokens,
                options.Temperature,
                options.TopP,
                options.FrequencyPenalty,
                options.PresencePenalty,
                options.Stop,
                options.Stream,
                options.Seed,
                options.User,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result, options.ResourceName, options.Deployment),
                FoundryExtensionsJsonContext.Default.OpenAiChatCompletionsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record OpenAiChatCompletionsCreateCommandResult(
        ChatCompletionResult Result,
        string ResourceName,
        string DeploymentName);
}
