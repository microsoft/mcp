// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Connectors;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Connectors;

[CommandMetadata(
    Id = "abf7823e-3dc7-4d6b-bf00-65576e56b402",
    Name = "get",
    Title = "Get SRE Agent Connector",
    Description = "Get details for a connector configured on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsGetCommand(ILogger<ConnectorsGetCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConnectorsGetOptions, ConnectorsGetCommand.ConnectorsGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ConnectorsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConnectorsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var connector = await _sreAgentService.GetConnectorAsync(
                options.Subscription!,
                resourceGroup,
                options.Agent,
                options.Name,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(connector), SreAgentJsonContext.Default.ConnectorsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConnectorsGetCommandResult(AgentConnector Connector);
}

