// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Agents;

[CommandMetadata(
    Id = "7385f6f1-c535-4edb-908a-65d6e78ed51a",
    Name = "get",
    Title = "Get SRE Agent",
    Description = "Show the configuration details of a named SRE Agent. Retrieves endpoint, provisioning state, location, and settings for a specific SRE Agent by name, optionally filtered by resource group. Required: --subscription, --agent.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsGetCommand(ILogger<AgentsGetCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, AgentsGetCommand.AgentsGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var agent = await _sreAgentService.GetAgentAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Agent,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            if (agent is null)
            {
                context.Response.Status = System.Net.HttpStatusCode.NotFound;
                context.Response.Message = $"SRE Agent '{options.Agent}' not found.";
                return context.Response;
            }

            context.Response.Results = ResponseResult.Create(new(agent), SreAgentJsonContext.Default.AgentsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent resource {Agent}. Subscription: {Subscription}.", options.Agent, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AgentsGetCommandResult(SreAgentResource Agent);
}
