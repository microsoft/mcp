// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Skills;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.SreAgent.Commands.Agents;

[CommandMetadata(
    Id = "f3ed0747-6f67-451a-a699-cfaf7ad33f4d",
    Name = "list",
    Title = "List SRE Agent Tools",
    Description = "Lists custom tools on a targeted SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsToolsListCommand(ILogger<AgentsToolsListCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<AgentToolsListOptions>
{
    private readonly ILogger<AgentsToolsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
    }

    protected override AgentToolsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
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
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options.Subscription!,
                options.ResourceGroup,
                options.Agent!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var tools = await _sreAgentService.ListAgentToolsAsync(endpoint, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(tools), SreAgentJsonContext.Default.AgentToolsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent tools from agent resource {Agent}.", options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentToolsListCommandResult(List<SreAgentTool> Tools);
}
