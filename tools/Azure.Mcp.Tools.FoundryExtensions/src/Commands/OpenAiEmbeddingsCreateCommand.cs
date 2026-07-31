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
    Id = "f6a7b8c9-6789-abcd-f012-345678901234",
    Name = "embeddings-create",
    Title = "Create OpenAI Embeddings",
    Description = """
        Create/generate vector embeddings from text using Azure OpenAI deployments in your Microsoft Foundry resource
        for semantic search, similarity comparisons, clustering, or machine learning. Use this when you need to create
        foundry embeddings, generate vectors from text, or convert text to numerical representations using Azure OpenAI.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class OpenAiEmbeddingsCreateCommand(IFoundryExtensionsService foundryExtensionsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<OpenAiEmbeddingsCreateOptions, OpenAiEmbeddingsCreateCommand.OpenAiEmbeddingsCreateCommandResult>(subscriptionResolver)
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, OpenAiEmbeddingsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _foundryExtensionsService.CreateEmbeddingsAsync(
                options.ResourceName,
                options.Deployment,
                options.InputText,
                options.Subscription!,
                options.ResourceGroup,
                options.User,
                options.EncodingFormat!,
                options.Dimensions,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result, options.ResourceName, options.Deployment, options.InputText),
                FoundryExtensionsJsonContext.Default.OpenAiEmbeddingsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record OpenAiEmbeddingsCreateCommandResult(
        EmbeddingResult EmbeddingResult,
        string ResourceName,
        string DeploymentName,
        string InputText);
}
