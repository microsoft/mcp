// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Agents;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
public sealed class AgentsToolsGetCommand(ILogger<AgentsToolsGetCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AgentsToolsGetOptions, AgentsToolsGetCommand.AgentsToolsGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsToolsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AgentsToolsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var tool = await _sreAgentService.GetAgentToolAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(tool), SreAgentJsonContext.Default.AgentsToolsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent tool {Name} from agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AgentsToolsGetCommandResult(SreAgentTool Tool);
}
