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

public sealed class OpenAiModelsListCommand(IFoundryExtensionsService foundryExtensionsService) : SubscriptionCommand<OpenAiModelsListOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "List OpenAI Models";

    public override string Id => "a7b8c9d0-7890-bcde-0123-456789012345";

    public override string Name => "models-list";

    public override string Description =>
        $"""
        List all available Azure OpenAI models and deployments in a Microsoft Foundry resource. This tool retrieves information
        about Azure OpenAI models deployed in your Microsoft Foundry resource including model names, versions, capabilities,
        and deployment status. Use this when you need to see what OpenAI models are available, check model deployments,
        or list Azure OpenAI models in your foundry resource. Returns model information as JSON array. Requires resource-name.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
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
    }

    protected override OpenAiModelsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.ResourceNameOption.Name);
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
            var result = await foundryService.ListOpenAiModelsAsync(
                options.ResourceName!,
                options.Subscription!,
                options.ResourceGroup!,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result, options.ResourceName!),
                FoundryExtensionsJsonContext.Default.OpenAiModelsListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiModelsListCommandResult(OpenAiModelsListResult ModelsListResult, string ResourceName);
}
