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
    Id = "53dbfc5d-95f3-4b68-94d0-f7fc5bd390ba",
    Name = "delete",
    Title = "Delete SRE Sub-Agent",
    Description = "Deletes a sub-agent from a targeted SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsDeleteCommand(ILogger<AgentsDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AgentsDeleteOptions, AgentsDeleteCommand.AgentsDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AgentsDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete sub-agent '{options.Name}': destructive operation requires --confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var result = await _sreAgentService.DeleteSubAgentAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), SreAgentJsonContext.Default.AgentsDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE sub-agent {Name} from agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AgentsDeleteCommandResult(SreAgentDeleteResult Agent);
}
