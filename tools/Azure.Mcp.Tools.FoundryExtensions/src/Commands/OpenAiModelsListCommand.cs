// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
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
        List Azure OpenAI model deployments in a Microsoft Foundry resource, including deployment names, model names,
        versions, capabilities, and deployment status. Use this to show model deployments, check which OpenAI models
        are deployed, or see available models in a specific Foundry resource. Requires resource-name and resource-group.
        For Foundry resource-level details like endpoint URL, location, or SKU, use the resource get command instead.
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
