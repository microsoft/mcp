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
    Id = "f5e93db8-f78a-4409-b625-176611ee0b0c",
    Name = "deactivate",
    Title = "Deactivate SRE Agent Thread Hook",
    Description = "Deactivate an on-demand hook for a thread on an Azure SRE Agent resource.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksThreadDeactivateCommand(ILogger<HooksThreadDeactivateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HooksThreadDeactivateOptions, HooksThreadDeactivateCommand.HooksThreadDeactivateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<HooksThreadDeactivateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HooksThreadDeactivateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            await _sreAgentService.DeactivateThreadHookAsync(endpoint, options.ThreadId, options.HookName, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(true, options.ThreadId, options.HookName), SreAgentJsonContext.Default.HooksThreadDeactivateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating SRE Agent thread hook.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record HooksThreadDeactivateCommandResult(bool Deactivated, string ThreadId, string HookName);
}

