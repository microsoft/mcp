// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class AgentsConnectCommand : GlobalCommand<AgentsConnectOptions>
{
    private const string CommandTitle = "Connect to AI Agent and Run a Query";
    private readonly Option<string> _agentIdOption = FoundryOptionDefinitions.AgentIdOption;
    private readonly Option<string> _queryOption = FoundryOptionDefinitions.QueryOption;
    private readonly Option<string> _endpointOption = FoundryOptionDefinitions.EndpointOption;

    public override string Name => "connect";

    public override string Description =>
        """
        Connect to a specific Azure AI Agent and run a query.
        Returns the agent's response along with thread and run IDs for potential evaluation.
        """;

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
        command.Options.Add(_agentIdOption);
        command.Options.Add(_queryOption);
        command.Options.Add(_endpointOption);
    }

    protected override AgentsConnectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AgentId = parseResult.GetValue(_agentIdOption);
        options.Query = parseResult.GetValue(_queryOption);
        options.Endpoint = parseResult.GetValue(_endpointOption);
        return options;
    }

    public override string Title => CommandTitle;

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
            var response = await service.ConnectAgent(
                options.AgentId!,
                options.Query!,
                options.Endpoint!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new AgentsConnectCommandResult(response),
                FoundryJsonContext.Default.AgentsConnectCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsConnectCommandResult(Dictionary<string, object> Response);
}
