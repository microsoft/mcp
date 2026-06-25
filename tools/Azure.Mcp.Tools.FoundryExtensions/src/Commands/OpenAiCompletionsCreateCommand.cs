// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    Id = "e5f6a7b8-5678-9abc-ef01-234567890123",
    Name = "create-completion",
    Title = "Create OpenAI Completion",
    Description = """
        Create text completions using Azure OpenAI in Microsoft Foundry. Send a prompt or question to Azure OpenAI models
        deployed in your Microsoft Foundry resource and receive generated text answers. Use this when you need to create
        completions, get AI-generated content, generate answers to questions, or produce text completions from Azure
        OpenAI based on any input prompt. Supports customization with temperature and max tokens.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class OpenAiCompletionsCreateCommand(IFoundryExtensionsService foundryExtensionsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<OpenAiCompletionsCreateOptions, OpenAiCompletionsCreateCommand.OpenAiCompletionsCreateCommandResult>(subscriptionResolver)
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OpenAiCompletionsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var foundryService = _foundryExtensionsService;
            var result = await foundryService.CreateCompletionAsync(
                options.ResourceName,
                options.Deployment,
                options.PromptText,
                options.Subscription!,
                options.ResourceGroup,
                options.MaxTokens,
                options.Temperature,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result.CompletionText, result.UsageInfo),
                FoundryExtensionsJsonContext.Default.OpenAiCompletionsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record OpenAiCompletionsCreateCommandResult(string CompletionText, CompletionUsageInfo UsageInfo);
}
