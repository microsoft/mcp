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
    Id = "fdc972bc-cf9a-484e-bdc5-7a91a5cd330b",
    Name = "activate",
    Title = "Activate SRE Agent Thread Hook",
    Description = "Activate an on-demand hook for a thread on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksThreadActivateCommand(ILogger<HooksThreadActivateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HooksThreadActivateOptions, HooksThreadActivateCommand.HooksThreadActivateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<HooksThreadActivateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HooksThreadActivateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.ActivateThreadHookAsync(endpoint, options.ThreadId, options.HookName, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(true, options.ThreadId, options.HookName), SreAgentJsonContext.Default.HooksThreadActivateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating SRE Agent thread hook.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record HooksThreadActivateCommandResult(bool Activated, string ThreadId, string HookName);
}

