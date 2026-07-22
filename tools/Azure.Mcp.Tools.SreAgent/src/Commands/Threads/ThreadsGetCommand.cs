// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Threads;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Threads;

[CommandMetadata(
    Id = "efab1704-5543-496a-830d-19ddb816a102",
    Name = "get",
    Title = "Get Thread",
    Description = "Get messages for an SRE Agent thread.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ThreadsGetCommand(ILogger<ThreadsGetCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ThreadsGetOptions, ThreadsGetCommand.ThreadsGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ThreadsGetCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ThreadsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var messages = await _sreAgentService.GetThreadMessagesAsync(endpoint, options.ThreadId, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(options.ThreadId, messages), SreAgentJsonContext.Default.ThreadsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SRE Agent thread.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record ThreadsGetCommandResult(string ThreadId, List<SreAgentThreadMessage> Messages);
}
