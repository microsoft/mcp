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

namespace Azure.Mcp.Tools.SreAgent.Commands.Hooks;

[CommandMetadata(
    Id = "dd2962f2-1ba5-4cf6-a975-ed8b1ddab107",
    Name = "list",
    Title = "List SRE Agent Hooks",
    Description = "List hooks configured on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksListCommand(ILogger<HooksListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, HooksListCommand.HooksListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<HooksListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var hooks = await _sreAgentService.ListHooksAsync(endpoint, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(hooks), SreAgentJsonContext.Default.HooksListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent hooks.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record HooksListCommandResult(List<HookEnvelope> Hooks);
}

