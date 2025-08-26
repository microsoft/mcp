// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class AgentsEvaluateCommand : GlobalCommand<AgentsEvaluateOptions>
{
    private const string CommandTitle = "Evaluate Agent";
    private readonly Option<string> _queryOption = FoundryOptionDefinitions.QueryOption;
    private readonly Option<string> _evaluatorNameOption = FoundryOptionDefinitions.EvaluatorNameOption;
    private readonly Option<string> _responseOption = FoundryOptionDefinitions.ResponseOption;
    private readonly Option<string> _toolDefinitionsOption = FoundryOptionDefinitions.ToolDefinitionsOption;
    private readonly Option<string> _azureOpenAIEndpointOption = FoundryOptionDefinitions.AzureOpenAIEndpointOption;
    private readonly Option<string> _azureOpenAIDeploymentOption = FoundryOptionDefinitions.AzureOpenAIDeploymentOption;

    public override string Name => "evaluate";

    public override string Description =>
        """
        Run agent evaluation on agent data. Accepts JSON strings.
        Parameters:
        - evaluator_name: Name of the agent evaluator to use (intent_resolution, tool_call_accuracy, task_adherence)
        - query: User query (JSON string)
        - response: Agent response (JSON string)
        - tool_calls: Optional tool calls data (JSON string)
        - tool_definitions: Optional tool definitions (JSON string)
        - azure_openai_endpoint: Endpoint of model to be used for evaluation
        - azure_openai_deployment: Specific deployment of model to be used for evaluation
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_queryOption);
        command.AddOption(_evaluatorNameOption);
        command.AddOption(_responseOption);
        command.AddOption(_toolDefinitionsOption);
        command.AddOption(_azureOpenAIEndpointOption);
        command.AddOption(_azureOpenAIDeploymentOption);
    }

    protected override AgentsEvaluateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueForOption(_queryOption);
        options.EvaluatorName = parseResult.GetValueForOption(_evaluatorNameOption);
        options.Response = parseResult.GetValueForOption(_responseOption);
        options.ToolDefinitions = parseResult.GetValueForOption(_toolDefinitionsOption);
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

    internal record AgentsEvaluateCommandResult(Dictionary<string, object> Response);
}