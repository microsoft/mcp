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

public sealed class OpenAiModelsListCommand : SubscriptionCommand<OpenAiModelsListOptions>
{
    private const string CommandTitle = "List OpenAI Models";

    public override string Name => "models-list";

    public override string Description =>
        $"""
        List all available OpenAI models and deployments in an Azure resource. This tool retrieves information about 
        deployed models including model names, versions, capabilities, and deployment status. 
        Returns model information as JSON array. Requires resource-name.
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
        command.Options.Add(FoundryOptionDefinitions.ResourceNameOption);
    }

    protected override OpenAiModelsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResourceNameOption.Name);
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
            var result = await foundryService.ListOpenAiModelsAsync(
                options.ResourceName!,
                options.Subscription!,
                options.ResourceGroup!,
                options.Tenant,
                options.AuthMethod ?? AuthMethod.Credential,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create<OpenAiModelsListCommandResult>(
                new OpenAiModelsListCommandResult(result, options.ResourceName!),
                FoundryJsonContext.Default.OpenAiModelsListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record OpenAiModelsListCommandResult(
        OpenAiModelsListResult ModelsListResult,
        string ResourceName);
}
