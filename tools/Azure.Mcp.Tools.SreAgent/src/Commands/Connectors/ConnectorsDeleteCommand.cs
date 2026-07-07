// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Options.Connectors;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Connectors;

[CommandMetadata(
    Id = "50f58038-1258-48cc-a7d2-bc6c29614405",
    Name = "delete",
    Title = "Delete SRE Agent Connector",
    Description = "Delete a connector from an Azure SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConnectorsDeleteCommand(ILogger<ConnectorsDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConnectorsDeleteOptions, ConnectorsDeleteCommand.ConnectorsDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ConnectorsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConnectorsDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete connector '{options.Name}': destructive operation requires --confirm true.");
            }

            var resourceGroup = await SreAgentCommandHelpers.ResolveAgentResourceGroupAsync(
                _sreAgentService,
                options,
                cancellationToken);

            await _sreAgentService.DeleteConnectorAsync(
                options.Subscription!,
                resourceGroup,
                options.Agent,
                options.Name,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(true, options.Name), SreAgentJsonContext.Default.ConnectorsDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent connector.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConnectorsDeleteCommandResult(bool Deleted, string Name);
}

