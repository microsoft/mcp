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
    Id = "7b8619c2-11e2-4fa6-bff1-a925ad7ca4bb",
    Name = "create",
    Title = "Create SRE Sub-Agent",
    Description = "Creates or updates a sub-agent on a targeted SRE Agent resource. Required: --subscription, --agent, --name.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsCreateCommand(ILogger<AgentsCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AgentsCreateOptions, AgentsCreateCommand.AgentsCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AgentsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var request = new SreSubAgentCreateRequest
            {
                Name = options.Name,
                Properties = new SreSubAgentProperties
                {
                    HandoffDescription = options.Description,
                    Instructions = options.Instructions,
                    Tools = ToList(options.Tools),
                    Handoffs = ToList(options.Handoffs)
                }
            };

            var agent = await _sreAgentService.CreateSubAgentAsync(endpoint, request, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(agent), SreAgentJsonContext.Default.AgentsCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE sub-agent {Name} on agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static List<string> ToList(string[]? values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).ToList() ?? [];

    public sealed record AgentsCreateCommandResult(SreSubAgent Agent);
}
