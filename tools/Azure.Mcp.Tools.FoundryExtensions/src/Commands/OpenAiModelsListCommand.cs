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
    Id = "a7b8c9d0-7890-bcde-0123-456789012345",
    Name = "models-list",
    Title = "List OpenAI Models",
    Description = """
        List Azure OpenAI model deployments in a Microsoft Foundry resource, including deployment names, model names,
        versions, capabilities, and deployment status. Use this to show model deployments, check which OpenAI models
        are deployed, or see available models in a specific Foundry resource. Requires resource-name and resource-group.
        For Foundry resource-level details like endpoint URL, location, or SKU, use the resource get command instead.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class OpenAiModelsListCommand(IFoundryExtensionsService foundryExtensionsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<OpenAiModelsListOptions, OpenAiModelsListCommand.OpenAiModelsListCommandResult>(subscriptionResolver)
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OpenAiModelsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _foundryExtensionsService.ListOpenAiModelsAsync(
                options.ResourceName,
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result, options.ResourceName),
                FoundryExtensionsJsonContext.Default.OpenAiModelsListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record OpenAiModelsListCommandResult(OpenAiModelsListResult ModelsListResult, string ResourceName);
}
