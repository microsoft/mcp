// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.AI.Agents.Persistent;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class AgentsListCommand : GlobalCommand<AgentsListOptions>
{
    private const string CommandTitle = "List Evaluation Agents";
    private readonly Option<string> _endpointOption = FoundryOptionDefinitions.EndpointOption;

    public override string Name => "list";

    public override string Description =>
        """
        List all Azure AI Agents available in the configured project.
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
        command.Options.Add(_endpointOption);
    }

    protected override AgentsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValue(_endpointOption);
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
            var agents = await service.ListAgents(
                options.Endpoint!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = agents?.Count > 0 ?
                ResponseResult.Create(
                    new AgentsListCommandResult(agents),
                    FoundryJsonContext.Default.AgentsListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsListCommandResult(IEnumerable<PersistentAgent> Agents);
}
