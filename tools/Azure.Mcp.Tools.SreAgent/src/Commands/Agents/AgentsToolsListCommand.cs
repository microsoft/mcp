// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Skills;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Agents;

[CommandMetadata(
    Id = "f3ed0747-6f67-451a-a699-cfaf7ad33f4d",
    Name = "list",
    Title = "List SRE Agent Tools",
    Description = "Lists custom tools on a targeted SRE Agent resource. Required: --subscription and --agent.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsToolsListCommand(ILogger<AgentsToolsListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AgentToolsListOptions, AgentsToolsListCommand.AgentToolsListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsToolsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AgentToolsListOptions options, CancellationToken cancellationToken)
    {

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

    public record AgentToolsListCommandResult(List<SreAgentTool> Tools);
}
