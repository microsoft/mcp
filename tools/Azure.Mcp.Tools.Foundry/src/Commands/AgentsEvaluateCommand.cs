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
        command.Options.Add(_queryOption);
        command.Options.Add(_evaluatorNameOption);
        command.Options.Add(_responseOption);
        command.Options.Add(_toolDefinitionsOption);
        command.Options.Add(_azureOpenAIEndpointOption);
        command.Options.Add(_azureOpenAIDeploymentOption);
    }

    protected override AgentsEvaluateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValue(_queryOption);
        options.EvaluatorName = parseResult.GetValue(_evaluatorNameOption);
        options.Response = parseResult.GetValue(_responseOption);
        options.ToolDefinitions = parseResult.GetValue(_toolDefinitionsOption);
        options.AzureOpenAIEndpoint = parseResult.GetValue(_azureOpenAIEndpointOption);
        options.AzureOpenAIDeployment = parseResult.GetValue(_azureOpenAIDeploymentOption);
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

    internal record AgentsEvaluateCommandResult(Dictionary<string, object> Response);
}
