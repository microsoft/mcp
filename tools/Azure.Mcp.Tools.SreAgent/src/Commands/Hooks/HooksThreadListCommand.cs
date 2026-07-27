// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Hooks;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Hooks;

[CommandMetadata(
    Id = "b2413e2f-e121-4d63-860c-28eebf4fd00a",
    Name = "list",
    Title = "List SRE Agent Thread Hooks",
    Description = "List hook activation state for a thread on an Azure SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HooksThreadListCommand(ILogger<HooksThreadListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HooksThreadListOptions, HooksThreadListCommand.HooksThreadListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<HooksThreadListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HooksThreadListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var hooks = await _sreAgentService.ListThreadHooksAsync(endpoint, options.ThreadId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(hooks), SreAgentJsonContext.Default.HooksThreadListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SRE Agent thread hooks.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record HooksThreadListCommandResult(ThreadHooksResponse ThreadHooks);
}

