// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class AgentsQueryAndEvaluateCommand : GlobalCommand<AgentsQueryAndEvaluateOptions>
{
    private const string CommandTitle = "Query and Evaluate Agent";
    private readonly Option<string> _agentIdOption = FoundryOptionDefinitions.AgentIdOption;
    private readonly Option<string> _queryOption = FoundryOptionDefinitions.QueryOption;
    private readonly Option<string> _endpointOption = FoundryOptionDefinitions.EndpointOption;
    private readonly Option<string> _evaluators = FoundryOptionDefinitions.EvaluatorsOption;
    private readonly Option<string> _azureOpenAIEndpointOption = FoundryOptionDefinitions.AzureOpenAIEndpointOption;
    private readonly Option<string> _azureOpenAIDeploymentOption = FoundryOptionDefinitions.AzureOpenAIDeploymentOption;

    public override string Name => "query-and-evaluate";

    public override string Description =>
        """
        Query an agent and evaluate its response in a single operation.
        Parameters:
        - agent_id: ID of the agent to query
        - query: Text query to send to the agent
        - evaluators: Optional list of agent evaluator names to use (intent_resolution, tool_call_accuracy, task_adherence). Default is all evaluators if not specified.
        - Azure OpenAI endpoint: Endpoint of model to be used for evaluation
        - Azure OpenAI deployment: Specific deployment of model to be used for evaluation
        Returns both the agent response and evaluation results
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_agentIdOption);
        command.AddOption(_queryOption);
        command.AddOption(_endpointOption);
        command.AddOption(_evaluators);
        command.AddOption(_azureOpenAIEndpointOption);
        command.AddOption(_azureOpenAIDeploymentOption);
    }

    protected override AgentsQueryAndEvaluateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueForOption(_endpointOption);
        options.AgentId = parseResult.GetValueForOption(_agentIdOption);
        options.Query = parseResult.GetValueForOption(_queryOption);
        options.Evaluators = parseResult.GetValueForOption(_evaluators);
        options.AzureOpenAIEndpoint = parseResult.GetValueForOption(_azureOpenAIEndpointOption);
        options.AzureOpenAIDeployment = parseResult.GetValueForOption(_azureOpenAIDeploymentOption);

        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

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

    internal record AgentsQueryAndEvaluateCommandResult(Dictionary<string, object> Response);
}
