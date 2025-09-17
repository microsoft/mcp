// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class AgentsEvaluateCommand : GlobalCommand<AgentsEvaluateOptions>
{
    private const string CommandTitle = "Evaluate Agent";

    public override string Name => "evaluate";

    public override string Description =>
        """
        Run agent evaluation on agent data. Requires JSON strings for query, response, and tool definitions.
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
        command.Options.Add(FoundryOptionDefinitions.QueryOption);
        command.Options.Add(FoundryOptionDefinitions.EvaluatorNameOption);
        command.Options.Add(FoundryOptionDefinitions.ResponseOption);
        command.Options.Add(FoundryOptionDefinitions.ToolDefinitionsOption);
        command.Options.Add(FoundryOptionDefinitions.AzureOpenAIEndpointOption);
        command.Options.Add(FoundryOptionDefinitions.AzureOpenAIDeploymentOption);
    }

    protected override AgentsEvaluateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.QueryOption);
        options.EvaluatorName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EvaluatorNameOption);
        options.Response = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResponseOption);
        options.ToolDefinitions = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ToolDefinitionsOption);
        options.AzureOpenAIEndpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.AzureOpenAIEndpointOption);
        options.AzureOpenAIDeployment = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.AzureOpenAIDeploymentOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IFoundryService>();
            var result = await service.EvaluateAgent(
                options.EvaluatorName!,
                options.Query!,
                options.Response!,
                options.AzureOpenAIEndpoint!,
                options.AzureOpenAIDeployment!,
                options.ToolDefinitions);

            context.Response.Results = ResponseResult.Create(
                new AgentsEvaluateCommandResult(result),
                FoundryJsonContext.Default.AgentsEvaluateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsEvaluateCommandResult(AgentsEvaluateResult Response);
}
