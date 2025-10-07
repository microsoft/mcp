// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class OpenAiEmbeddingsCreateCommand : SubscriptionCommand<OpenAiEmbeddingsCreateOptions>
{
    private const string CommandTitle = "Create OpenAI Embeddings";

    public override string Name => "embeddings-create";

    public override string Description =>
        $"""
        Generate vector embeddings for text using Azure OpenAI embedding models. This tool converts text into 
        high-dimensional vector representations for similarity search and machine learning applications. 
        Returns embedding vectors as JSON array. Requires resource-name, deployment-name, and input-text.
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
        command.Options.Add(FoundryOptionDefinitions.ResourceNameOption);
        command.Options.Add(FoundryOptionDefinitions.DeploymentNameOption);
        command.Options.Add(FoundryOptionDefinitions.InputTextOption);
        command.Options.Add(FoundryOptionDefinitions.UserOption);
        command.Options.Add(FoundryOptionDefinitions.EncodingFormatOption);
        command.Options.Add(FoundryOptionDefinitions.DimensionsOption);
    }

    protected override OpenAiEmbeddingsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResourceNameOption.Name);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.DeploymentNameOption.Name);
        options.InputText = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.InputTextOption.Name);
        options.User = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.UserOption.Name);
        options.EncodingFormat = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EncodingFormatOption.Name);
        options.Dimensions = parseResult.GetValueOrDefault<int?>(FoundryOptionDefinitions.DimensionsOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = BindOptions(parseResult);

            var foundryService = context.GetService<IFoundryService>();
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
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create<OpenAiEmbeddingsCreateCommandResult>(
                new OpenAiEmbeddingsCreateCommandResult(result, options.ResourceName!, options.DeploymentName!, options.InputText!),
                FoundryJsonContext.Default.OpenAiEmbeddingsCreateCommandResult);
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
