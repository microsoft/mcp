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

namespace Azure.Mcp.Tools.SreAgent.Commands.Connectors;

[CommandMetadata(
    Id = "9a2bf176-b2c7-4d58-8fa4-3d5f9d4e1b01",
    Name = "list",
    Title = "List SRE Agent Connectors",
    Description = "List connectors configured on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsListCommand(ILogger<ConnectorsListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, ConnectorsListCommand.ConnectorsListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ConnectorsListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var connectors = await _sreAgentService.ListConnectorsAsync(
                options.Subscription!,
                resourceGroup,
                options.Agent,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(connectors), SreAgentJsonContext.Default.ConnectorsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent connectors.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConnectorsListCommandResult(List<AgentConnector> Connectors);
}

