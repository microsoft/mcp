// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Agents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.SreAgent.Commands.Agents;

[CommandMetadata(
    Id = "7385f6f1-c535-4edb-908a-65d6e78ed51a",
    Name = "get",
    Title = "Get SRE Sub-Agent",
    Description = "Gets a sub-agent definition from a targeted SRE Agent resource. Required: --subscription, --agent, --name.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsGetCommand(ILogger<AgentsGetCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<AgentsGetOptions>
{
    private readonly ILogger<AgentsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name);
    }

    protected override AgentsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Agent.Name);
        options.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name);
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

            var agent = await _sreAgentService.GetSubAgentAsync(endpoint, options.Name!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(agent), SreAgentJsonContext.Default.AgentsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE sub-agent {Name} from agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsGetCommandResult(SreSubAgent Agent);
}
