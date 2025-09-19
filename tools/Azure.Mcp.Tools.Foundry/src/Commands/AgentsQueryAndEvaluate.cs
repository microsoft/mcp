// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class AgentsQueryAndEvaluateCommand : GlobalCommand<AgentsQueryAndEvaluateOptions>
{
    private const string CommandTitle = "Query and Evaluate Agent";

    public override string Name => "query-and-evaluate";

    public override string Description =>
        """
        Query an agent and evaluate its response in a single operation.
        Returns both the agent response and evaluation results
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FoundryOptionDefinitions.AgentIdOption);
        command.Options.Add(FoundryOptionDefinitions.QueryOption);
        command.Options.Add(FoundryOptionDefinitions.EndpointOption);
        command.Options.Add(FoundryOptionDefinitions.EvaluatorsOption);
        command.Options.Add(FoundryOptionDefinitions.AzureOpenAIEndpointOption);
        command.Options.Add(FoundryOptionDefinitions.AzureOpenAIDeploymentOption);
    }

    protected override AgentsQueryAndEvaluateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption);
        options.AgentId = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.AgentIdOption);
        options.Query = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.QueryOption);
        options.Evaluators = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EvaluatorsOption);
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
            var result = await service.QueryAndEvaluateAgent(
                options.AgentId!,
                options.Query!,
                options.Endpoint!,
                options.AzureOpenAIEndpoint!,
                options.AzureOpenAIDeployment!,
                options.Tenant,
                options.Evaluators?.Split(',').Select(e => e.Trim()).ToList());

            context.Response.Results = ResponseResult.Create(
                new AgentsQueryAndEvaluateCommandResult(result),
                FoundryJsonContext.Default.AgentsQueryAndEvaluateCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsQueryAndEvaluateCommandResult(AgentsQueryAndEvaluateResult Response);
}
