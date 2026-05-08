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
    Id = "9c2d406d-61c3-4740-a0b4-68f27dd684e4",
    Name = "get",
    Title = "Get SRE Agent Tool",
    Description = "Gets a custom tool definition from a targeted SRE Agent resource. Required: --subscription, --agent, --name.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsToolsGetCommand(ILogger<AgentsToolsGetCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<AgentsToolsGetOptions>
{
    private readonly ILogger<AgentsToolsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name);
    }

    protected override AgentsToolsGetOptions BindOptions(ParseResult parseResult)
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

            var tool = await _sreAgentService.GetAgentToolAsync(endpoint, options.Name!, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(tool), SreAgentJsonContext.Default.AgentsToolsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent tool {Name} from agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AgentsToolsGetCommandResult(SreAgentTool Tool);
}
