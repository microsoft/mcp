// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Options.Hooks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Hooks;

[CommandMetadata(
    Id = "290f14b5-720e-4036-82b1-9fd9f577e009",
    Name = "delete",
    Title = "Delete SRE Agent Hook",
    Description = "Delete a hook from an Azure SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksDeleteCommand(ILogger<HooksDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HooksDeleteOptions, HooksDeleteCommand.HooksDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<HooksDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HooksDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete hook '{options.Name}': destructive operation requires --confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            await _sreAgentService.DeleteHookAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(true, options.Name), SreAgentJsonContext.Default.HooksDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent hook.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record HooksDeleteCommandResult(bool Deleted, string Name);
}

