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

    public override string Id => "8238b073-a302-49e6-8a27-8aab04c848fe";

    public override string Name => "list";

    public override string Description =>
        """
        List all Azure AI Agents in an Azure AI Foundry project. Shows agents that can be used for AI workflows, 
        evaluations, and interactive tasks. Requires the project endpoint URL (format: https://<resource>.services.ai.azure.com/api/projects/<project-name>).
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
        command.Options.Add(FoundryOptionDefinitions.EndpointOption);
    }

    protected override AgentsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
                options.RetryPolicy,
                cancellationToken: cancellationToken);

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
