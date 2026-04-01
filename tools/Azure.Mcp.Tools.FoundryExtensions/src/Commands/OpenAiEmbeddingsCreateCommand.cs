// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class OpenAiEmbeddingsCreateCommand(IFoundryExtensionsService foundryExtensionsService) : SubscriptionCommand<OpenAiEmbeddingsCreateOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "Create OpenAI Embeddings";

    public override string Id => "f6a7b8c9-6789-abcd-f012-345678901234";

    public override string Name => "embeddings-create";

    public override string Description =>
        $"""
        Create embeddings using Azure OpenAI in Microsoft Foundry. Generate vector embeddings from text using Azure OpenAI
        deployments in your Microsoft Foundry resource for semantic search, similarity comparisons, clustering, or machine
        learning. Use this when you need to create foundry embeddings, generate vectors from text, or convert text to
        numerical representations using Azure OpenAI. Requires resource-name, deployment-name, and input-text.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FoundryExtensionsOptionDefinitions.ResourceNameOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.DeploymentNameOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.InputTextOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.UserOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.EncodingFormatOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.DimensionsOption);
    }

    protected override OpenAiEmbeddingsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.ResourceNameOption.Name);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.DeploymentNameOption.Name);
        options.InputText = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.InputTextOption.Name);
        options.User = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.UserOption.Name);
        options.EncodingFormat = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.EncodingFormatOption.Name);
        options.Dimensions = parseResult.GetValueOrDefault<int?>(FoundryExtensionsOptionDefinitions.DimensionsOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = BindOptions(parseResult);

            var foundryService = _foundryExtensionsService;
            var result = await foundryService.CreateEmbeddingsAsync(
                options.ResourceName!,
                options.DeploymentName!,
                options.InputText!,
                options.Subscription!,
                options.ResourceGroup!,
                options.User,
                options.EncodingFormat!,
                options.Dimensions,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result, options.ResourceName!, options.DeploymentName!, options.InputText!),
                FoundryExtensionsJsonContext.Default.OpenAiEmbeddingsCreateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiEmbeddingsCreateCommandResult(
        EmbeddingResult EmbeddingResult,
        string ResourceName,
        string DeploymentName,
        string InputText);
}
