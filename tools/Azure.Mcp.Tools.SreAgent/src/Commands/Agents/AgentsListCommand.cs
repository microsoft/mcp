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
    Id = "5d6f6f8a-2b8e-4f0c-9c5d-2b3e2c0e1f01",
    Name = "list",
    Title = "List SRE Agents",
    Description = """
        List Azure SRE Agent resources in a subscription. Optionally filter by resource group.
        Each result includes: name, id, location, resourceGroup, provisioningState, endpoint.
        If no SRE Agent resources are found the tool returns an empty list.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AgentsListCommand(ILogger<AgentsListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AgentsListOptions, AgentsListCommand.AgentsListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AgentsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AgentsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var agents = await _sreAgentService.ListAgentsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(agents), SreAgentJsonContext.Default.AgentsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SRE Agent resources. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AgentsListCommandResult(List<SreAgentResource> Agents);
}
